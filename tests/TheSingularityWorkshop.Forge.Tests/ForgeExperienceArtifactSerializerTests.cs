using System.Text;
using System.Text.Json;
using TheSingularityWorkshop.Forge;
using TheSingularityWorkshop.MicroBundleDomain;
using Xunit;

namespace TheSingularityWorkshop.Forge.Tests;

public sealed class ForgeExperienceArtifactSerializerTests
{
    [Fact]
    public void RoundTripPreservesPortableAuthoringMetadataAndOpaqueConfiguration()
    {
        var source = new ForgeExperience(7100, "Portable Experience", [2, 9, 4]);
        source.AddMicroBundle(
            7200,
            "1.2.3",
            dependencies: [new MicroBundleDependency(7201)],
            providers: [new MicroBundleProvider("renderer"), new MicroBundleProvider("lighting")],
            configuration: new byte[] { 0, 1, 2, 127, 255 });
        source.AddMicroBundle(7201, "4.5.6", configuration: Encoding.UTF8.GetBytes("opaque"));

        var bytes = ForgeExperienceArtifactSerializer.Serialize(source);
        var restored = ForgeExperienceArtifactSerializer.Deserialize(bytes);

        Assert.Equal(source.Id, restored.Id);
        Assert.Equal(source.Name, restored.Name);
        Assert.Equal(source.Ontology, restored.Ontology);
        Assert.Equal(source.MicroBundles.Count, restored.MicroBundles.Count);

        var first = restored.MicroBundles[0];
        Assert.Equal(7200UL, first.Descriptor.Id);
        Assert.Equal("1.2.3", first.Descriptor.Version);
        Assert.Equal(7201UL, Assert.Single(first.Descriptor.Dependencies).BundleId);
        Assert.Equal(new[] { "renderer", "lighting" }, first.Descriptor.Providers.Select(x => x.Id));
        Assert.Equal(new byte[] { 0, 1, 2, 127, 255 }, first.Configuration.ToArray());

        Assert.Equal(bytes, ForgeExperienceArtifactSerializer.Serialize(restored));
    }

    [Fact]
    public void SerializationIsDeterministicForAnUnchangedExperience()
    {
        var experience = new ForgeExperience(7101, "Deterministic", [9, 8, 7]);
        experience.AddMicroBundle(
            7202,
            "1.0.0",
            dependencies: [new MicroBundleDependency(7203), new MicroBundleDependency(7204)],
            providers: [new MicroBundleProvider("zeta"), new MicroBundleProvider("alpha")],
            configuration: new byte[] { 4, 3, 2, 1 });

        Assert.Equal(
            ForgeExperienceArtifactSerializer.Serialize(experience),
            ForgeExperienceArtifactSerializer.Serialize(experience));
    }

    [Fact]
    public void VersionOneArtifactMatchesGoldenJsonContract()
    {
        var experience = new ForgeExperience(7105, "Golden", [3, 1]);
        experience.AddMicroBundle(
            7205,
            "1.0.0",
            configuration: new byte[] { 1, 2, 3 });

        var json = Encoding.UTF8.GetString(ForgeExperienceArtifactSerializer.Serialize(experience));

        Assert.Equal(
            "{\"format\":\"thesingularityworkshop.experience\",\"schemaVersion\":1,\"experienceId\":7105,\"name\":\"Golden\",\"ontology\":[3,1],\"microBundles\":[{\"id\":7205,\"version\":\"1.0.0\",\"dependencies\":[],\"providers\":[],\"configuration\":\"AQID\"}]}",
            json);
    }

    [Fact]
    public void PortableArtifactCanEnterTheExternalSubmissionBoundary()
    {
        var source = new ForgeExperience(7106, "External submission");
        source.AddMicroBundle(7206, "2.0.0", configuration: new byte[] { 6, 7, 8 });
        var bytes = ForgeExperienceArtifactSerializer.Serialize(source);

        var submission = ForgeExperienceArtifactSerializer.DeserializeSubmission(bytes);
        submission.Validate();

        Assert.Equal(ForgeExperienceArtifactSerializer.FormatId, submission.SourceFormat);
        Assert.Equal("1", submission.SourceVersion);
        Assert.Equal(source.Id, submission.Experience.Id);
        Assert.Equal(source.Name, submission.Experience.Name);
        Assert.Equal(new byte[] { 6, 7, 8 }, Assert.Single(submission.Experience.MicroBundles).Configuration.ToArray());
    }

    [Fact]
    public void DeserializerRejectsUnsupportedSchemaVersion()
    {
        var experience = new ForgeExperience(7102, "Version check");
        var json = Encoding.UTF8.GetString(ForgeExperienceArtifactSerializer.Serialize(experience))
            .Replace("\"schemaVersion\":1", "\"schemaVersion\":2", StringComparison.Ordinal);

        Assert.Throws<NotSupportedException>(() =>
            ForgeExperienceArtifactSerializer.Deserialize(Encoding.UTF8.GetBytes(json)));
    }

    [Fact]
    public void DeserializerRejectsAnUnknownArtifactFormat()
    {
        var experience = new ForgeExperience(7104, "Format check");
        var json = Encoding.UTF8.GetString(ForgeExperienceArtifactSerializer.Serialize(experience))
            .Replace(ForgeExperienceArtifactSerializer.FormatId, "another.format", StringComparison.Ordinal);

        Assert.Throws<NotSupportedException>(() =>
            ForgeExperienceArtifactSerializer.Deserialize(Encoding.UTF8.GetBytes(json)));
    }

    [Fact]
    public void DeserializerRejectsUnknownPropertiesInsteadOfSilentlyIgnoringThem()
    {
        var experience = new ForgeExperience(7103, "Strict schema");
        var json = Encoding.UTF8.GetString(ForgeExperienceArtifactSerializer.Serialize(experience));
        var closingBrace = json.LastIndexOf('}');
        json = json.Insert(closingBrace, ",\"unexpected\":true");

        Assert.Throws<JsonException>(() =>
            ForgeExperienceArtifactSerializer.Deserialize(Encoding.UTF8.GetBytes(json)));
    }

    [Fact]
    public void DeserializerRejectsEmptyArtifactBytes()
    {
        Assert.Throws<ArgumentException>(() =>
            ForgeExperienceArtifactSerializer.Deserialize(ReadOnlyMemory<byte>.Empty));
    }
}
