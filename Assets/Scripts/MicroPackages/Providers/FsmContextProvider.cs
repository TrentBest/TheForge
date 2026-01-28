using System;
using System.Collections.Generic;
using System.Text;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.MicroPackages.Providers
{
    public class FsmContextProvider : IProvider
    {
        public FsmContextProvider(string contextName, IStateContext context)
        {
        }

        public int Id => throw new NotImplementedException();

        public ProviderType ProviderType => throw new NotImplementedException();
    }
}
