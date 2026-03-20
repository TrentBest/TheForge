using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public enum Side { Left, Right }

    public class SplitPanelBuilder : IGuiProvider
    {
        public string Title { get; set; } = "Sectorized Workspace";
        private int _rows;
        private int _cols;
        private int _sidebarWidth = -1;
        private Side _side = Side.Left; // Default to legacy behavior
        private Dictionary<(int row, int col), IGuiProvider> _providers = new();

        // 1. Sidebar Constructor (Now with Side support)
        public SplitPanelBuilder(int sidebarWidth = 250, Side side = Side.Left)
        {
            _rows = 1;
            _cols = 2;
            _sidebarWidth = sidebarWidth;
            _side = side;
        }

        // 2. Grid Constructor
        public SplitPanelBuilder(int rows, int cols)
        {
            _rows = rows;
            _cols = cols;
            _sidebarWidth = -1;
        }

        public SplitPanelBuilder WithSector(int row, int col, IGuiProvider provider)
        {
            _providers[(row, col)] = provider;
            return this;
        }

        // --- DYNAMIC BRIDGES ---
        // Maps "Sidebar" and "Main" based on which side is chosen
        public SplitPanelBuilder WithSidebar(IGuiProvider guiProvider)
            => WithSector(0, _side == Side.Left ? 0 : 1, guiProvider);

        public SplitPanelBuilder WithMain(IGuiProvider guiProvider)
            => WithSector(0, _side == Side.Left ? 1 : 0, guiProvider);

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("SectorRoot")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100)
                .WithFlexGrow(1);

            for (int r = 0; r < _rows; r++)
            {
                var rowBuilder = new GraphicalUserInterfaceBuilder($"Row_{r}")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                    .WithFlexGrow(1);

                for (int c = 0; c < _cols; c++)
                {
                    var colBuilder = new GraphicalUserInterfaceBuilder($"Sector_{r}_{c}")
                        .WithBorderWidth(1)
                        .WithBorderColor(new Color(0.15f, 0.15f, 0.15f));

                    // --- THE FIX: Logic-based width locking ---
                    bool isSidebarCol = (_side == Side.Left && c == 0) || (_side == Side.Right && c == 1);

                    if (_sidebarWidth > 0 && isSidebarCol)
                    {
                        colBuilder.WithWidth(_sidebarWidth).WithFlexGrow(0).WithFlexShrink(0);
                    }
                    else
                    {
                        colBuilder.WithFlexGrow(1).WithFlexShrink(1);
                    }

                    if (_providers.TryGetValue((r, c), out var provider))
                    {
                        colBuilder.AddChild(provider);
                    }

                    rowBuilder.AddChild(colBuilder);
                }
                rootBuilder.AddChild(rowBuilder);
            }

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }

        public SplitPanelBuilder AddChild(IGuiProvider child)
        {

            return this;
        }

        public SplitPanelBuilder AddChild(VisualElement child)
        {

            return this;
        }
    }
}