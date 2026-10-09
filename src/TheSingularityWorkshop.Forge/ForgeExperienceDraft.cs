using TheSingularityWorkshop.MicroBundleDomain;

namespace TheSingularityWorkshop.Forge;

/// <summary>
/// Mutable, in-memory authoring draft copied from an Experience.
/// Each accepted semantic edit advances <see cref="Revision"/>. Reverting all
/// semantic differences restores <see cref="IsModified"/> to false, but does not
/// rewind the revision counter, so preview results can still be scoped safely.
/// This type does not itself represent a published artifact or implement publication.
/// </summary>
public sealed class ForgeExperienceDraft
{
    private readonly ulong _id;
    private readonly List<int> _ontology;
    private readonly List<ForgeMicroBundle> _bundles = [];
    private readonly Dictionary<ulong, Dictionary<string, ForgeFieldValue>> _fieldValues = [];
    private readonly DraftBaseline _baseline;
    private string _name;

    public ForgeExperienceDraft(ForgeExperience source)
    {
        ArgumentNullException.ThrowIfNull(source);

        _id = source.Id;
        _name = source.Name;
        _ontology = source.Ontology.ToList();
        _bundles.AddRange(source.MicroBundles.Select(CloneBundle));
        _baseline = DraftBaseline.Capture(_name, _ontology, _bundles, _fieldValues);
    }

    public ulong Id => _id;

    public string Name => _name;

    public IReadOnlyList<int> Ontology => _ontology.AsReadOnly();

    public IReadOnlyList<ForgeMicroBundle> MicroBundles => _bundles.AsReadOnly();

    /// <summary>
    /// Monotonically increasing revision of accepted semantic edits in this draft.
    /// No-op edits do not advance the revision; reverting an edit does.
    /// </summary>
    public long Revision { get; private set; }

    /// <summary>
    /// True when the current semantic content differs from the copied baseline.
    /// This is not yet a publication-readiness or validation result.
    /// </summary>
    public bool IsModified => !_baseline.Matches(_name, _ontology, _bundles, _fieldValues);

    public bool SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("An Experience name is required.", nameof(name));

        if (string.Equals(_name, name, StringComparison.Ordinal))
            return false;

        _name = name;
        AdvanceRevision();
        return true;
    }

    public bool SetOntology(IEnumerable<int> ontology)
    {
        ArgumentNullException.ThrowIfNull(ontology);
        var values = ontology.ToArray();

        if (_ontology.SequenceEqual(values))
            return false;

        _ontology.Clear();
        _ontology.AddRange(values);
        AdvanceRevision();
        return true;
    }

    public ForgeMicroBundle AddMicroBundle(
        ulong bundleId,
        string version,
        IEnumerable<MicroBundleDependency>? dependencies = null,
        IEnumerable<MicroBundleProvider>? providers = null,
        ReadOnlyMemory<byte> configuration = default)
    {
        if (_bundles.Any(x => x.Descriptor.Id == bundleId))
            throw new InvalidOperationException($"MicroBundle {bundleId} is already part of draft Experience {_id}.");

        var descriptor = new MicroBundleDescriptor(bundleId, version, dependencies, providers);
        var bundle = new ForgeMicroBundle(descriptor, configuration.ToArray());
        _bundles.Add(bundle);
        AdvanceRevision();
        return bundle;
    }

    public bool RemoveMicroBundle(ulong bundleId)
    {
        var index = _bundles.FindIndex(x => x.Descriptor.Id == bundleId);
        if (index < 0)
            return false;

        _bundles.RemoveAt(index);
        _fieldValues.Remove(bundleId);
        AdvanceRevision();
        return true;
    }

    /// <summary>
    /// Replaces the opaque configuration payload for a MicroBundle.
    /// Schema-aware field editing is intentionally separate until a typed
    /// field-to-payload contract exists.
    /// </summary>
    public bool SetConfiguration(ulong bundleId, ReadOnlyMemory<byte> configuration)
    {
        var index = _bundles.FindIndex(x => x.Descriptor.Id == bundleId);
        if (index < 0)
            throw new KeyNotFoundException($"MicroBundle {bundleId} is not part of draft Experience {_id}.");

        var current = _bundles[index];
        var replacement = configuration.ToArray();
        if (current.Configuration.Span.SequenceEqual(replacement))
            return false;

        _bundles[index] = current with { Configuration = replacement };
        AdvanceRevision();
        return true;
    }

    /// <summary>
    /// Attempts to accept a typed value for one top-level field of a MicroBundle.
    /// The supplied definition must match the exact MicroBundle ID and version in
    /// this draft. Rejected edits do not change values, IsModified, or Revision.
    /// Typed values remain editor-only until a compatible payload codec is supplied.
    /// </summary>
    public ForgeFieldEditResult TrySetFieldValue(
        ulong bundleId,
        MicroBundleDefinition definition,
        string fieldName,
        ForgeFieldValue value)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(value);
        if (string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentException("A field name is required.", nameof(fieldName));

        var bundle = _bundles.SingleOrDefault(x => x.Descriptor.Id == bundleId);
        if (bundle is null)
            return ForgeFieldEditResult.Rejected(new("bundle-not-in-draft", fieldName,
                $"MicroBundle {bundleId} is not part of draft Experience {_id}."));

        if (definition.Id != bundle.Descriptor.Id ||
            !string.Equals(definition.Version, bundle.Descriptor.Version, StringComparison.Ordinal))
        {
            return ForgeFieldEditResult.Rejected(new("schema-identity-mismatch", fieldName,
                $"The supplied schema identity/version does not match MicroBundle {bundleId} at version '{bundle.Descriptor.Version}'."));
        }

        var matches = definition.Fields.Where(x => string.Equals(x.Name, fieldName, StringComparison.Ordinal)).ToArray();
        if (matches.Length == 0)
            return ForgeFieldEditResult.Rejected(new("unknown-field", fieldName,
                $"Field '{fieldName}' is not declared by MicroBundle {bundleId}'s schema."));
        if (matches.Length > 1)
            return ForgeFieldEditResult.Rejected(new("duplicate-schema-field", fieldName,
                $"Schema field name '{fieldName}' is declared more than once and cannot be addressed unambiguously."));

        var diagnostics = ForgeFieldValueValidator.Validate(matches[0], value, fieldName);
        if (diagnostics.Count > 0)
            return ForgeFieldEditResult.Rejected(diagnostics);

        if (!_fieldValues.TryGetValue(bundleId, out var values))
        {
            values = new Dictionary<string, ForgeFieldValue>(StringComparer.Ordinal);
            _fieldValues.Add(bundleId, values);
        }

        if (values.TryGetValue(fieldName, out var current) && ValueEquals(current, value))
            return ForgeFieldEditResult.AcceptedWithoutChange();

        values[fieldName] = value;
        AdvanceRevision();
        return ForgeFieldEditResult.AcceptedWithChange();
    }

    /// <summary>
    /// Returns a detached read-only snapshot of typed editor values for a MicroBundle.
    /// These values are not runtime configuration bytes.
    /// </summary>
    public IReadOnlyDictionary<string, ForgeFieldValue> GetFieldValues(ulong bundleId)
    {
        if (!_bundles.Any(x => x.Descriptor.Id == bundleId))
            throw new KeyNotFoundException($"MicroBundle {bundleId} is not part of draft Experience {_id}.");

        var copy = _fieldValues.TryGetValue(bundleId, out var values)
            ? new Dictionary<string, ForgeFieldValue>(values, StringComparer.Ordinal)
            : new Dictionary<string, ForgeFieldValue>(StringComparer.Ordinal);
        return new System.Collections.ObjectModel.ReadOnlyDictionary<string, ForgeFieldValue>(copy);
    }

    /// <summary>
    /// Produces a detached editor-time Experience snapshot suitable for the existing
    /// FSM_COS manifest compiler. Typed field edits cannot be compiled until a
    /// compatible payload codec exists, so this method refuses to discard them.
    /// </summary>
    public ForgeExperience ToExperience()
    {
        if (_fieldValues.Values.Any(values => values.Count > 0))
            throw new InvalidOperationException(
                "This draft contains typed field edits, but no compatible MicroBundle configuration codec is registered. Typed edits cannot be compiled into runtime payload bytes.");

        var experience = new ForgeExperience(_id, _name, _ontology);
        foreach (var bundle in _bundles)
        {
            experience.AddMicroBundle(
                bundle.Descriptor.Id,
                bundle.Descriptor.Version,
                bundle.Descriptor.Dependencies,
                bundle.Descriptor.Providers,
                bundle.Configuration);
        }

        return experience;
    }

    private void AdvanceRevision() => Revision = checked(Revision + 1);

    private static ForgeMicroBundle CloneBundle(ForgeMicroBundle bundle) =>
        new(bundle.Descriptor, bundle.Configuration.ToArray());

    private static bool ValueEquals(ForgeFieldValue left, ForgeFieldValue right)
    {
        if (left.Kind != right.Kind ||
            !string.Equals(left.StringValue, right.StringValue, StringComparison.Ordinal) ||
            left.IntegerValue != right.IntegerValue ||
            left.FloatValue != right.FloatValue ||
            left.BooleanValue != right.BooleanValue ||
            left.Children.Count != right.Children.Count)
            return false;

        foreach (var child in left.Children)
        {
            if (!right.Children.TryGetValue(child.Key, out var other) || !ValueEquals(child.Value, other))
                return false;
        }

        return true;
    }

    private sealed class DraftBaseline
    {
        private readonly string _name;
        private readonly int[] _ontology;
        private readonly ForgeMicroBundle[] _bundles;
        private readonly Dictionary<ulong, Dictionary<string, ForgeFieldValue>> _fieldValues;

        private DraftBaseline(string name, int[] ontology, ForgeMicroBundle[] bundles,
            Dictionary<ulong, Dictionary<string, ForgeFieldValue>> fieldValues)
        {
            _name = name;
            _ontology = ontology;
            _bundles = bundles;
            _fieldValues = fieldValues;
        }

        private static Dictionary<ulong, Dictionary<string, ForgeFieldValue>> CloneFieldValues(
            IReadOnlyDictionary<ulong, Dictionary<string, ForgeFieldValue>> source) => source.ToDictionary(
                pair => pair.Key,
                pair => new Dictionary<string, ForgeFieldValue>(pair.Value, StringComparer.Ordinal));

        public static DraftBaseline Capture(
            string name,
            IEnumerable<int> ontology,
            IEnumerable<ForgeMicroBundle> bundles,
            IReadOnlyDictionary<ulong, Dictionary<string, ForgeFieldValue>> fieldValues) =>
            new(name, ontology.ToArray(), bundles.Select(CloneBundle).ToArray(), CloneFieldValues(fieldValues));

        public bool Matches(
            string name,
            IReadOnlyList<int> ontology,
            IReadOnlyList<ForgeMicroBundle> bundles,
            IReadOnlyDictionary<ulong, Dictionary<string, ForgeFieldValue>> fieldValues)
        {
            if (!string.Equals(_name, name, StringComparison.Ordinal) ||
                !_ontology.SequenceEqual(ontology) ||
                _bundles.Length != bundles.Count ||
                !FieldValuesMatch(_fieldValues, fieldValues))
                return false;

            for (var i = 0; i < _bundles.Length; i++)
            {
                var left = _bundles[i];
                var right = bundles[i];

                if (left.Descriptor.Id != right.Descriptor.Id ||
                    !string.Equals(left.Descriptor.Version, right.Descriptor.Version, StringComparison.Ordinal) ||
                    !left.Descriptor.Dependencies.SequenceEqual(right.Descriptor.Dependencies) ||
                    !left.Descriptor.Providers.SequenceEqual(right.Descriptor.Providers) ||
                    !left.Configuration.Span.SequenceEqual(right.Configuration.Span))
                    return false;
            }

            return true;
        }

        private static bool FieldValuesMatch(
            IReadOnlyDictionary<ulong, Dictionary<string, ForgeFieldValue>> left,
            IReadOnlyDictionary<ulong, Dictionary<string, ForgeFieldValue>> right)
        {
            if (left.Count != right.Count)
                return false;

            foreach (var bundle in left)
            {
                if (!right.TryGetValue(bundle.Key, out var other) || bundle.Value.Count != other.Count)
                    return false;
                foreach (var field in bundle.Value)
                {
                    if (!other.TryGetValue(field.Key, out var value) || !ValueEquals(field.Value, value))
                        return false;
                }
            }

            return true;
        }
    }
}

public sealed record ForgeFieldEditResult(bool IsAccepted, bool HasChanged,
    IReadOnlyList<ForgeFieldValueDiagnostic> Diagnostics)
{
    internal static ForgeFieldEditResult Rejected(params ForgeFieldValueDiagnostic[] diagnostics) =>
        Rejected((IReadOnlyList<ForgeFieldValueDiagnostic>)diagnostics);

    internal static ForgeFieldEditResult Rejected(IReadOnlyList<ForgeFieldValueDiagnostic> diagnostics) =>
        new(false, false, Array.AsReadOnly(diagnostics.ToArray()));

    internal static ForgeFieldEditResult AcceptedWithoutChange() =>
        new(true, false, Array.Empty<ForgeFieldValueDiagnostic>());

    internal static ForgeFieldEditResult AcceptedWithChange() =>
        new(true, true, Array.Empty<ForgeFieldValueDiagnostic>());
}
