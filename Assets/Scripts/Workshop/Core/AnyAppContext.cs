// File: Assets/Scripts/Workshop/Core/AnyAppContext.cs
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using Workshop.Core.Memory;
using Workshop.Core.IO;
using Workshop.Core.Rendering;

namespace Workshop.Core
{
    public class AnyAppContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "Sovereign_Kernel_Context";

        public DataWarehouse Warehouse { get; private set; }
        public SingularityRenderPipeline Pipeline { get; private set; }

        public bool AreCorePackagesLoaded { get; set; } = false;
        public IManifest ActiveManifest { get; set; }

        public AnyAppContext(DataWarehouse warehouse, SingularityRenderPipeline pipeline)
        {
            // Null coalesce to prevent catastrophic failure if Pipeline isn't ready
            Warehouse = warehouse ?? new DataWarehouse();
            Pipeline = pipeline;
        }

        public ExperienceContext CreateExperienceContext()
        {
            return new ExperienceContext(this);
        }

        public void SetPipeline(SingularityRenderPipeline newPipeline)
        {
            if (newPipeline == null)
            {
                Diagnostics.ForgeLogger.LogWarning("SetPipeline called with a null pipeline.")
                    .WithHeader("Context").SendToUnity();
                return;
            }

            // If it's the exact same object reference, do nothing
            if (ReferenceEquals(Pipeline, newPipeline)) return;

            // 1. Properly shut down the old pipeline if it exists
            if (Pipeline != null)
            {
                Diagnostics.ForgeLogger.Log("Shutting down previous SingularityRenderPipeline to prepare for handoff.")
                    .WithHeader("Context").WithColor(UnityEngine.Color.yellow).SendToUnity();

                // If you added a specific 'Shutdown' or 'ClearPasses' method to your pipeline, call it here.
                // Pipeline.Dispose() is protected, so you might need a public wrapper if you want to force disposal.
                // For now, we just drop the reference so GC can claim it.
            }

            // 2. Assign the new pipeline
            Pipeline = newPipeline;

            Diagnostics.ForgeLogger.Log("New SingularityRenderPipeline successfully anchored to Sovereign Context.")
                .WithHeader("Context").WithColor(UnityEngine.Color.cyan).SendToUnity();

            // 3. IMPORTANT: If Core Packages were already loaded on the OLD pipeline, 
            // we MUST re-inject them into the NEW pipeline!
            if (AreCorePackagesLoaded)
            {
                Diagnostics.ForgeLogger.LogWarning("Pipeline swapped AFTER core packages loaded. Re-injecting passes...")
                    .WithHeader("Context").SendToUnity();

                // You will need to move InjectSplashPass into a place where AnyAppContext can reach it,
                // OR reset the flag so the Boot FSM re-runs the Init_Packages state.

                // For now, the safest route is to force the Bootloader to re-evaluate:
                AreCorePackagesLoaded = false;
            }
        }
    }

 
    
}