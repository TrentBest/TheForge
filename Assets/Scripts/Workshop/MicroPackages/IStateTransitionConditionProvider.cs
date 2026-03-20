using System;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.MicroPackages
{
    public interface IStateTransitionConditionProvider : IProvider
    {
       Func<IStateContext, bool> Provided { get; }
    }
}
