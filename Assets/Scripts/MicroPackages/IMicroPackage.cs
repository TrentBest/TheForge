using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.MicroPackages
{
    public interface IMicroPackage
    {
        Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; }

        void LoadPackage(IPackageArbitrator arbitrator);
        void Arbitrate(IPackageArbitrator arbitrator);

    }
}
