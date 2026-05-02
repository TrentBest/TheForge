using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.SolarSystemExplorer
{
    public class SolarSystem_ShowcaseProvider : IGuiProvider
    {
        public string Title => "SOLAR SYSTEM SHOWCASE";
        private GuiContext _lastCtx;

        public SolarSystem_ShowcaseProvider() { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var router = new GuiFlowRouterBuilder("SolarSystem_Router", "Intro")
                .AddRoute("Intro", r => new SolarSystem_Gui_Intro(r))
                // Pointing to the standardized Alpha Entry Point
                .AddRoute("Interactive", r => new SolarSystem_Playable_Alpha());

            return router.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }

    public class SolarSystem_Gui_Intro : IGuiProvider
    {
        public string Title => "SOLAR SYSTEM INTRO";
        private readonly IGuiRouter _router;

        public SolarSystem_Gui_Intro(IGuiRouter router) { _router = router; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("SolarIntro")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            rootBuilder.AddChild(new ForgeLabelBuilder("SOLAR SYSTEM EXPLORER")
                .WithFontSize(32)
                .WithColor(Color.yellow)
                .WithFontStyle(FontStyle.Bold)
                .WithMargin(0, 10));

            rootBuilder.AddChild(new ForgeLabelBuilder("ORBITAL MECHANICS & MACRO-SCALE FLOATING POINT")
                .WithFontSize(14)
                .WithColor(new Color(0.8f, 0.1f, 0.8f))
                .WithFontStyle(FontStyle.Italic)
                .WithMargin(0, 30));

            string explainer = "Standard game engines break down at cosmic scales due to floating-point imprecision. " +
                               "This showcase demonstrates The Forge's custom AstroInt architecture and WorldBuilding context. " +
                               "By decoupling mathematical coordinates from local rendering space, we can simulate massive planetary bodies, true orbital mechanics, and extreme parallax without jitter.";

            rootBuilder.AddChild(new ForgeLabelBuilder(explainer)
                .WithFontSize(16)
                .WithColor(Color.silver)
                .WithWhiteSpace(WhiteSpace.Normal)
                .WithMargin(0, 50));

            rootBuilder.AddChild(new ForgeButtonBuilder("INITIALIZE ASTRO-METRICS", () => _router.NavigateTo("Interactive"))
                .WithBackgroundColor(new Color(0.6f, 0.6f, 0.0f))
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