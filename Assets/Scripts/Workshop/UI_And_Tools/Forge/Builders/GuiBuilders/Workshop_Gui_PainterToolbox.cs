using System;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    public class Workshop_Gui_PainterToolbox : IGuiProvider
    {
        public string Title => "MATERIAL PAINTER";
        public Workshop_Gui_PainterToolbox()
        {

        }
        public UnityEngine.UIElements.VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("PainterToolbox")
                .WithFlexGrow(1)
                .WithFlexLayout(UnityEngine.UIElements.FlexDirection.Column);

            // --- SECTION 1: BRUSH DYNAMICS ---
            root.AddHeader("BRUSH DYNAMICS", Color.cyan);

            // We would map these to your Float/Slider builders
            root.AddChild(new ForgeLabelBuilder("Size (px)").WithColor(Color.gray).WithMarginTop(5));
            root.AddChild(new GraphicalUserInterfaceBuilder("MockSlider_Size").WithHeight(10).WithBackgroundColor(Color.gray).WithMarginBottom(10));

            root.AddChild(new ForgeLabelBuilder("Hardness").WithColor(Color.gray));
            root.AddChild(new GraphicalUserInterfaceBuilder("MockSlider_Hardness").WithHeight(10).WithBackgroundColor(Color.gray).WithMarginBottom(10));

            root.AddChild(new ForgeLabelBuilder("Opacity").WithColor(Color.gray));
            root.AddChild(new GraphicalUserInterfaceBuilder("MockSlider_Opacity").WithHeight(10).WithBackgroundColor(Color.gray).WithMarginBottom(15));

            root.AddSeparator(Color.cyan, 1);

            // --- SECTION 2: COLOR FORGE ---
            root.AddHeader("ACTIVE PIGMENT", new Color(0.8f, 0.1f, 0.8f));

            // Mocking the ColorForgePicker integration
            var colorPreview = new GraphicalUserInterfaceBuilder("ColorPreview")
                .WithHeight(40)
                .WithBackgroundColor(Color.red) // Active Color
                .WithBorderRadius(4)
                .WithMarginBottom(15)
                .AddChild(new ForgeLabelBuilder("#FF0000").WithColor(Color.white).WithBold(true).WithTextAlign(TextAnchor.MiddleCenter));

            root.AddChild(colorPreview);
            root.AddSeparator(new Color(0.8f, 0.1f, 0.8f), 1);

            // --- SECTION 3: TEXTURE LAYERS ---
            root.AddHeader("UV MAPPING CHANNELS", Color.yellow);

            root.AddChild(CreateChannelLayer("Albedo (Color)", true));
            root.AddChild(CreateChannelLayer("Emission (Glow)", false));
            root.AddChild(CreateChannelLayer("Metallic", false));

            return root.Build();
        }

        private IGuiProvider CreateChannelLayer(string name, bool isActive)
        {
            return new GraphicalUserInterfaceBuilder($"Layer_{name}")
                .WithFlexLayout(UnityEngine.UIElements.FlexDirection.Row, UnityEngine.UIElements.Justify.SpaceBetween, UnityEngine.UIElements.Align.Center)
                .WithBackgroundColor(isActive ? new Color(0.15f, 0.3f, 0.4f) : new Color(0.1f, 0.1f, 0.12f))
                .WithPadding(8)
                .WithMarginBottom(4)
                .WithBorderRadius(4)
                .AddChild(new ForgeLabelBuilder(name).WithColor(isActive ? Color.white : Color.gray).WithBold(isActive))
                .AddChild(new ForgeButtonBuilder("👁").WithBackgroundColor(Color.clear).WithTextColor(Color.gray));
        }

        public Action<UnityEngine.UIElements.VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}