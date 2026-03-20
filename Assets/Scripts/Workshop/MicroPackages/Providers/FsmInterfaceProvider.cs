using TheSingularityWorkshop.MicroPackages;
using System;
using System.Collections.Generic;
using System.Text;

namespace TheSingularityWorkshop.MicroPackages.Providers
{
    public class FsmInterfaceProvider : IProvider
    {
        public int Id { get; }

        public ProviderType ProviderType { get; }
    }
}
