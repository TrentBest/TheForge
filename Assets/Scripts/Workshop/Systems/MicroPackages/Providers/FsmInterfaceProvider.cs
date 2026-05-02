using Workshop.Systems.MicroPackages;

namespace TheSingularityWorkshop.MicroPackages.Providers
{
    public class FsmInterfaceProvider : IProvider
    {
        public int Id { get; }

        public ProviderType ProviderType { get; }
    }
}
