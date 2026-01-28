using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.MicroPackages.Providers
{
    public class FsmStateOnUpdateActionProvider : IProvider
    {
        public FsmStateOnUpdateActionProvider(StateMethod behavior)
        {
        }

        public int Id => throw new NotImplementedException();

        public ProviderType ProviderType => throw new NotImplementedException();
    }
}
