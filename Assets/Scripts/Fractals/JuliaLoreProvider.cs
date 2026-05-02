using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Fractals
{
    public class JuliaLoreProvider : IGuiProvider
    {
        public string Title => "THE JULIA SETS";
        private FractalForgeContext _ctx;

        public JuliaLoreProvider(FractalForgeContext ctx) { _ctx = ctx; }

        public VisualElement CreateGui(GuiContext guiCtx)
        {
            _ctx.CurrentFormula = FractalFormula.Julia;
            _ctx.Power = 2.0f; // Standard Julia is Z^2
            _ctx.ViewContext.IsDirty = true;

            var root = new ForgeContainerBuilder("JuliaLore")
                .WithDirection(FlexDirection.Column).WithPadding(15)
                .AddChild(new ForgeLabelBuilder("THE PRISONERS AND THE ESCAPEES")
                    .WithColor(Color.cyan).WithFontSize(22).WithBold().WithMarginBottom(10))

                .AddChild(new ForgeLabelBuilder("Pioneered by Gaston Julia and Pierre Fatou in 1918, decades before computers existed to visualize them. While the Mandelbrot set maps the parameter 'C', a Julia set maps the actual complex plane 'Z' for one single, fixed value of 'C'.")
                    .WithColor(new Color(0.8f, 0.8f, 0.8f)).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(10))
                .AddChild(new ForgeLabelBuilder("Fatou-Julia Theorem: If the chosen 'C' value lies inside the Mandelbrot set, the resulting Julia set is completely connected (a solid crystalline structure). If 'C' lies outside, the set explodes into infinite disconnected points, known as Fatou Dust.")
                    .WithColor(new Color(0.6f, 0.6f, 0.6f)).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(20))

                .AddChild(new ForgeLabelBuilder("MATHEMATICAL LABORATORY")
                    .WithColor(Color.green).WithBold().WithMarginBottom(10))

                .OnBuild(ve =>
                {
                    // Parameter: Real C
                    var realLabel = new Label($"Real Constant (C.x): {_ctx.JuliaConstant.x:F4}") { style = { color = Color.cyan } };
                    var realSlider = new Slider(-2.0f, 2.0f) { value = _ctx.JuliaConstant.x };
                    realSlider.RegisterValueChangedCallback(evt => {
                        _ctx.JuliaConstant.x = evt.newValue;
                        realLabel.text = $"Real Constant (C.x): {_ctx.JuliaConstant.x:F4}";
                        _ctx.ViewContext.IsDirty = true;
                    });
                    ve.Add(realLabel); ve.Add(realSlider);

                    // Parameter: Imaginary C
                    var imagLabel = new Label($"Imaginary Constant (C.y): {_ctx.JuliaConstant.y:F4}") { style = { color = Color.cyan, marginTop = 10 } };
                    var imagSlider = new Slider(-1.5f, 1.5f) { value = _ctx.JuliaConstant.y };
                    imagSlider.RegisterValueChangedCallback(evt => {
                        _ctx.JuliaConstant.y = evt.newValue;
                        imagLabel.text = $"Imaginary Constant (C.y): {_ctx.JuliaConstant.y:F4}";
                        _ctx.ViewContext.IsDirty = true;
                    });
                    ve.Add(imagLabel); ve.Add(imagSlider);
                });

            return root.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}