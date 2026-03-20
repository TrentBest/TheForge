using System.Collections.Generic;

namespace TheSingularityWorkshop.MicroPackages
{
    public interface IStateOnUpdateProvider : IProvider
    {
        List<StateMethod> Provided { get; }
    }
}
