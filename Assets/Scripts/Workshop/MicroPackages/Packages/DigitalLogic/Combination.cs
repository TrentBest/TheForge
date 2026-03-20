using TheSingularityWorkshop.MicroPackages.Providers;
using TheSingularityWorkshop.FsmApi.Behaviors.OnCondition;
using TheSingularityWorkshop.MicroPackages;
using System;
using System.Collections.Generic;
using System.Text;

namespace TheSingularityWorkshop.MicroPackages.Packages.DigitalLogic
{
    public class Combination : IMicroPackage
    {
        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            foreach (var gate in OnConditionBehaviorFactory.GetDigitalLogic())
            {
                arbitrator.AddProvider(new FsmStateConditionProvider(gate.Value));
            }
        }
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new();
        public void Arbitrate(IPackageArbitrator arbitrator) => arbitrator.None();
    }
}
