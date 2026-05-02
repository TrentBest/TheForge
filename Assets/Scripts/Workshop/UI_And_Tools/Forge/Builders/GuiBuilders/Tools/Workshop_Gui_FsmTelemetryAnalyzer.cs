using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

// MOCK API STRUCTURE FOR TELEMETRY REFACTORING
namespace TheSingularityWorkshop.FSM_API.Telemetry
{
    public enum FsmBackingMode { String_Verbose, Hash_Optimized }

    public struct FsmPerformanceSnapshot
    {
        public FsmBackingMode BackingMode;
        public int TotalInstances;
        public int TransitionsLastTick;
        public float AvgTickTimeMs; // Requires Stopwatch hooks in the API Update loop
        public Dictionary<string, int> StateDistribution; // StateName -> InstanceCount
        public Dictionary<string, int> TransitionHeatmap; // TransitionKey -> FireCount
    }
}

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    public class Workshop_Gui_FsmTelemetryAnalyzer : IGuiProvider
    {
        public string Title => "FSM TELEMETRY ANALYZER";

        private string _selectedFsmDefinition = null;
        private VisualElement _detailPaneArea;
        private Label _liveTickLabel;

        // Dummy data for the prototype - You will replace this with API calls
        private List<string> _knownDefinitions = new List<string> { "AI_Goblin", "Player_Locomotion", "Weapon_PlasmaRifle", "UI_MainMenu" };

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("TelemetryRoot")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Row)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f));

            // ==========================================
            // LEFT PANE: FSM Definition Browser
            // ==========================================
            var browserPane = new GraphicalUserInterfaceBuilder("BrowserPane")
                .WithWidth(280)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f))
                .WithBorderRightWidth(2).WithBorderRightColor(new Color(0.3f, 0.3f, 0.35f))
                .WithPadding(10);

            browserPane.AddHeader("ACTIVE FSM DEFINITIONS", Color.cyan);
            browserPane.AddChild(new ForgeLabelBuilder("Select a graph to inspect memory and execution telemetry.")
                .WithColor(Color.gray).WithFontSize(10).WithMarginBottom(15).WithWordWrap(true));

            // TODO REFACTOR: Replace _knownDefinitions with FSM_API.Interaction.GetAllDefinitionNames()
            foreach (var def in _knownDefinitions)
            {
                browserPane.AddChild(new ForgeButtonBuilder(def)
                    .WithBackgroundColor(new Color(0.18f, 0.18f, 0.2f))
                    .WithTextColor(Color.silver).WithMarginBottom(4)
                    .OnClick(() => {
                        _selectedFsmDefinition = def;
                        RefreshDetailPane();
                    }));
            }

            root.AddChild(browserPane);

            // ==========================================
            // RIGHT PANE: Deep Telemetry Inspector
            // ==========================================
            var detailPane = new GraphicalUserInterfaceBuilder("DetailPane")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))
                .WithPadding(20)
                .WithScrollable(true)
                .OnBuild(ve => {
                    _detailPaneArea = ve;
                    RefreshDetailPane();
                });

            root.AddChild(detailPane);

            var visualRoot = root.Build();

            // Set up the Live Tick for the Telemetry data
            visualRoot.schedule.Execute(() => {
                if (_liveTickLabel != null && !string.IsNullOrEmpty(_selectedFsmDefinition))
                {
                    // Simulated pulsing to show active profiling
                    float pulse = (Mathf.Sin(Time.realtimeSinceStartup * 5f) + 1f) / 2f;
                    _liveTickLabel.style.color = Color.Lerp(Color.yellow, Color.red, pulse);
                }
            }).Every(100); // 10 ticks a second

            return visualRoot;
        }

        private void RefreshDetailPane()
        {
            if (_detailPaneArea == null) return;
            _detailPaneArea.Clear();

            if (string.IsNullOrEmpty(_selectedFsmDefinition))
            {
                _detailPaneArea.Add(new ForgeLabelBuilder("AWAITING TELEMETRY LOCK...")
                    .WithColor(new Color(0.3f, 0.3f, 0.3f)).WithFontSize(24).WithBold(true).Build());
                return;
            }

            // TODO REFACTOR: Fetch real telemetry struct from your API here
            var mockTelemetry = GetMockTelemetry(_selectedFsmDefinition);

            var builder = new GraphicalUserInterfaceBuilder("TelemetryDetails").WithFlexGrow(1);

            // --- 1. HEADER & MEMORY MODE ---
            var headerRow = new GraphicalUserInterfaceBuilder("HeaderRow")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithBorderBottomWidth(1).WithBorderBottomColor(Color.gray)
                .WithPaddingBottom(10).WithMarginBottom(20);

            headerRow.AddChild(new ForgeLabelBuilder($"PROFILING: {_selectedFsmDefinition}")
                .WithColor(Color.white).WithFontSize(20).WithBold(true));

            // Hash vs String visualizer
            Color modeColor = mockTelemetry.BackingMode == TheSingularityWorkshop.FSM_API.Telemetry.FsmBackingMode.Hash_Optimized ? Color.green : Color.yellow;
            headerRow.AddChild(new GraphicalUserInterfaceBuilder("ModeBadge")
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f)).WithBorderWidth(1).WithBorderAllColor(modeColor).WithPadding(10)
                .AddChild(new ForgeLabelBuilder($"BACKING: {mockTelemetry.BackingMode}").WithColor(modeColor).WithBold(true).WithFontSize(10)));

            builder.AddChild(headerRow);

            // --- 2. VITAL STATISTICS (Performance) ---
            builder.AddHeader("EXECUTION VITAL STATISTICS", new Color(0.8f, 0.4f, 0.8f));
            var vitalsRow = new GraphicalUserInterfaceBuilder("Vitals")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween)
                .WithMarginBottom(20);

            vitalsRow.AddChild(CreateStatCard("Active Instances", mockTelemetry.TotalInstances.ToString(), Color.cyan));
            vitalsRow.AddChild(CreateStatCard("Avg Tick Chronos", $"{mockTelemetry.AvgTickTimeMs:F3} ms", mockTelemetry.AvgTickTimeMs > 1.0f ? Color.red : Color.green));

            var velocityCard = CreateStatCard("Transitions / Frame", mockTelemetry.TransitionsLastTick.ToString(), Color.yellow) as GraphicalUserInterfaceBuilder;
            velocityCard.OnBuild(ve => _liveTickLabel = ve.Q<Label>("StatValue")); // Hook for the pulse effect
            vitalsRow.AddChild(velocityCard);

            builder.AddChild(vitalsRow);

            // --- 3. STATE HEATMAP ---
            builder.AddHeader("POPULATION HEATMAP (ACTIVE STATES)", Color.cyan);
            var heatmapContainer = new GraphicalUserInterfaceBuilder("Heatmap")
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithPadding(15).WithBorderRadius(6).WithMarginBottom(20);

            foreach (var state in mockTelemetry.StateDistribution.OrderByDescending(x => x.Value))
            {
                float percentage = mockTelemetry.TotalInstances > 0 ? (float)state.Value / mockTelemetry.TotalInstances : 0;
                heatmapContainer.AddChild(CreateHeatmapRow(state.Key, state.Value, percentage, Color.cyan));
            }
            builder.AddChild(heatmapContainer);

            // --- 4. BOTTLENECK ANALYSIS (Transitions) ---
            builder.AddHeader("TRANSITION BOTTLENECK ANALYSIS", new Color(0.9f, 0.6f, 0.2f));
            builder.AddChild(new ForgeLabelBuilder("Identifies which edges are being evaluated/fired the most frequently.")
                .WithColor(Color.gray).WithFontSize(10).WithMarginBottom(10));

            var edgeContainer = new GraphicalUserInterfaceBuilder("EdgeAnalysis")
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithPadding(15).WithBorderRadius(6);

            foreach (var edge in mockTelemetry.TransitionHeatmap.OrderByDescending(x => x.Value).Take(5))
            {
                edgeContainer.AddChild(new GraphicalUserInterfaceBuilder($"Edge_{edge.Key}")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween)
                    .WithBorderBottomWidth(1).WithBorderBottomColor(new Color(0.2f, 0.2f, 0.2f)).WithPaddingBottom(5).WithMarginBottom(5)
                    .AddChild(new ForgeLabelBuilder(edge.Key).WithColor(Color.silver).WithFontSize(12))
                    .AddChild(new ForgeLabelBuilder($"{edge.Value} fires").WithColor(new Color(0.9f, 0.6f, 0.2f)).WithBold(true)));
            }
            builder.AddChild(edgeContainer);

            _detailPaneArea.Add(builder.Build());
        }

        // ==========================================
        // UI COMPONENT BUILDERS
        // ==========================================
        private IGuiProvider CreateStatCard(string title, string value, Color accent)
        {
            return new GraphicalUserInterfaceBuilder($"Stat_{title}")
                .WithFlexGrow(1).WithMarginRight(10).WithPadding(15)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithBorderTopWidth(3).WithBorderTopColor(accent)
                .AddChild(new ForgeLabelBuilder(title).WithColor(Color.gray).WithFontSize(10).WithBold(true).WithMarginBottom(5))
                .AddChild(new ForgeLabelBuilder(value).WithName("StatValue").WithColor(Color.white).WithFontSize(24).WithBold(true));
        }

        private IGuiProvider CreateHeatmapRow(string label, int count, float percentage, Color accent)
        {
            var row = new GraphicalUserInterfaceBuilder($"Heat_{label}")
                .WithMarginBottom(8).WithFlexLayout(FlexDirection.Column);

            row.AddChild(new GraphicalUserInterfaceBuilder("TextRow")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween)
                .AddChild(new ForgeLabelBuilder(label).WithColor(Color.white).WithFontSize(12))
                .AddChild(new ForgeLabelBuilder($"{count} ({(percentage * 100):F1}%)").WithColor(Color.gray).WithFontSize(11)));

            var barBg = new GraphicalUserInterfaceBuilder("BarBg")
                .WithHeight(6).WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f)).WithMarginTop(4).WithBorderRadius(3);

            var barFill = new GraphicalUserInterfaceBuilder("BarFill")
                .WithHeight(6).WithBackgroundColor(accent).WithBorderRadius(3)
                .OnBuild(ve => ve.style.width = new StyleLength(new Length(percentage * 100, LengthUnit.Percent)));

            barBg.AddChild(barFill);
            row.AddChild(barBg);
            return row;
        }

        // --- MOCK DATA GENERATOR ---
        private TheSingularityWorkshop.FSM_API.Telemetry.FsmPerformanceSnapshot GetMockTelemetry(string fsmName)
        {
            // Simulated response
            return new TheSingularityWorkshop.FSM_API.Telemetry.FsmPerformanceSnapshot
            {
                BackingMode = fsmName.Contains("UI") ? TheSingularityWorkshop.FSM_API.Telemetry.FsmBackingMode.String_Verbose : TheSingularityWorkshop.FSM_API.Telemetry.FsmBackingMode.Hash_Optimized,
                TotalInstances = 142,
                TransitionsLastTick = UnityEngine.Random.Range(2, 15),
                AvgTickTimeMs = UnityEngine.Random.Range(0.1f, 1.5f),
                StateDistribution = new Dictionary<string, int> {
                    { "Idle", 80 }, { "Patrol", 40 }, { "Attack", 12 }, { "Dead", 10 }
                },
                TransitionHeatmap = new Dictionary<string, int> {
                    { "Idle -> Patrol", 1042 }, { "Patrol -> Idle", 980 }, { "Patrol -> Attack", 312 }, { "Attack -> Dead", 45 }
                }
            };
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}