using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// Constructs dynamic grid-based layouts (Inventories, Periodic Tables, World Maps).
    /// Fully integrated with the Forge Container system and Dynamic Adapter.
    /// </summary>
    public class ForgeGridGuiBuilder : IGuiProvider
    {
        public string Title => "Forge Grid";
        private readonly string _id;
        private readonly int _rows;
        private readonly int _cols;
        private readonly IGuiProvider[,] _cells;
        private Action<ForgeContainerBuilder, int> _rowModifier;
        private Action<VisualElement, int, int> _cellRenderer;

        public ForgeGridGuiBuilder(string id, int rows, int cols)
        {
            _id = id;
            _rows = rows;
            _cols = cols;
            _cells = new IGuiProvider[rows, cols];
        }

        /// <summary>
        /// FIX: Restored the missing definition. 
        /// Allows modification of the ForgeContainerBuilder for a specific row.
        /// </summary>
        public ForgeGridGuiBuilder WithRowModifier(Action<ForgeContainerBuilder, int> modifier)
        {
            _rowModifier = modifier;
            return this;
        }

        /// <summary>
        /// Provides a procedural hook to modify the realized VisualElement of a cell.
        /// Essential for Dragon Warrior tile logic.
        /// </summary>
        public ForgeGridGuiBuilder OnCellRender(Action<VisualElement, int, int> renderer)
        {
            _cellRenderer = renderer;
            return this;
        }

        public ForgeGridGuiBuilder SetCell(int row, int col, IGuiProvider provider)
        {
            if (row >= 0 && row < _rows && col >= 0 && col < _cols)
                _cells[row, col] = provider;
            return this;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Root Grid Container
            var gridRoot = new ForgeContainerBuilder($"{_id}_Root")
                .WithDirection(FlexDirection.Column);

            for (int r = 0; r < _rows; r++)
            {
                // Create the Row Container
                var rowBuilder = new ForgeContainerBuilder($"{_id}_Row_{r}")
                    .WithDirection(FlexDirection.Row);

                // Invoke external styling (e.g. adding margins to specific rows)
                _rowModifier?.Invoke(rowBuilder, r);

                for (int c = 0; c < _cols; c++)
                {
                    // Resolve the cell content or create a fallback
                    var cellElement = _cells[r, c]?.CreateGui(ctx) ?? new VisualElement { name = "EmptyCell" };

                    // Apply the renderer callback (Procedural Styling)
                    _cellRenderer?.Invoke(cellElement, c, r);

                    // Add to the row using the DynamicGuiProvider adapter
                    rowBuilder.AddChild(new DynamicGuiProvider(cellElement));
                }

                gridRoot.AddChild(rowBuilder);
            }

            return gridRoot.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string p) { }
        public void FromUIDocument(string p) { }
    }
}