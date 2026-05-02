using System.Collections.Generic;

namespace Workshop.Systems.MicroPackages
{
    public interface IStateOnExitProvider : IProvider
    {
        List<StateMethod> Provided { get; }
    }
}
