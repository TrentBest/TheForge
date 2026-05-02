using System;
using System.Collections.Generic;
using UnityEngine;
using Workshop.Systems.MicroPackages;

namespace TheSingularityWorkshop.MicroPackages.Packages.DigitalLogic
{
    /// <summary>
    /// A Meta-Package (Aggregator) that pulls in the entire Digital Computing Stack.
    /// </summary>
    public class DigitalLogic : IMicroPackage, IArchivalMetadata
    {
        // Standardized IDs mapped to the specific sub-packages
        private readonly string _physicalId = "Workshop.DigitalLogic.Physical";
        private readonly string _combinationalId = "Workshop.DigitalLogic.Combination";
        private readonly string _sequentialId = "Workshop.DigitalLogic.Sequential";
        private readonly string _modularId = "Workshop.DigitalLogic.ModularFunctionalUnits";
        private readonly string _controlId = "Workshop.DigitalLogic.ControlTiming";
        private readonly string _programmableId = "Workshop.DigitalLogic.Programmable";
        private readonly string _visualizationId = "Workshop.DigitalLogic.Visualization";

        // --- Core Identity ---
        public string PackageId { get; set; } = "Workshop.DigitalLogic.Core";
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>();

        // --- Architectural Intent ---
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // As a massive umbrella package, it triggers a large data stream sequence
        public string DeliveryProfileId => "Delivery_DataStreamBurst";

        // THE AGGREGATION: By declaring these as dependencies, the Arbitrator will automatically
        // download and safely install them (checking for exclusions) before this package boots.
        public string[] GetDependencies() => new[]
        {
            _physicalId,
            _combinationalId,
            _sequentialId,
            _modularId,
            _controlId,
            _programmableId,
            _visualizationId
        };

        public string[] GetExclusions() => new string[0]; // Core logic plays nice with everyone

        // ====================================================================
        // 🏛️ IArchivalMetadata Implementation (The Data Card)
        // ====================================================================
        public string Version { get; set; } = "1.0.0";
        public string Author { get; set; } = "The Singularity Workshop";
        public ForgeSpatialZone SpatialZone { get; set; } = ForgeSpatialZone.LogicAndCompute;

        public string ShortDescription { get; set; } = "The complete Digital Logic meta-package.";
        public string LongDescription { get; set; } = "A foundational aggregator that injects the entire digital computing stack (Physical, Combinational, Sequential, Modular, Timing, Programmable, and Visualization) into The Cortex.";
        public string ThumbnailUrl { get; set; } = "Textures/Thumbnails/Archive_DigitalLogic";

        public int ReleaseYear { get; set; } = DateTime.Now.Year;
        public string OriginalAuthor { get; set; } = "";
        public string OriginalPublisher { get; set; } = "";
        public string OriginalPlatform { get; set; } = "The Singularity";
        public string Era { get; set; } = "Modern Foundation";
        public string HistoricalSignificance { get; set; } = "The foundational bedrock of all in-engine binary computation.";
        public Color AccentColor { get; set; } = new Color(0.1f, 0.8f, 0.8f); // Cybernetic Cyan

        // --- Lifecycle ---
        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[Digital Logic]</b> Core meta-package initialized. Delegating subsystem injection to Arbitrator dependency resolution.");

            // We no longer manually call arbitrator.LoadPackage() here! 
            // The system handles the logistics based on the GetDependencies() list.
        }

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            // Meta-packages generally do not need to arbitrate; they exist just to pull in dependencies.
            arbitrator.None();
        }
    }
}