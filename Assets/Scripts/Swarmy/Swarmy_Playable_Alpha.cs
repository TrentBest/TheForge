using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Swarmy
{
    // ====================================================================
    // STANDARDIZED ALPHA ENTRY POINT (Stub for compilation)
    // ====================================================================
    public class Swarmy_Playable_Alpha : IGuiProvider
    {
        public string Title => "Swarmy Alpha";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("SwarmyAlphaStub")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            rootBuilder.AddChild(new ForgeLabelBuilder("DRONE SWARM ALPHA STANDING BY...")
                .WithColor(new Color(0.2f, 0.8f, 0.2f)) // Tactical Green Accent
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