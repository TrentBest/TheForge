using TheSingularityWorkshop.MicroPackages;
using System;
using System.Collections.Generic;
using System.Text;

namespace TheSingularityWorkshop.MicroPackages.Packages.FSM
{
    public class FSM : IMicroPackage
    {
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage => throw new NotImplementedException();

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            throw new NotImplementedException();
        }

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            throw new NotImplementedException();
        }
    }

}
