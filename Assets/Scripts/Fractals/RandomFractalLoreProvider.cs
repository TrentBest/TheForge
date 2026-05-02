using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Fractals
{
    public class RandomFractalLoreProvider : IGuiProvider
    {
        public string Title => "STOCHASTIC LANDSCAPES";
        private FractalForgeContext _ctx;

        public RandomFractalLoreProvider(FractalForgeContext ctx) { _ctx = ctx; }

        public VisualElement CreateGui(GuiContext guiCtx)
        {
            var root = new ForgeContainerBuilder("RandomFractalLore")
                .WithDirection(FlexDirection.Column).WithPadding(15)
                .AddChild(new ForgeLabelBuilder("THE ARCHITECTURE OF CHAOS")
                    .WithColor(new Color(0.6f, 0.4f, 0.2f)).WithFontSize(22).WithBold().WithMarginBottom(10)) // Earth tone theme

                .AddChild(new ForgeLabelBuilder("Not all fractals are rigidly deterministic. Random fractals use stochastic rules (like Brownian motion or Lévy flights) to generate highly irregular structures that mimic real-world phenomena.")
                    .WithColor(new Color(0.8f, 0.8f, 0.8f)).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(10))
                .AddChild(new ForgeLabelBuilder("In the 1980s, Loren Carpenter used fractional Brownian motion to render the first computer-generated landscapes. By recursively subdividing geometry and introducing random vertical displacement proportional to the scale of the subdivision, synthetic mountains and coastlines instantly mirrored the statistical self-similarity found in nature.")
                    .WithColor(new Color(0.6f, 0.6f, 0.6f)).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(20))

                .AddChild(new ForgeLabelBuilder("TERRAIN LABORATORY")
                    .WithColor(Color.green).WithBold().WithMarginBottom(10))

                .OnBuild(ve =>
                {
                    // Parameter: Hurst Exponent (Roughness)
                    var hurstLabel = new Label($"Hurst Exponent (Roughness): {_ctx.ColorStretch:F2}") { style = { color = Color.cyan } };
                    var hurstSlider = new Slider(0.1f, 1.0f) { value = _ctx.ColorStretch };
                    hurstSlider.RegisterValueChangedCallback(evt => {
                        _ctx.ColorStretch = evt.newValue;
                        hurstLabel.text = $"Hurst Exponent (Roughness): {_ctx.ColorStretch:F2}";
                        _ctx.ViewContext.IsDirty = true;
                    });
                    ve.Add(hurstLabel); ve.Add(hurstSlider);
                });

            return root.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}