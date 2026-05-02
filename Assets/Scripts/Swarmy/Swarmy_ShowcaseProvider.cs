using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Swarmy
{
    public class Swarmy_ShowcaseProvider : IGuiProvider
    {
        public string Title => "SWARMY SHOWCASE";
        private GuiContext _lastCtx;

        public Swarmy_ShowcaseProvider() { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var router = new GuiFlowRouterBuilder("Swarmy_Router", "Intro")
                .AddRoute("Intro", r => new Swarmy_Gui_Intro(r))
                // Pointing to the standardized Alpha Entry Point
                .AddRoute("Interactive", r => new Swarmy_Playable_Alpha());

            return router.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }

    public class Swarmy_Gui_Intro : IGuiProvider
    {
        public string Title => "SWARMY INTRO";
        private readonly IGuiRouter _router;

        public Swarmy_Gui_Intro(IGuiRouter router) { _router = router; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("SwarmyIntro")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            rootBuilder.AddChild(new ForgeLabelBuilder("SWARMY: DRONE KINEMATICS")
                .WithFontSize(32)
                .WithColor(new Color(0.2f, 0.8f, 0.2f))
                .WithFontStyle(FontStyle.Bold)
                .WithMargin(0, 10));

            rootBuilder.AddChild(new ForgeLabelBuilder("ASYMMETRIC THRUST & TARGET TRACKING")
                .WithFontSize(14)
                .WithColor(new Color(0.8f, 0.1f, 0.8f))
                .WithFontStyle(FontStyle.Italic)
                .WithMargin(0, 30));

            string explainer = "Swarmy demonstrates mid-level agent logic focused heavily on physics vectors and threat assessment. " +
                               "The DroneControllers evaluate Foe, Friendly, and NotDetermined targets dynamically. The simulation relies on The Forge to parse continuous motor thrust data into smooth, emergent flocking and attack behaviors.";

            rootBuilder.AddChild(new ForgeLabelBuilder(explainer)
                .WithFontSize(16)
                .WithColor(Color.silver)
                .WithWhiteSpace(WhiteSpace.Normal)
                .WithMargin(0, 50));

            rootBuilder.AddChild(new ForgeButtonBuilder("INITIALIZE THRUST VECTORS", () => _router.NavigateTo("Interactive"))
                .WithBackgroundColor(new Color(0.1f, 0.5f, 0.1f))
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