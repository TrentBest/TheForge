using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    /// <summary>
    /// A floating, minimizable telemetry overlay for monitoring and routing FSM workloads 
    /// across Native CPU, C# Jobs, and GPU Compute architectures.
    /// </summary>
    public class Workshop_Gui_ComputeProfiler : IGuiProvider
    {
        public string Title => "COMPUTE PROFILER OVERLAY";

        private GuiContext _guiContext;
        private VisualElement _rootContainer;

        // State
        private bool _isExpanded = false;
        private string _activeTab = "RUNTIME"; // RUNTIME or EDITOR

        // --- SIMULATED WORKLOAD STATE ---
        // (To be replaced by actual FSM_API Stopwatch telemetry later)
        private int _cpuLoadCount = 1500;
        private int _jobLoadCount = 0;
        private int _gpuLoadCount = 0;

        // --- HISTORICAL GHOST METRICS ---
        // Preserves the time so we can see the performance gains!
        private float _historyCpuTime = 0f;
        private float _historyJobTime = 0f;

        private float _simulatedFps = 120f;
        private float _simulatedRam = 2.4f; // GB

        public VisualElement CreateGui(GuiContext ctx)
        {
            _guiContext = ctx;

            _rootContainer = new VisualElement
            {
                name = "ComputeProfilerRoot",
                style = {
                    position = Position.Absolute,
                    top = 10, right = 10,
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.FlexEnd
                }
            };

            RenderState();

            _rootContainer.RegisterCallback<AttachToPanelEvent>(e => {
                _rootContainer.schedule.Execute(UpdateMetrics).Every(500); // 2Hz update
            });

            return _rootContainer;
        }

        private void RenderState()
        {
            _rootContainer.Clear();

            if (!_isExpanded) _rootContainer.Add(BuildMinimizedPill());
            else _rootContainer.Add(BuildExpandedDashboard());
        }

        // --- MINIMIZED STATE ---

        private VisualElement BuildMinimizedPill()
        {
            var pill = new VisualElement
            {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    backgroundColor = new Color(0.1f, 0.1f, 0.12f, 0.9f),
                    borderTopLeftRadius = 15, borderTopRightRadius = 15, borderBottomLeftRadius = 15, borderBottomRightRadius = 15,
                    borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1,
                    borderTopColor = new Color(0.3f, 0.3f, 0.3f), borderBottomColor = new Color(0.3f, 0.3f, 0.3f), borderLeftColor = new Color(0.3f, 0.3f, 0.3f), borderRightColor = new Color(0.3f, 0.3f, 0.3f),
                    paddingLeft = 15, paddingRight = 5, paddingTop = 5, paddingBottom = 5
                }
            };

            var fpsLbl = new Label($"FPS: {_simulatedFps:F0}") { name = "lbl_fps", style = { color = new Color(0.2f, 0.8f, 0.2f), unityFontStyleAndWeight = FontStyle.Bold, marginRight = 15 } };
            var ramLbl = new Label($"RAM: {_simulatedRam:F1}GB") { name = "lbl_ram", style = { color = new Color(0.4f, 0.8f, 1f), unityFontStyleAndWeight = FontStyle.Bold, marginRight = 15 } };
            var loadLbl = new Label($"FSMs: {_cpuLoadCount + _jobLoadCount + _gpuLoadCount}") { name = "lbl_fsms", style = { color = new Color(0.9f, 0.6f, 0.2f), unityFontStyleAndWeight = FontStyle.Bold, marginRight = 15 } };

            pill.Add(fpsLbl);
            pill.Add(ramLbl);
            pill.Add(loadLbl);

            var expandBtn = new Button(() => { _isExpanded = true; RenderState(); }) { text = "⛶" };
            expandBtn.style.width = 30; expandBtn.style.height = 30; expandBtn.style.borderTopLeftRadius = 15; expandBtn.style.borderTopRightRadius = 15; expandBtn.style.borderBottomLeftRadius = 15; expandBtn.style.borderBottomRightRadius = 15;
            expandBtn.style.backgroundColor = new Color(0.2f, 0.2f, 0.25f); expandBtn.style.color = Color.white;
            pill.Add(expandBtn);

            return pill;
        }

        // --- EXPANDED STATE ---

        private VisualElement BuildExpandedDashboard()
        {
            var dashboard = new VisualElement
            {
                style = {
                    width = 900,
                    backgroundColor = new Color(0.08f, 0.08f, 0.1f, 0.95f),
                    borderTopWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, borderRightWidth = 2,
                    borderTopColor = new Color(0.4f, 0.8f, 1f, 0.5f), borderBottomColor = new Color(0.4f, 0.8f, 1f, 0.5f), borderLeftColor = new Color(0.4f, 0.8f, 1f, 0.5f), borderRightColor = new Color(0.4f, 0.8f, 1f, 0.5f),
                    borderTopLeftRadius = 8, borderTopRightRadius = 8, borderBottomLeftRadius = 8, borderBottomRightRadius = 8,
                    flexDirection = FlexDirection.Column
                }
            };

            var header = new VisualElement { style = { flexDirection = FlexDirection.Row, backgroundColor = new Color(0.12f, 0.12f, 0.15f), borderBottomWidth = 1, borderBottomColor = Color.gray } };

            var runtimeTab = new Button(() => { _activeTab = "RUNTIME"; RenderState(); }) { text = "RUNTIME PIPELINE" };
            StyleTab(runtimeTab, _activeTab == "RUNTIME");
            header.Add(runtimeTab);

            var editorTab = new Button(() => { _activeTab = "EDITOR"; RenderState(); }) { text = "EDITOR OVERHEAD" };
            StyleTab(editorTab, _activeTab == "EDITOR");
            header.Add(editorTab);

            var spacer = new VisualElement { style = { flexGrow = 1 } };
            header.Add(spacer);

            var minBtn = new Button(() => { _isExpanded = false; RenderState(); }) { text = "✖" };
            minBtn.style.width = 40; minBtn.style.backgroundColor = Color.clear; minBtn.style.color = Color.red; minBtn.style.fontSize = 16; minBtn.style.borderTopWidth = 0; minBtn.style.borderBottomWidth = 0; minBtn.style.borderLeftWidth = 0; minBtn.style.borderRightWidth = 0;
            header.Add(minBtn);

            dashboard.Add(header);

            var content = new ScrollView(ScrollViewMode.Vertical) { style = { paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15 } };

            if (_activeTab == "RUNTIME")
            {
                content.Add(BuildDynamicLoopVisualizer());
                content.Add(new VisualElement { style = { height = 2, backgroundColor = new Color(0.2f, 0.2f, 0.25f), marginTop = 20, marginBottom = 20 } });
                content.Add(BuildThreeColumnComputeEngine());
            }
            else
            {
                content.Add(new Label("Editor overhead profiling tools go here...") { style = { color = Color.gray } });
            }

            dashboard.Add(content);
            return dashboard;
        }

        private void StyleTab(Button btn, bool isActive)
        {
            btn.style.flexGrow = 1; btn.style.maxWidth = 200; btn.style.height = 40;
            btn.style.borderTopWidth = 0; btn.style.borderLeftWidth = 0; btn.style.borderRightWidth = 0;
            btn.style.borderBottomWidth = isActive ? 3 : 0;
            btn.style.borderBottomColor = new Color(0.4f, 0.8f, 1f);
            btn.style.backgroundColor = isActive ? new Color(0.15f, 0.15f, 0.18f) : Color.clear;
            btn.style.color = isActive ? Color.white : Color.gray;
            btn.style.unityFontStyleAndWeight = FontStyle.Bold;
        }

        // --- TOP HALF: DYNAMIC LOOP VISUALIZER ---

        private VisualElement BuildDynamicLoopVisualizer()
        {
            var container = new VisualElement();
            container.Add(new Label("DYNAMIC UPDATE LOOP TOPOLOGY") { style = { color = new Color(0.8f, 0.8f, 0.8f), fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } });

            var loopFlow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };

            Func<string, string[], Color, VisualElement> BuildPhase = (phaseName, groups, color) => {
                var phaseCol = new VisualElement { style = { width = 250, backgroundColor = new Color(0.1f, 0.1f, 0.1f), borderTopWidth = 3, borderTopColor = color, paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10, } };
                phaseCol.Add(new Label(phaseName) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10, unityTextAlign = TextAnchor.MiddleCenter } });

                foreach (var g in groups)
                {
                    var groupBlk = new VisualElement { style = { backgroundColor = new Color(0.15f, 0.15f, 0.15f), paddingTop = 8, paddingBottom = 8, paddingLeft = 8, paddingRight = 8, marginBottom = 5, borderLeftWidth = 2, borderLeftColor = color } };
                    groupBlk.Add(new Label(g) { style = { color = Color.gray, fontSize = 12 } });
                    groupBlk.Add(new Label("FSMs: " + UnityEngine.Random.Range(10, 500)) { style = { color = new Color(0.4f, 0.8f, 1f), fontSize = 10, unityTextAlign = TextAnchor.MiddleRight } });
                    phaseCol.Add(groupBlk);
                }
                return phaseCol;
            };

            loopFlow.Add(BuildPhase("1. PRE-UPDATE", new[] { "Input Routing", "Network Sync" }, new Color(0.2f, 0.6f, 0.2f)));

            var updatePhase = BuildPhase("2. MAIN UPDATE", new[] { "Physics Arbitration", "Agent Brains", "[INJECTED] Boss Logic" }, new Color(0.9f, 0.6f, 0.1f));
            var injectedBlk = updatePhase.ElementAt(3);
            injectedBlk.style.borderLeftColor = Color.red;
            injectedBlk.style.backgroundColor = new Color(0.3f, 0.1f, 0.1f);
            ((Label)injectedBlk.ElementAt(0)).style.color = Color.white;
            ((Label)injectedBlk.ElementAt(0)).text += " (ACTIVE)";

            loopFlow.Add(updatePhase);
            loopFlow.Add(BuildPhase("3. POST-UPDATE", new[] { "Causality Resolution", "Cleanup" }, new Color(0.2f, 0.4f, 0.8f)));

            container.Add(loopFlow);
            return container;
        }

        // --- BOTTOM HALF: 3-COLUMN COMPUTE ROUTER ---

        private VisualElement BuildThreeColumnComputeEngine()
        {
            var container = new VisualElement();

            var headerRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center, marginBottom = 15 } };
            headerRow.Add(new Label("WORKLOAD ROUTING & EXECUTION PIPELINE") { style = { color = new Color(0.8f, 0.8f, 0.8f), fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold } });

            var addLoadBtn = new Button(() => {
                _cpuLoadCount += 1000;
                _historyCpuTime = 0; // Clear history if we are adding fresh load
                RenderState();
            })
            { text = "+ SPAWN 1000x FSM INSTANCES" };
            addLoadBtn.style.backgroundColor = new Color(0.1f, 0.4f, 0.1f); addLoadBtn.style.color = Color.white; addLoadBtn.style.paddingLeft = 10; addLoadBtn.style.paddingRight = 10;
            headerRow.Add(addLoadBtn);

            container.Add(headerRow);

            var cols = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            // CPU Column
            var colCpu = BuildComputeColumn("NATIVE CPU (MAIN THREAD)", _cpuLoadCount, 12.4f, new Color(0.8f, 0.3f, 0.3f), true, _historyCpuTime);
            cols.Add(colCpu);

            // CPU -> Jobs Arrow
            var arrow1 = new Button(() => {
                if (_cpuLoadCount > 0)
                {
                    // Capture the historical execution time before we move it
                    _historyCpuTime = 12.4f * (_cpuLoadCount / 1000f);
                    _jobLoadCount += _cpuLoadCount;
                    _cpuLoadCount = 0;
                    RenderState();
                }
            })
            { text = "▶\nJOBIFY" };
            StyleArrow(arrow1);
            cols.Add(arrow1);

            // Jobs Column
            var colJobs = BuildComputeColumn("BURST COMPILER (C# JOBS)", _jobLoadCount, 1.8f, new Color(0.9f, 0.7f, 0.1f), true, _historyJobTime);
            cols.Add(colJobs);

            // Jobs -> GPU Arrow
            var arrow2 = new Button(() => {
                if (_jobLoadCount > 0)
                {
                    // Capture the historical execution time before we move it
                    _historyJobTime = 1.8f * (_jobLoadCount / 1000f);
                    _gpuLoadCount += _jobLoadCount;
                    _jobLoadCount = 0;
                    RenderState();
                }
            })
            { text = "▶\nGPU DISPATCH" };
            StyleArrow(arrow2);
            cols.Add(arrow2);

            // GPU Column
            var colGpu = BuildComputeColumn("COMPUTE SHADER (GPU VRAM)", _gpuLoadCount, 0.2f, new Color(0.2f, 0.8f, 0.4f), false, 0f);
            cols.Add(colGpu);

            container.Add(cols);
            return container;
        }

        private VisualElement BuildComputeColumn(string title, int loadCount, float baseMsTime, Color themeColor, bool showList, float historyTime)
        {
            var col = new VisualElement { style = { flexGrow = 1, flexBasis = 0, backgroundColor = new Color(0.1f, 0.1f, 0.12f), paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10, borderTopWidth = 4, borderTopColor = themeColor } };

            col.Add(new Label(title) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10, unityTextAlign = TextAnchor.MiddleCenter } });

            // Metrics Card
            var metrics = new VisualElement { style = { backgroundColor = new Color(0.05f, 0.05f, 0.05f), paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10, marginBottom = 10 } };
            metrics.Add(new Label($"ACTIVE INSTANCES: {loadCount:N0}") { style = { color = Color.white, fontSize = 14, marginBottom = 5 } });

            float actualTime = loadCount > 0 ? (baseMsTime * (loadCount / 1000f)) : 0f;
            metrics.Add(new Label($"EXECUTION TIME: {actualTime:F2} ms") { style = { color = themeColor, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold } });
            col.Add(metrics);

            // THE GHOST METRIC: If workload was moved, display what it *used* to cost.
            if (loadCount == 0 && historyTime > 0)
            {
                var empty = new VisualElement { style = { height = 150, justifyContent = Justify.Center, alignItems = Align.Center, backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f) } };
                empty.Add(new Label("WORKLOAD MIGRATED") { style = { color = new Color(0.4f, 0.4f, 0.4f), unityFontStyleAndWeight = FontStyle.Bold } });

                // Show the preserved time in grey/red to indicate "this is what we avoided"
                empty.Add(new Label($"Previous Cost: {historyTime:F2} ms") { style = { color = new Color(0.8f, 0.3f, 0.3f), marginTop = 10, unityFontStyleAndWeight = FontStyle.Italic } });
                col.Add(empty);
            }
            else if (showList && loadCount > 0)
            {
                var list = new ScrollView(ScrollViewMode.Vertical) { style = { height = 150, backgroundColor = new Color(0.08f, 0.08f, 0.08f), borderLeftWidth = 1, borderRightWidth = 1, borderTopWidth = 1, borderBottomWidth = 1, borderTopColor = new Color(0.2f, 0.2f, 0.2f), borderBottomColor = new Color(0.2f, 0.2f, 0.2f), borderLeftColor = new Color(0.2f, 0.2f, 0.2f), borderRightColor = new Color(0.2f, 0.2f, 0.2f) } };

                int blocks = Mathf.Min(loadCount / 250 + 1, 20);
                for (int i = 0; i < blocks; i++)
                {
                    int itemsInBlock = (i == blocks - 1) ? (loadCount % 250 == 0 ? 250 : loadCount % 250) : 250;
                    var item = new Label($"[Group_{i}] FSMs: {itemsInBlock}") { style = { color = Color.gray, paddingTop = 5, paddingBottom = 5, paddingLeft = 5, paddingRight = 5, borderBottomWidth = 1, borderBottomColor = new Color(0.15f, 0.15f, 0.15f) } };
                    list.Add(item);
                }
                col.Add(list);
            }
            else if (loadCount == 0)
            {
                var empty = new VisualElement { style = { height = 150, justifyContent = Justify.Center, alignItems = Align.Center } };
                empty.Add(new Label("NO WORKLOAD") { style = { color = new Color(0.3f, 0.3f, 0.3f), unityFontStyleAndWeight = FontStyle.Bold } });
                col.Add(empty);
            }

            return col;
        }

        private void StyleArrow(Button btn)
        {
            btn.style.width = 60;
            btn.style.backgroundColor = Color.clear;
            btn.style.borderTopWidth = 0; btn.style.borderBottomWidth = 0; btn.style.borderLeftWidth = 0; btn.style.borderRightWidth = 0;
            btn.style.color = new Color(0.4f, 0.8f, 1f);
            btn.style.unityFontStyleAndWeight = FontStyle.Bold;
            btn.style.fontSize = 14;
            btn.style.whiteSpace = WhiteSpace.Normal;
        }

        // --- UPDATE LOOP ---

        private void UpdateMetrics()
        {
            if (_rootContainer == null) return;

            _simulatedFps = Mathf.Clamp(_simulatedFps + UnityEngine.Random.Range(-2f, 2f), 55f, 144f);
            _simulatedRam = Mathf.Clamp(_simulatedRam + UnityEngine.Random.Range(-0.05f, 0.05f), 1.5f, 6.0f);

            if (!_isExpanded)
            {
                var fpsLbl = _rootContainer.Q<Label>("lbl_fps");
                if (fpsLbl != null) fpsLbl.text = $"FPS: {_simulatedFps:F0}";

                var ramLbl = _rootContainer.Q<Label>("lbl_ram");
                if (ramLbl != null) ramLbl.text = $"RAM: {_simulatedRam:F1}GB";
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string path) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), path);
#endif
        public void FromUIDocument(string path) => _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(path));
    }
}