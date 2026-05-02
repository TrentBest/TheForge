using System.Collections.Generic;

namespace Workshop.Systems.MicroPackages
{
    public interface IFSMProvider : IProvider
    {
        List<FSM> Provided { get; }
    }
}
