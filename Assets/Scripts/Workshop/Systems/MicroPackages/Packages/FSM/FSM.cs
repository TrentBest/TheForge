using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_API.Scripts;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace TheSingularityWorkshop.MicroPackages.Packages.FSM
{
    /// <summary>
    /// The FSM Micro-Package: The Orchestrator of Simulation Chronos.
    /// Bridges Unity's Lifecycle with the Forge's Processing Groups.
    /// </summary>
    public class FSMPackage : IMicroPackage, IGuiProvider
    {
        // --- Core Identity ---
        public string PackageId { get; set; } = "Workshop.Core.FSM";

        // --- Architectural Intent ---
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // Core logic arrives as pure architectural data
        public string DeliveryProfileId => "Delivery_DigitalDataStream";

        // Foundational time management has no dependencies
        public string[] GetDependencies() => new string[0];

        // Plays nicely with everyone
        public string[] GetExclusions() => new string[0];

        // --- IMICROPACKAGE IMPLEMENTATION ---

        /// <summary>
        /// Defines the standard execution pipeline for the entire Workshop.
        /// This is the "God Mode" schedule for the universe.
        /// </summary>
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>
        {
            { "Update", new List<string> { "Logic_Early", "UI_Processing", "Input_Buffer" } },
            { "FixedUpdate", new List<string> { "Physics_Kinematics", "AI_Movement", "Collision_Resolution" } },
            { "LateUpdate", new List<string> { "Logic_Late", "Network_Outbound", "Memory_Cleanup" } }
        };

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            // Dependency Check: Ensure the DataWarehouse is online before we try to load FSM definitions
            if (arbitrator.InstalledPackages.Contains("DataWarehouse"))
            {
                Debug.Log("[FSM_Package] Arbitration Successful: Logic pipelines synchronized with DataWarehouse.");
            }
        }

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log("[FSM_Package] Initializing Finite State Machine Clusters...");

            // Registering the core processing groups with the Advanced Integration Engine
            foreach (var mapping in ProcessGroupsPerUnityMessage)
            {
                foreach (var group in mapping.Value)
                {
                    // This creates the logic buckets that your Freighters and Colonies subscribe to
                    FSM_UnityIntegrationAdvanced.Instance.AddProcessingGroup(mapping.Key, group);
                }
            }
        }

        // --- IGUIPROVIDER IMPLEMENTATION (Forge Monitoring Dashboard) ---

        public string Title => "FSM CLUSTER MONITOR";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("FSM_Monitor_Root")
                .WithFlexGrow(1f)
                .WithPadding(20f)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.07f)); // Void Black

            // Header - The Simulation Clockwork
            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithBorderColor(new Color(0.8f, 0.2f, 0.2f)) // Core Red
                .WithBorderWidth(0, 0, 3f, 0)
                .WithMarginBottom(15f)
                .AddChild(new ForgeLabelBuilder("FSM QUANTUM HEARTBEAT")
                    .WithFontSize(22).WithBold().WithColor(new Color(1.0f, 0.4f, 0.4f)))
                .AddChild(new ForgeLabelBuilder("Active Processing Groups & Lifecycle Mapping")
                    .WithFontSize(10).WithColor(Color.gray))
            );

            // Mapping Visualization
            var body = new ForgeContainerBuilder("ProcessGrid").WithDirection(FlexDirection.Row).WithFlexGrow(1f);

            foreach (var message in ProcessGroupsPerUnityMessage)
            {
                var col = new ForgeContainerBuilder($"{message.Key}_Col")
                    .WithFlexGrow(1f)
                    .WithMargin(5)
                    .WithPadding(10f)
                    .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                    .WithBorderRadius(5f);

                col.AddChild(new ForgeLabelBuilder(message.Key.ToUpper())
                    .WithBold().WithColor(Color.cyan).WithMarginBottom(10f));

                foreach (var group in message.Value)
                {
                    col.AddChild(new ForgeContainerBuilder($"Group_{group}")
                        .WithDirection(FlexDirection.Row)
                        .WithJustifyContent(Justify.SpaceBetween)
                        .WithMarginBottom(5f)
                        .AddChild(new ForgeLabelBuilder($"• {group}").WithFontSize(11))
                        .AddChild(new ForgeLabelBuilder("ACTIVE").WithFontSize(9).WithColor(Color.green))
                    );
                }

                body.AddChild(col);
            }

            rootBuilder.AddChild(body);

            // Action Bar
            rootBuilder.AddChild(new ForgeContainerBuilder("Actions")
                .WithMarginTop(15f)
                .WithDirection(FlexDirection.Row)
                .AddChild(new ForgeButtonBuilder("♻️ REBOOT PIPELINES")
                    .WithBackgroundColor(new Color(0.3f, 0.1f, 0.1f))
                    .OnClick(() => LoadPackage(null)))
            );

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(new GuiContext());
            WorkshopUxmlBaker.Bake(root, "FSM_Package_Monitor");
        }

        public void FromUIDocument(string assetPath) { }
    }
}