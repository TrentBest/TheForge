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
    public void DraftAcceptsTypedFieldEditAndAdvancesRevisionOnlyWhenChanged()
    {
        var source = new ForgeExperience(3030, "Typed draft");
        source.AddMicroBundle(4030, "1.0.0");
        var draft = new ForgeExperienceDraft(source);
        var schema = new TheSingularityWorkshop.MicroBundleDomain.MicroBundleDefinition(
            "Example",
            new TheSingularityWorkshop.MicroBundleDomain.MicroBundleDescriptor(4030, "1.0.0"),
            [new TheSingularityWorkshop.MicroBundleDomain.MicroBundleField("Count",
                TheSingularityWorkshop.MicroBundleDomain.MicroBundleFieldKind.Integer, 1, 0, 10)]);

        var accepted = draft.TrySetFieldValue(4030, schema, "Count", ForgeFieldValue.FromInteger(5));
        Assert.True(accepted.IsAccepted);
        Assert.True(accepted.HasChanged);
        Assert.Empty(accepted.Diagnostics);
        Assert.Equal(1, draft.Revision);
        Assert.True(draft.IsModified);
        Assert.Equal(5, draft.GetFieldValues(4030)["Count"].IntegerValue);

        var noOp = draft.TrySetFieldValue(4030, schema, "Count", ForgeFieldValue.FromInteger(5));
        Assert.True(noOp.IsAccepted);
        Assert.False(noOp.HasChanged);
        Assert.Equal(1, draft.Revision);
    }

    [Fact]
    public void DraftRejectsInvalidTypedEditWithoutChangingRevisionOrValues()
    {
        var source = new ForgeExperience(3031, "Rejected typed draft");
        source.AddMicroBundle(4031, "1.0.0");
        var draft = new ForgeExperienceDraft(source);
        var schema = new TheSingularityWorkshop.MicroBundleDomain.MicroBundleDefinition(
            "Example",
            new TheSingularityWorkshop.MicroBundleDomain.MicroBundleDescriptor(4031, "1.0.0"),
            [new TheSingularityWorkshop.MicroBundleDomain.MicroBundleField("Count",
                TheSingularityWorkshop.MicroBundleDomain.MicroBundleFieldKind.Integer, 1, 0, 10)]);

        var result = draft.TrySetFieldValue(4031, schema, "Count", ForgeFieldValue.FromInteger(11));

        Assert.False(result.IsAccepted);
        Assert.False(result.HasChanged);
        Assert.Equal("above-maximum", Assert.Single(result.Diagnostics).Code);
        Assert.Equal(0, draft.Revision);
        Assert.False(draft.IsModified);
        Assert.Empty(draft.GetFieldValues(4031));
    }

    [Fact]
    public void DraftRequiresMatchingSchemaIdentityAndRefusesToCompileTypedEditsWithoutCodec()
    {
        var source = new ForgeExperience(3032, "Codec boundary");
        source.AddMicroBundle(4032, "1.0.0");
        var draft = new ForgeExperienceDraft(source);
        var wrongSchema = new TheSingularityWorkshop.MicroBundleDomain.MicroBundleDefinition(
            "Example",
            new TheSingularityWorkshop.MicroBundleDomain.MicroBundleDescriptor(4032, "2.0.0"),
            [new TheSingularityWorkshop.MicroBundleDomain.MicroBundleField("Label",
                TheSingularityWorkshop.MicroBundleDomain.MicroBundleFieldKind.String, "default")]);

        var rejected = draft.TrySetFieldValue(4032, wrongSchema, "Label", ForgeFieldValue.FromString("edited"));
        Assert.Equal("schema-identity-mismatch", Assert.Single(rejected.Diagnostics).Code);
        Assert.Equal(0, draft.Revision);

        var schema = new TheSingularityWorkshop.MicroBundleDomain.MicroBundleDefinition(
            "Example",
            new TheSingularityWorkshop.MicroBundleDomain.MicroBundleDescriptor(4032, "1.0.0"),
            [new TheSingularityWorkshop.MicroBundleDomain.MicroBundleField("Label",
                TheSingularityWorkshop.MicroBundleDomain.MicroBundleFieldKind.String, "default")]);
        Assert.True(draft.TrySetFieldValue(4032, schema, "Label", ForgeFieldValue.FromString("edited")).IsAccepted);
        Assert.Throws<InvalidOperationException>(() => draft.ToExperience());
    }

    [Fact]
    public void ClearingTypedEditRestoresBaselineButDoesNotRewindRevision()
    {
        var source = new ForgeExperience(3033, "Clear typed edit");
        source.AddMicroBundle(4033, "1.0.0");
        var draft = new ForgeExperienceDraft(source);
        var schema = new TheSingularityWorkshop.MicroBundleDomain.MicroBundleDefinition(
            "Example",
            new TheSingularityWorkshop.MicroBundleDomain.MicroBundleDescriptor(4033, "1.0.0"),
            [new TheSingularityWorkshop.MicroBundleDomain.MicroBundleField("Enabled",
                TheSingularityWorkshop.MicroBundleDomain.MicroBundleFieldKind.Boolean, false)]);

        Assert.True(draft.TrySetFieldValue(4033, schema, "Enabled", ForgeFieldValue.FromBoolean(true)).IsAccepted);
        Assert.True(draft.ClearFieldValue(4033, "Enabled"));
        Assert.Equal(2, draft.Revision);
        Assert.False(draft.IsModified);
        Assert.Empty(draft.GetFieldValues(4033));
        Assert.Single(draft.ToExperience().MicroBundles);
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
