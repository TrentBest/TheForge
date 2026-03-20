using System;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.MicroPackages
{
    public interface IStateOnEnterProvider : IProvider
    {
       Action<IStateContext> Provided { get; }
    }
}
