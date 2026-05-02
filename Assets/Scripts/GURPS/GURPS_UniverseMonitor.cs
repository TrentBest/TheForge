using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.GURPS;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Assets.Scripts.raWWar.Gameplay; // Reference for TacticalCommander metrics
using TheSingularityWorkshop.FSM_API; // Reference for IStateContext and FSM instantiation

namespace Workshop.GURPS
{
    /// <summary>
    /// A high-level monitoring terminal that bridges macroscopic GPU simulation data 
    /// with granular GURPS mechanical resolution.
    /// </summary>
    public class GURPS_UniverseMonitor : IGuiProvider
    {
        // Adhering to IGuiProvider contract
        public string Title => "GURPS :: Universe Population Monitor";

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Pillar 8: Context-First Initialization
            if (ctx == null) ctx = new GuiContext();
            ComputeShader populationShader = null;
#if UNITY_EDITOR
            populationShader = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Shaders/Compute/GPU_Population_Sim.compute");
#endif
            // Pillar 1: UI built using Provider pattern with Fluent Builders
            var builder = new GraphicalUserInterfaceBuilder("Universe Statistics")
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.04f, 0.04f, 0.06f)) // Standard Forge Deep Space
                .AddChild(new ForgeLabelBuilder("Galactic Population Matrix")
                    .WithFontStyle(FontStyle.Bold)
                    .WithColor(Color.cyan)
                    .Build())

                // 2. Visualize the 3d6 Bell Curve for the selected "Class"
                // Utilizing the ProbabilityGraphBuilder from the GURPS Domain
                .AddChild(new ProbabilityGraphBuilder("Strength Distribution")
                    .WithMean(10.5f)
                    .WithStandardDeviation(2.5f)
                    .WithColor(Color.cyan)
                    .CreateGui(ctx))

                // 3. Use the ComputeShaderGuiBuilder to map GPU buffer counts to UI labels
                // Registry component for mapping compute parameters to UI
                .AddChild(new ComputeShaderGuiBuilder(populationShader)
                    .BindUniform("TotalEntities", $"Active: {TacticalCommander.ActiveTroopCount:N0}")
                    .BindUniform("ActiveProcessingTime", "Telemetry: Nominal")
                    .CreateGui(ctx))

                .AddSeparator(Color.gray, 1)

                // 4. Interaction: Collapse a statistical unit into a persistent GURPS Actor
                .AddChild(new ForgeButtonBuilder("Collapse NPC from Stats")
                    .WithHeight(40)
                    .WithBackgroundColor(new Color(0.65f, 0.2f, 0.95f)) // Singularity Purple
                    .OnClick(() => GenerateNPCFromStats())
                    .Build());

            return builder.Build();
        }

        // --- IGuiProvider Requirements ---

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }

        /// <summary>
        /// Implements the "Experience Model": Logic for JIT character creation.
        /// Data moves from unmanaged mass-simulation to individual FSM-driven logic.
        /// </summary>
        private void GenerateNPCFromStats()
        {
            // 1. Sample from the GPU Population Matrix (Forge Chronos Logic)
            // Accessing the unmanaged entity count from the TacticalCommander
            if (TacticalCommander.ActiveTroopCount <= 0)
            {
                Debug.LogWarning("[GURPS] Reality Collapse aborted: No entities found in GPU buffers.");
                return;
            }

            int targetIndex = UnityEngine.Random.Range(0, TacticalCommander.ActiveTroopCount);

            // 2. Instantiate a GURPS_ActorContext (Experience Chronos Logic)
            // Rule 2: Decouple Context (POCO) from FSM logic
            var actorContext = new GURPS_ActorContext
            {
                Name = $"Collapsed_Unit_{targetIndex:X4}",
                Strength = 10, // Base GURPS Mean
                IsValid = true
            };

            // 3. The NPC is now "Real" and possesses a Brain FSM.
            // Pillar 2: Assign FSM to a Processing Group (NPC_Brain_Ticking)
            var handle = FSM_API.Create.CreateInstance("NPC_Universal_Brain", actorContext, "GURPS_AI_Processing");

            Debug.Log($"[GURPS] Reality Collapse successful for {actorContext.Name}. FSM initialized.");
        }
    }

    /// <summary>
    /// Decoupled Context for the individual GURPS Actor.
    /// Implements IStateContext for the FSM Engine.
    /// </summary>
    public class GURPS_ActorContext : IStateContext
    {
        public string Name { get; set; } = "Unknown Unit";
        public bool IsValid { get; set; } = true;
        public int Strength = 10;
        public Vector3 Position = Vector3.zero;
    }
}