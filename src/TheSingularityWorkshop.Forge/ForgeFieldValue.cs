using System.Collections.ObjectModel;
using System.Globalization;
using TheSingularityWorkshop.MicroBundleDomain;

namespace TheSingularityWorkshop.Forge;

/// <summary>
/// A typed, immutable authoring value. This is an editor-side value model, not a
/// MicroBundle runtime payload and not a serialization format.
/// </summary>
public sealed class ForgeFieldValue
{
    private ForgeFieldValue(
        ForgeFieldValueKind kind,
        string? stringValue = null,
        long? integerValue = null,
        double? floatValue = null,
        bool? booleanValue = null,
        IReadOnlyDictionary<string, ForgeFieldValue>? children = null)
    {
        Kind = kind;
        StringValue = stringValue;
        IntegerValue = integerValue;
        FloatValue = floatValue;
        BooleanValue = booleanValue;
        Children = children ?? EmptyChildren;
    }

    private static readonly IReadOnlyDictionary<string, ForgeFieldValue> EmptyChildren =
        new ReadOnlyDictionary<string, ForgeFieldValue>(
            new Dictionary<string, ForgeFieldValue>(StringComparer.Ordinal));

    public ForgeFieldValueKind Kind { get; }
    public string? StringValue { get; }
    public long? IntegerValue { get; }
    public double? FloatValue { get; }
    public bool? BooleanValue { get; }
    public IReadOnlyDictionary<string, ForgeFieldValue> Children { get; }

    public static ForgeFieldValue FromString(string value) =>
        new(ForgeFieldValueKind.String, stringValue: value ?? throw new ArgumentNullException(nameof(value)));

    public static ForgeFieldValue FromInteger(long value) =>
        new(ForgeFieldValueKind.Integer, integerValue: value);

    public static ForgeFieldValue FromFloat(double value) =>
        new(ForgeFieldValueKind.Float, floatValue: value);

    public static ForgeFieldValue FromBoolean(bool value) =>
        new(ForgeFieldValueKind.Boolean, booleanValue: value);

    public static ForgeFieldValue FromObject(IEnumerable<KeyValuePair<string, ForgeFieldValue>> children)
    {
        ArgumentNullException.ThrowIfNull(children);

        var copy = new Dictionary<string, ForgeFieldValue>(StringComparer.Ordinal);
        foreach (var pair in children)
        {
            if (string.IsNullOrWhiteSpace(pair.Key))
                throw new ArgumentException("Object field names cannot be empty.", nameof(children));
            ArgumentNullException.ThrowIfNull(pair.Value);
            if (!copy.TryAdd(pair.Key, pair.Value))
                throw new ArgumentException($"Duplicate object field '{pair.Key}'.", nameof(children));
        }

        return new ForgeFieldValue(
            ForgeFieldValueKind.Object,
            children: new ReadOnlyDictionary<string, ForgeFieldValue>(copy));
    }

    public static ForgeFieldValue FromObject(params (string Name, ForgeFieldValue Value)[] children)
    {
        ArgumentNullException.ThrowIfNull(children);
        return FromObject(children.Select(x => new KeyValuePair<string, ForgeFieldValue>(x.Name, x.Value)));
    }
}

/// <summary>Supported value shapes in the Forge's schema-backed editor model.</summary>
public enum ForgeFieldValueKind
{
    String,
    Integer,
    Float,
    Boolean,
    Object
}

/// <summary>
/// Validates typed authoring values against MicroBundleDomain field descriptions.
/// Missing fields are allowed, defaults are not applied, and unknown fields are
/// rejected. This validator does not encode values into runtime configuration bytes.
/// </summary>
public static class ForgeFieldValueValidator
{
    public static IReadOnlyList<ForgeFieldValueDiagnostic> Validate(
        MicroBundleField field,
        ForgeFieldValue value,
        string? path = null)
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(value);

        var diagnostics = new List<ForgeFieldValueDiagnostic>();
        ValidateCore(field, value, string.IsNullOrWhiteSpace(path) ? field.Name : path, diagnostics);
        return Array.AsReadOnly(diagnostics.ToArray());
    }

    private static void ValidateCore(
        MicroBundleField field,
        ForgeFieldValue value,
        string path,
        List<ForgeFieldValueDiagnostic> diagnostics)
    {
        if (!TryMapKind(field.Kind, out var expected))
        {
            diagnostics.Add(new("unsupported-schema-kind", path,
                $"Schema kind '{field.Kind}' is not supported by the Forge typed editor."));
            return;
        }

        if (value.Kind != expected)
        {
            diagnostics.Add(new("value-kind-mismatch", path,
                $"Expected {expected}, but received {value.Kind}."));
            return;
        }

        switch (value.Kind)
        {
            case ForgeFieldValueKind.Integer:
                ValidateNumber(value.IntegerValue!.Value, field, path, diagnostics);
                break;
            case ForgeFieldValueKind.Float:
                if (!double.IsFinite(value.FloatValue!.Value))
                {
                    diagnostics.Add(new("non-finite-number", path, "Float values must be finite."));
                    break;
                }
                ValidateNumber(value.FloatValue.Value, field, path, diagnostics);
                break;
            case ForgeFieldValueKind.Object:
                var duplicateNames = field.Children
                    .GroupBy(x => x.Name, StringComparer.Ordinal)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key)
                    .ToHashSet(StringComparer.Ordinal);
                foreach (var duplicateName in duplicateNames.OrderBy(x => x, StringComparer.Ordinal))
                {
                    diagnostics.Add(new("duplicate-schema-field", Join(path, duplicateName),
                        $"Schema field name '{duplicateName}' is declared more than once and cannot be addressed unambiguously."));
                }

                var schemaChildren = field.Children
                    .Where(x => !duplicateNames.Contains(x.Name))
                    .ToDictionary(x => x.Name, StringComparer.Ordinal);
                foreach (var supplied in value.Children)
                {
                    if (duplicateNames.Contains(supplied.Key))
                        continue;

                    if (!schemaChildren.TryGetValue(supplied.Key, out var childSchema))
                    {
                        diagnostics.Add(new("unknown-field", Join(path, supplied.Key),
                            $"Field '{supplied.Key}' is not declared by the schema."));
                        continue;
                    }

                    ValidateCore(childSchema, supplied.Value, Join(path, supplied.Key), diagnostics);
                }
                break;
        }
    }

    private static void ValidateNumber(
        double value,
        MicroBundleField field,
        string path,
        List<ForgeFieldValueDiagnostic> diagnostics)
    {
        if (field.Minimum is double minimum && value < minimum)
            diagnostics.Add(new("below-minimum", path, $"Value {value.ToString(CultureInfo.InvariantCulture)} is below the minimum {minimum.ToString(CultureInfo.InvariantCulture)}."));
        if (field.Maximum is double maximum && value > maximum)
            diagnostics.Add(new("above-maximum", path, $"Value {value.ToString(CultureInfo.InvariantCulture)} exceeds the maximum {maximum.ToString(CultureInfo.InvariantCulture)}."));
    }

    private static bool TryMapKind(MicroBundleFieldKind kind, out ForgeFieldValueKind valueKind)
    {
        switch (kind)
        {
            case MicroBundleFieldKind.String:
                valueKind = ForgeFieldValueKind.String;
                return true;
            case MicroBundleFieldKind.Integer:
                valueKind = ForgeFieldValueKind.Integer;
                return true;
            case MicroBundleFieldKind.Float:
                valueKind = ForgeFieldValueKind.Float;
                return true;
            case MicroBundleFieldKind.Boolean:
                valueKind = ForgeFieldValueKind.Boolean;
                return true;
            case MicroBundleFieldKind.Object:
                valueKind = ForgeFieldValueKind.Object;
                return true;
            default:
                valueKind = default;
                return false;
        }
    }

    private static string Join(string parent, string child) => $"{parent}.{child}";
}

/// <summary>A machine-readable validation finding for a typed authoring value.</summary>
public sealed record ForgeFieldValueDiagnostic(string Code, string Path, string Message);
