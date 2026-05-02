using Workshop.Systems.MicroPackages;

// --- THE FIX: Wrap the Editor namespace ---
#if UNITY_EDITOR
using static UnityEditor.Rendering.FilterWindow;
#endif

using IProvider = Workshop.Systems.MicroPackages.IProvider;

namespace TheSingularityWorkshop.MicroPackages.Providers
{
    public class FsmTransitionProvider : IProvider
    {
        public FsmTransitionProvider(string v1, string v2, string v3)
        {
        }

        public int Id { get; }

        public ProviderType ProviderType { get; }
    }
}