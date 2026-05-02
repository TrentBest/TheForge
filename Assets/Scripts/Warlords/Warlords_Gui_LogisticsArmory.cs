using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Warlords
{
    public class Warlords_Gui_LogisticsArmory : IGuiProvider
    {
        private GuiContext _lastCtx;
        public string Title => "ROYAL ARMORY: LOGISTICS & PROCUREMENT";

        private int _soldierCount = 100;
        private float _basePay = 1.0f;
        private Label _totalCostLabel;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            var builder = new GraphicalUserInterfaceBuilder("Logistics_Root")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.03f));

            // --- LEFT: EQUIPMENT & TRAINING MANIFEST ---
            builder.AddChild(root =>
            {
                var left = new VisualElement { style = { width = 400, paddingTop = 20, paddingLeft = 20, paddingBottom = 20, paddingRight = 20, backgroundColor = new Color(0.05f, 0.05f, 0.07f) } };
                left.style.paddingTop = 20; left.style.paddingBottom = 20; left.style.paddingLeft = 20; left.style.paddingRight = 20;

                left.Add(new Label("PROCUREMENT") { style = { color = Color.cyan, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold } });

                // Scale Slider
                var scaleSlider = new SliderInt("Regiment Size", 10, 10000) { value = _soldierCount };
                scaleSlider.RegisterValueChangedCallback(evt => {
                    _soldierCount = evt.newValue;
                    UpdateCalculations();
                });
                left.Add(scaleSlider);

                // Training Level (GURPS Skill levels)
                left.Add(new Label("\nTRAINING LEVEL") { style = { color = Color.gray, fontSize = 10 } });
                var trainingList = new DropdownField(new System.Collections.Generic.List<string> { "Green (DX 10)", "Trained (DX 11)", "Veteran (DX 12)", "Elite (DX 13)" }, 0);
                left.Add(trainingList);

                // Equipment Selection
                left.Add(new Label("\nWEAPONRY") { style = { color = Color.gray, fontSize = 10 } });
                left.Add(new RadioButtonGroup("Weapon", new System.Collections.Generic.List<string> { "Pike (+0.2 GP)", "Halberd (+0.5 GP)", "Greatsword (+1.2 GP)" }));

                left.Add(new Label("\nARMOR") { style = { color = Color.gray, fontSize = 10 } });
                left.Add(new RadioButtonGroup("Armor", new System.Collections.Generic.List<string> { "None (+0 GP)", "Leather (+0.5 GP)", "Full Plate (+2.5 GP)" }));

                return left;
            });

            // --- RIGHT: THE FINANCIAL LEDGER ---
            builder.AddChild(root =>
            {
                var right = new VisualElement { style = { flexGrow = 1, paddingTop  = 40, paddingRight = 40, paddingBottom = 40, paddingLeft = 40, backgroundColor = new Color(0.08f, 0.08f, 0.08f) } };
                right.style.paddingTop = 40; right.style.paddingLeft = 40;

                right.Add(new Label("BATTLE GROUP PROJECTION") { style = { fontSize = 28, color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });

                // The Visual Unit Card (shows the single soldier's stats)
                var previewCard = new Warlords_UnitCard("Heavy Pikeman", 11, 11, 10, 11, "Pike", "Leather", 8, "1d+2 imp");
                right.Add(previewCard);

                // The Ledger
                var ledger = new VisualElement { style = { marginTop = 30, paddingTop = 20, paddingRight = 20, paddingBottom = 20, paddingLeft = 20, backgroundColor = Color.black } };
                ledger.style.paddingTop = 20; ledger.style.paddingBottom = 20; ledger.style.paddingLeft = 20; ledger.style.paddingRight = 20;

                _totalCostLabel = new Label("ESTIMATED UPKEEP: 0 GP / TURN") { style = { fontSize = 20, color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold } };
                ledger.Add(_totalCostLabel);

                ledger.Add(new Label("\nBREAKDOWN:") { style = { color = Color.gray } });
                ledger.Add(new Label("• Base Wages: $1.00 / soldier") { style = { color = Color.white } });
                ledger.Add(new Label("• Gear Maintenance: $0.70 / soldier") { style = { color = Color.white } });
                ledger.Add(new Label("• Combat Training: $0.30 / soldier") { style = { color = Color.white } });

                right.Add(ledger);

                UpdateCalculations();
                return right;
            });

            return builder.Build();
        }

        private void UpdateCalculations()
        {
            // Simple logic for the demo: 1 base + 0.7 gear + 0.3 training = 2.0 GP per soldier
            float perSoldier = 2.0f;
            float total = _soldierCount * perSoldier;
            _totalCostLabel.text = $"ESTIMATED UPKEEP: {total:N0} GP / TURN";
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}