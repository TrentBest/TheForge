using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    public class Workshop_Gui_ApiBenchmarkProfiler : IGuiProvider
    {
        public string Title => "API BENCHMARK PROFILER";

        private class BenchRound
        {
            public int Interval;
            public int StartLoad;
            public int Increment;
        }

        // Benchmark Configuration matching Program.cs
        private List<BenchRound> _rounds = new List<BenchRound>
        {
            new BenchRound { Interval = 1, StartLoad = 50000, Increment = 5000 },
            new BenchRound { Interval = 2, StartLoad = 80000, Increment = 5000 },
            new BenchRound { Interval = 3, StartLoad = 120000, Increment = 10000 }
        };

        private double _minFpsThreshold = 30.0;
        private int _framesPerBatch = 60;

        // Execution State
        private bool _isRunning = false;
        private int _currentRoundIndex = 0;
        private int _currentLoad = 0;

        // UI Injection Points
        private Label _lblStatus;
        private VisualElement _resultsTableArea;
        private Button _btnRun;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("BenchmarkRoot")
                .WithFlexGrow(1).WithPadding(20)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f));

            root.AddHeader("THE SINGULARITY WORKSHOP: FINAL BASELINE PROFILER", Color.yellow);

            // --- 1. CONTROL PANEL ---
            var controlPanel = new GraphicalUserInterfaceBuilder("ControlPanel")
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f))
                .WithPadding(15).WithBorderRadius(6).WithMarginBottom(20)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center);

            controlPanel.AddChild(new ForgeLabelBuilder("STATUS: IDLE")
                .WithColor(Color.gray).WithBold(true).WithFontSize(14)
                .OnBuild(ve => _lblStatus = ve as Label));

            controlPanel.AddChild(new ForgeButtonBuilder("▶ RUN AUTOMATED BENCHMARK")
                .WithBackgroundColor(new Color(0.2f, 0.6f, 0.2f)).WithTextColor(Color.white).WithBold(true)
                .WithHeight(40).WithWidth(250)
                .OnBuild(ve => _btnRun = ve as Button)
                .OnClick(StartBenchmark));

            root.AddChild(controlPanel);

            // --- 2. RESULTS TABLE ---
            var tableContainer = new GraphicalUserInterfaceBuilder("TableContainer")
                .WithFlexGrow(1).WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderWidth(1).WithBorderAllColor(Color.gray);

            // Table Header
            var tableHeader = new GraphicalUserInterfaceBuilder("TableHeader")
                .WithFlexLayout(FlexDirection.Row).WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithPadding(10).WithBorderBottomWidth(2).WithBorderBottomColor(Color.gray)
                .AddChild(CreateCell("INTERVAL", 80, true))
                .AddChild(CreateCell("AGENTS", 100, true))
                .AddChild(CreateCell("FPS (LOGIC)", 100, true))
                .AddChild(CreateCell("OPS / FRAME", 120, true))
                .AddChild(CreateCell("MEM DELTA (MB)", 120, true));

            tableContainer.AddChild(tableHeader);

            // Table Body
            tableContainer.AddChild(new GraphicalUserInterfaceBuilder("TableBody")
                .WithFlexGrow(1).WithScrollable(true)
                .OnBuild(ve => _resultsTableArea = ve));

            root.AddChild(tableContainer);

            var visualRoot = root.Build();

            // --- 3. THE ASYNC BATCH PROCESSOR ---
            visualRoot.schedule.Execute(() =>
            {
                if (!_isRunning) return;
                ProcessNextBatch();
            }).Every(100); // Give Unity 100ms to render the UI before freezing for the next 60-frame logic batch

            return visualRoot;
        }

        private void StartBenchmark()
        {
            if (_isRunning) return;

            if (_resultsTableArea != null) _resultsTableArea.Clear();

            _currentRoundIndex = 0;
            _currentLoad = _rounds[_currentRoundIndex].StartLoad;
            _isRunning = true;

            if (_btnRun != null) _btnRun.SetEnabled(false);
            if (_lblStatus != null)
            {
                _lblStatus.text = "STATUS: BENCHMARK IN PROGRESS... (Editor may stutter during evaluation)";
                _lblStatus.style.color = Color.yellow;
            }
        }

        private void ProcessNextBatch()
        {
            var round = _rounds[_currentRoundIndex];
            string groupName = "BenchmarkExecutionGroup";

            // 1. Setup
            var stressContext = new Stress(_currentLoad, round.Interval, groupName);
            stressContext.SpawnWorkers();
            FSM_API.Interaction.Update(groupName); // Settle tick

            // 2. Execution (Pure Logic Measurement)
            long totalOps = 0;
            double totalTimeMs = 0;
            Stopwatch batchTimer = new Stopwatch();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            long startMem = GC.GetTotalMemory(true);

            for (int frame = 0; frame < _framesPerBatch; frame++)
            {
                batchTimer.Restart();
                FSM_API.Interaction.Update(groupName);

                // Aggregate metrics
                foreach (var wHandle in stressContext.Workers)
                {
                    if (wHandle.Context is Stress wCtx)
                    {
                        totalOps += wCtx.LastFrameOps;
                        wCtx.LastFrameOps = 0;
                    }
                }
                batchTimer.Stop();
                totalTimeMs += batchTimer.Elapsed.TotalMilliseconds;
            }

            // 3. Analysis
            double avgFrameTime = totalTimeMs / _framesPerBatch;
            double fps = 1000.0 / avgFrameTime;
            long opsPerFrame = totalOps / _framesPerBatch;
            long memUsed = (GC.GetTotalMemory(false) - startMem) / 1024 / 1024;

            // 4. Update UI
            AddResultRowToTable(round.Interval, _currentLoad, fps, opsPerFrame, memUsed);

            // 5. Cleanup
            foreach (var w in stressContext.Workers) FSM_API.Interaction.DestroyInstance(w);
            FSM_API.Interaction.DestroyInstance(stressContext.Status);
            stressContext.IsValid = false;

            // FSM_API.Interaction.DestroyProcessingGroup(groupName); // Optional deep cleanup
            FSM_API.Interaction.DestroyFiniteStateMachine($"WorkerFSM_{round.Interval}", groupName);

            // 6. Progress Routing
            if (fps < _minFpsThreshold)
            {
                // We broke the threshold. Move to next interval round.
                _currentRoundIndex++;
                if (_currentRoundIndex >= _rounds.Count)
                {
                    // Benchmark Complete
                    _isRunning = false;
                    if (_btnRun != null) _btnRun.SetEnabled(true);
                    if (_lblStatus != null)
                    {
                        _lblStatus.text = "STATUS: BENCHMARK COMPLETE";
                        _lblStatus.style.color = Color.green;
                    }
                }
                else
                {
                    // Setup next interval starting load
                    _currentLoad = _rounds[_currentRoundIndex].StartLoad;
                    AddSeparatorToTable($"--- SHIFTING TO INTERVAL {_rounds[_currentRoundIndex].Interval} ---");
                }
            }
            else
            {
                // We passed. Increase load for next batch.
                _currentLoad += round.Increment;
            }
        }

        // ==========================================
        // UI HELPERS
        // ==========================================
        private void AddResultRowToTable(int interval, int agents, double fps, long ops, long mem)
        {
            if (_resultsTableArea == null) return;

            bool isWarning = fps < 35.0; // Highlight rows nearing the failure limit
            Color rowColor = isWarning ? new Color(0.4f, 0.1f, 0.1f) : new Color(0.1f, 0.1f, 0.12f);
            Color txtColor = isWarning ? Color.yellow : Color.silver;

            var row = new GraphicalUserInterfaceBuilder($"Result_{agents}")
                .WithFlexLayout(FlexDirection.Row).WithBackgroundColor(rowColor)
                .WithPadding(10).WithBorderBottomWidth(1).WithBorderBottomColor(new Color(0.2f, 0.2f, 0.2f))
                .AddChild(CreateCell(interval.ToString(), 80, false, txtColor))
                .AddChild(CreateCell(agents.ToString("N0"), 100, isWarning, txtColor))
                .AddChild(CreateCell($"{fps:F2}", 100, isWarning, txtColor))
                .AddChild(CreateCell(ops.ToString("N0"), 120, false, txtColor))
                .AddChild(CreateCell(mem.ToString(), 120, false, txtColor));

            _resultsTableArea.Add(row.Build());

            // Auto-scroll to bottom (simulated via layout tricks if necessary, but visually it appends)
        }

        private void AddSeparatorToTable(string text)
        {
            if (_resultsTableArea == null) return;
            var sep = new GraphicalUserInterfaceBuilder("Separator")
                .WithPadding(15).WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .AddChild(new ForgeLabelBuilder(text).WithColor(Color.cyan).WithBold(true).WithTextAlign(TextAnchor.MiddleCenter));
            _resultsTableArea.Add(sep.Build());
        }

        private IGuiProvider CreateCell(string text, float width, bool bold = false, Color? color = null)
        {
            return new ForgeLabelBuilder(text)
                .WithWidth(width)
                .WithColor(color ?? Color.gray)
                .WithBold(bold);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}