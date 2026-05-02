using System;
using System.Collections.Generic;
using UnityEngine;
using Workshop.Systems.MicroPackages;

namespace TheSingularityWorkshop.MicroPackages.Packages.DigitalLogic
{
    /// <summary>
    /// The Sequential Logic Layer.
    /// Introduces the dimension of Chronos to the logic stack.
    /// Responsible for stateful memory: Flip-Flops, Latches, and Registers.
    /// </summary>
    public class Sequential : IMicroPackage, IArchivalMetadata
    {
        // --- Core Identity ---
        public string PackageId { get; set; } = "Workshop.DigitalLogic.Sequential";

        /// <summary>
        /// Sequential logic requires a synchronized clock pulse to transition states.
        /// We map the "Memory_Tock" to the LateUpdate to ensure all combinational 
        /// logic has settled before we latch the new state.
        /// </summary>
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>
        {
            { "LateUpdate", new List<string> { "Sequential_Latch_Pulse", "Register_Synchronization" } }
        };

        // --- Architectural Intent ---
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // Arrives as pure architectural logic
        public string DeliveryProfileId => "Delivery_DigitalDataStream";

        /// <summary>
        /// Sequential logic (Memory) is built out of Combinational logic (Gates).
        /// You cannot have a Flip-Flop without NAND/NOR gates.
        /// </summary>
        public string[] GetDependencies() => new[] { "Workshop.DigitalLogic.Combination" };

        public string[] GetExclusions() => new string[0];

        #region --- IArchivalMetadata (The Data Card) ---

        public string Version => "1.0.0";
        public string Author => "The Singularity Workshop";
        public ForgeSpatialZone SpatialZone => ForgeSpatialZone.LogicAndCompute; // The Cortex

        public string ShortDescription => "Stateful memory and clock-synchronized logic.";
        public string LongDescription => "Introduces the concept of 'State' to the digital stack. Provides Flip-Flops, Latches, and bit-registers. Essential for building counters, shift-registers, and CPU-style memory banks.";
        public string ThumbnailUrl => "Textures/Thumbnails/Archive_DigitalLogic_Sequential";

        public int ReleaseYear => DateTime.Now.Year;
        public string OriginalAuthor => "Trent Best";
        public string OriginalPublisher => "The Singularity Workshop";
        public string OriginalPlatform => "The Forge";
        public string Era => "Emergence";
        public string HistoricalSignificance => "The transition from instant math to persistent simulation; the birth of 'Memory'.";
        public Color AccentColor => new Color(0.2f, 0.4f, 1.0f); // Memory Blue

        #endregion

        /// <summary>
        /// Phase 1: Injection.
        /// Register the stateful memory behaviors that allow virtual registers 
        /// to persist data across simulation ticks.
        /// </summary>
        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[The Cortex]</b> {PackageId} online. Latch timing synchronized. Memory banks ready.");

            // Register stateful memory behaviors
            // foreach (var behavior in DigitalLogicStateFactory.GetDigitalStates())
            // {
            //    // Deliver as OnUpdate behaviors for our virtual registers
            //    arbitrator.AddProvider(new FsmStateOnUpdateActionProvider(behavior));
            // }
        }

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            // Sequential logic often arbitrates with 'ControlTiming' to sync clock pulses.
            if (arbitrator.InstalledPackages.Contains("Workshop.DigitalLogic.ControlTiming"))
            {
                Debug.Log($"<b>[Sequential]</b> Global Clock detected. Slaving latches to Master Timing signal.");
            }
        }
    }
}