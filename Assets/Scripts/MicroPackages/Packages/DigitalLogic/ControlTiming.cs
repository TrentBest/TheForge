
using Assets.Scripts.MicroPackages.Providers;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.MicroPackages.Packages.DigitalLogic
{
    public class ControlTiming : IMicroPackage
    {
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new();

        public void Arbitrate(IPackageArbitrator arbitrator) => arbitrator.None();

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            // Provide global clock toggle behavior
            arbitrator.AddProvider(new FsmStateOnUpdateActionProvider(
                new StateMethod("TOGGLE_CLOCK", "Inverts signal on clock channel",
                ctx => { /* Toggle logic */ })));
        }
    }
}