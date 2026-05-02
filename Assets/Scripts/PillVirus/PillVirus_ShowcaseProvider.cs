using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.PillVirus
{
    public class PillVirus_ShowcaseProvider : IGuiProvider
    {
        public string Title => "PILL VIRUS SHOWCASE";
        private GuiContext _lastCtx;

        public PillVirus_ShowcaseProvider() { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var router = new GuiFlowRouterBuilder("PillVirus_Router", "Intro")
                .AddRoute("Intro", r => new PillVirus_Gui_Intro(r))
                .AddRoute("Interactive", r => new PillVirus_Playable_Alpha());

            return router.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }

    public class PillVirus_Gui_Intro : IGuiProvider
    {
        public string Title => "PILL VIRUS INTRO";
        private readonly IGuiRouter _router;

        public PillVirus_Gui_Intro(IGuiRouter router) { _router = router; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("PillIntro")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            rootBuilder.AddChild(new ForgeLabelBuilder("PILL VIRUS")
                .WithFontSize(32)
                .WithColor(new Color(1f, 0.4f, 0.8f)) // Arcade Pink Accent
                .WithFontStyle(FontStyle.Bold)
                .WithMargin(0, 10));

            rootBuilder.AddChild(new ForgeLabelBuilder("TIGHT KINEMATICS & CASCADING GRID LOGIC")
                .WithFontSize(14)
                .WithColor(new Color(0.8f, 0.1f, 0.8f))
                .WithFontStyle(FontStyle.Italic)
                .WithMargin(0, 30));

            string explainer = "A high-speed demonstration of The Forge's strict causality engine. " +
                               "Inspired by classic falling-block puzzles, PillVirus requires rapid, zero-latency state reactions. " +
                               "This showcase tests the FSM API's ability to execute pattern matching, gravity ticks, and cascading grid state resolutions within a microsecond budget.";

            rootBuilder.AddChild(new ForgeLabelBuilder(explainer)
                .WithFontSize(16)
                .WithColor(Color.silver)
                .WithWhiteSpace(WhiteSpace.Normal)
                .WithMargin(0, 50));

            rootBuilder.AddChild(new ForgeButtonBuilder("START ARCADE SEQUENCE", () => _router.NavigateTo("Interactive"))
                .WithBackgroundColor(new Color(0.6f, 0.1f, 0.4f))
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