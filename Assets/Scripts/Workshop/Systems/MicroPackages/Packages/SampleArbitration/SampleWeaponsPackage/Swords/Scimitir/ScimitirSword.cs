using System;
using System.Collections.Generic;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;
using Workshop.Systems.MicroPackages;

namespace Assets.Scripts.Warlords.Equipment
{
    /// <summary>
    /// Tactical Micro-Package for Curved Blade combat logic.
    /// Manages the transition between idle, active swing, and recovery states.
    /// Slotted into The Armory for gameplay systems orchestration.
    /// </summary>
    public class ScimitirSword : IMicroPackage, IArchivalMetadata
    {
        // --- Core Identity ---
        public string PackageId { get; set; } = "Workshop.Warlords.Equipment.Scimitar";

        /// <summary>
        /// Registers the "Combat_Logic" group to the main Unity Update loop.
        /// This ensures the scimitar's state machine pulses with the simulation clock.
        /// </summary>
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>
        {
            { "Update", new List<string> { "Combat_Logic" } }
        };

        // --- Architectural Intent ---
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // Arrives via a kinetic sled drop in the Foundry/Armory sectors.
        public string DeliveryProfileId => "Delivery_KineticSled";

        // Requires the FSM core to define the tactical state machine.
        public string[] GetDependencies() => new[] { "Workshop.Core.FSM" };

        // No known conflicts; steel is universal.
        public string[] GetExclusions() => new string[0];

        #region --- IArchivalMetadata (The Data Card) ---

        public string Version => "1.0.0";
        public string Author => "The Singularity Workshop";
        public ForgeSpatialZone SpatialZone => ForgeSpatialZone.GameplaySystems; // Quadrant 3: The Armory

        public string ShortDescription => "Tactical FSM for curved blade combat.";
        public string LongDescription => "Provides a high-performance combat FSM for scimitar-class weapons. Manages swing physics, recovery timing, and state-driven hit detection. Reforged from the 1989 Warlords era.";
        public string ThumbnailUrl => "Textures/Thumbnails/Archive_Warlords_Scimitar";

        public int ReleaseYear => 1989; // Warlords original release era
        public string OriginalAuthor => "Steve Fawkner";
        public string OriginalPublisher => "Strategic Studies Group (SSG)";
        public string OriginalPlatform => "MS-DOS";
        public string Era => "The Golden Age of Fantasy Strategy";
        public string HistoricalSignificance => "Warlords defined the fantasy strategy genre; this package preserves the tactical 'weight' of its martial combat logic.";
        public Color AccentColor => new Color(0.95f, 0.77f, 0.06f); // Warlords Gold

        #endregion

        /// <summary>
        /// Constructor: Setup the default processing pipeline.
        /// </summary>
        public ScimitirSword() { }

        /// <summary>
        /// SHOWCASE: The Arbitration Phase.
        /// This is where the Scimitar looks at the ecosystem to see if it should 
        /// 're-forge' itself into a magical variant.
        /// </summary>
        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            if (arbitrator.Warehouse == null)
            {
                Debug.LogError($"[Scimitar] Arbitration Failed: DataWarehouse is offline.");
                return;
            }

            // 1. Check for Magic Package to add Elemental Damage
            if (arbitrator.InstalledPackages.Contains("Workshop.Sample.Magic"))
            {
                Debug.Log($"<color=#f1c40f>[Scimitar]</color> <b>Magic Detected!</b> Submitting arbitration to add Ethereal Burn to Swing states.");

                arbitrator.SubmitArbitration(new IPackageArbitrator.Arbitration
                {
                    requestingPackage = this.PackageId,
                    targetedPackage = this.PackageId,
                    arbitrationType = IPackageArbitrator.ArbitrationType.Modification,
                    payload = "Add_Elemental_Damage_Buff"
                });
            }

            Debug.Log($"<color=#f1c40f>[Scimitar]</color> Arbitration Complete. Combat registries synchronized.");
        }

        /// <summary>
        /// Manifests the Scimitar FSM definition and registers it to the processing pipeline.
        /// </summary>
        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            if (!FSM_API.Interaction.Exists("Scimitar_FSM", "Combat_Logic"))
            {
                FSM_API.Create.CreateFiniteStateMachine("Scimitar_FSM", -1, "Combat_Logic")
                    .State("Sheathed", null, OnSheathedTick, null)
                    .State("Active_Swing", OnSwingEnter, OnSwingTick, OnSwingExit)
                    .State("Recovery", null, OnRecoveryTick, null)

                    .Transition("Sheathed", "Active_Swing", ctx => (ctx as ScimitarContext).IsTriggered)
                    .Transition("Active_Swing", "Recovery", ctx => (ctx as ScimitarContext).SwingProgress >= 1f)
                    .Transition("Recovery", "Sheathed", ctx => (ctx as ScimitarContext).RecoveryTime <= 0f)

                    .WithInitialState("Sheathed")
                    .BuildDefinition();
            }

            Debug.Log($"<color=#f1c40f>[Scimitar]</color> Tactical FSM loaded and registered to 'Combat_Logic'.");
        }

        // --- FSM LOGIC DELEGATES ---

        private static void OnSheathedTick(object context)
        {
            var ctx = context as ScimitarContext;
            ctx.IsTriggered = false;
        }

        private static void OnSwingEnter(object context)
        {
            var ctx = context as ScimitarContext;
            ctx.SwingProgress = 0f;
        }

        private static void OnSwingTick(object context)
        {
            var ctx = context as ScimitarContext;
            ctx.SwingProgress += Time.deltaTime * ctx.AttackSpeed;
        }

        private static void OnSwingExit(object context)
        {
            var ctx = context as ScimitarContext;
            ctx.RecoveryTime = 0.5f;
        }

        private static void OnRecoveryTick(object context)
        {
            var ctx = context as ScimitarContext;
            ctx.RecoveryTime -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Blittable-friendly context for Scimitar logic.
    /// </summary>
    public class ScimitarContext
    {
        public bool IsTriggered;
        public float SwingProgress;
        public float RecoveryTime;
        public float AttackSpeed = 2.0f;
    }
}