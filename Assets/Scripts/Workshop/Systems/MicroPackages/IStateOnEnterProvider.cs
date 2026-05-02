using System;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.Systems.MicroPackages
{
    public interface IStateOnEnterProvider : IProvider
    {
       Action<IStateContext> Provided { get; }
    }
}
