using System.Collections.Generic;

namespace Assets.Scripts.MicroPackages
{
    public interface IStateOnUpdateProvider : IProvider
    {
        List<StateMethod> Provided { get; }
    }
}
