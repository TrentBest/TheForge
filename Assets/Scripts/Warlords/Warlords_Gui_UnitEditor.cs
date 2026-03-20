using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Warlords
{
    public class Warlords_Gui_UnitEditor : IGuiProvider
    {
        public string Title => "WARLORDS: UNIT ARCHITECT";
        private GuiContext _lastCtx;

        public class UnitTemplate
        {
            public string Name { get; set; } = "New Unit";
            public int ST { get; set; } = 10;
            public int DX { get; set; } = 10;
            public int IQ { get; set; } = 10;
            public int HT { get; set; } = 10;
            public int Cost { get; set; } = 10;
            public int Turns { get; set; } = 2;
            public string Description { get; set; } = "A basic warrior.";
        }

        // Dummy database for now
        private List<UnitTemplate> _unitDatabase = new List<UnitTemplate>
        {
            new UnitTemplate { Name = "Heavy Infantry", ST = 12, Cost = 20 }
        };

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            // Instantiate the powerful CRUD_Builder you already built!
            var crudBuilder = new CRUD_Builder<UnitTemplate>(
                "UNIT REGISTRY",
                () => _unitDatabase,
                unit => unit.Name,          // Display Name in the list
                BuildEditorForm,            // The Form generation method
                OnSave,                     // Save action
                OnDelete                    // Delete action
            );

            // Customize your CrudStyle here if needed
            crudBuilder.Style.AccentColor = new Color(0.8f, 0.6f, 0.2f); // Warlords Gold

            return crudBuilder.CreateGui(ctx);
        }

        private VisualElement BuildEditorForm(UnitTemplate unit)
        {
            // Utilizing your fluent GraphicalUserInterfaceBuilder
            var builder = new GraphicalUserInterfaceBuilder("UnitDetails")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)

                .AddStringData("Unit Name", unit.Name, v => unit.Name = v)
                .AddSeparator()

                .AddChild(new Label("GURPS STATS") { style = { color = Color.gray, marginTop = 10, marginBottom = 10 } })
                .AddChild(new GraphicalUserInterfaceBuilder("StatsRow")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                    .WithWrap(Wrap.Wrap)
                    .AddIntegerData("ST (Strength)", unit.ST, v => unit.ST = v)
                    .AddIntegerData("DX (Dexterity)", unit.DX, v => unit.DX = v)
                    .AddIntegerData("IQ (Intellect)", unit.IQ, v => unit.IQ = v)
                    .AddIntegerData("HT (Hardiness)", unit.HT, v => unit.HT = v)
                )

                .AddSeparator()
                .AddChild(new Label("LOGISTICS") { style = { color = Color.gray, marginTop = 10, marginBottom = 10 } })
                .AddIntSliderData("Gold Cost", 1, 100, unit.Cost, v => unit.Cost = v)
                .AddIntSliderData("Turns to Build", 1, 10, unit.Turns, v => unit.Turns = v)

                .AddSeparator()
                .AddStringData("Description", unit.Description, v => unit.Description = v);

            return builder.Build();
        }

        private void OnSave(UnitTemplate unit) { if (!_unitDatabase.Contains(unit)) _unitDatabase.Add(unit); }
        private void OnDelete(UnitTemplate unit) { _unitDatabase.Remove(unit); }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}