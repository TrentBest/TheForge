using TheSingularityWorkshop.FSM_API;
using System;
using Workshop.Systems.MicroPackages;

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

        public FsmStateOnEnterActionProvider(string v1, string v2, Action<object> value)
        {
        }
    }
}