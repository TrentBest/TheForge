using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.MicroPackages.Packages.DigitalLogic
{
    public class Programmable : IMicroPackage
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
