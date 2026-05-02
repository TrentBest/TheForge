using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.SC2
{
    // ====================================================================
    // STANDARDIZED ALPHA ENTRY POINT
    // ====================================================================
    public class SC2Bot_Playable_Alpha : IGuiProvider
    {
        public string Title => "SC2 Alpha";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("SC2BotAlphaStub")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            rootBuilder.AddChild(new ForgeLabelBuilder("SC2 BOT ALPHA STANDING BY...")
                .WithColor(new Color(0.8f, 0.1f, 0.8f)) // Singularity Purple
                .WithFontSize(24)
                .WithFontStyle(FontStyle.Bold)
                .WithMargin(40, 40));

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}