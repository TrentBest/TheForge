using System.Globalization;
using System.Text;
using TheSingularityWorkshop.Forge;
using TheSingularityWorkshop.MicroBundleDomain;
using Xunit;

namespace TheSingularityWorkshop.Forge.Tests;

public sealed class ForgeConfigurationCodecTests
{
    [Fact]
    public void RegistryResolvesOnlyExactIdentityAndVersion()
    {
        var registry = new ForgeConfigurationCodecRegistry();
        var v1 = new IntegerCountFixtureCodec(5100, "1.0.0");
        var v2 = new IntegerCountFixtureCodec(5100, "2.0.0");

        registry.Register(v1);
        registry.Register(v2);

        Assert.True(registry.TryGetCodec(5100, "1.0.0", out var foundV1));
        Assert.Same(v1, foundV1);
        Assert.True(registry.TryGetCodec(5100, "2.0.0", out var foundV2));
        Assert.Same(v2, foundV2);
        Assert.False(registry.TryGetCodec(5100, "3.0.0", out _));
        Assert.Throws<InvalidOperationException>(() => registry.Register(new IntegerCountFixtureCodec(5100, "1.0.0")));
    }

    [Fact]
    public void DraftUsesMatchingCodecToEncodeTypedEditsIntoRuntimeConfiguration()
    {
        const ulong bundleId = 5101;
        const string version = "1.0.0";
        var source = new ForgeExperience(6101, "Codec round trip");
        source.AddMicroBundle(bundleId, version, configuration: Encoding.UTF8.GetBytes("original"));
        var draft = new ForgeExperienceDraft(source);
        var schema = new MicroBundleDefinition(
            "Fixture",
            new MicroBundleDescriptor(bundleId, version),
            [new MicroBundleField("Count", MicroBundleFieldKind.Integer, 0, 0, 100)]);
        Assert.True(draft.TrySetFieldValue(bundleId, schema, "Count", ForgeFieldValue.FromInteger(42)).IsAccepted);

        var codec = new IntegerCountFixtureCodec(bundleId, version);
        var registry = new ForgeConfigurationCodecRegistry();
        registry.Register(codec);

        var compiled = draft.ToExperience(registry);
        var emitted = Assert.Single(compiled.MicroBundles);
        Assert.Equal("count=42", Encoding.UTF8.GetString(emitted.Configuration.Span));
        Assert.Equal(42, codec.Decode(emitted.Configuration)["Count"].IntegerValue);
    }

    [Fact]
    public void DraftRefusesMissingOrWrongVersionCodecInsteadOfSilentlyUsingAnotherVersion()
    {
        const ulong bundleId = 5102;
        var source = new ForgeExperience(6102, "No codec fallback");
        source.AddMicroBundle(bundleId, "1.0.0");
        var draft = new ForgeExperienceDraft(source);
        var schema = new MicroBundleDefinition(
            "Fixture",
            new MicroBundleDescriptor(bundleId, "1.0.0"),
            [new MicroBundleField("Count", MicroBundleFieldKind.Integer, 0, 0, 100)]);
        Assert.True(draft.TrySetFieldValue(bundleId, schema, "Count", ForgeFieldValue.FromInteger(7)).IsAccepted);

        Assert.Throws<InvalidOperationException>(() => draft.ToExperience());

        var registry = new ForgeConfigurationCodecRegistry();
        registry.Register(new IntegerCountFixtureCodec(bundleId, "2.0.0"));
        Assert.Throws<InvalidOperationException>(() => draft.ToExperience(registry));
    }

    private sealed class IntegerCountFixtureCodec(ulong id, string version) : IForgeMicroBundleConfigurationCodec
    {
        public ulong MicroBundleId => id;
        public string MicroBundleVersion => version;

        public ReadOnlyMemory<byte> Encode(
            IReadOnlyDictionary<string, ForgeFieldValue> values,
            ReadOnlyMemory<byte> existingPayload)
        {
            if (!values.TryGetValue("Count", out var count) || count.Kind != ForgeFieldValueKind.Integer)
                throw new InvalidOperationException("The fixture codec requires an integer Count value.");

            return Encoding.UTF8.GetBytes("count=" + count.IntegerValue!.Value.ToString(CultureInfo.InvariantCulture));
        }

        public IReadOnlyDictionary<string, ForgeFieldValue> Decode(ReadOnlyMemory<byte> payload)
        {
            var text = Encoding.UTF8.GetString(payload.Span);
            if (!text.StartsWith("count=", StringComparison.Ordinal) ||
                !long.TryParse(text.AsSpan("count=".Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out var count))
            {
                throw new FormatException("Invalid fixture Count payload.");
            }

            return new Dictionary<string, ForgeFieldValue>(StringComparer.Ordinal)
            {
                ["Count"] = ForgeFieldValue.FromInteger(count)
            };
        }
    }
}
