using System.Text.Json;
using System.Text.Json.Serialization;
using TheSingularityWorkshop.MicroBundleDomain;

namespace TheSingularityWorkshop.Forge;

/// <summary>
/// Reads and writes the portable, UI-independent representation of a Forge Experience.
/// This format preserves authoring metadata that the current FSM_COS RuntimeManifest
/// does not carry. It is not itself a runtime manifest or a MicroBundle payload format.
/// </summary>
public static class ForgeExperienceArtifactSerializer
{
    /// <summary>
    /// Current version of the portable Forge Experience artifact envelope.
    /// Unsupported versions are rejected rather than silently reinterpreted.
    /// </summary>
    public const int CurrentSchemaVersion = 1;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an Experience into deterministic UTF-8 JSON for this schema version.
    /// Array order is preserved; opaque configuration bytes are represented as Base64.
    /// </summary>
    public static byte[] Serialize(ForgeExperience experience)
    {
        ArgumentNullException.ThrowIfNull(experience);

        var document = new ArtifactDocument
        {
            SchemaVersion = CurrentSchemaVersion,
            ExperienceId = experience.Id,
            Name = experience.Name,
            Ontology = experience.Ontology.ToArray(),
            MicroBundles = experience.MicroBundles.Select(bundle => new ArtifactMicroBundle
            {
                Id = bundle.Descriptor.Id,
                Version = bundle.Descriptor.Version,
                Dependencies = bundle.Descriptor.Dependencies.Select(x => x.BundleId).ToArray(),
                Providers = bundle.Descriptor.Providers.Select(x => x.Id).ToArray(),
                Configuration = bundle.Configuration.ToArray()
            }).ToArray()
        };

        return JsonSerializer.SerializeToUtf8Bytes(document, SerializerOptions);
    }

    /// <summary>
    /// Deserializes a portable Experience artifact. Only the exact supported schema
    /// version is accepted; unknown properties and incomplete required data are rejected.
    /// </summary>
    public static ForgeExperience Deserialize(ReadOnlyMemory<byte> utf8Json)
    {
        if (utf8Json.IsEmpty)
            throw new ArgumentException("Artifact content cannot be empty.", nameof(utf8Json));

        var document = JsonSerializer.Deserialize<ArtifactDocument>(utf8Json.Span, SerializerOptions)
            ?? throw new JsonException("The Experience artifact document cannot be null.");

        if (document.SchemaVersion != CurrentSchemaVersion)
            throw new NotSupportedException(
                $"Experience artifact schema version {document.SchemaVersion} is not supported. Expected {CurrentSchemaVersion}.");

        if (document.Name is null)
            throw new JsonException("The Experience artifact must contain a name.");
        if (document.Ontology is null)
            throw new JsonException("The Experience artifact must contain an ontology array.");
        if (document.MicroBundles is null)
            throw new JsonException("The Experience artifact must contain a MicroBundles array.");

        var experience = new ForgeExperience(document.ExperienceId, document.Name, document.Ontology);
        foreach (var bundle in document.MicroBundles)
        {
            if (bundle is null)
                throw new JsonException("A MicroBundle artifact entry cannot be null.");
            if (bundle.Version is null)
                throw new JsonException($"MicroBundle {bundle.Id} must contain a version.");
            if (bundle.Dependencies is null)
                throw new JsonException($"MicroBundle {bundle.Id} must contain a dependencies array.");
            if (bundle.Providers is null)
                throw new JsonException($"MicroBundle {bundle.Id} must contain a providers array.");
            if (bundle.Configuration is null)
                throw new JsonException($"MicroBundle {bundle.Id} must contain a configuration byte array.");

            experience.AddMicroBundle(
                bundle.Id,
                bundle.Version,
                bundle.Dependencies.Select(id => new MicroBundleDependency(id)),
                bundle.Providers.Select(id => new MicroBundleProvider(id)),
                bundle.Configuration);
        }

        return experience;
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    private sealed class ArtifactDocument
    {
        public ArtifactDocument() { }

        public required int SchemaVersion { get; init; }
        public required ulong ExperienceId { get; init; }
        public required string? Name { get; init; }
        public required int[]? Ontology { get; init; }
        public required ArtifactMicroBundle[]? MicroBundles { get; init; }
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    private sealed class ArtifactMicroBundle
    {
        public ArtifactMicroBundle() { }

        public required ulong Id { get; init; }
        public required string? Version { get; init; }
        public required ulong[]? Dependencies { get; init; }
        public required string[]? Providers { get; init; }
        public required byte[]? Configuration { get; init; }
    }
}
