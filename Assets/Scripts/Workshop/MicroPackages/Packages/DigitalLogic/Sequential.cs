using TheSingularityWorkshop.FsmApi.Factories;
using TheSingularityWorkshop.MicroPackages.Providers;
using TheSingularityWorkshop.MicroPackages;
using System.Collections.Generic;

namespace TheSingularityWorkshop.MicroPackages.Packages.DigitalLogic
{
    public class Sequential : IMicroPackage
    {
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new();

        public void Arbitrate(IPackageArbitrator arbitrator) => arbitrator.None();

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            // Register stateful memory behaviors
            //foreach (var behavior in DigitalLogicStateFactory.GetDigitalStates())
            //{
            //    // Deliver as OnUpdate behaviors for our virtual registers
            //    arbitrator.AddProvider(new FsmStateOnUpdateActionProvider(behavior));
            //}
        }
    }
}