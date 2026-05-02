#if UNITY_EDITOR
using Assets.Scripts.MastersOfOrionII.Government;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_FactionTraitBuilder : IGuiProvider
    {
        public string Title => "FACTION TRAIT ENGINEER";

        private GuiContext _lastCtx;
        private CRUD_Builder<FactionTrait> _crudInterface;

        // Master DB for Traits
        public static List<FactionTrait> TraitDatabase = new List<FactionTrait>
        {
            new FactionTrait { Name = "Brutal Industrialists", Domain = TraitDomain.Economic, IsDetrimental = false, CostModifier = 0.7f, MoraleModifier = 0.8f },
            new FactionTrait { Name = "Decadent Aristocracy", Domain = TraitDomain.Society, IsDetrimental = true, CostModifier = 1.5f, StructuralModifier = 0.8f }
        };

        public MastersOfOrionII_Gui_FactionTraitBuilder() { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            _crudInterface = new CRUD_Builder<FactionTrait>(
                title: "CULTURAL & ECONOMIC TRAITS",
                dataSource: () => TraitDatabase,
                getDisplayName: (trait) => string.IsNullOrEmpty(trait.Name) ? "Unknown Trait" : trait.Name,
                getGroupCategory: (trait) => trait.IsDetrimental ? $"Flaws ({trait.Domain})" : $"Advantages ({trait.Domain})",

                buildEditorForm: (trait) =>
                {
                    var form = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.FlexStart } };

                    // --- LEFT: NARRATIVE & CLASSIFICATION ---
                    var loreCol = new VisualElement { style = { width = 300, marginRight = 20 } };
                    loreCol.Add(new Label("TRAIT DEFINITION") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                    var loreBox = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 10, paddingRight = 10, paddingTop = 10, paddingBottom = 10, borderLeftWidth = 2, borderLeftColor = Color.cyan } };

                    var nameField = new TextField("Trait Name") { value = trait.Name };
                    nameField.RegisterValueChangedCallback(e => trait.Name = e.newValue);
                    loreBox.Add(nameField);

                    var domainField = new EnumField("Trait Domain", trait.Domain);
                    domainField.RegisterValueChangedCallback(e => trait.Domain = (TraitDomain)e.newValue);
                    loreBox.Add(domainField);

                    var detToggle = new Toggle("Is Detrimental (Flaw)") { value = trait.IsDetrimental };
                    detToggle.RegisterValueChangedCallback(e => trait.IsDetrimental = e.newValue);
                    loreBox.Add(detToggle);

                    loreCol.Add(loreBox);
                    form.Add(loreCol);

                    // --- RIGHT: MATHEMATICAL MODIFIERS ---
                    var mathCol = new VisualElement { style = { flexGrow = 1 } };
                    mathCol.Add(new Label("MECHANICAL EXPLANATION (1.0 = Baseline)") { style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                    var mathBox = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 10, paddingRight = 10, paddingTop = 10, paddingBottom = 10, borderLeftWidth = 2, borderLeftColor = Color.yellow } };

                    mathBox.Add(CreateModField("Ship Cost Modifier", trait.CostModifier, val => trait.CostModifier = val, "< 1.0 is cheaper"));
                    mathBox.Add(CreateModField("Troop Morale Modifier", trait.MoraleModifier, val => trait.MoraleModifier = val, "> 1.0 is stronger"));
                    mathBox.Add(CreateModField("Science/Research Velocity", trait.ScienceModifier, val => trait.ScienceModifier = val, "> 1.0 is faster"));
                    mathBox.Add(CreateModField("Structural Shear Tolerance", trait.StructuralModifier, val => trait.StructuralModifier = val, "> 1.0 is tougher"));

                    mathCol.Add(mathBox);
                    form.Add(mathCol);

                    return form;
                },
                onSave: (trait) => { if (!TraitDatabase.Contains(trait)) TraitDatabase.Add(trait); },
                onDelete: (trait) => { TraitDatabase.Remove(trait); },
                getSubtitle: (trait) => trait.IsDetrimental ? "Debuff" : "Buff"
            );

            return _crudInterface.CreateGui(ctx);
        }

        private VisualElement CreateModField(string label, float val, Action<float> onUpdate, string tooltip)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 5 } };
            var field = new FloatField(label) { value = val, tooltip = tooltip, style = { flexGrow = 1 } };
            field.RegisterValueChangedCallback(e => onUpdate(e.newValue));
            row.Add(field);

            // Dynamic color indicator to instantly show if the math is "Good" or "Bad"
            Color indicator = val == 1.0f ? Color.gray : (val > 1.0f ? Color.green : new Color(0.8f, 0.2f, 0.2f));
            if (label.Contains("Cost")) indicator = val == 1.0f ? Color.gray : (val < 1.0f ? Color.green : new Color(0.8f, 0.2f, 0.2f));

            row.Add(new VisualElement { style = { width = 10, height = 10, borderBottomRightRadius = 5,
                    borderBottomLeftRadius = 5, borderTopLeftRadius = 5, borderTopRightRadius = 5, backgroundColor = indicator, marginLeft = 10 } });
            return row;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}
#endif