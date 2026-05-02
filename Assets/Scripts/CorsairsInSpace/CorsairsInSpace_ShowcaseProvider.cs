using Assets.Scripts.CorsairsInSpace.Data; // Keep your original domain data usings
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Corsair;
using Workshop.UI_And_Tools.Showcase;

namespace Assets.Scripts.CorsairsInSpace
{
    public class CorsairsInSpace_ShowcaseProvider : IShowcasePortal
    {
        // --- IGuiProvider Implementation ---
        public string Title => "Corsairs In Space";

        // --- IShowcasePortal Hooks (Auto-Discovery) ---
        public bool IsDiscoverable => true;
        public string Category => "PART IV: STRATEGIC ABSTRACTIONS";
        public string Overview => "Macro-tycoon base-building. Manages subterranean voxel excavation, minion swarms, and raiding operations using sequential experience routing.";
        public Color AccentColor => Color.red;
        public List<string> CoreTechnologies => new List<string> { "Voxel Grid", "Lair Architect", "Minion FSMs", "Cosmic Cells" };
        public string TelemetryState => "MANAGING PIRATE LAIR";

        public bool IsUnderConstruction => true;

        private IGuiRouter _parentRouter;
        private GuiContext _lastCtx;

        // 1. Required Parameterless Constructor for Reflection
        public CorsairsInSpace_ShowcaseProvider() { }

        // 2. Injected by the ExperienceRegistry before boot
        public void InjectRouter(IGuiRouter router)
        {
            _parentRouter = router;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            // Corsairs uses its own internal router to manage the sequence of screens!
            var internalRouter = new GuiFlowRouterBuilder("Corsairs_Router", "Intro")
                .AddRoute("Intro", r => new CorsairsInSpace_Gui_Intro(r))
                .AddRoute("Placement", r => new CorsairsInSpace_Gui_LairPlacement(r, new CosmicCellContext(7, 2048))) // Fringe Sector, Massive rock
                .AddRoute("Deployment", r => new CorsairsInSpace_Gui_Deployment(r))
                .AddRoute("Architect", r => new CorsairsInSpace_Gui_LairArchitect(r)); // The City Builder

            return internalRouter.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }

    public class CorsairsInSpace_Gui_Intro : IGuiProvider
    {
        public string Title => "CORSAIRS INTRO";
        private readonly IGuiRouter _localRouter;

        // Receives the internal GuiFlowRouterBuilder from the parent class
        public CorsairsInSpace_Gui_Intro(IGuiRouter router) { _localRouter = router; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("CorsairsIntro")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            rootBuilder.AddChild(new ForgeLabelBuilder("CORSAIRS IN SPACE")
                .WithFontSize(32)
                .WithColor(Color.red) // Pirate/Aggressive Accent
                .WithFontStyle(FontStyle.Bold)
                .WithMargin(0, 10));

            rootBuilder.AddChild(new ForgeLabelBuilder("MACRO-TYCOON & LAIR ARCHITECTURE")
                .WithFontSize(14)
                .WithColor(new Color(0.8f, 0.1f, 0.8f))
                .WithFontStyle(FontStyle.Italic)
                .WithMargin(0, 30));

            string explainer = "Applying 'Evil Genius' mechanics to a galactic scale. " +
                               "Corsairs In Space utilizes The Forge's WorldBuilding and Cartography toolsets to manage massive persistent domains. " +
                               "It showcases complex UI-to-Data binding, where Lair Architecture, pirate operations, and minion logistics are seamlessly arbitrated by localized FSMs.";

            rootBuilder.AddChild(new ForgeLabelBuilder(explainer)
                .WithFontSize(16)
                .WithColor(Color.silver)
                .WithWhiteSpace(WhiteSpace.Normal)
                .WithMargin(0, 50));

            // FIX: Updated "Interactive" to "Placement" to match your internal router table
            rootBuilder.AddChild(new ForgeButtonBuilder("ENTER CORSAIR LAIR", () => _localRouter.NavigateTo("Placement"))
                .WithBackgroundColor(new Color(0.5f, 0.1f, 0.1f))
                .WithTextColor(Color.white)
                .WithHeight(50)
                .WithWidth(300)
                .WithMargin(0, 0, 0, 0)
                .WithFontSize(16)
                .WithFontStyle(FontStyle.Bold));

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}