using System.Collections.Generic;

namespace Assets.Scripts.MicroPackages
{
    public interface IStateProvider : IProvider
    {
        List<State> Provided { get; }
    }
}
