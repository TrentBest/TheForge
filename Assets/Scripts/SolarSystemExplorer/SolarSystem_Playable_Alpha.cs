using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.SolarSystemExplorer
{
    // ====================================================================
    // STANDARDIZED ALPHA ENTRY POINT (Stub for compilation)
    // ====================================================================
    public class SolarSystem_Playable_Alpha : IGuiProvider
    {
        public string Title => "Solar System Alpha";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("SolarSystemAlphaStub")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            rootBuilder.AddChild(new ForgeLabelBuilder("SOLAR SYSTEM ALPHA STANDING BY...")
                .WithColor(Color.yellow) // Cosmic Accent
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