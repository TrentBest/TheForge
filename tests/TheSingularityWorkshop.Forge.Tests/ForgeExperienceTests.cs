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
    public void DraftRevisionAdvancesOnlyForActualSemanticEdits()
    {
        var source = new ForgeExperience(3020, "Original", [1, 2, 3]);
        source.AddMicroBundle(4020, "1.0.0", configuration: new byte[] { 1, 2, 3 });
        var draft = new ForgeExperienceDraft(source);

        Assert.Equal(0, draft.Revision);
        Assert.False(draft.IsModified);
        Assert.False(draft.SetName("Original"));
        Assert.Equal(0, draft.Revision);

        Assert.True(draft.SetName("Edited"));
        Assert.Equal(1, draft.Revision);
        Assert.True(draft.IsModified);

        Assert.True(draft.SetName("Original"));
        Assert.Equal(2, draft.Revision);
        Assert.False(draft.IsModified);
    }

    [Fact]
    public void DraftConfigurationEditsAreCopiedAndReversible()
    {
        var source = new ForgeExperience(3021, "Configuration draft");
        source.AddMicroBundle(4021, "1.0.0", configuration: new byte[] { 1, 2, 3 });
        var draft = new ForgeExperienceDraft(source);

        var editedConfiguration = new byte[] { 4, 5, 6 };
        Assert.True(draft.SetConfiguration(4021, editedConfiguration));
        editedConfiguration[0] = 99;

        Assert.Equal(1, draft.Revision);
        Assert.True(draft.IsModified);
        Assert.Equal(new byte[] { 4, 5, 6 }, Assert.Single(draft.ToExperience().Compile().Bundles).Configuration.ToArray());

        Assert.True(draft.SetConfiguration(4021, new byte[] { 1, 2, 3 }));
        Assert.Equal(2, draft.Revision);
        Assert.False(draft.IsModified);
    }

    [Fact]
    public void DraftCompositionCanAddAndRemoveWithoutMutatingSource()
    {
        var source = new ForgeExperience(3022, "Composition draft");
        source.AddMicroBundle(4022, "1.0.0");
        var draft = new ForgeExperienceDraft(source);

        Assert.True(draft.RemoveMicroBundle(4022));
        Assert.True(draft.IsModified);
        Assert.Equal(1, draft.Revision);
        Assert.False(draft.RemoveMicroBundle(4022));

        draft.AddMicroBundle(4022, "1.0.0");
        Assert.Equal(2, draft.Revision);
        Assert.False(draft.IsModified);
        Assert.Single(source.MicroBundles);
        Assert.Single(draft.MicroBundles);
    }

    [Fact]
    public void DraftRejectsConfigurationForMissingBundle()
    {
        var draft = new ForgeExperienceDraft(new ForgeExperience(3023, "Missing bundle"));

        Assert.Throws<KeyNotFoundException>(() => draft.SetConfiguration(9999, new byte[] { 1 }));
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
