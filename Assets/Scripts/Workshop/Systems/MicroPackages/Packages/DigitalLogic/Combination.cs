using System.Collections.Generic;
using UnityEngine;
using TheSingularityWorkshop.MicroPackages.Providers;
using Workshop.Systems.MicroPackages;
using Workshop.Systems.FSMs.Behaviors.OnCondition;

namespace TheSingularityWorkshop.MicroPackages.Packages.DigitalLogic
{
    public class Combination : IMicroPackage
    {
        // --- Core Identity ---
        public string PackageId { get; set; } = "Workshop.DigitalLogic.Combination";
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new();

        // --- Architectural Intent ---
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // Delivered as a raw data stream into The Cortex
        public string DeliveryProfileId => "Delivery_DigitalDataStream";

        // Foundational logic gates generally don't have dependencies
        public string[] GetDependencies() => new string[0];

        // No exclusions for basic math/logic
        public string[] GetExclusions() => new string[0];

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[Digital Logic]</b> Injecting Combinational Logic Gates into The Cortex.");

            foreach (var gate in OnConditionBehaviorFactory.GetDigitalLogic())
            {
                arbitrator.AddProvider(new FsmStateConditionProvider(gate.Value));
            }
        }

        public void Arbitrate(IPackageArbitrator arbitrator) => arbitrator.None();
    }
}