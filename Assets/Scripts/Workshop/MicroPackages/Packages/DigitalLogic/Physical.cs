using TheSingularityWorkshop.MicroPackages.Providers;
using TheSingularityWorkshop.MicroPackages;
using System;
using System.Collections.Generic;
using System.Text;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.MicroPackages.Packages.DigitalLogic
{
    public class Physical : IMicroPackage
    {
       

        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>();

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            arbitrator.None();
        }

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            // Logic for raw switches and passive signal timing
            //arbitrator.AddProvider(new FsmStateConditionProvider(logicFunc));
            //arbitrator.AddProvider(new FsmStateConditionProvider( logicFunc));
        }

       
    }
}
