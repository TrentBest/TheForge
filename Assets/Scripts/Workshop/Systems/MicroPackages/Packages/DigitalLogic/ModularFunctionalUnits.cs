using System.Collections.Generic;
using UnityEngine;
using Workshop.Systems.MicroPackages;
// using TheSingularityWorkshop.MicroPackages.Providers; // Uncomment when restoring the provider below

namespace TheSingularityWorkshop.MicroPackages.Packages.DigitalLogic
{
    public class ModularFunctionalUnits : IMicroPackage
    {
        // --- Core Identity ---
        public string PackageId { get; set; } = "Workshop.DigitalLogic.ModularFunctionalUnits";
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>();

        // --- Architectural Intent ---
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // Arrives as pure architectural logic
        public string DeliveryProfileId => "Delivery_DigitalDataStream";

        // To build ALUs and Adders, you absolutely need gates (Combinational) and memory (Sequential)
        public string[] GetDependencies() => new[]
        {
            "Workshop.DigitalLogic.Combination",
            "Workshop.DigitalLogic.Sequential"
        };

        // Plays nicely with everyone
        public string[] GetExclusions() => new string[0];

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[Digital Logic]</b> Booting Modular Functional Units. 8-bit Adders, ALUs, and Bus Architectures online in The Cortex.");

            // Provide 8-bit Adder logic
            // arbitrator.AddProvider(new FsmStateOnUpdateActionProvider(
            //    new StateMethod("ADD_8BIT", "Sums Channel A and B into Register",
            //    ctx => { /* Summing logic */ })));
        }

        public void Arbitrate(IPackageArbitrator arbitrator) => arbitrator.None();
    }
}