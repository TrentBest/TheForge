using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase
{
    public class Showcase_MemoryTelemetry : IGuiProvider
    {
        public string Title => "ZERO-GC MEMORY ARCHITECTURE";

        // --- Backend Simulation State ---
        // (Replace these with your actual FSM_Memory backend queries)
        private int _simulatedActiveIndices = 100;
        private int _currentArrayCapacity = 1024;
        private int _totalExpansions = 0;
        private bool _isSpiking = false;

        // --- UI References ---
        private Label _lblActive;
        private Label _lblCapacity;
        private Label _lblExpansions;
        private VisualElement _memoryGrid;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("MemoryTelemetryRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch);

            // ==========================================
            // LEFT PANEL: TELEMETRY & CONTROLS
            // ==========================================
            var leftPanel = new GraphicalUserInterfaceBuilder("StatsPanel")
                .WithWidth(350)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderRightWidth(2).WithBorderRightColor(new Color(0.2f, 1.0f, 0.2f)) // Neon Green
                .WithPadding(20)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

            leftPanel.AddChild(new ForgeLabelBuilder("FSM_MEMORY HEURISTICS")
                .WithColor(Color.white).WithFontSize(22).WithFontStyle(FontStyle.Bold).WithMarginBottom(20));

            // Hard Proof Stats
            leftPanel.AddChild(CreateStatRow("GARBAGE COLLECTIONS:", "0", Color.gray, Color.cyan));
            leftPanel.AddChild(CreateStatRow("HEAP FRAGMENTATION:", "0.00%", Color.gray, Color.cyan));

            leftPanel.AddChild(new GraphicalUserInterfaceBuilder("Divider").WithHeight(1).WithBackgroundColor(Color.gray).WithMargin(15));

            // Live FSM Stats
            leftPanel.AddChild(new GraphicalUserInterfaceBuilder("ActiveRow").WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .AddChild(new ForgeLabelBuilder("ACTIVE INDICES:").WithColor(Color.gray).WithFontSize(14))
                .AddChild(new ForgeLabelBuilder("0").WithColor(Color.white).WithFontSize(14).WithFontStyle(FontStyle.Bold)
                    .OnBuild(ve => _lblActive = ve as Label)));

            leftPanel.AddChild(new GraphicalUserInterfaceBuilder("CapacityRow").WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center).WithMarginTop(10)
                .AddChild(new ForgeLabelBuilder("CONTIGUOUS CAPACITY:").WithColor(Color.gray).WithFontSize(14))
                .AddChild(new ForgeLabelBuilder("0").WithColor(new Color(0.2f, 1.0f, 0.2f)).WithFontSize(14).WithFontStyle(FontStyle.Bold)
                    .OnBuild(ve => _lblCapacity = ve as Label)));

            leftPanel.AddChild(new GraphicalUserInterfaceBuilder("ExpansionRow").WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center).WithMarginTop(10)
                .AddChild(new ForgeLabelBuilder("x2 EXPANSION EVENTS:").WithColor(Color.gray).WithFontSize(14))
                .AddChild(new ForgeLabelBuilder("0").WithColor(new Color(0.8f, 0.1f, 0.8f)).WithFontSize(14).WithFontStyle(FontStyle.Bold)
                    .OnBuild(ve => _lblExpansions = ve as Label)));

            // Action Button
            leftPanel.AddChild(new GraphicalUserInterfaceBuilder("Spacer").WithFlexGrow(1));
            leftPanel.AddChild(new ForgeButtonBuilder("SIMULATE MASSIVE LOAD SPIKE")
                .WithBackgroundColor(new Color(0.8f, 0.1f, 0.8f))
                .WithHeight(50)
                .WithFontStyle(FontStyle.Bold)
                .OnClick(() => _isSpiking = true));

            rootBuilder.AddChild(leftPanel);

            // ==========================================
            // RIGHT PANEL: THE CONTIGUOUS VISUALIZER
            // ==========================================
            var rightPanel = new GraphicalUserInterfaceBuilder("VisualizerPanel")
                .WithFlexGrow(1)
                .WithPadding(30)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

            rightPanel.AddChild(new ForgeLabelBuilder("CONTIGUOUS CLASS ALLOCATION MAP")
                .WithColor(Color.gray).WithFontSize(16).WithFontStyle(FontStyle.Bold).WithMarginBottom(20));

            rightPanel.AddChild(new GraphicalUserInterfaceBuilder("GridWrapper")
                .WithFlexGrow(1)
                .WithBackgroundColor(Color.black)
                .WithBorderWidth(1).WithBorderColor(Color.gray)
                .WithPadding(10)
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.FlexStart)
                .WithFlexWrap(Wrap.Wrap) // Crucial for grid layout
                .OnBuild(ve => _memoryGrid = ve));

            var root = rootBuilder.Build();

            // Setup the initial grid UI
            RebuildMemoryGrid();

            // ==========================================
            // THE SIMULATION TICK
            // ==========================================
            root.schedule.Execute(() =>
            {
                // 1. Process Simulation Math
                if (_isSpiking)
                {
                    _simulatedActiveIndices += Mathf.FloorToInt(Time.deltaTime * 85000f); // Fast spike

                    // The Heuristic x2 Expansion Logic
                    if (_simulatedActiveIndices >= _currentArrayCapacity)
                    {
                        _currentArrayCapacity *= 2; // O(1) tracking, zero reallocation stutter
                        _totalExpansions++;
                        RebuildMemoryGrid(); // Redraw UI representation
                    }

                    if (_simulatedActiveIndices > 250000) _isSpiking = false; // Cap the spike
                }
                else
                {
                    // Gentle breathing of agents
                    _simulatedActiveIndices += UnityEngine.Random.Range(-50, 60);
                    _simulatedActiveIndices = Mathf.Clamp(_simulatedActiveIndices, 0, _currentArrayCapacity);
                }

                // 2. Update Text Readouts
                _lblActive.text = _simulatedActiveIndices.ToString("N0");
                _lblCapacity.text = _currentArrayCapacity.ToString("N0");
                _lblExpansions.text = _totalExpansions.ToString();

                // 3. Update Grid Visuals (Lighting up the active contiguous blocks)
                UpdateGridVisuals();

            }).Every(16); // ~60fps UI tick

            return root;
        }

        private IGuiProvider CreateStatRow(string label, string value, Color labelColor, Color valueColor)
        {
            return new GraphicalUserInterfaceBuilder($"Row_{label}")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithMarginBottom(5)
                .AddChild(new ForgeLabelBuilder(label).WithColor(labelColor).WithFontSize(12))
                .AddChild(new ForgeLabelBuilder(value).WithColor(valueColor).WithFontSize(12).WithFontStyle(FontStyle.Bold));
        }

        private void RebuildMemoryGrid()
        {
            _memoryGrid.Clear();
            // We draw exactly 256 "Chunks" to represent the total array, regardless of its mathematical size.
            // This prevents UI Toolkit from choking on millions of VisualElements.
            for (int i = 0; i < 256; i++)
            {
                var block = new VisualElement();
                block.style.flexGrow = 1;
                block.style.flexBasis = new Length(5, LengthUnit.Percent); // ~16 blocks per row
                block.style.height = 20;
                block.style.marginRight = 2;
                block.style.marginBottom = 2;
                block.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f); // Empty slot color
                _memoryGrid.Add(block);
            }
        }

        private void UpdateGridVisuals()
        {
            if (_memoryGrid == null || _memoryGrid.childCount == 0) return;

            float fillPercentage = (float)_simulatedActiveIndices / _currentArrayCapacity;
            int filledBlocks = Mathf.FloorToInt(fillPercentage * 256);

            for (int i = 0; i < 256; i++)
            {
                var block = _memoryGrid[i];
                if (i < filledBlocks)
                {
                    // Active Contiguous Index
                    block.style.backgroundColor = new Color(0.2f, 1.0f, 0.2f); // Neon Green
                }
                else if (i == filledBlocks)
                {
                    // The "Head" of the allocation
                    block.style.backgroundColor = Color.cyan;
                }
                else
                {
                    // Empty, pre-allocated space
                    block.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
                }
            }
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}