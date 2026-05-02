using System;
using TheSingularityWorkshop.FSM_API;
using Workshop.Systems.MicroPackages;

namespace TheSingularityWorkshop.MicroPackages.Providers
{
    public class FsmStateConditionProvider : IStateTransitionConditionProvider
    {
        public int Id { get; }
        public ProviderType ProviderType => ProviderType.TransitionCondition;
        public Func<IStateContext, bool> Provided { get; } 

        public FsmStateConditionProvider(Func<IStateContext, bool> item)
        {
          
            // Matches the TransitionMethod struct required by IProvider.cs
            Provided = item;
        }

        public FsmStateConditionProvider(string v1, string v2, string v3, Func<object, bool> value)
        {
        }
    }
}