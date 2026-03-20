using TheSingularityWorkshop.MicroPackages;
using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;

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

        
    }
}