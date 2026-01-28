using Assets.Scripts.MicroPackages.Providers;
using System.Collections.Generic;

namespace Assets.Scripts.MicroPackages.DigitalLogic
{
    public class ModularFunctionalUnits : IMicroPackage
    {
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new();

        public void Arbitrate(IPackageArbitrator arbitrator) => arbitrator.None();

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            // Provide 8-bit Adder logic
            arbitrator.AddProvider(new FsmStateOnUpdateActionProvider(
                new StateMethod("ADD_8BIT", "Sums Channel A and B into Register",
                ctx => { /* Summing logic */ })));
        }
    }
}