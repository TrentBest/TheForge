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
    private readonly DraftBaseline _baseline;
    private string _name;

    public ForgeExperienceDraft(ForgeExperience source)
    {
        ArgumentNullException.ThrowIfNull(source);

        _id = source.Id;
        _name = source.Name;
        _ontology = source.Ontology.ToList();
        _bundles.AddRange(source.MicroBundles.Select(CloneBundle));
        _baseline = DraftBaseline.Capture(_name, _ontology, _bundles);
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
    public bool IsModified => !_baseline.Matches(_name, _ontology, _bundles);

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
    /// Produces a detached editor-time Experience snapshot suitable for the existing
    /// FSM_COS manifest compiler. It does not execute or mutate a live runtime.
    /// </summary>
    public ForgeExperience ToExperience()
    {
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

    private sealed class DraftBaseline
    {
        private readonly string _name;
        private readonly int[] _ontology;
        private readonly ForgeMicroBundle[] _bundles;

        private DraftBaseline(string name, int[] ontology, ForgeMicroBundle[] bundles)
        {
            _name = name;
            _ontology = ontology;
            _bundles = bundles;
        }

        public static DraftBaseline Capture(
            string name,
            IEnumerable<int> ontology,
            IEnumerable<ForgeMicroBundle> bundles) =>
            new(name, ontology.ToArray(), bundles.Select(CloneBundle).ToArray());

        public bool Matches(
            string name,
            IReadOnlyList<int> ontology,
            IReadOnlyList<ForgeMicroBundle> bundles)
        {
            if (!string.Equals(_name, name, StringComparison.Ordinal) ||
                !_ontology.SequenceEqual(ontology) ||
                _bundles.Length != bundles.Count)
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
    }
}
