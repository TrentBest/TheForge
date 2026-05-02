using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Assets.Editor.SpaceShip
{
    /// <summary>
    /// The Navigation Command Hub.
    /// Manages interstellar registries and stellar chart projections.
    /// Reforged to follow the Forge Protocol and the Experience Model.
    /// </summary>
    public class NavigationTab : IShipTabBuilder, IGuiProvider
    {
        public string TabName => "Navigation";
        public string TabIcon => "🗺️";
        public string Title => TabName;

        private float _currentInterceptDist = 0.00f;

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. Sidebar: The Stel-Map Registry
            var sidebar = new ForgeContainerBuilder("Destinations")
                .WithPadding(15f)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.07f))
                .AddChild(new ForgeLabelBuilder("STEL-MAP REGISTRY")
                    .WithFontSize(14).WithBold().WithColor(Color.cyan).WithMarginBottom(15f))

                .AddChild(new ForgeLabelBuilder("PLOT DESTINATION...").WithColor(Color.gray).WithFontSize(10))

                .AddChild(CreateNavButton("Sol System", 0.00f))
                .AddChild(CreateNavButton("Alpha Centauri", 4.37f))
                .AddChild(CreateNavButton("Canopus III", 310.0f))
                .AddChild(CreateNavButton("Andromeda Gateway", 2537000f));

            // 2. Main View: Astral Chart Projection
            var mainView = new ForgeContainerBuilder("AstralChart")
                .WithFlexGrow(1f)
                .WithBackgroundColor(Color.black)
                .AddChild(new ForgeContainerBuilder("ChartHeader")
                    .WithPadding(15f)
                    .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                    .OnBuild(ve => {
                        ve.style.borderBottomWidth = 1f;
                        ve.style.borderBottomColor = Color.cyan;
                    })
                    .AddChild(new ForgeLabelBuilder("LONG-RANGE STELLAR PROJECTION").WithBold().WithColor(Color.cyan))
                    .AddChild(new DynamicGuiProvider(c => new ForgeLabelBuilder($"Course Intercept: {_currentInterceptDist:F2} LY")
                        .WithColor(new Color(0f, 1f, 1f, 0.8f))
                        .WithFontSize(11)
                        .Build())))

                // Placeholder for the high-performance Orrery or Sector View
                .AddChild(new ForgeContainerBuilder("Viewport")
                    .WithFlexGrow(1f)
                    .WithAlignItems(Align.Center)
                    .WithJustifyContent(Justify.Center)
                    .AddChild(new ForgeLabelBuilder("[ SECTOR MAP ACTIVE ]")
                        .OnBuild(l => l.style.opacity = 0.3f)));

            // 3. Assembly via SplitPanel
            var root = new ForgeSplitPanelBuilder(250)
                .WithSidebar(sidebar)
                .WithMain(mainView)
                .CreateGui(ctx);

            // --- CRITICAL UI TICK PATTERN ---
            // Drive the navigation FSM locally for the astral chart parallax
            root.schedule.Execute(() => {
                // FSM_API.Interaction.Update("Navigation_Parallax");
                // _currentInterceptDist = LogicRegistry.GetIntercept(); 
            }).Every(16);

            return root;
        }

        private IGuiProvider CreateNavButton(string systemName, float dist)
        {
            return new ForgeButtonBuilder(systemName)
                .WithHeight(35f)
                .WithMarginBottom(5f)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                .OnClick(() => {
                    _currentInterceptDist = dist;
                    ForgeLogger.Log($"[Navigation] Plotting intercept to {systemName}...");
                });
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(new GuiContext());
            WorkshopUxmlBaker.Bake(root, "ShipNav_Tab_Snapshot");
        }

        public void FromUIDocument(string assetPath)
        {
            ForgeLogger.Log("[NavTab] Static UXML hydration is bypassed. Manifestation is procedurally driven.");
        }
    }
}