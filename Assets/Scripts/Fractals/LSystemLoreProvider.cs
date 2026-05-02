using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Fractals
{
    public class LSystemLoreProvider : IGuiProvider
    {
        public string Title => "L-SYSTEMS & MORPHOLOGY";
        private FractalForgeContext _ctx;

        public LSystemLoreProvider(FractalForgeContext ctx) { _ctx = ctx; }

        public VisualElement CreateGui(GuiContext guiCtx)
        {
            var root = new ForgeContainerBuilder("LSystemLore")
                .WithDirection(FlexDirection.Column).WithPadding(15)
                .AddChild(new ForgeLabelBuilder("ALGORITHMIC BOTANY")
                    .WithColor(new Color(0.2f, 0.8f, 0.2f)).WithFontSize(22).WithBold().WithMarginBottom(10)) // Green theme

                .AddChild(new ForgeLabelBuilder("Lindenmayer systems (L-Systems) use string-rewriting rules to model the growth processes of plant development. A simple axiom (like 'Draw Forward') is recursively replaced by a more complex rule (like 'Draw Forward, Branch Left, Branch Right').")
                    .WithColor(new Color(0.8f, 0.8f, 0.8f)).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(10))
                .AddChild(new ForgeLabelBuilder("This form of self-similarity is how nature efficiently encodes complex structures into minimal DNA. The exact same branching algorithms used to render a synthetic fern in computer graphics dictate the growth of human pulmonary vessels, Purkinje cells in the brain, and river networks.")
                    .WithColor(new Color(0.6f, 0.6f, 0.6f)).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(20))

                .AddChild(new ForgeLabelBuilder("MORPHOLOGY LABORATORY")
                    .WithColor(Color.green).WithBold().WithMarginBottom(10))

                .OnBuild(ve =>
                {
                    // Parameter: Branching Angle
                    var angleLabel = new Label($"Bifurcation Angle: {_ctx.Power * 10f:F1}°") { style = { color = Color.cyan } }; // Repurposing Power for angle
                    var angleSlider = new Slider(5.0f, 45.0f) { value = _ctx.Power * 10f };
                    angleSlider.RegisterValueChangedCallback(evt => {
                        _ctx.Power = evt.newValue / 10f;
                        angleLabel.text = $"Bifurcation Angle: {evt.newValue:F1}°";
                        _ctx.ViewContext.IsDirty = true;
                    });
                    ve.Add(angleLabel); ve.Add(angleSlider);
                });

            return root.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}