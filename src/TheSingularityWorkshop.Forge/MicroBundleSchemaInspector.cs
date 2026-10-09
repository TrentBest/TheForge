using System.Collections.ObjectModel;
using System.Globalization;
using TheSingularityWorkshop.MicroBundleDomain;

namespace TheSingularityWorkshop.Forge;

/// <summary>
/// Projects a MicroBundleDomain definition into a Forge-owned, read-only inspection model.
/// This adapter never loads or executes the described MicroBundle.
/// </summary>
public static class MicroBundleSchemaInspector
{
    /// <summary>
    /// Creates a read-only inspection snapshot of a MicroBundle definition.
    /// </summary>
    /// <param name="definition">The editor-time domain definition to inspect.</param>
    /// <returns>A detached snapshot suitable for a generic inspector.</returns>
    public static MicroBundleInspection Inspect(MicroBundleDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return new MicroBundleInspection(
            definition.Name,
            definition.Id,
            definition.Version,
            Array.AsReadOnly(definition.Descriptor.Dependencies.Select(x => x.BundleId).ToArray()),
            Array.AsReadOnly(definition.Descriptor.Providers.Select(x => x.Id).ToArray()),
            Array.AsReadOnly(definition.Fields.Select(InspectField).ToArray()));
    }

    private static MicroBundleFieldInspection InspectField(MicroBundleField field) =>
        new(
            field.Name,
            field.Kind,
            FormatDefault(field.DefaultValue),
            field.Minimum,
            field.Maximum,
            Array.AsReadOnly(field.Children.Select(InspectField).ToArray()));

    private static string? FormatDefault(object? value) =>
        value switch
        {
            null => null,
            string text => text,
            bool boolean => boolean ? "true" : "false",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
}

/// <summary>
/// Detached, read-only summary of a MicroBundle definition for authoring tools.
/// Default values are display text, not editable or serialized configuration values.
/// </summary>
public sealed record MicroBundleInspection
{
    internal MicroBundleInspection(
        string name,
        ulong id,
        string version,
        IReadOnlyList<ulong> dependencyIds,
        IReadOnlyList<string> providerIds,
        IReadOnlyList<MicroBundleFieldInspection> fields)
    {
        Name = name;
        Id = id;
        Version = version;
        DependencyIds = dependencyIds;
        ProviderIds = providerIds;
        Fields = fields;
    }

    public string Name { get; }
    public ulong Id { get; }
    public string Version { get; }
    public IReadOnlyList<ulong> DependencyIds { get; }
    public IReadOnlyList<string> ProviderIds { get; }
    public IReadOnlyList<MicroBundleFieldInspection> Fields { get; }
}

/// <summary>
/// Detached, read-only description of one schema field, including nested fields.
/// </summary>
public sealed record MicroBundleFieldInspection
{
    internal MicroBundleFieldInspection(
        string name,
        MicroBundleFieldKind kind,
        string? defaultValueDisplay,
        double? minimum,
        double? maximum,
        IReadOnlyList<MicroBundleFieldInspection> children)
    {
        Name = name;
        Kind = kind;
        DefaultValueDisplay = defaultValueDisplay;
        Minimum = minimum;
        Maximum = maximum;
        Children = children;
    }

    public string Name { get; }
    public MicroBundleFieldKind Kind { get; }
    public string? DefaultValueDisplay { get; }
    public double? Minimum { get; }
    public double? Maximum { get; }
    public IReadOnlyList<MicroBundleFieldInspection> Children { get; }
}
