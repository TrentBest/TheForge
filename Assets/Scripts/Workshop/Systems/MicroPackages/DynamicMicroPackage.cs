using System.Collections.Generic;

namespace Workshop.Systems.MicroPackages
{
    public class DynamicMicroPackage : IMicroPackage
    {
        public string PackageId { get; set; }
        public string Version { get; set; } = "1.0.0";
        public bool IsLocalShunt { get; set; } = true;

        // --- NEW: Exposing the Intent to the JSON ---
        public PackageExecutionScope ExecutionScope { get; set; } = PackageExecutionScope.Universal;

        public List<string> Dependencies { get; set; } = new List<string>();
        public List<string> Exclusions { get; set; } = new List<string>();

        // Defaults to the standard orbital drop sled
        public string DeliveryProfileId { get; set; } = "Delivery_StandardKineticSled";

        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; set; }
            = new Dictionary<string, List<string>>();

        public List<IPackageArbitrator.Arbitration> Arbitrations { get; set; } = new List<IPackageArbitrator.Arbitration>();
        public List<IProvider> Providers { get; set; } = new List<IProvider>();
        public ForgeSpatialZone SpatialZone { get; internal set; }

        // --- Interface Implementations ---
        public string[] GetDependencies() => Dependencies.ToArray();
        public string[] GetExclusions() => Exclusions.ToArray();

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            // The Bootloader/Arbitrator checks scope BEFORE calling this.
            // If we are in Runtime, and Scope == ForgeOnly, this is never executed.
            foreach (var provider in Providers)
            {
                arbitrator.AddProvider(provider);
            }
        }

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            foreach (var arbitration in Arbitrations)
            {
                arbitrator.SubmitArbitration(arbitration);
            }
        }
    }
}