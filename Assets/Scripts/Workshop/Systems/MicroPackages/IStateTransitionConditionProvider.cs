using System;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.Systems.MicroPackages
{
    public interface IStateTransitionConditionProvider : IProvider
    {
       Func<IStateContext, bool> Provided { get; }
    }
}
