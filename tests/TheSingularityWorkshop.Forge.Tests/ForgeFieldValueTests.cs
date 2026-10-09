using TheSingularityWorkshop.Forge;
using TheSingularityWorkshop.MicroBundleDomain;
using Xunit;

namespace TheSingularityWorkshop.Forge.Tests;

public sealed class ForgeFieldValueTests
{
    [Fact]
    public void ValuesAreTypedAndObjectChildrenAreDetachedAndReadOnly()
    {
        var input = new[]
        {
            new KeyValuePair<string, ForgeFieldValue>("Label", ForgeFieldValue.FromString("before"))
        };

        var value = ForgeFieldValue.FromObject(input);
        input[0] = new("Label", ForgeFieldValue.FromString("after"));

        Assert.Equal("before", value.Children["Label"].StringValue);
        Assert.Throws<NotSupportedException>(() =>
            ((IDictionary<string, ForgeFieldValue>)value.Children).Add("Other", ForgeFieldValue.FromBoolean(true)));
    }

    [Fact]
    public void ObjectRejectsDuplicateOrBlankChildNames()
    {
        Assert.Throws<ArgumentException>(() => ForgeFieldValue.FromObject(
            ("Label", ForgeFieldValue.FromString("one")),
            ("Label", ForgeFieldValue.FromString("two"))));

        Assert.Throws<ArgumentException>(() => ForgeFieldValue.FromObject(
            (" ", ForgeFieldValue.FromString("blank"))));
    }

    [Fact]
    public void ValidatorAcceptsMatchingTypesAndInclusiveBounds()
    {
        var schema = new MicroBundleField("Count", MicroBundleFieldKind.Integer, 2, 1, 3);

        var diagnostics = ForgeFieldValueValidator.Validate(schema, ForgeFieldValue.FromInteger(3));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ValidatorRejectsWrongTypeAndOutOfRangeValues()
    {
        var schema = new MicroBundleField("Count", MicroBundleFieldKind.Integer, 2, 1, 3);

        var wrongType = Assert.Single(
            ForgeFieldValueValidator.Validate(schema, ForgeFieldValue.FromString("2")));
        Assert.Equal("value-kind-mismatch", wrongType.Code);
        Assert.Equal("Count", wrongType.Path);

        var outOfRange = Assert.Single(
            ForgeFieldValueValidator.Validate(schema, ForgeFieldValue.FromInteger(4)));
        Assert.Equal("above-maximum", outOfRange.Code);
    }

    [Fact]
    public void ValidatorUsesInvariantNumericSemanticsAndRejectsNonFiniteFloats()
    {
        var schema = new MicroBundleField("Weight", MicroBundleFieldKind.Float, 1.25, 0, 2);
        var previousCulture = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");
            Assert.Empty(ForgeFieldValueValidator.Validate(schema, ForgeFieldValue.FromFloat(1.25)));
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = previousCulture;
        }

        var diagnostic = Assert.Single(
            ForgeFieldValueValidator.Validate(schema, ForgeFieldValue.FromFloat(double.PositiveInfinity)));
        Assert.Equal("non-finite-number", diagnostic.Code);
    }

    [Fact]
    public void ValidatorReportsNestedPathsAndUnknownFields()
    {
        var schema = new MicroBundleField(
            "Physical",
            MicroBundleFieldKind.Object,
            children:
            [
                new MicroBundleField("Density", MicroBundleFieldKind.Float, 1.25, 0, 1000),
                new MicroBundleField("Label", MicroBundleFieldKind.String, "Hydrogen")
            ]);

        var value = ForgeFieldValue.FromObject(
            ("Density", ForgeFieldValue.FromFloat(1001)),
            ("Unexpected", ForgeFieldValue.FromBoolean(true)));

        var diagnostics = ForgeFieldValueValidator.Validate(schema, value);

        Assert.Collection(
            diagnostics,
            d =>
            {
                Assert.Equal("above-maximum", d.Code);
                Assert.Equal("Physical.Density", d.Path);
            },
            d =>
            {
                Assert.Equal("unknown-field", d.Code);
                Assert.Equal("Physical.Unexpected", d.Path);
            });
    }

    [Fact]
    public void ValidatorReportsDuplicateSchemaNamesInsteadOfThrowing()
    {
        var schema = new MicroBundleField(
            "Settings",
            MicroBundleFieldKind.Object,
            children:
            [
                new MicroBundleField("Label", MicroBundleFieldKind.String),
                new MicroBundleField("Label", MicroBundleFieldKind.String)
            ]);

        var diagnostic = Assert.Single(ForgeFieldValueValidator.Validate(
            schema,
            ForgeFieldValue.FromObject(("Label", ForgeFieldValue.FromString("value")))));

        Assert.Equal("duplicate-schema-field", diagnostic.Code);
        Assert.Equal("Settings.Label", diagnostic.Path);
    }

    [Fact]
    public void NumericDiagnosticTextIsInvariantAcrossCultures()
    {
        var schema = new MicroBundleField("Weight", MicroBundleFieldKind.Float, 1.0, 0, 2);
        var previousCulture = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");
            var diagnostic = Assert.Single(
                ForgeFieldValueValidator.Validate(schema, ForgeFieldValue.FromFloat(2.5)));

            Assert.Equal("Value 2.5 exceeds the maximum 2.", diagnostic.Message);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void ValidatorAllowsOmittedFieldsWithoutApplyingDisplayDefaults()
    {
        var schema = new MicroBundleField(
            "Settings",
            MicroBundleFieldKind.Object,
            children:
            [
                new MicroBundleField("Label", MicroBundleFieldKind.String, "default"),
                new MicroBundleField("Enabled", MicroBundleFieldKind.Boolean, true)
            ]);

        var diagnostics = ForgeFieldValueValidator.Validate(schema, ForgeFieldValue.FromObject());

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ValidatorRejectsNullAndUnsupportedSchemaValuesWithoutEncodingPayload()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ForgeFieldValueValidator.Validate(null!, ForgeFieldValue.FromString("x")));
        Assert.Throws<ArgumentNullException>(() =>
            ForgeFieldValueValidator.Validate(new MicroBundleField("Label", MicroBundleFieldKind.String), null!));
    }
}
