using System.Collections.Generic;
using UnityEngine;
using Workshop.Systems.MicroPackages;
// using TheSingularityWorkshop.MicroPackages.Providers; // Uncomment when restoring the provider below

namespace TheSingularityWorkshop.MicroPackages.Packages.DigitalLogic
{
    public class ControlTiming : IMicroPackage
    {
        // --- Core Identity ---
        public string PackageId { get; set; } = "Workshop.DigitalLogic.ControlTiming";
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new();

        // --- Architectural Intent ---
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // Delivered as a raw data stream into The Cortex
        public string DeliveryProfileId => "Delivery_DigitalDataStream";

        // Foundational timing logic does not require external dependencies
        public string[] GetDependencies() => new string[0];

        // Plays nicely with all other packages
        public string[] GetExclusions() => new string[0];

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[Digital Logic]</b> Injecting Control & Timing Systems (Clocks, Oscillators) into The Cortex.");

            // Provide global clock toggle behavior
            // arbitrator.AddProvider(new FsmStateOnUpdateActionProvider(
            //    new StateMethod("TOGGLE_CLOCK", "Inverts signal on clock channel",
            //    ctx => { /* Toggle logic */ })));
        }

        public void Arbitrate(IPackageArbitrator arbitrator) => arbitrator.None();
    }
}