using TheSingularityWorkshop.MicroPackages.Providers;
using TheSingularityWorkshop.MicroPackages;
using System;
using System.Collections.Generic;
using System.Text;

namespace TheSingularityWorkshop.MicroPackages.Packages.FSM.HelloWorld
{
    class HelloWorld : IMicroPackage
    {
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>();

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            
        }

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            //arbitrator.AddProvider(new FsmProvider());
        }

        public HelloWorld()
        {
            ProcessGroupsPerUnityMessage.Add("Update", new List<string> { "Update" });

        }
    }
}
