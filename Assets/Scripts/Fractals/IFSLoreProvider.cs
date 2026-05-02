using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Fractals
{
    public class IFSLoreProvider : IGuiProvider
    {
        public string Title => "ITERATED FUNCTION SYSTEMS";
        private FractalForgeContext _ctx;

        public IFSLoreProvider(FractalForgeContext ctx) { _ctx = ctx; }

        public VisualElement CreateGui(GuiContext guiCtx)
        {
            var root = new ForgeContainerBuilder("IFSLore")
                .WithDirection(FlexDirection.Column).WithPadding(15)
                .AddChild(new ForgeLabelBuilder("THE GEOMETRY OF VOIDS")
                    .WithColor(Color.cyan).WithFontSize(22).WithBold().WithMarginBottom(10))

                .AddChild(new ForgeLabelBuilder("Pioneered by Wacław Sierpiński in 1915, Iterated Function Systems (IFS) rely on fixed geometric replacement rules rather than complex plane escape-time equations. The most famous example is the Sierpinski Carpet, created by dividing a square into 9 smaller squares and removing the central one, ad infinitum.")
                    .WithColor(new Color(0.8f, 0.8f, 0.8f)).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(10))
                .AddChild(new ForgeLabelBuilder("These structures perfectly illustrate fractional dimensions. The Sierpinski Carpet has a topological dimension of 1 (it contains no solid 2D areas), yet its Hausdorff dimension is approximately 1.8928, meaning it fills space much more densely than a simple line.")
                    .WithColor(new Color(0.6f, 0.6f, 0.6f)).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(20))

                .AddChild(new ForgeLabelBuilder("MATHEMATICAL LABORATORY")
                    .WithColor(Color.green).WithBold().WithMarginBottom(10))

                .OnBuild(ve =>
                {
                    // Parameter: Recursion Depth
                    var depthLabel = new Label($"Recursion Depth: {_ctx.MaxIterations}") { style = { color = Color.cyan } };
                    var depthSlider = new SliderInt(1, 8) { value = _ctx.MaxIterations }; // Keep low, IFS grows exponentially
                    depthSlider.RegisterValueChangedCallback(evt => {
                        _ctx.MaxIterations = evt.newValue;
                        depthLabel.text = $"Recursion Depth: {_ctx.MaxIterations}";
                        _ctx.ViewContext.IsDirty = true;
                    });
                    ve.Add(depthLabel); ve.Add(depthSlider);

                    // Parameter: Affine Scale
                    var scaleLabel = new Label($"Affine Void Scale: {_ctx.ColorStretch:F2}") { style = { color = Color.cyan, marginTop = 10 } };
                    var scaleSlider = new Slider(0.1f, 0.9f) { value = _ctx.ColorStretch };
                    scaleSlider.RegisterValueChangedCallback(evt => {
                        _ctx.ColorStretch = evt.newValue;
                        scaleLabel.text = $"Affine Void Scale: {_ctx.ColorStretch:F2}";
                        _ctx.ViewContext.IsDirty = true;
                    });
                    ve.Add(scaleLabel); ve.Add(scaleSlider);
                });

            return root.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}