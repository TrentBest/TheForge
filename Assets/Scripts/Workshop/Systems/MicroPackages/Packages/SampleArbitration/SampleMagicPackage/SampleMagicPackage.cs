using System.Collections.Generic;
using UnityEngine;

namespace Workshop.Systems.MicroPackages.Packages.SampleArbitration
{
    public class SampleMagicPackage : MonoBehaviour, IMicroPackage
    {
        public string PackageId { get; set; } = "Workshop.Sample.Magic";

        // --- NEW ARCHITECTURAL INTENT ---
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // Arrives via a magical spatial rift instead of a physical sled
        public string DeliveryProfileId => "Delivery_SpatialRift";

        public string[] GetDependencies() => new[] { "Workshop.Sample.Elements" };
        public string[] GetExclusions() => new string[0];

        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new();
        private bool _hasIgnitedSword = false;

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[Magic]</b> Injecting ethereal logic into The Brain.");
        }

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            if (!_hasIgnitedSword &&
                arbitrator.InstalledPackages.Contains("Workshop.Sample.Weapons") &&
                arbitrator.InstalledPackages.Contains("Workshop.Sample.Elements"))
            {
                Debug.Log($"<b>[Magic]</b> Elements + Weapons found. Igniting Broadsword.");
                arbitrator.SubmitArbitration(new IPackageArbitrator.Arbitration
                {
                    requestingPackage = this.PackageId,
                    targetedPackage = "Workshop.Sample.Weapons",
                    arbitrationType = IPackageArbitrator.ArbitrationType.Modification,
                    payload = "Modify_Broadsword_To_Flaming"
                });
                _hasIgnitedSword = true;
            }
        }
    }
}