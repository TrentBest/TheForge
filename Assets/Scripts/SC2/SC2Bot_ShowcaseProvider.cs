using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.SC2
{
    public class SC2Bot_ShowcaseProvider : IGuiProvider
    {
        public string Title => "SC2 BOT SHOWCASE";
        private GuiContext _lastCtx;

        public SC2Bot_ShowcaseProvider() { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var router = new GuiFlowRouterBuilder("SC2Bot_Router", "Intro")
                .AddRoute("Intro", r => new SC2Bot_Gui_Intro(r))
                .AddRoute("Interactive", r => new SC2Bot_Playable_Alpha());

            return router.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }

    public class SC2Bot_Gui_Intro : IGuiProvider
    {
        public string Title => "SC2 BOT INTRO";
        private readonly IGuiRouter _router;

        public SC2Bot_Gui_Intro(IGuiRouter router) { _router = router; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("SC2Intro")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            rootBuilder.AddChild(new ForgeLabelBuilder("STARCRAFT 2: AI AGENT")
                .WithFontSize(32)
                .WithColor(new Color(0.8f, 0.1f, 0.8f)) // Singularity Purple
                .WithFontStyle(FontStyle.Bold)
                .WithMargin(0, 10));

            rootBuilder.AddChild(new ForgeLabelBuilder("EXTERNAL API BRIDGING & DECISION TREES")
                .WithFontSize(14)
                .WithColor(Color.cyan)
                .WithFontStyle(FontStyle.Italic)
                .WithMargin(0, 30));

            string explainer = "A demonstration of The Forge operating entirely headlessly. " +
                               "This module bridges the FSM API with an external game client, executing complex decision trees, build orders, and micro-management states. " +
                               "It proves the architecture can arbitrate high-frequency strategic logic independently of local Unity render loops.";

            rootBuilder.AddChild(new ForgeLabelBuilder(explainer)
                .WithFontSize(16)
                .WithColor(Color.silver)
                .WithWhiteSpace(WhiteSpace.Normal)
                .WithMargin(0, 50));

            rootBuilder.AddChild(new ForgeButtonBuilder("INITIALIZE AI PROTOCOLS", () => _router.NavigateTo("Interactive"))
                .WithBackgroundColor(new Color(0.4f, 0.05f, 0.4f))
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