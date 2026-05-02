// File: Assets/Scripts/Workshop/Core/ExperienceContext.cs
using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.Core
{
    public enum HostEnvironment
    {
        AnyApp_Forge,
        MyVR_Showcase,
        Production
    }

    /// <summary>
    /// The User-Space context. Defined dynamically by the loaded Micro-Packages.
    /// Acts as the FSM substrate for both Forge OS and Live Experiences.
    /// </summary>
    public class ExperienceContext : IStateContext, IDisposable
    {
        // --- GLOBAL SOVEREIGN ACCESS ---
        // THE FIX: This is the anchor the Forge FSM checks during [Boot]
        public static ExperienceContext Active { get; set; }

        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "Active_Experience_Context";
        public HostEnvironment CurrentHost { get; set; }

        public AnyAppContext Kernel { get; private set; }

        // --- FSM FLOW GATES (For ExperienceEngine) ---
        public bool IsLoaded { get; set; } = false;
        public bool IsInitialized { get; set; } = false;
        public bool ShouldShutdown { get; set; } = false;

        // Micro-Packages inject their custom state variables into this dictionary
        public Dictionary<string, object> DynamicState { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Constructor for the Forge OS / Authoring Context
        /// </summary>
        public ExperienceContext(string name, HostEnvironment host)
        {
            Name = name;
            CurrentHost = host;
            Active = this; // Anchor to reality!
        }

        /// <summary>
        /// Constructor for Live Experiences requiring the full Kernel
        /// </summary>
        public ExperienceContext(AnyAppContext kernel, string name = "Live_Experience", HostEnvironment host = HostEnvironment.Production)
        {
            Kernel = kernel;
            Name = name;
            CurrentHost = host;
            Active = this; // Anchor to reality!
        }

        public void Dispose()
        {
            IsValid = false;
            if (Active == this) Active = null;
        }
    }
}