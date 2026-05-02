using System;
using System.Collections.Generic;
using UnityEngine;
using Workshop.Systems.MicroPackages;

namespace TheSingularityWorkshop.MicroPackages.Packages.DigitalLogic
{
    /// <summary>
    /// The Programmable Logic Layer.
    /// Handles the manifestation of Instruction Sets (ISA), Microcode, 
    /// and Programmable Gate Arrays (FPGA-style logic).
    /// This is the "Brain" that orchestrates the Modular Functional Units.
    /// </summary>
    public class Programmable : IMicroPackage, IArchivalMetadata
    {
        // --- Core Identity ---
        public string PackageId { get; set; } = "Workshop.DigitalLogic.Programmable";

        /// <summary>
        /// Maps the Instruction Fetch/Decode cycle to the main simulation tick.
        /// </summary>
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; private set; } = new Dictionary<string, List<string>>
        {
            { "Update", new List<string> { "ISA_Fetch_Decode", "Register_Writeback" } }
        };

        // --- Architectural Intent ---
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // Arrives as a dense stream of binary microcode
        public string DeliveryProfileId => "Delivery_DigitalDataStream";

        /// <summary>
        /// A CPU cannot function without its ALU (ModularFunctionalUnits) 
        /// or its foundational gates (Combination).
        /// </summary>
        public string[] GetDependencies() => new[]
        {
            "Workshop.DigitalLogic.Combination",
            "Workshop.DigitalLogic.ModularFunctionalUnits"
        };

        public string[] GetExclusions() => new string[0];

        #region --- IArchivalMetadata (The Data Card) ---

        public string Version => "1.0.0";
        public string Author => "The Singularity Workshop";
        public ForgeSpatialZone SpatialZone => ForgeSpatialZone.LogicAndCompute; // The Cortex

        public string ShortDescription => "Instruction Set Architecture and Programmable Logic.";
        public string LongDescription => "Provides the framework for custom ISA development, including microcode definition and instruction decoding. Essential for forging virtual CPUs (like MIPS reconstructions) within the simulation.";
        public string ThumbnailUrl => "Textures/Thumbnails/Archive_DigitalLogic_Programmable";

        public int ReleaseYear => DateTime.Now.Year;
        public string OriginalAuthor => "Trent Best";
        public string OriginalPublisher => "The Singularity Workshop";
        public string OriginalPlatform => "The Forge";
        public string Era => "Emergence";
        public string HistoricalSignificance => "The pinnacle of the digital logic stack; where hardware design becomes software architecture.";
        public Color AccentColor => new Color(0.6f, 0.2f, 0.8f); // CPU Purple

        #endregion

        /// <summary>
        /// Phase 1: Injection.
        /// Register the Instruction Set Providers so the DataWarehouse 
        /// can store and retrieve custom opcodes.
        /// </summary>
        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[The Cortex]</b> {PackageId} online. Microcode buffers initialized. Ready to decode instructions.");

            // Example: arbitrator.AddProvider(new IsaProvider(mipsSet));
        }

        /// <summary>
        /// Phase 2: Convergence.
        /// We can scan for "Visualization" packages to auto-wire the register status
        /// to a diegetic debug screen in the Forge.
        /// </summary>
        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            if (arbitrator.InstalledPackages.Contains("Workshop.DigitalLogic.Visualization"))
            {
                Debug.Log($"<b>[Programmable]</b> Visualization detected. Mapping register bus to UI telemetry.");
            }
        }
    }
}