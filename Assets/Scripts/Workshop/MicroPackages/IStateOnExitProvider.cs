using System.Collections.Generic;

namespace TheSingularityWorkshop.MicroPackages
{
    public interface IStateOnExitProvider : IProvider
    {
        List<StateMethod> Provided { get; }
    }
}
