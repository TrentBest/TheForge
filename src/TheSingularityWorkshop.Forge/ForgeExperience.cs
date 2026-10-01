using TheSingularityWorkshop.FSM_COS;
using TheSingularityWorkshop.MicroBundleDomain;

namespace TheSingularityWorkshop.Forge;

/// <summary>
/// Editor-time description of an Experience that the Forge can compile into a
/// machine-oriented FSM_COS runtime manifest.
/// </summary>
public sealed class ForgeExperience
{
    private readonly List<ForgeMicroBundle> _bundles = [];

    public ForgeExperience(ulong id, string name, IEnumerable<int>? ontology = null)
    {
        if (id == 0) throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("An Experience name is required.", nameof(name));

        Id = id;
        Name = name;
        Ontology = (ontology ?? []).ToArray();
    }

    public ulong Id { get; }
    public string Name { get; }
    public IReadOnlyList<int> Ontology { get; }

    public IReadOnlyList<ForgeMicroBundle> MicroBundles => _bundles;

    public ForgeMicroBundle AddMicroBundle(
        ulong bundleId,
        string version,
        IEnumerable<MicroBundleDependency>? dependencies = null,
        IEnumerable<MicroBundleProvider>? providers = null,
        ReadOnlyMemory<byte> configuration = default)
    {
        var descriptor = new MicroBundleDescriptor(bundleId, version, dependencies, providers);
        var bundle = new ForgeMicroBundle(descriptor, configuration);

        if (_bundles.Any(x => x.Descriptor.Id == bundleId))
            throw new InvalidOperationException($"MicroBundle {bundleId} is already part of Experience {Id}.");

        _bundles.Add(bundle);
        return bundle;
    }

    /// <summary>
    /// Compiles this editor-time Experience into the current FSM_COS machine contract.
    /// </summary>
    public RuntimeManifest Compile()
    {
        var requests = _bundles
            .Select(x => new BundleRequest(x.Descriptor.Id, x.Configuration))
            .ToArray();

        return new RuntimeManifest(Id, requests);
    }
}

/// <summary>
/// Editor-owned MicroBundle description plus opaque runtime configuration.
/// </summary>
public sealed record ForgeMicroBundle(
    MicroBundleDescriptor Descriptor,
    ReadOnlyMemory<byte> Configuration);
