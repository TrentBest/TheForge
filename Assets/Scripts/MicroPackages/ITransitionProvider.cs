using System.Collections.Generic;

namespace Assets.Scripts.MicroPackages
{
    public interface ITransitionProvider : IProvider
    {
        List<Transition> Provided { get; }
    }
}
