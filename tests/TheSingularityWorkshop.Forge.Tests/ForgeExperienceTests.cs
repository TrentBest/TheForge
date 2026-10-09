using Xunit;
using TheSingularityWorkshop.Forge;

namespace TheSingularityWorkshop.Forge.Tests;

public sealed class ForgeExperienceTests
{
    [Fact]
    public void ExperienceCompilesIntoCurrentFsmCosManifest()
    {
        var experience = new ForgeExperience(3010, "REST Capability Lab", [1, 2, 3, 4, 5, 6, 7, 8, 9]);
        experience.AddMicroBundle(4010, "1.0.0", configuration: new byte[] { 1, 2, 3 });

        var manifest = experience.Compile();

        Assert.Equal(3010UL, manifest.RuntimeId);
        var request = Assert.Single(manifest.Bundles);
        Assert.Equal(4010UL, request.BundleId);
        Assert.Equal(new byte[] { 1, 2, 3 }, request.Configuration.ToArray());
    }

    [Fact]
    public void ExperienceCollectionsCannotBeMutatedThroughDowncasts()
    {
        var ontologyInput = new[] { 1, 2, 3 };
        var experience = new ForgeExperience(3011, "Protected snapshots", ontologyInput);
        ontologyInput[0] = 99;

        Assert.Equal(1, experience.Ontology[0]);
        Assert.Throws<NotSupportedException>(() => ((IList<int>)experience.Ontology)[0] = 42);

        experience.AddMicroBundle(4011, "1.0.0");
        Assert.Throws<NotSupportedException>(() =>
            ((IList<ForgeMicroBundle>)experience.MicroBundles).Clear());
    }

    [Fact]
    public void ExperienceCopiesConfigurationInputBuffer()
    {
        var configuration = new byte[] { 1, 2, 3 };
        var experience = new ForgeExperience(3012, "Configuration snapshot");
        experience.AddMicroBundle(4012, "1.0.0", configuration: configuration);

        configuration[0] = 99;

        var request = Assert.Single(experience.Compile().Bundles);
        Assert.Equal(new byte[] { 1, 2, 3 }, request.Configuration.ToArray());
    }

    [Fact]
    public void ExternalSubmissionDoesNotRequireForgeUi()
    {
        var experience = new ForgeExperience(9001, "Externally Authored Experience");
        var submission = new ForgeSubmission(experience, "json", "1");

        submission.Validate();

        Assert.Same(experience, submission.Experience);
    }
}
