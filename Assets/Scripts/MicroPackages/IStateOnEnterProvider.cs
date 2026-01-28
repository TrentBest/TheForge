using System;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.MicroPackages
{
    public interface IStateOnEnterProvider : IProvider
    {
       Action<IStateContext> Provided { get; }
    }
}
