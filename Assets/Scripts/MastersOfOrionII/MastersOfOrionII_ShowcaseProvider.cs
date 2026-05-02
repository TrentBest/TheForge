using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_ShowcaseProvider : IGuiProvider
    {
        public string Title => "ARMADA 2525 SHOWCASE";
        private GuiContext _lastCtx;

        public MastersOfOrionII_ShowcaseProvider() { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var router = new GuiFlowRouterBuilder("Armada_Router", "Intro")
                .AddRoute("Intro", r => new MastersOfOrionII_Gui_Intro(r))
                // NOTE: Replace MastersOfOrionII_Gui_GridUniverse with your actual Playable Alpha entry if different!
                .AddRoute("Interactive", r => new MastersOfOrionII_Playable_Alpha());

            return router.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }

    public class MastersOfOrionII_Gui_Intro : IGuiProvider
    {
        public string Title => "ARMADA INTRO";
        private readonly IGuiRouter _router;

        public MastersOfOrionII_Gui_Intro(IGuiRouter router) { _router = router; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("ArmadaIntro")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            rootBuilder.AddChild(new ForgeLabelBuilder("ARMADA 2525")
                .WithFontSize(32)
                .WithColor(new Color(1f, 0.5f, 0f)) // Orange Accent
                .WithFontStyle(FontStyle.Bold)
                .WithMargin(0, 10));

            rootBuilder.AddChild(new ForgeLabelBuilder("GRAND STRATEGY & MACRO-STATE ABSTRACTION")
                .WithFontSize(14)
                .WithColor(new Color(0.8f, 0.1f, 0.8f))
                .WithFontStyle(FontStyle.Italic)
                .WithMargin(0, 30));

            string explainer = "A masterclass in managing macro-state data. Armada 2525 leverages The Forge's WorldBuilding architecture to map out a persistent, dynamic 4X galactic environment. " +
                               "The underlying FSM API ensures thousands of data points—from fleet kinematics to planetary economies and cosmic grid models—are arbitrated efficiently without bottlenecking the main UI thread.";

            rootBuilder.AddChild(new ForgeLabelBuilder(explainer)
                .WithFontSize(16)
                .WithColor(Color.silver)
                .WithWhiteSpace(WhiteSpace.Normal)
                .WithMargin(0, 50));

            rootBuilder.AddChild(new ForgeButtonBuilder("INITIALIZE GALACTIC GRID", () => _router.NavigateTo("Interactive"))
                .WithBackgroundColor(new Color(0.6f, 0.3f, 0.0f))
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