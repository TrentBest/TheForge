using System;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using Workshop.Core.Diagnostics; // Required for ForgeLogger
using Workshop.Core.IO; // Required for IManifest

namespace Workshop
{
    /// <summary>
    /// The ticking heart of the Experience. 
    /// Manages the lifecycle FSM and orchestrates package loading with ZERO allocations.
    /// </summary>
    public class ExperienceEngine
    {
        private ExperienceContext _ctx;
        private FSMHandle _handle;

        /// <summary>
        /// Static entry point for the Autopoietic pathway.
        /// </summary>
        public static void Run(IManifest manifest)
        {
            ForgeLogger.Log($"Igniting Experience Engine from Manifest: {manifest.EntryTrajectory}")
                .WithHeader("Experience Engine")
                .WithColor("#00FF00") // Lime Green
                .SendToUnity();

            // Resolve the context from the manifest trajectory
            var context = new ExperienceContext(manifest.EntryTrajectory, HostEnvironment.Production);
            new ExperienceEngine(context);
        }

        public ExperienceEngine(ExperienceContext context)
        {
            _ctx = context;

            if (!FSM_API.Interaction.Exists("Experience_Lifecycle", "Update"))
            {
                ForgeLogger.Log("Defining Finite State Machine: Experience_Lifecycle")
                    .WithHeader("FSM Engine")
                    .WithColor("#3399FF") // Sky Blue
                    .SendToUnity();

                FSM_API.Create.CreateFiniteStateMachine("Experience_Lifecycle", -1, "Update")
                    // PURE STATIC REFERENCES - Zero Garbage Collection Allocation
                    .State("Loading", OnLoading, null, null)
                    .State("Initializing", OnInitializing, null, null)
                    .State("Acting", OnActingEnter, OnActingUpdate, null)
                    .State("Shutdown", OnShutdown, null, null)

                    .Transition("Loading", "Initializing", c => ((ExperienceContext)c).IsLoaded)
                    .Transition("Initializing", "Acting", c => ((ExperienceContext)c).IsInitialized)
                    .Transition("Acting", "Shutdown", c => ((ExperienceContext)c).ShouldShutdown)
                    .BuildDefinition();
            }

            _handle = FSM_API.Create.CreateInstance("Experience_Lifecycle", _ctx, "Update");
        }

        // ==============================================================================
        // PURE STATIC FSM BEHAVIORS (Zero-Allocation)
        // ==============================================================================

        private static void OnLoading(IStateContext context)
        {
            var ctx = context as ExperienceContext;

            ForgeLogger.Log($"Hydrating Warehouse for Experience: {ctx.Name}...")
                .WithHeader("Experience Engine")
                .WithColor(Color.yellow)
                .SendToUnity();

            // Logic to kick off the Arbitrator.BootFromManifest() would go here

            // Mark loaded to trigger transition to Initializing
            ctx.IsLoaded = true;
        }

        private static void OnInitializing(IStateContext context)
        {
            var ctx = context as ExperienceContext;

            ForgeLogger.Log("Data Warehouse Hydrated. Initializing Context constraints...")
                .WithHeader("Experience Engine")
                .WithColor(Color.yellow)
                .SendToUnity();

            ctx.IsInitialized = true;
        }

        private static void OnActingEnter(IStateContext context)
        {
            ForgeLogger.Log("Experience is LIVE. Entering [Acting] state.")
                .WithHeader("Experience Engine")
                .WithColor("#00FFCC") // Neon Cyan
                .SendToUnity();
        }

        private static void OnActingUpdate(IStateContext context)
        {
            // The simulation is actively ticking here.
            // Any high-frequency updates that govern the experience lifecycle go here.
        }

        private static void OnShutdown(IStateContext context)
        {
            ForgeLogger.Log("Shutdown Requested. Deconstructing Experience Context.")
                .WithHeader("Experience Engine")
                .WithColor(Color.red)
                .SendToUnity();

            (context as ExperienceContext)?.Dispose();
        }
    }
}