using System.Collections.Generic;

namespace Assets.Scripts.MicroPackages
{
    public interface IFSMProvider : IProvider
    {
        List<FSM> Provided { get; }
    }
}
