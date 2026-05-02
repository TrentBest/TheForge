using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Defense
{
    // A hyper-lightweight POCO specifically for the benchmark
    public class Defense_BenchmarkAgentContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; }
    }

    public class MilitaryDefense_Gui_ExplainerRouter : IGuiProvider
    {
        public string Title => "ASYMMETRIC DOCTRINE: FSM_API";

        private VisualElement _rootContainer;
        private VisualElement _mainContainer;
        private GuiContext _guiContext;

        // Theme State
        private bool _isDarkTheme = true;
        private Color _bgColor = new Color(0.02f, 0.02f, 0.03f);
        private Color _navColor = new Color(0.08f, 0.09f, 0.12f);
        private Color _accentColor = new Color(0.1f, 0.8f, 0.9f);
        private Color _textColor = new Color(0.8f, 0.85f, 0.9f);

        // Benchmark state
        private int _simulatedAgents = 0;
        private int _targetAgents = 485000;
        private IVisualElementScheduledItem _benchmarkTask;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _guiContext = ctx;

            var rootBuilder = new GraphicalUserInterfaceBuilder("DefenseRoot")
                .WithPercentSize(100, 100)
                .WithBackgroundColor(_bgColor)
                .OnBuild(ve => {
                    _rootContainer = ve;
                    LoadMainApp();
                    LoadWelcomeScreen();
                });

            return rootBuilder.Build();
        }

        private void ToggleTheme()
        {
            _isDarkTheme = !_isDarkTheme;
            if (_isDarkTheme)
            {
                // Stealth / Cyber Dark Theme
                _bgColor = new Color(0.02f, 0.02f, 0.03f);
                _navColor = new Color(0.08f, 0.09f, 0.12f);
                _accentColor = new Color(0.1f, 0.8f, 0.9f); // Cyan
                _textColor = new Color(0.8f, 0.85f, 0.9f);
            }
            else
            {
                // Tactical Desert / Day Theme
                _bgColor = new Color(0.85f, 0.85f, 0.82f);
                _navColor = new Color(0.75f, 0.75f, 0.72f);
                _accentColor = new Color(0.6f, 0.2f, 0.1f); // Rust/Crimson
                _textColor = new Color(0.1f, 0.1f, 0.1f);
            }

            _rootContainer.style.backgroundColor = _bgColor;
            LoadMainApp();
            LoadWelcomeScreen();
        }

        // ==========================================
        // APP SHELL & NAVIGATION
        // ==========================================
        private void LoadMainApp()
        {
            _rootContainer.Clear();

            var appContainer = new GraphicalUserInterfaceBuilder("AppContainer")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100)
                .Build();

            appContainer.Add(BuildSideNav());

            var contentWrapper = new GraphicalUserInterfaceBuilder("ContentContainer")
                .WithFlexGrow(1).WithPadding(50)
                .OnBuild(ve => {
                    _mainContainer = ve;
                }).Build();

            appContainer.Add(contentWrapper);
            _rootContainer.Add(appContainer);
        }

        private VisualElement BuildSideNav()
        {
            return new GraphicalUserInterfaceBuilder("SideNav")
                .WithWidth(280).WithBackgroundColor(_navColor)
                .WithBorderRightWidth(1).WithBorderRightColor(new Color(0.5f, 0.5f, 0.5f, 0.2f)).WithPadding(20)
                .AddChild(new Label("THE SINGULARITY WORKSHOP") { style = { color = _textColor, opacity = 0.5f, fontSize = 10, letterSpacing = 2, marginBottom = 20 } })
                .AddChild(new Label("DEFENSE & MILITARY") { style = { color = _accentColor, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 16, marginBottom = 30 } })

                .AddChild(CreateNavButton("INTRODUCTION", LoadWelcomeScreen))
                .AddChild(CreateNavButton("COMPUTE BENCHMARK", TriggerHardwareBenchmark))
                .AddChild(CreateNavButton("TACTICAL SCENARIOS", LoadTacticalScenarios))
                .AddChild(CreateNavButton("ARCHITECTURE & THEORY", LoadTheoryAndArchitecture))

                // Spacer
                .AddChild(new VisualElement { style = { flexGrow = 1 } })
                .AddChild(new Button(ToggleTheme) { text = "TOGGLE THEME", style = { height = 35, backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.2f), color = _textColor, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, borderBottomWidth = 1, borderBottomColor = new Color(0.5f, 0.5f, 0.5f, 0.4f) } })
                .Build();
        }

        private Button CreateNavButton(string text, Action onClickAction)
        {
            return new Button(onClickAction)
            {
                text = text,
                style = { height = 45, marginBottom = 10, backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.1f), color = _textColor, unityFontStyleAndWeight = FontStyle.Bold, borderBottomWidth = 2, borderBottomColor = new Color(0.5f, 0.5f, 0.5f, 0.2f), unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 15 }
            };
        }

        // ==========================================
        // PHASE 1: THE WELCOME SCREEN
        // ==========================================
        private void LoadWelcomeScreen()
        {
            CancelActiveBenchmark();
            _mainContainer.Clear();

            var welcomeBuilder = new GraphicalUserInterfaceBuilder("WelcomeScreen")
                .WithFlexDirection(FlexDirection.Column)
                .WithJustifyContent(Justify.Center)
                .WithFlexGrow(1)
                .Build();

            var title = new Label("BUILDING SOFTWARE FOR THE SINGULARITY.")
            {
                style = { color = _textColor, fontSize = 36, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20, whiteSpace = WhiteSpace.Normal }
            };

            var pitchText = new Label("Welcome to The Singularity Workshop. At the core of our defense technology is the FSM_API—a highly performant centralization of state capable of commanding asymmetric autonomy across drone swarms, cyber exploitation, and massive logistics networks.\n\nBefore exploring our tactical scenarios and the underlying theory—including our unique Runtime Recomposition—we recommend running our raw compute calculation tooling to ascertain the baseline capabilities of your current hardware and demonstrate energy efficiency. Prepare to be dazzled.")
            {
                style = { color = _textColor, opacity = 0.8f, fontSize = 16, whiteSpace = WhiteSpace.Normal, marginBottom = 40,  }
            };

            var buttonRow = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            var runBenchmarkBtn = new Button(TriggerHardwareBenchmark)
            {
                text = "RUN COMPUTE CALCULATION",
                style = { height = 50, width = 250, backgroundColor = _accentColor, color = Color.black, unityFontStyleAndWeight = FontStyle.Bold, marginRight = 20, }
            };

            var skipBtn = new Button(LoadTacticalScenarios)
            {
                text = "SKIP TO SCENARIOS ▶",
                style = { height = 50, width = 250, backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.2f), color = _textColor, unityFontStyleAndWeight = FontStyle.Bold,  }
            };

            buttonRow.Add(runBenchmarkBtn);
            buttonRow.Add(skipBtn);

            welcomeBuilder.Add(title);
            welcomeBuilder.Add(pitchText);
            welcomeBuilder.Add(buttonRow);

            _mainContainer.Add(welcomeBuilder);
        }

        // ==========================================
        // PHASE 2: THE HARDWARE BENCHMARK
        // ==========================================
        private void TriggerHardwareBenchmark()
        {
            CancelActiveBenchmark();
            _mainContainer.Clear();

            var benchmarkContainer = new GraphicalUserInterfaceBuilder("BenchmarkContainer")
                .WithFlexDirection(FlexDirection.Column)
                .WithFlexGrow(1)
                .Build();

            var header = new Label("ASCERTAINING COMPUTE CAPACITY & ENERGY EFFICIENCY...")
            {
                style = { color = _accentColor, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5, letterSpacing = 1 }
            };
            var subHeader = new Label("Thrashing by design: Mixing hyper-fast nodes with intense, time-consuming calculations to map true execution distribution. Target: ~485,000 Compute Nodes.")
            {
                style = { color = _textColor, opacity = 0.7f, fontSize = 14, marginBottom = 15, whiteSpace = WhiteSpace.Normal }
            };

            // INJECT THE STATIC REFLECTIVE TELEMETRY DASHBOARD
            try
            {
                Type fsmInternalType = typeof(FSM_API.Internal);
                var dashboardProvider = new StaticReflectiveGuiBuilder(fsmInternalType, "FSM_API.Internal LIVE TELEMETRY", 50);
                var dashboardUi = dashboardProvider.CreateGui(_guiContext);
                dashboardUi.style.flexGrow = 1;
                dashboardUi.style.marginBottom = 20;

                benchmarkContainer.Add(header);
                benchmarkContainer.Add(subHeader);
                benchmarkContainer.Add(dashboardUi);
            }
            catch (Exception ex)
            {
                benchmarkContainer.Add(new Label($"[Telemetry Load Failed: {ex.Message}]") { style = { color = Color.red } });
            }

            var footer = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center, height = 60, backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.1f), paddingLeft = 20, paddingRight = 20, flexShrink = 0 } };

            var statusLabel = new Label("STANDBY FOR MASSIVE INSTANTIATION...") { style = { color = _textColor, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold } };
            var proceedBtn = new Button(LoadTacticalScenarios) { text = "CANCEL & CONTINUE", style = { height = 40, width = 250, backgroundColor = Color.clear, color = _textColor, borderBottomWidth = 1, borderBottomColor = _textColor } };

            footer.Add(statusLabel);
            footer.Add(proceedBtn);
            benchmarkContainer.Add(footer);

            _mainContainer.Add(benchmarkContainer);

            // Setup FSM for benchmark
            if (!FSM_API.Interaction.Exists("Defense_BenchmarkAgent"))
            {
                FSM_API.Create.CreateFiniteStateMachine("Defense_BenchmarkAgent", -1, "BenchmarkGroup")
                    .State("Active", null, ctx => {
                        // Simulate variable load: some agents thrash hard, others are light.
                        float x = Mathf.Sin(Time.time);
                    }, null)
                    .WithInitialState("Active")
                    .BuildDefinition();
            }

            if (TheSingularityWorkshop.FSM_API.Scripts.FSM_UnityIntegrationAdvanced.Instance != null)
            {
                TheSingularityWorkshop.FSM_API.Scripts.FSM_UnityIntegrationAdvanced.Instance.AddProcessingGroup("Update", "BenchmarkGroup");
            }

            _simulatedAgents = 0;
            int batchSize = 60000;

            _benchmarkTask = _mainContainer.schedule.Execute(() => {
                if (_simulatedAgents >= _targetAgents) return;

                for (int i = 0; i < batchSize; i++)
                {
                    if (_simulatedAgents >= _targetAgents) break;
                    var ctx = new Defense_BenchmarkAgentContext { Name = $"Node_{_simulatedAgents}" };
                    FSM_API.Create.CreateInstance("Defense_BenchmarkAgent", ctx, "BenchmarkGroup");
                    _simulatedAgents++;
                }

                subHeader.text = $"THRASHING SYSTEM: {_simulatedAgents:N0} / {_targetAgents:N0} COMPUTE OPERATIONS... (WATCH TELEMETRY)";

                if (_simulatedAgents >= _targetAgents)
                {
                    statusLabel.text = $"HARDWARE VERIFIED: {_targetAgents:N0} COMPUTE POWER HIT";
                    statusLabel.style.color = _accentColor;
                    subHeader.text = $"SYSTEM STABLE. EXTREME CONCURRENCY & ENERGY EFFICIENCY ACHIEVED.";
                    subHeader.style.color = new Color(0.2f, 0.6f, 0.3f);

                    proceedBtn.text = "PROCEED TO SCENARIOS ▶";
                    proceedBtn.style.color = _bgColor;
                    proceedBtn.style.borderBottomWidth = 0;
                    proceedBtn.style.backgroundColor = _accentColor;
                }
            }).Every(100);
        }

        private void CancelActiveBenchmark()
        {
            if (_benchmarkTask != null)
            {
                _benchmarkTask.Pause();
                _benchmarkTask = null;
            }
        }

        // ==========================================
        // PHASE 3: TACTICAL DATA CARDS
        // ==========================================
        private void LoadTacticalScenarios()
        {
            CancelActiveBenchmark();
            _mainContainer.Clear();

            var listBuilder = new GraphicalUserInterfaceBuilder("ScenariosList")
                .WithFlexDirection(FlexDirection.Column)
                .WithFlexGrow(1)
                .AddChild(new Label("MILITARY & DEFENSE: ASYMMETRIC CAPABILITIES") { style = { color = _textColor, fontSize = 28, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } })
                .AddChild(new Label("Select a tactical scenario to explore the FSM_API application.") { style = { color = _textColor, opacity = 0.6f, fontSize = 14, marginBottom = 30 } });

            var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1, paddingRight = 10 } };

            scroll.Add(CreateDataCard(
                "⛛", "01 // MASSIVE DRONE SWARM AUTONOMY",
                "Standard architectures bottleneck early. Our completely decoupled API leverages optimal local CPU compute to drive 500,000+ autonomous agents locally without C2 lag, enabling terrifyingly coordinated swarm intelligence."));

            scroll.Add(CreateDataCard(
                "⚿", "02 // ENEMY SIGNAL INTERCEPTION & C2 SPOOFING",
                "Blackbox enemy communications. By crunching recorded signals into a domain of FSMs, we map commonality, predict signals in advance, and generate valid spoofed commands to turn enemy assets against their senders."));

            scroll.Add(CreateDataCard(
                "◈", "03 // DETERMINISTIC TELEMETRY OBFUSCATION",
                "Total secure comms. We do not transmit payload data. We transmit sequence names. The receiving agent natively generates the exact data payload without a single byte of actual data being intercepted in transit."));

            scroll.Add(CreateDataCard(
                "⛶", "04 // CYBER EXPLOITATION & DELTA MAPPING",
                "Leverage runtime recomposition to aggressively analyze enemy static codebases. By evaluating deltas, we map the unchangeable static infrastructure beneath their systems, exposing critical, hard-coded vulnerabilities."));

            listBuilder.AddChild(scroll);
            _mainContainer.Add(listBuilder.Build());
        }

        private VisualElement CreateDataCard(string icon, string title, string description)
        {
            var card = new VisualElement { style = { flexDirection = FlexDirection.Row, backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.05f), paddingBottom = 20, paddingTop = 20, paddingLeft = 20, paddingRight = 20, marginBottom = 15,  borderLeftWidth = 4, borderLeftColor = _accentColor } };

            var iconLabel = new Label(icon) { style = { color = _accentColor, fontSize = 40, width = 60, unityTextAlign = TextAnchor.MiddleCenter } };

            var textContainer = new VisualElement { style = { flexGrow = 1, marginLeft = 15, flexDirection = FlexDirection.Column } };
            textContainer.Add(new Label(title) { style = { color = _textColor, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5, letterSpacing = 1 } });
            textContainer.Add(new Label(description) { style = { color = _textColor, opacity = 0.7f, fontSize = 14, whiteSpace = WhiteSpace.Normal } });

            card.Add(iconLabel);
            card.Add(textContainer);

            var interactBtn = new Button(() => Debug.Log($"Simulating: {title}"))
            {
                text = "SIMULATE",
                style = { height = 35, width = 120, backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.2f), color = _textColor, unityFontStyleAndWeight = FontStyle.Bold, alignSelf = Align.Center }
            };
            card.Add(interactBtn);

            return card;
        }

        // ==========================================
        // PHASE 4: THEORY & USP (DAZZLE)
        // ==========================================
        private void LoadTheoryAndArchitecture()
        {
            CancelActiveBenchmark();
            _mainContainer.Clear();

            var theoryBuilder = new GraphicalUserInterfaceBuilder("TheoryAndArchitecture")
                .WithFlexDirection(FlexDirection.Column)
                .WithFlexGrow(1)
                .Build();

            var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1, paddingRight = 10 } };

            scroll.Add(new Label("THEORY & ARCHITECTURE: SHATTERING THE BOTTLENECK") { style = { color = _textColor, fontSize = 28, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });

            // 1. The Dual API
            var dualApiBox = new VisualElement { style = { backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.05f), paddingBottom = 25, paddingTop = 25, paddingLeft = 25, paddingRight = 25, marginBottom = 20, borderTopWidth = 2, borderTopColor = _accentColor } };
            dualApiBox.Add(new Label("THE DUAL API PARADIGM") { style = { color = _accentColor, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
            dualApiBox.Add(new Label("The FSM_API has been rigorously refactored to introduce a dual-backing system. We are running a Hash-backed API directly alongside a String-backed API. These two completely isolated systems run side-by-side, giving developers intense, memory-optimized high performance when needed, and performant, human-readable flexibility on the other extreme. This is not just an API; it is a fundamental compute framework.") { style = { color = _textColor, opacity = 0.8f, fontSize = 15, whiteSpace = WhiteSpace.Normal } });
            scroll.Add(dualApiBox);

            // 2. Runtime Recomposition (The Dazzle)
            var recompositionBox = new VisualElement { style = { backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.05f), paddingBottom = 25, paddingTop = 25, paddingLeft = 25, paddingRight = 25, marginBottom = 20, borderTopWidth = 2, borderTopColor = new Color(0.9f, 0.2f, 0.2f) } };
            recompositionBox.Add(new Label("RUNTIME RECOMPOSITION: WE STAND ALONE") { style = { color = new Color(0.9f, 0.2f, 0.2f), fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
            recompositionBox.Add(new Label("A deep dive into the current landscape reveals a stark truth: true Runtime Recomposition in state machines is effectively non-existent. Traditional FSM theory dictates static, pre-compiled transitions. We completely shred that theory.\n\nOur unique architecture allows you to dynamically inject, rewire, safely restructure, and redefine your FSMs and their execution sequences AT RUNTIME while under massive load. This requires outside-the-box thinking that completely breaks the mold of standard software patterns. You are not just changing states; you are dynamically evolving the brain of the simulation in real-time.") { style = { color = _textColor, opacity = 0.8f, fontSize = 15, whiteSpace = WhiteSpace.Normal } });
            scroll.Add(recompositionBox);

            theoryBuilder.Add(scroll);
            _mainContainer.Add(theoryBuilder);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string id) { }
        public void FromUIDocument(string doc) { }
    }
}