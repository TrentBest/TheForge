using System;
using System.Collections.Generic;
using UnityEngine;
using Workshop.Systems.MicroPackages;

namespace TheSingularityWorkshop.MicroPackages.Packages.DigitalLogic
{
    /// <summary>
    /// The Physical Layer of the Digital Logic Stack.
    /// Handles the manifestation of hardware-level components: switches, relays, 
    /// physical interlocks, and mechanical input devices (Keypads, Combo Locks).
    /// </summary>
    public class Physical : IMicroPackage, IArchivalMetadata
    {
        // --- Core Identity ---
        public string PackageId { get; set; } = "Workshop.DigitalLogic.Physical";

        /// <summary>
        /// Maps physical signal polling to the Unity Update loop.
        /// </summary>
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>
        {
            { "Update", new List<string> { "Physical_Signal_Polling", "Mechanical_Debounce" } }
        };

        // --- Architectural Intent ---
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // Physical hardware requires a heavy architectural drop
        public string DeliveryProfileId => "Delivery_ArchitecturalFabrication";

        // Foundational physical components have no software dependencies
        public string[] GetDependencies() => new string[0];

        // No exclusions
        public string[] GetExclusions() => new string[0];

        #region --- IArchivalMetadata (The Data Card) ---

        public string Version => "1.0.0";
        public string Author => "The Singularity Workshop";
        public ForgeSpatialZone SpatialZone => ForgeSpatialZone.PhysicalAssets; // The Foundry

        public string ShortDescription => "Manifestation of physical logical hardware.";
        public string LongDescription => "Provides the bridge between diegetic world interactions and binary logic. Includes templates for physical switches, combination locks, and relay-based interlocks.";
        public string ThumbnailUrl => "Textures/Thumbnails/Archive_DigitalLogic_Physical";

        public int ReleaseYear => DateTime.Now.Year;
        public string OriginalAuthor => "Trent Best";
        public string OriginalPublisher => "The Singularity Workshop";
        public string OriginalPlatform => "The Forge";
        public string Era => "Emergence";
        public string HistoricalSignificance => "The 'Layer 0' of the logic stack, allowing world-space objects to drive FSM state transitions.";
        public Color AccentColor => new Color(0.5f, 0.5f, 0.5f); // Industrial Grey

        #endregion

        /// <summary>
        /// Phase 1: Injection. 
        /// We register the physical condition providers that allow a Door FSM 
        /// to check if a "Physical_Combination_Resolved" bit is set.
        /// </summary>
        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[The Foundry]</b> {PackageId} online. Physical signal buffers cleared.");

            // Example: Registering a 10-key passkey behavior
            // arbitrator.AddProvider(new FsmStateConditionProvider(passkeyLogic));
        }

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            // Scanning for 'PowerGrid' packages to see if physical switches should be powered.
            if (arbitrator.InstalledPackages.Contains("Workshop.Systems.PowerGrid"))
            {
                Debug.Log($"<b>[Physical]</b> Power Grid detected. Enabling electrical requirements for mechanical relays.");
            }
        }
    }
}