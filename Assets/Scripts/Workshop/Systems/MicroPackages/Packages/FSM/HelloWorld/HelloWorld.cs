using System.Collections.Generic;
using UnityEngine;
using Workshop.Systems.MicroPackages;

namespace TheSingularityWorkshop.MicroPackages.Packages.FSM.HelloWorld
{
    /// <summary>
    /// The "Hello World" of MicroPackages. 
    /// This class serves as the fundamental blueprint for extending the Singularity Engine.
    /// It demonstrates the minimum required implementation to hook into the Arbitrator,
    /// define execution schedules, and participate in ecosystem convergence.
    /// </summary>
    public class HelloWorld : IMicroPackage, IArchivalMetadata
    {
        /// <summary>
        /// Maps internal logic groups to Unity's main-thread lifecycle messages.
        /// Here, we tell the engine to run our "Update" group during the standard Mono Update loop.
        /// </summary>
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>();

        /// <summary>
        /// Unique identifier for the package registry. 
        /// Use dot-notation for organizational hierarchy.
        /// </summary>
        public string PackageId { get; set; } = "Workshop.Samples.HelloWorld";

        /// <summary>
        /// Determines where the package code is allowed to execute.
        /// Universal ensures this logic exists in both the Forge (Editor) and the final Build.
        /// </summary>
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        /// <summary>
        /// Specifies the diegetic delivery method used by the Manifestation Engine.
        /// "DigitalStream" bypasses heavy physical crate drops for lightweight logic.
        /// </summary>
        public string DeliveryProfileId => "Delivery_DigitalDataStream";

        #region --- IArchivalMetadata (The Data Card) ---

        /// <summary> Modern versioning for the package manager. </summary>
        public string Version => "1.0.0";
        /// <summary> The entity responsible for this logic board. </summary>
        public string Author => "The Singularity Workshop";
        /// <summary> Physical/Logical sector in the Forge where this package manifests. </summary>
        public ForgeSpatialZone SpatialZone => ForgeSpatialZone.LogicAndCompute;
        /// <summary> Displayed on the Gondola Yard terminal. </summary>
        public string ShortDescription => "Foundational entrypoint for MicroPackage development.";
        /// <summary> Deep-dive explanation for the creator. </summary>
        public string LongDescription => "A pedagogical sample illustrating FSM injection, dependency declaration, and the load-arbitrate lifecycle.";
        /// <summary> Path to the preview image in the DataWarehouse. </summary>
        public string ThumbnailUrl => "Textures/Thumbnails/Samples_HelloWorld";
        /// <summary> Preserved year for the historical registry. </summary>
        public int ReleaseYear => 2026;
        public string OriginalAuthor => "Trent Best";
        public string OriginalPublisher => "The Singularity Workshop";
        public string OriginalPlatform => "The Forge";
        public string Era => "Emergence";
        public string HistoricalSignificance => "The very first pattern established for the decoupled MicroPackage architecture.";
        /// <summary> The UI color signature used in the Kiosk dashboard. </summary>
        public Color AccentColor => new Color(0.4f, 1.0f, 0.4f); // Neon 'Hello' Green

        #endregion

        /// <summary>
        /// Constructor: Initializes the default execution pipeline.
        /// </summary>
        public HelloWorld()
        {
            // Register a logic bucket that will be pulsed every frame.
            ProcessGroupsPerUnityMessage.Add("Update", new List<string> { "HelloWorld_Pulse" });
        }

        /// <summary>
        /// Phase 1: The Initial Handshake.
        /// Triggered once when the Kinetic Sled finishes unpacking.
        /// Use this to register IProviders (UIs, FSMs, Tools) with the Arbitrator.
        /// </summary>
        /// <param name="arbitrator">The central conductor of the Forge.</param>
        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[Cortex]</b> {PackageId} online. Greetings, Creator.");

            // Example: arbitrator.AddProvider(new FsmProvider(MyCustomFsm));
        }

        /// <summary>
        /// Phase 2: The Convergence Loop.
        /// Triggered during every pass of the negotiation phase.
        /// Use this to scan for other packages and submit Modifications or Additions.
        /// </summary>
        /// <param name="arbitrator">The active ecosystem context.</param>
        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            // Hello World is self-contained and does not require negotiation.
        }

        /// <summary>
        /// Lists other PackageIds required for this code to function.
        /// The ForgeNetworkBus will resolve these recursively.
        /// </summary>
        public string[] GetDependencies() => new string[0];

        /// <summary>
        /// Lists PackageIds that would cause a state-conflict or crash.
        /// The Exclusion Wall in the Arbitrator will block installation if these are present.
        /// </summary>
        public string[] GetExclusions() => new string[0];
    }
}