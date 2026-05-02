using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Hermit.UI
{
    public class AI_GeneratedHermitLayout : IGuiProvider
    {
        public string Title => "Generated Hermit Layout";

        public VisualElement CreateGui(GuiContext ctx)
        {
            // --- Parent Layout Builder ---
            var rootBuilder = new GraphicalUserInterfaceBuilder("GeneratedRootLayout")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithWrap(Wrap.Wrap) // CORRECTED: Moved Wrap to its own dedicated method
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f))
                .WithBorderWidth(2)
                .WithBorderRadius(8)
                .WithBorderColor(new Color(0.00f, 1.00f, 1.00f)); // Cyan

            // --- 1. LMPB Placeholder ---
            rootBuilder.AddChild(new GraphicalUserInterfaceBuilder("LMPB_Placeholder")
                .WithWidth(1250)
                .WithHeight(450)
                .WithFlexGrow(1)
                .WithFlexShrink(1)
                .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f)) // Slightly lighter so you can see the box
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .AddChild(new ForgeLabelBuilder("[ LMPB PLACEHOLDER ]").WithColor(Color.gray))
            );

            // --- 2. Hermit Controls Placeholder ---
            rootBuilder.AddChild(new GraphicalUserInterfaceBuilder("HermitControls_Placeholder")
                // Width/Height Auto is default, so we just set flex behaviors
                .WithFlexGrow(1)
                .WithFlexShrink(1)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.17f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .AddChild(new ForgeLabelBuilder("[ HERMIT CONTROLS PLACEHOLDER ]").WithColor(Color.gray))
            );

            return rootBuilder.Build();
        }

        // --- IGuiProvider Requirements ---
        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
    }
}