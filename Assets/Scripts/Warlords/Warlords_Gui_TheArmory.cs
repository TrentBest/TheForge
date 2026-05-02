using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Warlords
{
    public class Warlords_Gui_TheArmory : IGuiProvider
    {
        private GuiContext _lastCtx;

        public string Title => "THE ROYAL ARMORY";

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            var builder = new GraphicalUserInterfaceBuilder("Armory_Root")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.04f, 0.03f, 0.02f)); // Deep forge-ember brown

            // --- LEFT: THE ARMORY INVENTORY ---
            builder.AddChild(root =>
            {
                var inventory = new VisualElement { style = { width = 320, backgroundColor = new Color(0.08f, 0.07f, 0.06f) } };
                inventory.style.borderRightWidth = 3; inventory.style.borderRightColor = new Color(0.2f, 0.15f, 0.1f);
                inventory.style.paddingTop = 20; inventory.style.paddingLeft = 15; inventory.style.paddingRight = 15;

                inventory.Add(new Label("EQUIPMENT REGISTRY") { style = { color = new Color(0.8f, 0.6f, 0.2f), fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });

                var tabHeader = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 10 } };
                tabHeader.Add(new Button { text = "WEAPONS", style = { flexGrow = 1, fontSize = 10 } });
                tabHeader.Add(new Button { text = "ARMOR", style = { flexGrow = 1, fontSize = 10 } });
                inventory.Add(tabHeader);

                var scroll = new ScrollView();
                // Placeholder list of gear
                string[] gear = { "Shortsword", "Broadsword", "Greatsword", "Padded Cloth", "Chainmail", "Full Plate" };
                foreach (var item in gear)
                {
                    var b = new Button { text = item };
                    b.style.unityTextAlign = TextAnchor.MiddleLeft;
                    b.style.backgroundColor = new Color(0.15f, 0.12f, 0.1f);
                    scroll.Add(b);
                }
                inventory.Add(scroll);
                return inventory;
            });

            // --- RIGHT: THE FORGE (UPGRADE WORKSHOP) ---
            builder.AddChild(root =>
            {
                var forge = new VisualElement { style = { flexGrow = 1, paddingLeft = 40, paddingRight = 40, paddingTop = 30 } };

                // Top: Unit Preview
                forge.Add(new Label("THE FORGE: UNIT AUGMENTATION") { style = { fontSize = 28, color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });

                // Show the current unit card at the top
                var unitPreview = new Warlords_UnitCard("Heavy Infantry", 12, 11, 10, 12, "Broadsword", "Chainmail", 10, "2d-1 cut");
                forge.Add(unitPreview);

                // Middle: Upgrade Options (The Diablo-style slotting)
                var upgradeArea = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 30, height = 200 } };

                // Weapon Upgrade Slot
                upgradeArea.Add(CreateUpgradeSlot("WEAPONRY", "Broadsword", "Masterwork Sword", "Increases ST bonus by +2", "150 GP / 3 Turns"));
                // Armor Upgrade Slot
                upgradeArea.Add(CreateUpgradeSlot("PROTECTION", "Chainmail", "Plate Reinforcement", "Increases DR (Armor) by +3", "300 GP / 5 Turns"));

                forge.Add(upgradeArea);

                // Action Bar
                var actions = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = Length.Auto(), marginBottom = 40, justifyContent = Justify.FlexEnd } };
                var cancelBtn = new Button { text = "LEAVE ARMORY", style = { width = 150, height = 45 } };
                var forgeBtn = new Button { text = "BEGIN UPGRADE", style = { width = 200, height = 45, backgroundColor = new Color(0.4f, 0.2f, 0.1f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } };

                actions.Add(cancelBtn);
                actions.Add(forgeBtn);
                forge.Add(actions);

                return forge;
            });

            return builder.Build();
        }

        private VisualElement CreateUpgradeSlot(string category, string current, string next, string effect, string cost)
        {
            var slot = new VisualElement { style = { flexGrow = 1, marginRight = 20, backgroundColor = new Color(0.12f, 0.1f, 0.08f), paddingTop = 15, paddingLeft = 15, paddingBottom = 15, paddingRight = 15 } };
            // Manual padding
            slot.style.paddingTop = 15; slot.style.paddingBottom = 15; slot.style.paddingLeft = 15; slot.style.paddingRight = 15;
            slot.style.borderTopWidth = 1; slot.style.borderTopColor = new Color(0.3f, 0.25f, 0.2f);

            slot.Add(new Label(category) { style = { color = Color.gray, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold } });
            slot.Add(new Label($"Current: {current}") { style = { color = Color.white, fontSize = 12, marginBottom = 10 } });

            var arrow = new Label("▼ UPGRADE TO ▼") { style = { unityTextAlign = TextAnchor.MiddleCenter, color = Color.green, fontSize = 10, marginBottom = 10 } };
            slot.Add(arrow);

            slot.Add(new Label(next) { style = { color = Color.yellow, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold } });
            slot.Add(new Label(effect) { style = { color = Color.gray, fontSize = 11,  } });
            slot.Add(new Label(cost) { style = { color = Color.cyan, fontSize = 12, marginTop = 10, unityFontStyleAndWeight = FontStyle.Bold } });

            return slot;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}