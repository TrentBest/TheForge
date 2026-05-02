using System.Collections.Generic;
using UnityEngine;

namespace Workshop.Systems.MicroPackages.Packages.SampleArbitration
{
    public class SampleElementsPackage : MonoBehaviour, IMicroPackage
    {
        public string PackageId { get; set; } = "Workshop.Sample.Elements";

        // --- NEW ARCHITECTURAL INTENT ---
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;
        public string DeliveryProfileId => "Delivery_KineticSled";
        // public ForgeZone SpatialZone => ForgeZone.Lab; // (Add this once you define the enum)

        public string[] GetDependencies() => new[]
        {
            "Workshop.Sample.Elements.Earth",
            "Workshop.Sample.Elements.Air",
            "Workshop.Sample.Elements.Water",
            "Workshop.Sample.Elements.Fire"
        };

        // Elements play nice with everyone, no exclusions
        public string[] GetExclusions() => new string[0];

        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new();

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[Elements]</b> Loading base ontologies into The Lab.");
        }

        public void Arbitrate(IPackageArbitrator arbitrator) { }
    }
}