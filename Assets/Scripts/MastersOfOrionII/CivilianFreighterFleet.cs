using Assets.Scripts.Workshop.Core.Physics.Chemistry;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    /// <summary>
    /// The Global Logistics Context.
    /// Manages the data state for the civilian fleet, supply chain, and planetary demand.
    /// Reforged to follow the Experience Model: Data-heavy, logic-light.
    /// </summary>
    [Serializable]
    public class CivilianFreighterFleet : IStateContext, IGuiProvider
    {
        // --- ISTATECONTEXT IMPLEMENTATION ---
        public bool IsValid { get; set; } = false;
        public string Name { get; set; } = "Global_Civilian_Logistics";

        // --- DATA STATE ---
        public List<Freighter> Fleet { get; private set; } = new List<Freighter>();

        /// <summary> Order: Element -> Colony ID -> Quantity Available. </summary>
        public Dictionary<Element, Dictionary<int, int>> Supply { get; private set; } = new Dictionary<Element, Dictionary<int, int>>();

        /// <summary> Order: Element -> Colony ID -> Quantity Needed. </summary>
        public Dictionary<Element, Dictionary<int, int>> Demand { get; private set; } = new Dictionary<Element, Dictionary<int, int>>();

        public IFreighterDispatch Dispatcher { get; private set; }
        public List<Colony> Colonies { get; set; } = new List<Colony>();

        public CivilianFreighterFleet()
        {
            IsValid = true; // Signal health to the Forge Orchestrator
        }

        // --- IGUIPROVIDER IMPLEMENTATION (Logistics Matrix UI) ---

        public string Title => "CIVILIAN LOGISTICS MANIFEST";

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Root Container - High-Tech Logistics Aesthetic
            var rootBuilder = new ForgeContainerBuilder("LogisticsManifest_Root")
                .WithFlexGrow(1)
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.08f, 0.07f, 0.05f)); // Logistics Amber Tint

            // Header Section
            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithMarginBottom(20)
                .WithBorderColor(new Color(0.8f, 0.5f, 0.1f)) // Logistics Orange
                .WithBorderWidth(0, 0, 3, 0)
                .AddChild(new ForgeLabelBuilder("COLONIAL FREIGHTER FLEET")
                    .WithFontSize(22).WithFontStyle(FontStyle.Bold).WithColor(new Color(1.0f, 0.7f, 0.2f)))
                .AddChild(new ForgeLabelBuilder($"Managing {Fleet.Count} Active Vessels across {Colonies.Count} Interstellar Ports.")
                    .WithColor(Color.gray).WithFontSize(11))
            );

            // Split Layout: Fleet Status vs. Economic Matrix
            rootBuilder.AddChild(new ForgeContainerBuilder("Body")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithFlexGrow(1)
                // LEFT: Fleet Snapshot
                .AddChild(new ForgeContainerBuilder("FleetList")
                    .WithWidth(new StyleLength(Length.Percent(30)))
                    .WithPadding(10)
                    .WithBackgroundColor(new Color(0.12f, 0.11f, 0.1f))
                    .AddChild(new ForgeLabelBuilder("UNIT STATUS").WithBold().WithMarginBottom(10))
                    .OnBuild(ve => RenderFleetList(ve)))

                // RIGHT: Resource Matrix
                .AddChild(new ForgeContainerBuilder("ResourceMatrix")
                    .WithFlexGrow(1)
                    .WithMarginLeft(15)
                    .WithPadding(10)
                    .WithBackgroundColor(new Color(0.12f, 0.11f, 0.1f))
                    .AddChild(new ForgeLabelBuilder("SUPPLY & DEMAND MATRIX").WithBold().WithMarginBottom(10))
                    .OnBuild(ve => RenderEconomicMatrix(ve)))
            );

            return rootBuilder.Build();
        }

        private void RenderFleetList(VisualElement container)
        {
            foreach (var ship in Fleet)
            {
                var row = new ForgeContainerBuilder($"Ship_{ship.Id}")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithPadding(5)
                    .WithMarginBottom(5)
                    .WithBorderColor(new Color(0.3f, 0.3f, 0.3f))
                    .WithBorderWidth(0, 0, 1, 0)
                    .AddChild(new ForgeLabelBuilder(ship.Name).WithFontSize(10))
                    .AddChild(new ForgeLabelBuilder(ship.Status).WithColor(Color.cyan).WithFontSize(9));

                container.Add(row.Build());
            }
        }

        private void RenderEconomicMatrix(VisualElement container)
        {
            // Implementation of the "Logistics & Armory Pipeline" visualization
            foreach (var elementEntry in Supply)
            {
                var elementLabel = new ForgeLabelBuilder(elementEntry.Key.ToString())
                    .WithColor(new Color(0.4f, 0.8f, 1.0f))
                    .WithMarginTop(10);

                container.Add(elementLabel.Build());

                // Nest colony-specific data
                foreach (var colonyEntry in elementEntry.Value)
                {
                    var detailRow = new ForgeContainerBuilder($"Resource_{colonyEntry.Key}")
                        .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                        .AddChild(new ForgeLabelBuilder($"Colony #{colonyEntry.Key}").WithFontSize(9).WithColor(Color.gray))
                        .AddChild(new ForgeLabelBuilder($"{colonyEntry.Value} Units").WithFontSize(9).WithColor(Color.white));

                    container.Add(detailRow.Build());
                }
            }
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        /// <summary>
        /// Serializes the current logistics manifest to UXML for archival.
        /// </summary>
        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? "Fleet_Logistics_Manifest" : System.IO.Path.GetFileNameWithoutExtension(assetPath);
            WorkshopUxmlBaker.Bake(root, fileName);
        }

        public void FromUIDocument(string assetPath)
        {
            Debug.LogWarning("[FreighterFleet] FromUIDocument is bypassed. UI is live-linked to the Economic Context.");
        }
    }
}