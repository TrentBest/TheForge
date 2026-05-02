using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics; // Injected for ForgeLogger
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools;

namespace Assets.Scripts.Workshop.Core.Diagnostics
{
    public class Workshop_Gui_StressTest : IGuiProvider
    {
        public string Title => "FSM STRESS TESTER";

        // State & Backend
        private Stress _stressManager;
        private int _targetWorkerCount = 10000;
        private int _targetTickInterval = 1;
        private string _workloadType = "Standard Math";

        // UI Telemetry Hooks (Captured via OnBuild)
        private Label _lblWorkerCount;
        private Label _lblTickInterval;
        private Label _lblActiveInstances;
        private Label _lblTheoreticalOps;
        private Label _lblFps;
        private Label _lblMemEstimate;

        // FPS Tracking
        private int _frameCount = 0;
        private float _dt = 0.0f;
        private float _fps = 0.0f;

        public VisualElement CreateGui(GuiContext ctx)
        {
            ForgeLogger.Log("Stress Test UI Manifesting.")
                .WithHeader("Diagnostics")
                .WithColor("#00FFCC")
                .SendToUnity();

            var root = new GraphicalUserInterfaceBuilder("StressTestRoot")
                .WithFlexGrow(1)
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f));

            root.AddHeader("FSM ENGINE: BRUTE FORCE STRESS TEST", Color.red);

            // --- 1. CONFIGURATION CONTROLS ---
            var configPanel = new GraphicalUserInterfaceBuilder("ConfigPanel")
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f))
                .WithPadding(15).WithBorderRadius(6).WithMarginBottom(20)
                .WithBorderTopWidth(2).WithBorderTopColor(Color.red);

            configPanel.AddChild(new ForgeLabelBuilder("SIMULATION PARAMETERS").WithColor(Color.gray).WithBold(true).WithMarginBottom(10));

            // Target Workers Control
            var workerControlRow = new GraphicalUserInterfaceBuilder("WorkerControls")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center).WithMarginBottom(10)
                .AddChild(new ForgeLabelBuilder("Target Instance Count:").WithColor(Color.silver))
                .AddChild(new ForgeLabelBuilder(_targetWorkerCount.ToString("N0"))
                    .WithColor(Color.white).WithBold(true).WithFontSize(18)
                    .OnBuild(ve => _lblWorkerCount = ve as Label));

            var workerButtons = new GraphicalUserInterfaceBuilder("WorkerBtns").WithFlexLayout(FlexDirection.Row, Justify.FlexEnd)
                .AddChild(CreateConfigButton("- 10k", () => AdjustWorkers(-10000)))
                .AddChild(CreateConfigButton("+ 10k", () => AdjustWorkers(10000)))
                .AddChild(CreateConfigButton("+ 50k", () => AdjustWorkers(50000)))
                .AddChild(CreateConfigButton("+ 100k", () => AdjustWorkers(100000)));

            configPanel.AddChild(workerControlRow).AddChild(workerButtons);
            configPanel.AddSeparator(new Color(0.2f, 0.2f, 0.2f), 1);

            // Tick Interval Control
            var intervalControlRow = new GraphicalUserInterfaceBuilder("IntervalControls")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center).WithMarginTop(10).WithMarginBottom(10)
                .AddChild(new ForgeLabelBuilder("Tick Interval (Frames):").WithColor(Color.silver))
                .AddChild(new ForgeLabelBuilder(_targetTickInterval.ToString())
                    .WithColor(Color.white).WithBold(true).WithFontSize(18)
                    .OnBuild(ve => _lblTickInterval = ve as Label));

            var intervalButtons = new GraphicalUserInterfaceBuilder("IntervalBtns").WithFlexLayout(FlexDirection.Row, Justify.FlexEnd)
                .AddChild(CreateConfigButton("60Hz (1)", () => SetInterval(1)))
                .AddChild(CreateConfigButton("12Hz (5)", () => SetInterval(5)))
                .AddChild(CreateConfigButton("6Hz (10)", () => SetInterval(10)));

            configPanel.AddChild(intervalControlRow).AddChild(intervalButtons);
            root.AddChild(configPanel);

            // --- 2. WORKLOAD COMPLEXITY (Payload Type) ---
            var workloadPanel = new GraphicalUserInterfaceBuilder("WorkloadPanel")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center).WithMarginBottom(20)
                .AddChild(new ForgeLabelBuilder("Payload:").WithColor(Color.gray).WithBold(true))
                .AddChild(CreateConfigButton("LOW (Math)", () => SetWorkload("Math")))
                .AddChild(CreateConfigButton("HEAVY (Sqrt/Muscles)", () => SetWorkload("Heavy")))
                .AddChild(CreateConfigButton("ASYNC (Latency Simulation)", () => SetWorkload("Async")));

            root.AddChild(workloadPanel);

            // --- 3. ACTION PANEL ---
            var actionRow = new GraphicalUserInterfaceBuilder("ActionRow")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween).WithMarginBottom(20)
                .AddChild(new ForgeButtonBuilder("🚀 INITIATE PROTOCOL")
                    .WithBackgroundColor(new Color(0.8f, 0.1f, 0.1f)).WithTextColor(Color.white).WithBold(true)
                    .WithHeight(50).WithFlexGrow(1).WithMarginRight(5)
                    .OnClick(InitiateStressTest))
                .AddChild(new ForgeButtonBuilder("🛑 ABORT & PURGE")
                    .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f)).WithTextColor(Color.white).WithBold(true)
                    .WithHeight(50).WithFlexGrow(1).WithMarginLeft(5)
                    .OnClick(AbortTest));

            root.AddChild(actionRow);

            // --- 4. LIVE TELEMETRY ---
            root.AddHeader("LIVE ENGINE TELEMETRY", Color.cyan);
            var telemetryPanel = new GraphicalUserInterfaceBuilder("TelemetryPanel")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween);

            telemetryPanel.AddChild(CreateTelemetryCard("ACTIVE WORKERS", "0", Color.cyan, ve => _lblActiveInstances = ve as Label));
            telemetryPanel.AddChild(CreateTelemetryCard("THEORETICAL OPS", "0", Color.yellow, ve => _lblTheoreticalOps = ve as Label));
            telemetryPanel.AddChild(CreateTelemetryCard("FPS", "00.0", Color.green, ve => _lblFps = ve as Label));
            telemetryPanel.AddChild(CreateTelemetryCard("UNMANAGED MEM", "0MB", Color.magenta, ve => _lblMemEstimate = ve as Label));

            root.AddChild(telemetryPanel);

            // --- 5. THE UI TICK (Isolated from Backend FSMs) ---
            var builtUi = root.Build();

            builtUi.schedule.Execute(() =>
            {
                // Calculate Engine FPS
                _frameCount++;
                _dt += Time.unscaledDeltaTime;
                if (_dt > 0.5f)
                {
                    _fps = _frameCount / _dt;
                    _frameCount = 0;
                    _dt -= 0.5f;

                    if (_lblFps != null)
                    {
                        _lblFps.text = $"{_fps:F1}";
                        _lblFps.style.color = _fps > 55 ? Color.green : (_fps > 30 ? Color.yellow : Color.red);
                    }
                }

                // Sample the FSM backend state
                if (_lblActiveInstances != null && _lblTheoreticalOps != null)
                {
                    if (_stressManager != null && _stressManager.IsValid)
                    {
                        int activeCount = _stressManager.Workers.Count;
                        _lblActiveInstances.text = activeCount.ToString("N0");

                        // Informative load assessment
                        int opsMultiplier = _workloadType == "Heavy" ? 15 : 3;
                        long totalOps = (long)activeCount * (long)opsMultiplier;
                        _lblTheoreticalOps.text = totalOps.ToString("N0");

                        // Estimated unmanaged footprint (FSMAgentData ~36-52 bytes)
                        float mb = (activeCount * 52f) / (1024f * 1024f);
                        _lblMemEstimate.text = $"{mb:F2} MB";
                    }
                    else
                    {
                        _lblActiveInstances.text = "0";
                        _lblTheoreticalOps.text = "0";
                        _lblMemEstimate.text = "0MB";
                    }
                }
            }).Every(16);

            return builtUi;
        }

        private void AdjustWorkers(int amount)
        {
            _targetWorkerCount = Mathf.Max(0, _targetWorkerCount + amount);
            if (_lblWorkerCount != null) _lblWorkerCount.text = _targetWorkerCount.ToString("N0");

            ForgeLogger.Log($"Stress Target Adjusted: {_targetWorkerCount} instances.")
                .WithHeader("Config")
                .WithColor(Color.yellow)
                .SendToUnity();
        }

        private void SetInterval(int interval)
        {
            _targetTickInterval = Mathf.Max(1, interval);
            if (_lblTickInterval != null) _lblTickInterval.text = _targetTickInterval.ToString();

            ForgeLogger.Log($"Tick Interval Set: every {interval} frames.")
                .WithHeader("Config")
                .WithColor(Color.yellow)
                .SendToUnity();
        }

        private void SetWorkload(string type)
        {
            _workloadType = type;
            ForgeLogger.Log($"Payload Complexity switched to: {type}")
                .WithHeader("Config")
                .WithColor(Color.magenta)
                .SendToUnity();
        }

        private void InitiateStressTest()
        {
            AbortTest();

            ForgeLogger.Log($"CRITICAL STRESS INITIATED: Spawning {_targetWorkerCount} agents [Type: {_workloadType}]")
                .WithHeader("Stress Protocol")
                .WithColor("#FF3333") // Bright Alert Red
                .SendToUnity();

            _stressManager = new Stress(_targetWorkerCount, _targetTickInterval, "StressExecutionGroup");
            _stressManager.SpawnWorkers();
        }

        private void AbortTest()
        {
            if (_stressManager != null)
            {
                ForgeLogger.Log("Purging stress-test agents and clearing Execution Group.")
                    .WithHeader("Abort")
                    .WithColor(Color.red)
                    .SendToUnity();

                _stressManager.Workers.Clear();
                _stressManager.IsValid = false;
                _stressManager = null;
            }
        }

        private IGuiProvider CreateConfigButton(string label, Action onClick)
        {
            return new ForgeButtonBuilder(label)
                .WithBackgroundColor(new Color(0.2f, 0.2f, 0.25f))
                .WithTextColor(Color.white).WithMarginLeft(5).WithPadding(5, 10, 5, 10)
                .OnClick(onClick);
        }

        private IGuiProvider CreateTelemetryCard(string title, string defaultVal, Color accentColor, Action<VisualElement> labelResolver)
        {
            return new GraphicalUserInterfaceBuilder($"Telemetry_{title}")
                .WithFlexGrow(1).WithMarginRight(5).WithMarginLeft(5).WithPadding(15)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithBorderTopWidth(3).WithBorderTopColor(accentColor)
                .AddChild(new ForgeLabelBuilder(title).WithColor(Color.gray).WithFontSize(10).WithBold(true).WithMarginBottom(5))
                .AddChild(new ForgeLabelBuilder(defaultVal).WithColor(Color.white).WithFontSize(24).WithBold(true)
                    .OnBuild(labelResolver));
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}