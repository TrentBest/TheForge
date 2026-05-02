using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.BardsTale.World;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.BardsTale.UI
{
    public class BardsTale_WorldForgeGuiProvider : IGuiProvider
    {
        // IGuiProvider Requirements
        public string Title => "Bard's Tale World Forge";

        private DungeonMapContext _map;
        private TileFeature _selectedFeature = TileFeature.Wall;
        public BardsTale_WorldForgeGuiProvider() { }
        public BardsTale_WorldForgeGuiProvider(DungeonMapContext map)
        {
            _map = map;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Fix: ForgeSplitPanelBuilder uses (int width, Side side) 
            // instead of TwoPaneSplitViewOrientation
            var splitBuilder = new ForgeSplitPanelBuilder(250, Side.Left);

            // LEFT: The Palette
            var paletteRoot = new GraphicalUserInterfaceBuilder("ForgePalette")
                .WithPadding(15)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                .AddChild(new ForgeLabelBuilder("MAP EDITOR")
                    .WithFontSize(18)
                    .WithFontStyle(FontStyle.Bold)) // Fix: Use WithFontStyle, not WithBold
                .AddSeparator(Color.gray, 1)
                .AddButton("PLACE WALL", () => _selectedFeature = TileFeature.Wall)
                .AddButton("PLACE DOOR", () => _selectedFeature = TileFeature.Door)
                .AddButton("CLEAR TILE", () => _selectedFeature = TileFeature.None)
                .Build();

            // RIGHT: The Interactive Grid
            // Fix: Forge_GridGuiBuilder requires (string id, int rows, int cols)
            var gridBuilder = new ForgeGridGuiBuilder("DungeonGrid", _map.Height, _map.Width)
               .WithRowModifier((row, index) => row.WithMarginBottom(2));

            // Fix: Use WithLeftPane/WithRightPane instead of SetLeftPane
            splitBuilder.WithLeftPane(new DynamicGuiProvider(c => paletteRoot));
            splitBuilder.WithRightPane(new DynamicGuiProvider(c => gridBuilder.Build()));

            return splitBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        // Boilerplate for Interface Compliance
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}