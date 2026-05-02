using System;
using System.Collections.Generic;
using System.Text;

namespace Workshop.Systems.MicroPackages
{
    public interface IMicroPackage
    {
        string PackageId { get; }

        // --- NEW: The Architectural Intent ---
        PackageExecutionScope ExecutionScope { get; }

        // Packages required for this to run
        string[] GetDependencies();

        // Packages that will cause a fatal crash if loaded alongside this one
        string[] GetExclusions();

        // An optional ID pointing to a delivery mechanism (e.g., "Delivery_KineticSled", "Delivery_Carryall")
        string DeliveryProfileId { get; }

        Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; }
        void LoadPackage(IPackageArbitrator arbitrator);
        void Arbitrate(IPackageArbitrator arbitrator);

    }

    public enum PackageExecutionScope
    {
        Universal,   // Ships to both Forge and the final Player Experience
        ForgeOnly,   // Image editors, layout tools, diegetic lab equipment
        RuntimeOnly  // Specific performance hooks that only run in the final build
    }

    public enum ForgeSpatialZone
    {
        Core,             // Reserved for native Workshop tools (The Center)
        LogicAndCompute,  // FSMs, AI, Math, Rules (Quadrant 1: The Cortex)
        PhysicalAssets,   // Meshes, Materials, Audio, Worlds (Quadrant 2: The Foundry)
        GameplaySystems,  // Weapons, Magic, Inventories, UI (Quadrant 3: The Armory)
        MetaTools         // Profilers, REST APIs, Dashboards (Quadrant 4: The Observatory)
    }
}
