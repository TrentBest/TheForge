using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.GURPS;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ForgeTailorRackBuilder : IGuiProvider
    {
        public string Title => "TAILOR CONTROLS";
        private readonly GURPS_EntityContext _ctx;
        private readonly Action _onChanged;

        public ForgeTailorRackBuilder(GURPS_EntityContext ctx, Action onChanged)
        {
            _ctx = ctx;
            _onChanged = onChanged;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("TailorControls")
                .WithPadding(15)
                .Build();

            // 1. ROTATION SLIDER (Y-Axis)
            var rotLabel = new Label("ROTATION (Y-AXIS)");
            rotLabel.style.fontSize = 10;
            rotLabel.style.color = Color.gray;
            root.Add(rotLabel);

            var slider = new Slider(0, 360);
            slider.value = _ctx.RotationY;
            slider.RegisterValueChangedCallback(evt => {
                _ctx.RotationY = evt.newValue;
                _onChanged?.Invoke(); // Trigger mirror update
            });
            root.Add(slider);

            // 2. RANDOMIZER
            var btn = new ForgeButtonBuilder("RE-MEASURE (RANDOMIZE)", () => {
                _ctx.RotationY = UnityEngine.Random.Range(0, 360);
                _onChanged?.Invoke();
            }).WithMarginTop(20);

            // Fix: UI Toolkit Style correction for IStyle shorthand errors
            var btnVe = btn.Build();
            btnVe.style.borderTopWidth = 1;
            btnVe.style.borderBottomWidth = 1;
            btnVe.style.borderLeftWidth = 1;
            btnVe.style.borderRightWidth = 1;
            btnVe.style.borderTopColor = Color.magenta;
            btnVe.style.borderBottomColor = Color.magenta;
            btnVe.style.borderLeftColor = Color.magenta;
            btnVe.style.borderRightColor = Color.magenta;

            root.Add(btnVe);
            return root;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}