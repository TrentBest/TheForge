using TheSingularityWorkshop.MicroPackages;
using System;
using System.Collections.Generic;
using System.Text;

namespace TheSingularityWorkshop.MicroPackages.Packages.DigitalLogic
{
    public class Programmable : IMicroPackage
    {
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; private set; } = new Dictionary<string, List<string>>();

        

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
           
        }

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            
        }
    }
}
