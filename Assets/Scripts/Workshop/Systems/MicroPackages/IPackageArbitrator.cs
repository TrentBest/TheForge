using TheSingularityWorkshop.MicroPackages.Providers;
using System.Collections.Generic;
using Workshop.GURPS;
using Workshop.Core.Memory; // Namespace for DataWarehouse

namespace Workshop.Systems.MicroPackages
{
    /// <summary>
    /// Mediates dependencies and life-cycle events for Micro-Packages.
    /// Reforged to provide global access to the DataWarehouse.
    /// </summary>
    public interface IPackageArbitrator
    {
        // --- DATA BRIDGE ---
        /// <summary>
        /// Access to the central data repository for registry sync and memory management.
        /// </summary>
        DataWarehouse Warehouse { get; }

        // --- PACKAGE STATUS ---
        List<string> InstalledPackages { get; }
        Dictionary<string, List<string>> DefinedProcessGroupsPerUnityMessage { get; }

        // --- ARBITRATION MODELS ---
        public struct Arbitration
        {
            public string requestingPackage;
            public string targetedPackage;
            public ArbitrationType arbitrationType;
            public object payload;
        }

        public enum ArbitrationType
        {
            Invalid = 0,
            Additive = 1,    // Adding logic/data
            Subtractive = 2, // Removing logic/data
            Modification = 3 // Mutating existing logic/data
        }

        // --- COMMAND API ---
        void SubmitArbitration(Arbitration arbitration);
        void None();
        void AddProvider(IProvider provider);
        void LoadPackage(IMicroPackage microPackage);
    }
}