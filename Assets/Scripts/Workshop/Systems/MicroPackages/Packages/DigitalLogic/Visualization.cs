using System;
using System.Collections.Generic;
using UnityEngine;
using Workshop.Systems.MicroPackages;

namespace TheSingularityWorkshop.MicroPackages.Packages.DigitalLogic
{
    /// <summary>
    /// The Visualization Layer of the Digital Logic Stack.
    /// Responsible for translating internal logic states into human-observable signals.
    /// Acts as the 'Observer' that bridges binary computation with visual feedback.
    /// </summary>
    public class Visualization : IMicroPackage, IArchivalMetadata
    {
        // --- Core Identity ---
        public string PackageId { get; set; } = "Workshop.DigitalLogic.Visualization";

        /// <summary>
        /// Visual updates are slaved to the main Update loop to ensure telemetry 
        /// matches the frame-by-frame state of the logic board.
        /// </summary>
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>
        {
            { "Update", new List<string> { "Telemetry_Refresh", "Display_Buffer_Output" } }
        };

        // --- Architectural Intent ---
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // Visualizations are logic-heavy but lightweight to deliver.
        public string DeliveryProfileId => "Delivery_DigitalDataStream";

        /// <summary>
        /// Visualization cannot exist without the signals it intends to observe.
        /// </summary>
        public string[] GetDependencies() => new[] { "Workshop.DigitalLogic.Combination" };

        public string[] GetExclusions() => new string[0];

        #region --- IArchivalMetadata (The Data Card) ---

        public string Version => "1.0.0";
        public string Author => "The Singularity Workshop";
        public ForgeSpatialZone SpatialZone => ForgeSpatialZone.MetaTools; // Quadrant 4: The Observatory

        public string ShortDescription => "Logic-to-Visual translation and diagnostic telemetry.";
        public string LongDescription => "Translates raw binary signals into human-readable formats. Includes BCD-to-7-segment decoders, logic probes, and diagnostic telemetry drivers for virtual hardware monitoring.";
        public string ThumbnailUrl => "Textures/Thumbnails/Archive_DigitalLogic_Visualization";

        public int ReleaseYear => DateTime.Now.Year;
        public string OriginalAuthor => "Trent Best";
        public string OriginalPublisher => "The Singularity Workshop";
        public string OriginalPlatform => "The Forge";
        public string Era => "Emergence";
        public string HistoricalSignificance => "The bridge between abstract silicon logic and human perception, enabling real-time diagnostics of virtual hardware.";
        public Color AccentColor => Color.red; // The classic glow of a vintage 7-segment LED

        #endregion

        /// <summary>
        /// Phase 1: Injection.
        /// Register the diagnostic providers that allow FSMs to output their state to visual buffers.
        /// </summary>
        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[The Observatory]</b> {PackageId} online. Diagnostic display pipelines active.");

            // Register the 7-segment decoder logic to translate 4-bit BCD into a-g segments
            // arbitrator.AddProvider(new FsmStateOnUpdateActionProvider(
            //    new StateMethod("DECODE_7SEGMENT", "Maps 4-bit BCD to segments a-g",
            //    ctx => { 
            //        /* //           Binary to Pattern Mapping (e.g. 0000 -> 1111110) 
            //           This is the pattern you once used for that digital clock!
            //        */ 
            //    })));
        }

        /// <summary>
        /// Phase 2: Convergence.
        /// We scan for 'Physical' logic to see if we need to wire up LED mesh emissives in the world.
        /// </summary>
        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            if (arbitrator.InstalledPackages.Contains("Workshop.DigitalLogic.Physical"))
            {
                Debug.Log($"<b>[Visualization]</b> Physical hardware detected. Calibrating mesh-emissive buffers for diegetic 7-segment displays.");
            }
        }
    }
}