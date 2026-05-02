// File: Assets/Scripts/Workshop/ExperienceContext.cs
using System;
using Workshop.Core.Memory;
using TheSingularityWorkshop.FSM_API;

namespace Workshop
{
    /// <summary>
    /// The specific environment the Singularity is manifesting within.
    /// </summary>
    public enum HostEnvironment
    {
        AnyApp_Forge,
        MyVR_Showcase,
        Production
    }

    /// <summary>
    /// The Sovereign Memory container for an Experience.
    /// Pure C# to support high-performance, non-Mono behaviors while maintaining
    /// the static surface area required by the Render Pipeline and authoring tools.
    /// </summary>
    public class ExperienceContext : IStateContext, IDisposable
    {
        // --- GLOBAL SOVEREIGN ACCESS ---
        public static ExperienceContext Active { get; private set; }

        /// <summary>
        /// Global entry point for the SingularityRenderPipeline to access unmanaged memory.
        /// </summary>
        public static DataWarehouse MainWarehouse => Active?.Warehouse;

        // --- IDENTITY & CONTEXT ---
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsValid { get; set; } = true;
        public HostEnvironment CurrentHost { get; set; }
        public DataWarehouse Warehouse { get; private set; }

        // --- FSM FLOW GATES ---
        public bool IsLoaded { get; set; } = false;
        public bool IsInitialized { get; set; } = false;
        public bool ShouldShutdown { get; set; } = false;

        public ExperienceContext()
        {
            Warehouse = new DataWarehouse(); // Initializes the unmanaged memory registry
            Active = this;
        }

        public ExperienceContext(string name, HostEnvironment host = HostEnvironment.MyVR_Showcase) : this()
        {
            Name = name;
            CurrentHost = host;
        }

        /// <summary>
        /// Compatibility hook for legacy authoring providers (e.g., QuestionnaireProvider).
        /// Resolves CS1061 errors by allowing access to the context instance.
        /// </summary>
        public ExperienceContext GetCurrentExperience() => this;

        public void Dispose()
        {
            Warehouse?.Dispose(); // Safely releases unmanaged memory shelves
            IsValid = false;
            if (Active == this) Active = null;
        }
    }
}