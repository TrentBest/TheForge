using System;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.MicroPackages
{
    public interface IStateTransitionConditionProvider : IProvider
    {
       Func<IStateContext, bool> Provided { get; }
    }
}
