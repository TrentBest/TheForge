using Assets.Scripts.FSMs.Behaviors.OnUpdate;
using Assets.Scripts.MicroPackages.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.MicroPackages.Packages.DigitalLogic
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
