using System.Collections.Generic;

namespace Workshop.Systems.MicroPackages
{
    public interface IStateOnUpdateProvider : IProvider
    {
        List<StateMethod> Provided { get; }
    }
}
