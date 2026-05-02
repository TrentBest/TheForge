using System.Collections.Generic;

namespace Workshop.Systems.MicroPackages
{
    public interface IStateProvider : IProvider
    {
        List<State> Provided { get; }
    }
}
