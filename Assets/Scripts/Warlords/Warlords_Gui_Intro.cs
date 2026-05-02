using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Warlords
{
    public class Warlords_Gui_Intro : IGuiProvider
    {
        public string Title => "WARLORDS INTRO";
        private readonly IGuiRouter _router;

        public Warlords_Gui_Intro()
        {

        }

        public Warlords_Gui_Intro(IGuiRouter router) { _router = router; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("WarlordsIntro")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            rootBuilder.AddChild(new ForgeLabelBuilder("WARLORDS")
                .WithFontSize(32)
                .WithColor(new Color(0.6f, 0.4f, 0.2f)) // Earth/Tactical Accent
                .WithFontStyle(FontStyle.Bold)
                .WithMargin(0, 10, 0, 0));

            rootBuilder.AddChild(new ForgeLabelBuilder("PERSISTENT DOMAINS & TACTICAL RULESETS")
                .WithFontSize(14)
                .WithColor(new Color(0.8f, 0.1f, 0.8f))
                .WithFontStyle(FontStyle.Italic)
                .WithMargin(0, 30, 0, 0));

            string explainer = "Warlords explores persistent domain mapping and complex RPG arbitration. " +
                               "Utilizing The Forge's data-binding architecture, this module handles the rapid state transitions of combat mechanics, actor stats, and territory control. " +
                               "It seamlessly bridges heavy backend causality systems with front-end visual staging elements.";

            rootBuilder.AddChild(new ForgeLabelBuilder(explainer)
                .WithFontSize(16)
                .WithColor(Color.silver)
                .WithWhiteSpace(WhiteSpace.Normal)
                .WithMargin(0, 50, 0, 0));

            rootBuilder.AddChild(new ForgeButtonBuilder("ENTER TACTICAL DOMAIN", () => _router.NavigateTo("Interactive"))
                .WithBackgroundColor(new Color(0.4f, 0.25f, 0.1f))
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