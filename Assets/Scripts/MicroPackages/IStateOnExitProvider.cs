using System.Collections.Generic;

namespace Assets.Scripts.MicroPackages
{
    public interface IStateOnExitProvider : IProvider
    {
        List<StateMethod> Provided { get; }
    }
}
