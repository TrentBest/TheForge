using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Showcase;

namespace Assets.Scripts.Ants
{
    public partial class Ants_ShowcaseProvider : IShowcasePortal
    {
        // ====================================================================
        // IGuiProvider Implementation
        // ====================================================================
        public string Title => "Myrmecology: GPU Swarm Arbitration";

        // ====================================================================
        // IShowcasePortal Implementation
        // ====================================================================
        public bool IsDiscoverable => true;
        public bool IsUnderConstruction => false;
        public string Category => "PART III: AI & SWARM LOGIC";
        public string Overview => "A technical showcase for Package Arbitration and Data-Oriented Design (DOD). Simulates massive quantities of emergent micro-entities on the GPU via compute buffers, driven by environmental thresholds.";
        public Color AccentColor => new Color(1f, 0.4f, 0f);
        public List<string> CoreTechnologies => new List<string> { "Environmental Arbitration", "1D Memory Flattening", "Compute Shaders", "Elastic Scheduling" };
        public string TelemetryState => "ARBITRATING MICRO-PACKAGES";

        private IGuiRouter _globalForgeRouter;

        // Required Parameterless Constructor
        public Ants_ShowcaseProvider() { }

        public void InjectRouter(IGuiRouter router)
        {
            _globalForgeRouter = router;
        }

        // ====================================================================
        // GUI Construction & Local Routing
        // ====================================================================
        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. CREATE THE LOCALIZED SANDBOX
            var localAppRouter = new GuiFlowRouterBuilder("MyrmecologyAppRouter", "Intro");

            // 2. THE ROUTER BRIDGE
            var internalBridge = new MyrmecologyInternalRouter(localAppRouter, _globalForgeRouter);

            // 3. REGISTER THE MICRO-PACKAGE SCREENS
            localAppRouter.AddRoute("Intro", r => new Ants_Gui_Intro(internalBridge));
            localAppRouter.AddRoute("Interactive", r => new Ants_Playable_Alpha(internalBridge));

            // Wrap both dashboards so they have an escape hatch back to the Hub
            localAppRouter.AddRoute("AntFarmCA", r => BuildFarmWrapper(ctx, internalBridge));
            localAppRouter.AddRoute("SimAnt", r => BuildSimAntWrapper(ctx, internalBridge));

            // Initialize the router
            localAppRouter.Build();

            var ui = localAppRouter.CreateGui(ctx);
            ui.style.backgroundColor = new Color(0.05f, 0.05f, 0.06f);

            return ui;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }

        // ====================================================================
        // DYNAMIC UI WRAPPERS
        // ====================================================================

        private IGuiProvider BuildFarmWrapper(GuiContext ctx, IGuiRouter internalRouter)
        {
            return new GraphicalUserInterfaceBuilder("FarmWrapper")
                .WithFlexGrow(1)
                .AddChild(new ForgeButtonBuilder("<- SUSPEND CA SIMULATION (RETURN TO HUB)", () => internalRouter.NavigateTo("Interactive"))
                    .WithBackgroundColor(new Color(0.8f, 0.2f, 0.2f))
                    .WithTextColor(Color.white)
                    .WithHeight(30)
                    .WithFontStyle(FontStyle.Bold)
                    .Build()
                )
                .AddChild(new Ants_FarmDashboard());
        }

        private IGuiProvider BuildSimAntWrapper(GuiContext ctx, IGuiRouter internalRouter)
        {
            return new GraphicalUserInterfaceBuilder("SimAntWrapper")
                .WithFlexGrow(1)
                .AddChild(new ForgeButtonBuilder("<- SUSPEND MACRO SIMULATION (RETURN TO HUB)", () => internalRouter.NavigateTo("Interactive"))
                    .WithBackgroundColor(new Color(0.8f, 0.2f, 0.2f))
                    .WithTextColor(Color.white)
                    .WithHeight(30)
                    .WithFontStyle(FontStyle.Bold)
                    .Build()
                )
                .AddChild(new Ants_SimAnt_Dashboard());
        }
    }
}