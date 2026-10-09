using System.Globalization;
using TheSingularityWorkshop.Forge;
using TheSingularityWorkshop.MicroBundleDomain;
using Xunit;

namespace TheSingularityWorkshop.Forge.Tests;

public sealed class MicroBundleSchemaInspectorTests
{
    [Fact]
    public void Inspect_ProjectsDefinitionAndNestedFieldsWithoutRuntimeExecution()
    {
        var definition = new MicroBundleDefinition(
            "Matter",
            new MicroBundleDescriptor(
                42,
                "1.2.0",
                dependencies: [new MicroBundleDependency(7)],
                providers: [new MicroBundleProvider("matter.schema")]),
            [
                new MicroBundleField(
                    "Physical",
                    MicroBundleFieldKind.Object,
                    children:
                    [
                        new MicroBundleField("Density", MicroBundleFieldKind.Float, 1.25, 0, 1000),
                        new MicroBundleField("Label", MicroBundleFieldKind.String, "Hydrogen"),
                        new MicroBundleField("Enabled", MicroBundleFieldKind.Boolean, true)
                    ])
            ]);

        var inspection = MicroBundleSchemaInspector.Inspect(definition);

        Assert.Equal("Matter", inspection.Name);
        Assert.Equal(42UL, inspection.Id);
        Assert.Equal("1.2.0", inspection.Version);
        Assert.Equal(new ulong[] { 7 }, inspection.DependencyIds);
        Assert.Equal(new[] { "matter.schema" }, inspection.ProviderIds);

        var physical = Assert.Single(inspection.Fields);
        Assert.Equal(MicroBundleFieldKind.Object, physical.Kind);
        Assert.Equal(3, physical.Children.Count);

        var density = physical.Children[0];
        Assert.Equal("Density", density.Name);
        Assert.Equal("1.25", density.DefaultValueDisplay);
        Assert.Equal(0, density.Minimum);
        Assert.Equal(1000, density.Maximum);

        Assert.Equal("Hydrogen", physical.Children[1].DefaultValueDisplay);
        Assert.Equal("true", physical.Children[2].DefaultValueDisplay);
    }

    [Fact]
    public void Inspect_ReturnsDetachedReadOnlyCollections()
    {
        var definition = new MicroBundleDefinition(
            "Simple",
            new MicroBundleDescriptor(43, "1.0.0"),
            [new MicroBundleField("Name", MicroBundleFieldKind.String, "original")]);

        var inspection = MicroBundleSchemaInspector.Inspect(definition);

        Assert.IsAssignableFrom<IReadOnlyList<MicroBundleFieldInspection>>(inspection.Fields);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<MicroBundleFieldInspection>)inspection.Fields).Clear());
        Assert.Throws<NotSupportedException>(() =>
            ((IList<ulong>)inspection.DependencyIds).Add(99));

        Assert.Equal("original", inspection.Fields[0].DefaultValueDisplay);
    }

    [Fact]
    public void Inspect_RejectsNullDefinition()
    {
        Assert.Throws<ArgumentNullException>(() => MicroBundleSchemaInspector.Inspect(null!));
    }
}
