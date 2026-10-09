using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.Forge;

/// <summary>
/// Encodes and decodes authoring values for one exact MicroBundle identity and version.
/// Implementations own the actual payload format and the meaning of combining typed
/// edits with an existing opaque payload. Forge does not prescribe JSON or binary layout.
/// </summary>
public interface IForgeMicroBundleConfigurationCodec
{
    ulong MicroBundleId { get; }

    string MicroBundleVersion { get; }

    /// <summary>
    /// Produces the MicroBundle's complete runtime configuration payload. The codec
    /// decides how to merge authored values with existing payload bytes.
    /// </summary>
    ReadOnlyMemory<byte> Encode(
        IReadOnlyDictionary<string, ForgeFieldValue> values,
        ReadOnlyMemory<byte> existingPayload);

    /// <summary>Decodes a payload into detached typed values using this codec's contract.</summary>
    IReadOnlyDictionary<string, ForgeFieldValue> Decode(ReadOnlyMemory<byte> payload);
}

/// <summary>
/// Resolves a configuration codec by exact MicroBundle ID and version.
/// A resolver must never silently substitute another version.
/// </summary>
public interface IForgeMicroBundleConfigurationCodecResolver
{
    bool TryGetCodec(
        ulong microBundleId,
        string version,
        out IForgeMicroBundleConfigurationCodec? codec);
}

/// <summary>
/// In-memory exact-match codec registry. Registration of a second codec for the
/// same ID/version is rejected so resolution remains deterministic.
/// </summary>
public sealed class ForgeConfigurationCodecRegistry : IForgeMicroBundleConfigurationCodecResolver
{
    private readonly Dictionary<(ulong Id, string Version), IForgeMicroBundleConfigurationCodec> _codecs = [];

    public void Register(IForgeMicroBundleConfigurationCodec codec)
    {
        ArgumentNullException.ThrowIfNull(codec);
        if (codec.MicroBundleId == 0)
            throw new ArgumentOutOfRangeException(nameof(codec), "A codec must target a non-zero MicroBundle ID.");
        if (string.IsNullOrWhiteSpace(codec.MicroBundleVersion))
            throw new ArgumentException("A codec must declare an exact MicroBundle version.", nameof(codec));

        var key = (codec.MicroBundleId, codec.MicroBundleVersion);
        if (!_codecs.TryAdd(key, codec))
            throw new InvalidOperationException(
                $"A configuration codec is already registered for MicroBundle {key.Item1} version '{key.Item2}'.");
    }

    public bool TryGetCodec(
        ulong microBundleId,
        string version,
        out IForgeMicroBundleConfigurationCodec? codec)
    {
        ArgumentNullException.ThrowIfNull(version);
        var found = _codecs.TryGetValue((microBundleId, version), out var registered);
        codec = registered;
        return found;
    }
}
