using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Fractals
{
    public class MultibrotLoreProvider : IGuiProvider
    {
        public string Title => "THE MULTIBROT";
        private FractalForgeContext _ctx;

        public MultibrotLoreProvider(FractalForgeContext ctx) { _ctx = ctx; }

        public VisualElement CreateGui(GuiContext guiCtx)
        {
            _ctx.CurrentFormula = FractalFormula.Mandelbrot;
            _ctx.Power = 3.0f; // Force start at Z^3
            _ctx.ViewContext.IsDirty = true;

            var root = new ForgeContainerBuilder("MultibrotLore")
                .WithDirection(FlexDirection.Column).WithPadding(15)
                .AddChild(new ForgeLabelBuilder("HIGHER DIMENSIONS")
                    .WithColor(Color.cyan).WithFontSize(22).WithBold().WithMarginBottom(10))

                .AddChild(new ForgeLabelBuilder("A Multibrot set maps polynomials of a higher degree: Z = Z^d + C. As computational power increased, mathematicians mapped these higher exponents to see how the parameter space evolved.")
                    .WithColor(new Color(0.8f, 0.8f, 0.8f)).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(10))
                .AddChild(new ForgeLabelBuilder("The defining feature of a Multibrot is its rotational symmetry, which always equals (d-1). For Z^3, the central body bifurcates into two main lobes. As the power approaches infinity, the shape perfectly approaches a unit circle, yet its boundary becomes infinitely dense with chaotic fibrous structures.")
                    .WithColor(new Color(0.6f, 0.6f, 0.6f)).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(20))

                .AddChild(new ForgeLabelBuilder("MATHEMATICAL LABORATORY")
                    .WithColor(Color.green).WithBold().WithMarginBottom(10))

                .OnBuild(ve =>
                {
                    var powerLabel = new Label($"Exponent (Z^n): {_ctx.Power:F2}") { style = { color = Color.cyan } };
                    var powerSlider = new Slider(3.0f, 15.0f) { value = _ctx.Power };
                    powerSlider.RegisterValueChangedCallback(evt => {
                        _ctx.Power = evt.newValue;
                        powerLabel.text = $"Exponent (Z^n): {_ctx.Power:F2}";
                        _ctx.ViewContext.IsDirty = true;
                    });
                    ve.Add(powerLabel); ve.Add(powerSlider);
                });

            return root.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}