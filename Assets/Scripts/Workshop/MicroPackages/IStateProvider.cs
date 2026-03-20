using System.Collections.Generic;

namespace TheSingularityWorkshop.MicroPackages
{
    public interface IStateProvider : IProvider
    {
        List<State> Provided { get; }
    }
}
