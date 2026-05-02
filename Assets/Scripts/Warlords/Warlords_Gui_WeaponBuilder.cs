using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Warlords
{
    public class Warlords_Gui_WeaponBuilder : IGuiProvider
    {
        private GuiContext _lastCtx;

        public string Title => "FORGE: WEAPON BUILDER";

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            var builder = new GraphicalUserInterfaceBuilder("WeaponBuilder_Root")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.07f));

            // LEFT PANEL: Selection List
            builder.AddChild(root =>
            {
                var list = new VisualElement { style = { width = 300, backgroundColor = new Color(0.1f, 0.1f, 0.12f), borderRightWidth = 2, borderRightColor = Color.black } };
                list.style.paddingTop = 20; list.style.paddingLeft = 15; list.style.paddingRight = 15;

                list.Add(new Label("THE ARMORY") { style = { color = Color.gray, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });
                list.Add(new Button { text = "+ NEW WEAPON", style = { height = 35, marginBottom = 10, backgroundColor = new Color(0.15f, 0.3f, 0.15f) } });

                var scroll = new ScrollView();
                string[] dummyWeapons = { "Broadsword", "Longbow", "Halberd", "Wizard Staff", "Morningstar" };
                foreach (var w in dummyWeapons)
                {
                    var b = new Button { text = w };
                    b.style.unityTextAlign = TextAnchor.MiddleLeft;
                    scroll.Add(b);
                }
                list.Add(scroll);
                return list;
            });

            // RIGHT PANEL: The Forge (Editor)
            builder.AddChild(root =>
            {
                var editor = new VisualElement { style = { flexGrow = 1, paddingLeft = 40, paddingRight = 40, paddingTop = 40 } };

                editor.Add(new Label("WEAPON PARAMETERS") { style = { fontSize = 28, color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 25 } });

                // Basic Info
                editor.Add(new TextField("Weapon Name") { value = "Broadsword" });

                // Damage Logic
                var damageGroup = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 20 } };
                damageGroup.Add(new TextField("Base Damage") { value = "sw+1", style = { flexGrow = 1, marginRight = 10 } });
                damageGroup.Add(new DropdownField("Type", new System.Collections.Generic.List<string> { "Cut", "Cr", "Imp", "Pi" }, "Cut") { style = { width = 150 } });
                editor.Add(damageGroup);

                // GURPS Specifics
                var specs = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 20 } };
                specs.Add(CreateSmallField("Reach", "1"));
                specs.Add(CreateSmallField("Min ST", "10"));
                specs.Add(CreateSmallField("Weight", "3.0"));
                specs.Add(CreateSmallField("Cost", "500"));
                editor.Add(specs);

                // Special Properties (Traits)
                editor.Add(new Label("SPECIAL TRAITS") { style = { marginTop = 20, color = Color.gray, fontSize = 12 } });
                editor.Add(new Toggle("Can Parry") { value = true });
                editor.Add(new Toggle("Requires Two Hands") { value = false });
                editor.Add(new Toggle("Armor Piercing") { value = false });

                // Save Action
                var saveBtn = new Button { text = "FORGE WEAPON" };
                saveBtn.style.height = 50; saveBtn.style.marginTop = Length.Auto();
                saveBtn.style.marginBottom = 40;
                saveBtn.style.backgroundColor = new Color(0.3f, 0.2f, 0.1f);
                saveBtn.style.color = Color.white;
                saveBtn.style.fontSize = 18;
                editor.Add(saveBtn);

                return editor;
            });

            return builder.Build();
        }

        private VisualElement CreateSmallField(string label, string val)
        {
            var c = new VisualElement { style = { width = 100, marginRight = 15 } };
            c.Add(new Label(label) { style = { fontSize = 10, color = Color.gray } });
            c.Add(new TextField { value = val });
            return c;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}