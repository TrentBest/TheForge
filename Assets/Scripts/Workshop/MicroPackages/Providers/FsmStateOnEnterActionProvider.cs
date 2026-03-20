
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using System;
using TheSingularityWorkshop.MicroPackages;

namespace TheSingularityWorkshop.MicroPackages.Providers
{
    // Implementation for OnEnter, OnUpdate, and OnExit
    public class FsmStateOnEnterActionProvider : IStateOnEnterProvider
    {
        public int Id { get; }
        public ProviderType ProviderType { get; }
        public Action<IStateContext> Provided { get; }

        public FsmStateOnEnterActionProvider(Action<IStateContext> context)
        {
            Provided = context;
        }
    }
}