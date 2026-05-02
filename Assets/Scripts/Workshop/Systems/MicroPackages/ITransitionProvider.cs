using System.Collections.Generic;

namespace Workshop.Systems.MicroPackages
{
    public interface ITransitionProvider : IProvider
    {
        List<Transition> Provided { get; }
    }
}
