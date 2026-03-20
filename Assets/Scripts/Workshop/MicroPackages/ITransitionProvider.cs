using System.Collections.Generic;

namespace TheSingularityWorkshop.MicroPackages
{
    public interface ITransitionProvider : IProvider
    {
        List<Transition> Provided { get; }
    }
}
