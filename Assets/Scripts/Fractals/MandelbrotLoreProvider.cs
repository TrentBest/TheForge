using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Fractals
{
    public class MandelbrotLoreProvider : IGuiProvider
    {
        public string Title => "THE MANDELBROT SET";
        private FractalForgeContext _ctx;

        public MandelbrotLoreProvider(FractalForgeContext ctx) { _ctx = ctx; }

        public VisualElement CreateGui(GuiContext guiCtx)
        {
            // Lock math context to Mandelbrot
            _ctx.CurrentFormula = FractalFormula.Mandelbrot;
            _ctx.ViewContext.IsDirty = true;

            var root = new ForgeContainerBuilder("MandelbrotLore")
                .WithDirection(FlexDirection.Column).WithPadding(15)
                .AddChild(new ForgeLabelBuilder("THE DICTIONARY OF CHAOS")
                    .WithColor(Color.cyan).WithFontSize(22).WithBold().WithMarginBottom(10))

                // Deep Lore
                .AddChild(new ForgeLabelBuilder("The Mandelbrot set is the set of complex numbers 'C' for which the function Z_{n+1} = Z_n^2 + C does not diverge when iterated from Z=0. It is famously described as the 'dictionary of all Julia sets' because the boundary of the Mandelbrot set catalogs the precise topologies of every possible Julia set.")
                    .WithColor(new Color(0.8f, 0.8f, 0.8f)).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(10))
                .AddChild(new ForgeLabelBuilder("Discovered by Benoit Mandelbrot in 1979 at IBM, it proved that infinite complexity could arise from a rule simple enough to be written on a napkin. The boundary of this set is a fractal curve with a Hausdorff dimension of exactly 2, meaning it is so impossibly rough that it effectively fills a 2D space.")
                    .WithColor(new Color(0.6f, 0.6f, 0.6f)).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(20))

                // Interactive Laboratory
                .AddChild(new ForgeLabelBuilder("MATHEMATICAL LABORATORY")
                    .WithColor(Color.green).WithBold().WithMarginBottom(10))
                .AddChild(new ForgeLabelBuilder("Polynomial Degree (Multibrot Morphing): As you increase the exponent above 2, the primary cardioid splits into (n-1) lobes.")
                    .WithColor(Color.white).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(5))

                .OnBuild(ve =>
                {
                    // Parameter: Z-Power (Multibrot exponent)
                    var powerLabel = new Label($"Exponent (Z^n): {_ctx.Power:F2}") { style = { color = Color.cyan } };
                    var powerSlider = new Slider(1.0f, 10.0f) { value = _ctx.Power };
                    powerSlider.RegisterValueChangedCallback(evt => {
                        _ctx.Power = evt.newValue;
                        powerLabel.text = $"Exponent (Z^n): {_ctx.Power:F2}";
                        _ctx.ViewContext.IsDirty = true;
                    });
                    ve.Add(powerLabel); ve.Add(powerSlider);

                    // Parameter: Fidelity (Iterations)
                    var iterLabel = new Label($"Escape Horizon (Max Iterations): {_ctx.MaxIterations}") { style = { color = Color.cyan, marginTop = 10 } };
                    var iterSlider = new SliderInt(10, 1500) { value = _ctx.MaxIterations };
                    iterSlider.RegisterValueChangedCallback(evt => {
                        _ctx.MaxIterations = evt.newValue;
                        iterLabel.text = $"Escape Horizon (Max Iterations): {_ctx.MaxIterations}";
                        _ctx.ViewContext.IsDirty = true;
                    });
                    ve.Add(iterLabel); ve.Add(iterSlider);
                });

            return root.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}