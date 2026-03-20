using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Warlords
{
    public class Warlords_Playable_Alpha : IGuiProvider
    {
        public string Title => "WARLORDS: TACTICAL VIEW";
        private VisualElement _mapGrid;
        private VisualElement _infoPanel;

        // Simple "Selected State"
        private string _selectedUnitName = "None";
        private GuiContext _lastCtx;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            var root = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row, backgroundColor = Color.black } };

            // 1. THE WORLD (Center/Left)
            _mapGrid = new VisualElement { style = { flexGrow = 1, paddingTop = 10, paddingLeft = 10, paddingBottom = 10, paddingRight = 10, flexDirection = FlexDirection.Column } };
            GeneratePlayableGrid(12, 12); // Create a 12x12 battlefield
            root.Add(_mapGrid);

            // 2. THE COMMAND CONSOLE (Right Side - Fixed 400px)
            var console = new VisualElement { style = { width = 400, backgroundColor = new Color(0.1f, 0.1f, 0.12f), borderLeftWidth = 3, borderLeftColor = Color.gray } };

            // Miniature Unit Card (Squished version for the sidebar)
            _infoPanel = new VisualElement { style = { paddingTop = 15, paddingRight = 15, paddingBottom = 15, paddingLeft = 15 } };
            UpdateSidebar("Select a Unit", 0);
            console.Add(_infoPanel);

            // Action Buttons
            var btnArea = new VisualElement { style = { marginTop = Length.Auto(), paddingTop = 20, paddingLeft = 20, paddingBottom = 20, paddingRight = 20 } };
            btnArea.Add(new Button { text = "END TURN", style = { height = 50, backgroundColor = new Color(0.5f, 0.1f, 0.1f), color = Color.white } });
            console.Add(btnArea);

            root.Add(console);
            return root;
        }

        private void GeneratePlayableGrid(int rows, int cols)
        {
            for (int r = 0; r < rows; r++)
            {
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                for (int c = 0; c < cols; c++)
                {
                    var tile = new Button(); // Using button for easy clicking
                    tile.style.width = 60; tile.style.height = 60;
                    tile.style.marginTop = 1;
                    tile.style.marginBottom = 1;
                    tile.style.marginLeft = 1;
                    tile.style.marginRight = 1;

                    tile.style.backgroundColor = new Color(0.15f, 0.25f, 0.15f); // Grass

                    // Add a "Unit" to some tiles
                    if (r == 5 && c == 5)
                    {
                        tile.text = "🛡️"; // Infantry
                        tile.clicked += () => UpdateSidebar("Heavy Infantry", 12);
                    }
                    if (r == 2 && c == 8)
                    {
                        tile.text = "🏰"; // Castle
                        tile.clicked += () => UpdateSidebar("White Citadel", 0);
                    }

                    row.Add(tile);
                }
                _mapGrid.Add(row);
            }
        }

        private void UpdateSidebar(string title, int st)
        {
            _infoPanel.Clear();
            _infoPanel.Add(new Label(title.ToUpper()) { style = { fontSize = 20, color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold } });

            if (st > 0)
            {
                _infoPanel.Add(new Label($"STRENGTH: {st}") { style = { color = Color.white } });
                _infoPanel.Add(new Label("STATUS: Ready to Move") { style = { color = Color.green, fontSize = 10 } });
                _infoPanel.Add(new Button { text = "MOVE STACK", style = { marginTop = 20 } });
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}