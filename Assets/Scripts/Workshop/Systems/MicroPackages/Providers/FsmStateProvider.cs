using Workshop.Systems.MicroPackages;

using IProvider = Workshop.Systems.MicroPackages.IProvider;

namespace TheSingularityWorkshop.MicroPackages.Providers
{
    public class FsmStateProvider : IProvider
    {
        public int Id { get; }

        public ProviderType ProviderType { get; }

        public FsmStateProvider(string name, string description)
        {

        }
    }
}
