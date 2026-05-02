using System.Collections.Generic;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;
using Workshop.Systems.MicroPackages;

namespace TheSingularityWorkshop.MicroPackages.Packages.DesignPatterns.Singleton
{
    public class SingletonPackage : IMicroPackage
    {
        // --- Core Identity ---
        public string PackageId { get; set; } = "Workshop.DesignPatterns.Singleton";
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>();

        // --- Architectural Intent ---
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // Pure logic arrives as a fast data stream, not a heavy physical crate
        public string DeliveryProfileId => "Delivery_DigitalDataStream";

        // Singletons are foundational, they don't depend on other patterns inherently
        public string[] GetDependencies() => new string[0];

        // No known exclusions
        public string[] GetExclusions() => new string[0];

        // --- Lifecycle ---
        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[Design Patterns]</b> {PackageId} loaded. Singleton enforcement constraints available in The Brain.");

            // If this package provided GUI tools (like a Singleton config window),
            // you would call arbitrator.AddProvider(...) here.
        }

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            // Singletons generally don't actively modify other packages automatically.
            // They wait for other packages to utilize them.
        }
    }

    /// <summary>
    /// Wraps an existing IStateContext to enforce Singleton constraints.
    /// </summary>
    public class SingletonContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "SingletonWrapper";

        // The actual instance being protected
        public IStateContext WrappedInstance { get; private set; }

        public SingletonContext(IStateContext singleton)
        {
            WrappedInstance = singleton;

            if (singleton != null)
            {
                Name = $"{singleton.Name}_Singleton";
            }
        }
    }
}