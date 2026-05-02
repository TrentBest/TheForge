using System.Collections.Generic;
using UnityEngine;

namespace Workshop.Systems.MicroPackages.Packages.SampleArbitration
{
    public class SampleWeaponsPackage : MonoBehaviour, IMicroPackage
    {
        public string PackageId { get; set; } = "Workshop.Sample.Weapons";

        // --- NEW ARCHITECTURAL INTENT ---
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // Delivered by a heavy dropship instead of a fast sled
        public string DeliveryProfileId => "Delivery_HeavyCarryall";

        public string[] GetDependencies() => new string[0];

        // Let's pretend it fatally crashes with a specific pacifist package
        public string[] GetExclusions() => new[] { "Workshop.Sample.PacifismOverhaul" };

        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new();
        private bool _hasAddedSingingSword = false;

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[Weapons]</b> Forging physical armaments on The Stage.");
        }

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            if (!_hasAddedSingingSword && arbitrator.InstalledPackages.Contains("Workshop.Sample.Magic"))
            {
                Debug.Log($"<b>[Weapons]</b> Magic detected! Singing Scimitar arbitration submitted.");
                arbitrator.SubmitArbitration(new IPackageArbitrator.Arbitration
                {
                    requestingPackage = this.PackageId,
                    targetedPackage = this.PackageId,
                    arbitrationType = IPackageArbitrator.ArbitrationType.Additive,
                    payload = "Weapon_SingingScimitar"
                });
                _hasAddedSingingSword = true;
            }
        }
    }
}