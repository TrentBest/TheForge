using System.Collections.Generic;

namespace TheSingularityWorkshop.MicroPackages
{
    public interface IFSMProvider : IProvider
    {
        List<FSM> Provided { get; }
    }
}
