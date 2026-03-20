using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Warlords
{
    public class Warlords_UnitCard : VisualElement
    {
        private const float TOTAL_WIDTH = 768f;
        private const float IMAGE_SIZE = 256f;
        private const float DATA_WIDTH = 512f;

        public Warlords_UnitCard(string unitName, int st, int dx, int iq, int ht, string weapon, string armor, int move, string damage)
        {
            // --- OUTER CONTAINER ---
            style.width = TOTAL_WIDTH;
            style.height = IMAGE_SIZE; // Perfectly square based on image
            style.flexDirection = FlexDirection.Row;
            style.marginBottom = 15;
            style.backgroundColor = new Color(0.08f, 0.08f, 0.1f);

            // Ornate Heavy Border
            style.borderTopWidth = 2; style.borderTopColor = new Color(0.4f, 0.4f, 0.45f);
            style.borderLeftWidth = 2; style.borderLeftColor = new Color(0.4f, 0.4f, 0.45f);
            style.borderBottomWidth = 4; style.borderBottomColor = Color.black;
            style.borderRightWidth = 4; style.borderRightColor = Color.black;

            // --- 1. PORTRAIT (Left 256px) ---
            var portrait = new VisualElement { style = { width = IMAGE_SIZE, height = IMAGE_SIZE, backgroundColor = Color.black } };
            portrait.style.borderRightWidth = 2;
            portrait.style.borderRightColor = new Color(0.2f, 0.2f, 0.25f);

            var imgPlaceholder = new Label("256x256\nPORTRAIT")
            {
                style = { unityTextAlign = TextAnchor.MiddleCenter, color = new Color(0.3f, 0.3f, 0.3f), flexGrow = 1 }
            };
            portrait.Add(imgPlaceholder);
            Add(portrait);

            // --- 2. DATA BLOCK (Right 512px) ---
            var dataBlock = new VisualElement { style = { width = DATA_WIDTH, height = IMAGE_SIZE, paddingLeft = 20, paddingRight = 20, paddingTop = 15, paddingBottom = 15 } };

            // Header Row
            var header = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center } };
            header.Add(new Label(unitName.ToUpper()) { style = { fontSize = 28, color = new Color(0.9f, 0.8f, 0.5f), unityFontStyleAndWeight = FontStyle.Bold } });
            header.Add(new Label($"MOVE: {move}") { style = { fontSize = 16, color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold } });
            dataBlock.Add(header);

            // Divider
            var div = new VisualElement { style = { height = 2, backgroundColor = new Color(0.3f, 0.3f, 0.35f), marginTop = 5, marginBottom = 15 } };
            dataBlock.Add(div);

            // Attributes (GURPS Core)
            var attrGrid = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginBottom = 20 } };
            attrGrid.Add(CreateStatDiamond("ST", st, new Color(0.8f, 0.3f, 0.3f)));
            attrGrid.Add(CreateStatDiamond("DX", dx, new Color(0.3f, 0.8f, 0.3f)));
            attrGrid.Add(CreateStatDiamond("IQ", iq, new Color(0.3f, 0.5f, 0.9f)));
            attrGrid.Add(CreateStatDiamond("HT", ht, new Color(0.8f, 0.8f, 0.3f)));
            dataBlock.Add(attrGrid);

            // Combat Specs
            dataBlock.Add(CreateCombatRow("WEAPON", weapon, Color.white));
            dataBlock.Add(CreateCombatRow("ARMOR", armor, Color.white));
            dataBlock.Add(CreateCombatRow("DAMAGE", damage, Color.yellow));

            Add(dataBlock);
        }

        private VisualElement CreateStatDiamond(string label, int val, Color theme)
        {
            var container = new VisualElement { style = { alignItems = Align.Center, width = 100 } };
            container.Add(new Label(label) { style = { fontSize = 12, color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold } });
            container.Add(new Label(val.ToString()) { style = { fontSize = 32, color = theme, unityFontStyleAndWeight = FontStyle.Bold, marginTop = -5 } });
            return container;
        }

        private VisualElement CreateCombatRow(string label, string val, Color valColor)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 4 } };
            row.Add(new Label(label + ":") { style = { width = 120, fontSize = 12, color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold } });
            row.Add(new Label(val) { style = { fontSize = 13, color = valColor, unityFontStyleAndWeight = FontStyle.Bold } });
            return row;
        }
    }
}