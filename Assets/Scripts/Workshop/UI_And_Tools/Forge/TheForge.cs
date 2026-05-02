using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using Workshop.Core; // Assuming ExperienceContext lives here or similar based on your architecture
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders;

namespace Workshop.UI_And_Tools.Forge
{
    /// <summary>
    /// The Pure C# Operating System for MetaDev.
    /// Logic for layout math, tool arbitration, and OS state transitions.
    /// </summary>
    public class TheForge : IStateContext
    {
        public string Name { get; set; } = "Forge_OS_Core";
        public bool IsValid { get; set; } = true;
        public FSMHandle Status { get; private set; }

        public Dictionary<string, IForgeBuilder> ToolingRegistry = new();
        public bool _isPrompting = false;
        private readonly IForgeFabricator _fabricator;
        private bool _isEstablished = false;
        private bool _needsReconfiguration = false;

        public TheForge(IForgeFabricator fabricator)
        {
            _fabricator = fabricator;

            ForgeLogger.Log("Forge OS Core Manifesting. Connecting Sovereign Fabricator.")
                .WithHeader("Forge OS")
                .WithColor("#00FFCC") // Neon Cyan
                .SendToUnity();

            InitializeFsm();
        }

        private void InitializeFsm()
        {
            if (!FSM_API.Interaction.Exists("TheForge_OS"))
            {
                ForgeLogger.Log("Defining Finite State Machine: TheForge_OS")
                    .WithHeader("FSM Engine")
                    .WithColor("#3399FF") // Sky Blue
                    .SendToUnity();

                FSM_API.Create.CreateFiniteStateMachine("TheForge_OS", -1, "Workshop")
                    // Converted to pure static delegates to prevent GC allocation
                    .State("Boot", OnBoot, null, null)
                    .State("Questionnaire", OnQuestionnaireEnter, null, null)
                    .State("Running", OnRunningEnter, null, OnRunningExit)
                    .State("Reconfigure", OnReconfigureEnter, null, null)

                    .Transition("Boot", "Questionnaire", ctx => ((TheForge)ctx).IsExperienceReady() && ((TheForge)ctx).IsGenesisMode())
                    .Transition("Boot", "Running", ctx => ((TheForge)ctx).IsExperienceReady() && !((TheForge)ctx).IsGenesisMode())
                    .Transition("Questionnaire", "Running", ctx => ((TheForge)ctx)._isEstablished)
                    .Transition("Running", "Reconfigure", ctx => ((TheForge)ctx)._needsReconfiguration)
                    .Transition("Reconfigure", "Running", ctx => !((TheForge)ctx)._needsReconfiguration)
                    .BuildDefinition();
            }

            Status = FSM_API.Create.CreateInstance("TheForge_OS", this, "Workshop");

            ForgeLogger.Log("FSM Instance linked. Forge OS is now state-aware and ticking.")
                .WithHeader("Forge OS")
                .WithColor("#00FFCC")
                .SendToUnity();
        }

        private bool IsExperienceReady() => ExperienceContext.Active != null && ExperienceContext.Active.IsValid;
        private bool IsGenesisMode() => ExperienceContext.Active?.Name == "Genesis_Workshop";

        // ==============================================================================
        // PURE STATIC FSM BEHAVIORS (Zero-Allocation)
        // ==============================================================================

        private static void OnBoot(IStateContext ctx)
        {
            var forge = (TheForge)ctx;

            ForgeLogger.Log("State Transition: [Boot]. Verifying Experience Context...")
                .WithHeader("OS State")
                .WithColor(Color.yellow)
                .SendToUnity();
            // Put the engine in gear! The constructor automatically binds to ExperienceContext.Active.
            if (ExperienceContext.Active == null)
            {
                new ExperienceContext("Genesis_Workshop", HostEnvironment.AnyApp_Forge);
            }
            // THE INJECTION: Explicitly log the variables gating the transition
            bool hasContext = ExperienceContext.Active != null;
            bool isValid = hasContext && ExperienceContext.Active.IsValid;
            string contextName = hasContext ? ExperienceContext.Active.Name : "NULL";

            ForgeLogger.Log($"Boot Gate Telemetry -> Context Found: {hasContext} | Context Valid: {isValid} | Target Experience: {contextName}")
                .WithHeader("Diagnostics")
                .WithColor("#FF8C00") // Dark Orange for high visibility
                .SendToUnity();

            if (!hasContext)
            {
                ForgeLogger.Log("WARNING: ExperienceContext.Active is NULL. FSM will idle in [Boot] until a context is anchored.")
                    .WithHeader("Diagnostics")
                    .WithColor(Color.red)
                    .SendToUnity();
            }
        }

        private static void OnQuestionnaireEnter(IStateContext ctx)
        {
            var forge = (TheForge)ctx;

            // THE SHIELD: Prevent the FSM from spamming the Fabricator every tick
            if (forge._isPrompting) return;
            forge._isPrompting = true;

            ForgeLogger.Log("State Transition: [Questionnaire]. Prompting for Genesis credentials.")
                .WithHeader("OS State")
                .WithColor("#FF00FF")
                .SendToUnity();

            var provider = new QuestionnaireProvider(() =>
            {
                forge._isEstablished = true;
                ForgeLogger.Log("Genesis Questionnaire complete. User context established.")
                    .WithHeader("Forge OS")
                    .WithColor(Color.green)
                    .SendToUnity();
            });

            forge._fabricator.ManifestQuestionnaire(provider);
        }

        private static void OnRunningEnter(IStateContext ctx)
        {
            var forge = (TheForge)ctx;

            ForgeLogger.Log($"State Transition: [Running]. Manifesting Workshop with {forge.ToolingRegistry.Count} registered tools.")
                .WithHeader("OS State")
                .WithColor("#FF00FF") // Magenta
                .SendToUnity();

            // Calculate and manifest the layout through the fabricator
            forge._fabricator.ManifestWorkshopLayout(forge.ToolingRegistry);
        }

        private static void OnRunningExit(IStateContext ctx)
        {
            var forge = (TheForge)ctx;

            ForgeLogger.Log("State Exit: [Running]. Commencing UI Teardown.")
                .WithHeader("OS State")
                .WithColor(Color.red)
                .SendToUnity();

            forge._fabricator.TeardownDisplays();
        }

        private static void OnReconfigureEnter(IStateContext ctx)
        {
            var forge = (TheForge)ctx;

            ForgeLogger.Log("State Transition: [Reconfigure]. Hot-swapping Tool Registry.")
                .WithHeader("OS State")
                .WithColor(Color.yellow)
                .SendToUnity();

            forge._fabricator.RefreshToolRegistry(forge.ToolingRegistry);
            forge._needsReconfiguration = false;
        }

        public void RequestReconfiguration()
        {
            ForgeLogger.Log("Dynamic Reconfiguration Request Captured.")
                .WithHeader("Forge OS")
                .WithColor(Color.yellow)
                .SendToUnity();

            _needsReconfiguration = true;
        }
    }

    /// <summary>
    /// Contract for the Unity-side "Fabricator" that manifests the Forge's logic.
    /// </summary>
    public interface IForgeFabricator
    {
        void ManifestQuestionnaire(QuestionnaireProvider provider);
        void ManifestWorkshopLayout(Dictionary<string, IForgeBuilder> tools);
        void TeardownDisplays();
        void RefreshToolRegistry(Dictionary<string, IForgeBuilder> registry);
    }
}