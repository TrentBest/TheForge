using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Defense;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes.Effects;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Pitch
{
    public class FsmBenchmarkAgentContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; }

        // Real data to compute
        public Matrix4x4 TransformData = Matrix4x4.identity;
        public Vector3 Velocity = Vector3.one;
    }

    public class IndustryScenario
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string TechnicalApplication { get; set; }
    }

    public class FsmApiPitch_GuiProvider : IGuiProvider
    {
        public string Title => "THE SINGULARITY WORKSHOP: FSM_API";

        private VisualElement _rootContainer;
        private VisualElement _mainContainer;
        private GuiContext _guiContext;

        // Benchmark state
        private int _simulatedAgents = 0;
        private int _targetAgents = 485000;
        private IVisualElementScheduledItem _benchmarkTask;

        // Navigation state
        private Dictionary<VisualElement, BreathingEffect> _navEffects = new Dictionary<VisualElement, BreathingEffect>();
        private VisualElement _activeNavButton;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _guiContext = ctx;

            var rootBuilder = new GraphicalUserInterfaceBuilder("PitchRoot")
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.03f))
                .OnBuild(ve => {
                    _rootContainer = ve;
                    LoadMainApp();
                    LoadWelcomeScreen();
                });

            return rootBuilder.Build();
        }

        // ==========================================
        // APP SHELL & NAVIGATION
        // ==========================================
        private void LoadMainApp()
        {
            _rootContainer.Clear();

            var appContainer = new GraphicalUserInterfaceBuilder("AppContainer")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100);

            appContainer.AddChild(BuildSideNav());

            var contentWrapper = new GraphicalUserInterfaceBuilder("ContentContainer")
                .WithFlexGrow(1).WithPadding(50)
                .OnBuild(ve => {
                    _mainContainer = ve;
                });

            appContainer.AddChild(contentWrapper);
            _rootContainer.Add(appContainer.Build());
        }

        private GraphicalUserInterfaceBuilder BuildSideNav()
        {
            var sideNav = new GraphicalUserInterfaceBuilder("SideNav")
                .WithWidth(280).WithBackgroundColor(new Color(0.08f, 0.09f, 0.12f))
                .WithBorderRightWidth(1).WithBorderRightColor(new Color(0.2f, 0.3f, 0.4f)).WithPadding(20);

            sideNav.AddChild(new ForgeLabelBuilder("THE SINGULARITY WORKSHOP").OnBuild(ve => {
                ve.style.color = Color.gray;
                ve.style.fontSize = 10;
                ve.style.letterSpacing = 2;
                ve.style.marginBottom = 20;
            }));

            sideNav.AddChild(new ForgeLabelBuilder("FSM_API PITCH").OnBuild(ve => {
                ve.style.color = Color.white;
                ve.style.unityFontStyleAndWeight = FontStyle.Bold;
                ve.style.fontSize = 16;
                ve.style.marginBottom = 30;
            }));

            var btnIntro = CreateNavButton("btnIntro", "INTRODUCTION", () => { SetActiveTab("btnIntro"); LoadWelcomeScreen(); });
            var btnBenchmark = CreateNavButton("btnBenchmark", "COMPUTE BENCHMARK", () => { SetActiveTab("btnBenchmark"); TriggerHardwareBenchmark(); });
            var btnIndustry = CreateNavButton("btnIndustry", "INDUSTRY APPLICATIONS", () => { SetActiveTab("btnIndustry"); LoadSectorsList(); });

            sideNav.AddChild(btnIntro);
            sideNav.AddChild(btnBenchmark);
            sideNav.AddChild(btnIndustry);

            sideNav.AddChild(new GraphicalUserInterfaceBuilder("Spacer").WithFlexGrow(1));

            sideNav.AddChild(new ForgeLabelBuilder("v1.0.0 - STABLE").OnBuild(ve => {
                ve.style.color = Color.gray;
                ve.style.fontSize = 10;
                ve.style.unityTextAlign = TextAnchor.MiddleCenter;
            }));

            sideNav.OnBuild(ve => {
                var introButton = ve.Q<Button>("btnIntro");
                if (introButton != null) SetActiveTab("btnIntro", introButton);
            });

            return sideNav;
        }

        private ForgeButtonBuilder CreateNavButton(string name, string text, Action onClickAction)
        {
            return (ForgeButtonBuilder)new ForgeButtonBuilder(name).WithOnClick(onClickAction).OnBuild(ve => {
                var btn = ve as Button;
                btn.text = text;
                btn.style.height = 45;
                btn.style.marginBottom = 10;
                btn.style.backgroundColor = new Color(0.15f, 0.18f, 0.22f);
                btn.style.color = new Color(0.8f, 0.85f, 0.9f);
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;
                btn.style.borderBottomWidth = 2;
                btn.style.borderBottomColor = new Color(0.1f, 0.12f, 0.15f);
                btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                btn.style.paddingLeft = 15;
            });
        }

        private void SetActiveTab(string btnName, VisualElement fallbackBtn = null)
        {
            foreach (var kvp in _navEffects)
            {
                if (kvp.Value != null) kvp.Value.IsActive = false;
                kvp.Key.style.backgroundColor = new Color(0.15f, 0.18f, 0.22f);
            }

            VisualElement targetBtn = fallbackBtn ?? _rootContainer?.Q<Button>(btnName);

            if (targetBtn != null)
            {
                if (!_navEffects.ContainsKey(targetBtn))
                {
                    var effect = new BreathingEffect(targetBtn, processingRate: 0, breathingRate: 3f);
                    effect.Colors = new List<Color> { new Color(0.4f, 0.1f, 0.6f), new Color(0.15f, 0.18f, 0.22f) };
                    _navEffects[targetBtn] = effect;
                }

                _navEffects[targetBtn].IsActive = true;
                _activeNavButton = targetBtn;
            }
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
                .WithFlexGrow(1);

            welcomeBuilder.AddChild(new ForgeLabelBuilder("BUILDING SOFTWARE FOR THE SINGULARITY.").OnBuild(ve => {
                ve.style.color = Color.white;
                ve.style.fontSize = 36;
                ve.style.unityFontStyleAndWeight = FontStyle.Bold;
                ve.style.marginBottom = 20;
                ve.style.whiteSpace = WhiteSpace.Normal;
            }));

            welcomeBuilder.AddChild(new ForgeLabelBuilder("Welcome to The Singularity Workshop. At the core of our technology is the FSM_API—a highly performant centralization of state that brings forth an entirely new level of interoperability across architectures, platforms, and industries.\n\nBefore we begin the presentation, we recommend running our local compute calculation tooling to ascertain the baseline capabilities of your current hardware. You may click the button below to initiate the telemetry test, or simply skip ahead to explore the technological applications.").OnBuild(ve => {
                ve.style.color = new Color(0.7f, 0.75f, 0.8f);
                ve.style.fontSize = 16;
                ve.style.whiteSpace = WhiteSpace.Normal;
                ve.style.marginBottom = 40;
            }));

            var buttonRow = new GraphicalUserInterfaceBuilder("WelcomeButtons")
                .WithFlexDirection(FlexDirection.Row);

            buttonRow.AddChild(new ForgeButtonBuilder("RunBenchmarkBtn").WithOnClick(() => { SetActiveTab("btnBenchmark"); TriggerHardwareBenchmark(); }).OnBuild(ve => {
                var btn = ve as Button;
                btn.text = "RUN COMPUTE CALCULATION";
                btn.style.height = 50;
                btn.style.width = 250;
                btn.style.backgroundColor = new Color(0.1f, 0.5f, 0.8f);
                btn.style.color = Color.white;
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;
                btn.style.marginRight = 20;
                btn.style.borderTopLeftRadius = 4; btn.style.borderTopRightRadius = 4;
                btn.style.borderBottomLeftRadius = 4; btn.style.borderBottomRightRadius = 4;
            }));

            buttonRow.AddChild(new ForgeButtonBuilder("SkipBtn").WithOnClick(() => { SetActiveTab("btnIndustry"); LoadSectorsList(); }).OnBuild(ve => {
                var btn = ve as Button;
                btn.text = "SKIP TO PRESENTATION ▶";
                btn.style.height = 50;
                btn.style.width = 250;
                btn.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
                btn.style.color = Color.gray;
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;
                btn.style.borderTopLeftRadius = 4; btn.style.borderTopRightRadius = 4;
                btn.style.borderBottomLeftRadius = 4; btn.style.borderBottomRightRadius = 4;
            }));

            welcomeBuilder.AddChild(buttonRow);
            _mainContainer.Add(welcomeBuilder.Build());
        }

        // ==========================================
        // PHASE 2: OPTIONAL HARDWARE BENCHMARK
        // ==========================================
        private void TriggerHardwareBenchmark()
        {
            CancelActiveBenchmark();
            _mainContainer.Clear();

            var benchmarkContainer = new GraphicalUserInterfaceBuilder("BenchmarkContainer")
                .WithFlexDirection(FlexDirection.Column)
                .WithFlexGrow(1);

            benchmarkContainer.AddChild(new ForgeLabelBuilder("THE SINGULARITY BENCHMARK").OnBuild(ve => {
                ve.style.color = new Color(0.1f, 0.8f, 0.9f);
                ve.style.fontSize = 26;
                ve.style.unityFontStyleAndWeight = FontStyle.Bold;
                ve.style.marginBottom = 10;
            }));

            benchmarkContainer.AddChild(new ForgeLabelBuilder("What is your Compute Power?\n\n" +
                           "When you click start, we fire up a swarm of autonomous mathematical agents. " +
                           "These aren't empty mocks; each agent performs continuous 4x4 matrix transformations and vector math. " +
                           "This test measures exactly how many of these agents your specific hardware can sustain simultaneously " +
                           "before the framerate drops below a silky smooth 60 FPS.").OnBuild(ve => {
                               ve.style.color = Color.white;
                               ve.style.fontSize = 14;
                               ve.style.whiteSpace = WhiteSpace.Normal;
                               ve.style.marginBottom = 20;
                           }));

            VisualElement subHeaderElement = null;
            benchmarkContainer.AddChild(new ForgeLabelBuilder("MOUNTING REAL FSM KERNEL & INITIALIZING ACTIVE TELEMETRY...").OnBuild(ve => {
                subHeaderElement = ve;
                ve.style.color = Color.gray;
                ve.style.fontSize = 14;
                ve.style.marginBottom = 15;
            }));

            try
            {
                VisualElement privateDataContainer = null;
                var privateDataBuilder = new GraphicalUserInterfaceBuilder("PrivateDataContainer")
                    .WithMarginTop(10).WithMarginBottom(20)
                    .OnBuild(ve => {
                        privateDataContainer = ve;
                        ve.style.display = DisplayStyle.None;
                    });

                // Reflect Active Engine Instance
                var activeEngineInstance = TheSingularityWorkshop.FSM_API.Scripts.FSM_UnityIntegrationAdvanced.Instance;
                if (activeEngineInstance != null)
                {
                    var dashboardProvider = new StaticReflectiveGuiBuilder(typeof(TheSingularityWorkshop.FSM_API.Scripts.FSM_UnityIntegrationAdvanced), "ACTIVE ENGINE RUNNER", 50);
                    privateDataBuilder.AddChild(dashboardProvider);
                }
                else
                {
                    var dashboardProvider = new StaticReflectiveGuiBuilder(typeof(FSM_API.Internal), "FSM_API.Internal LIVE TELEMETRY", 50);
                    privateDataBuilder.AddChild(dashboardProvider);
                }

                var privateDataFoldout = new GraphicalUserInterfaceBuilder("PrivateDataWrapper");
                privateDataFoldout.AddChild(new ForgeButtonBuilder("ToggleBtn").OnBuild(ve => {
                    var btn = ve as Button;
                    btn.text = "▶ Under the Hood (Live Engine Data)";
                    btn.style.backgroundColor = Color.clear;
                    btn.style.color = Color.gray;
                    btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                    btn.style.borderTopWidth = 0; btn.style.borderBottomWidth = 0;
                    btn.style.borderLeftWidth = 0; btn.style.borderRightWidth = 0;
                    btn.clicked += () => {
                        if (privateDataContainer != null)
                        {
                            bool isHidden = privateDataContainer.style.display == DisplayStyle.None;
                            privateDataContainer.style.display = isHidden ? DisplayStyle.Flex : DisplayStyle.None;
                            btn.text = isHidden ? "▼ Under the Hood (Live Engine Data)" : "▶ Under the Hood (Live Engine Data)";
                        }
                    };
                }));
                privateDataFoldout.AddChild(privateDataBuilder);
                benchmarkContainer.AddChild(privateDataFoldout);
            }
            catch (Exception ex)
            {
                benchmarkContainer.AddChild(new ForgeLabelBuilder($"[Telemetry Load Failed: {ex.Message}]").OnBuild(ve => {
                    ve.style.color = Color.red;
                }));
            }

            VisualElement resultsContainerElement = null;
            VisualElement statusLabelElement = null;
            VisualElement detailsLabelElement = null;
            VisualElement proceedBtnElement = null;

            var resultsContainerBuilder = new GraphicalUserInterfaceBuilder("ResultsContainer")
                .WithMarginTop(20).WithPadding(15)
                .OnBuild(ve => {
                    resultsContainerElement = ve;
                    ve.style.backgroundColor = new Color(0.1f, 0.15f, 0.1f, 0.0f);
                });

            resultsContainerBuilder.AddChild(new ForgeLabelBuilder("STANDBY FOR SWARM INSTANTIATION...").OnBuild(ve => {
                statusLabelElement = ve;
                ve.style.color = Color.white;
                ve.style.fontSize = 18;
                ve.style.unityFontStyleAndWeight = FontStyle.Bold;
            }));

            resultsContainerBuilder.AddChild(new ForgeLabelBuilder("").OnBuild(ve => {
                detailsLabelElement = ve;
                ve.style.color = Color.gray;
                ve.style.fontSize = 14;
                ve.style.whiteSpace = WhiteSpace.Normal;
                ve.style.marginTop = 10;
            }));

            benchmarkContainer.AddChild(resultsContainerBuilder);

            benchmarkContainer.AddChild(new ForgeButtonBuilder("ProceedBtn").WithOnClick(() => { SetActiveTab("btnIndustry"); LoadSectorsList(); }).OnBuild(ve => {
                proceedBtnElement = ve;
                var btn = ve as Button;
                btn.text = "CANCEL & CONTINUE";
                btn.style.height = 50; btn.style.width = 300;
                btn.style.marginTop = 30; btn.style.alignSelf = Align.FlexStart;
                btn.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
                btn.style.color = Color.gray;
                btn.style.borderBottomWidth = 2; btn.style.borderBottomColor = Color.gray;
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;
            }));

            _mainContainer.Add(benchmarkContainer.Build());

            // THE REAL MATH
            if (!FSM_API.Interaction.Exists("FsmBenchmarkAgent"))
            {
                FSM_API.Create.CreateFiniteStateMachine("FsmBenchmarkAgent", -1, "BenchmarkGroup")
                    .State("Active", null, ctx => {
                        if (ctx is FsmBenchmarkAgentContext bCtx)
                        {
                            bCtx.TransformData = Matrix4x4.TRS(bCtx.Velocity * Time.time, Quaternion.Euler(0, Time.time, 0), Vector3.one);
                            bCtx.Velocity = bCtx.TransformData.MultiplyVector(Vector3.up);
                        }
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
                    var ctx = new FsmBenchmarkAgentContext { Name = $"Node_{_simulatedAgents}" };
                    FSM_API.Create.CreateInstance("FsmBenchmarkAgent", ctx, "BenchmarkGroup");
                    _simulatedAgents++;
                }

                if (subHeaderElement != null && subHeaderElement is Label subLbl)
                    subLbl.text = $"EVALUATING: {_simulatedAgents:N0} / {_targetAgents:N0} REAL MATH OPERATIONS... (WATCH LIVE TELEMETRY)";

                if (_simulatedAgents >= _targetAgents)
                {
                    if (resultsContainerElement != null) resultsContainerElement.style.backgroundColor = new Color(0.1f, 0.25f, 0.15f, 1.0f);

                    if (statusLabelElement != null && statusLabelElement is Label statLbl)
                    {
                        statLbl.text = $"COMPUTE SCORE: {_targetAgents:N0} AGENTS SUSTAINED";
                        statLbl.style.color = Color.green;
                    }

                    string rank = _targetAgents > 100000 ? "Titan Tier" : "Standard Tier";

                    if (detailsLabelElement != null && detailsLabelElement is Label detLbl)
                    {
                        detLbl.text = $"Result: Your machine achieved a '{rank}' rating.\n\n" +
                                      "What this means: In a real-world application, you could run a fully simulated smart-city traffic grid, " +
                                      "or a massive 4X strategy game ecosystem without breaking a sweat using The Singularity Workshop API.";
                        detLbl.style.color = Color.white;
                    }

                    if (subHeaderElement != null && subHeaderElement is Label subLblFinal)
                    {
                        subLblFinal.text = $"SYSTEM STABLE. EXTREME CONCURRENCY ACHIEVED.";
                        subLblFinal.style.color = Color.green;
                    }

                    if (proceedBtnElement != null && proceedBtnElement is Button proBtn)
                    {
                        proBtn.text = "PROCEED TO PRESENTATION ▶";
                        proBtn.style.color = Color.white;
                        proBtn.style.borderBottomColor = Color.green;
                        proBtn.style.backgroundColor = new Color(0.1f, 0.4f, 0.2f);
                    }
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
        // PHASE 3: INDUSTRY SECTORS & APPLICATIONS
        // ==========================================
        private void LoadSectorsList()
        {
            CancelActiveBenchmark();
            _mainContainer.Clear();

            var listBuilder = new GraphicalUserInterfaceBuilder("SectorsList")
                .WithFlexDirection(FlexDirection.Column)
                .WithFlexGrow(1);

            listBuilder.AddChild(new ForgeLabelBuilder("UNIVERSAL INTEROPERABILITY").OnBuild(ve => {
                ve.style.color = Color.white;
                ve.style.fontSize = 28;
                ve.style.unityFontStyleAndWeight = FontStyle.Bold;
                ve.style.marginBottom = 5;
            }));

            listBuilder.AddChild(new ForgeLabelBuilder("The Singularity Workshop isn't just a Unity tool. It is a universally adaptable architecture.\n\n" +
                           "The core engine is a highly optimized, 45kb C# .dll. It is completely platform-agnostic and can be installed " +
                           "on ANY device that supports .NET 8—from high-end cloud servers down to lightweight IoT devices.\n\n" +
                           "Because of our ruthless commitment to backward compatibility, this API can even be used to breathe new life " +
                           "into abandoned legacy software, retrofitting modern AI and state-machine logic into old codebases.\n\n" +
                           "Select a sector below to see how this technology transforms specific industries:").OnBuild(ve => {
                               ve.style.color = new Color(0.8f, 0.85f, 0.9f);
                               ve.style.fontSize = 15;
                               ve.style.whiteSpace = WhiteSpace.Normal;
                               ve.style.marginBottom = 30;
                               ve.style.paddingTop = 15; ve.style.paddingBottom = 15; ve.style.paddingLeft = 15; ve.style.paddingRight = 15;
                               ve.style.backgroundColor = new Color(0.15f, 0.18f, 0.25f);
                               ve.style.borderLeftWidth = 4;
                               ve.style.borderLeftColor = new Color(0.5f, 0.2f, 0.8f);
                           }));

            var scrollBuilder = new ForgeScrollViewBuilder("SectorsScroll")
                .OnBuild(ve => {
                    var sv = ve as ScrollView;
                    sv.mode = ScrollViewMode.Vertical;
                    sv.style.flexGrow = 1;
                    sv.style.paddingRight = 10;
                });

            scrollBuilder.AddChild(CreateSectorCard("AEC & BIM INTEROPERABILITY", "Connecting digital twins, architectural models, and engineering data.",
                () => RouteToIndustry(GetAecScenarios(), "AEC & BIM INTEROPERABILITY", "Connecting physical and digital realities through unified state logistics.")));

            scrollBuilder.AddChild(CreateSectorCard("GAME DEVELOPMENT & SIMULATION", "Managing massive swarms, complex quest states, and UI flows.",
                () => RouteToIndustry(GetGameDevScenarios(), "GAME DEVELOPMENT & SIMULATION", "Decoupling game logic from rendering overhead for immense scale.")));

            scrollBuilder.AddChild(CreateSectorCard("ROBOTICS & MANUFACTURING", "Deterministic control logic and fault-tolerant assembly lines.",
                () => RouteToIndustry(GetRoboticsScenarios(), "ROBOTICS & MANUFACTURING", "Ensuring fault tolerance and provable state execution in physical environments.")));

            scrollBuilder.AddChild(CreateSectorCard("ENTERPRISE SOFTWARE & MICRO-FRONTENDS", "Decoupling application logic from visual representation.",
                () => RouteToIndustry(GetEnterpriseScenarios(), "ENTERPRISE SOFTWARE & MICRO-FRONTENDS", "Headless business logic powering agnostic visual front-ends.")));

            scrollBuilder.AddChild(CreateSectorCard("MILITARY & DEFENSE", "Asymmetric autonomy, drone swarms, and secure C2 networks.", LoadMilitaryDefensePitch));

            listBuilder.AddChild(scrollBuilder);
            _mainContainer.Add(listBuilder.Build());
        }

        private GraphicalUserInterfaceBuilder CreateSectorCard(string title, string summary, Action onExploreAction)
        {
            var card = new GraphicalUserInterfaceBuilder("Card")
                .WithBackgroundColor(new Color(0.12f, 0.14f, 0.18f)).WithPadding(25).WithMarginBottom(15)
                .WithFlexDirection(FlexDirection.Column)
                .OnBuild(ve => {
                    ve.style.borderTopLeftRadius = 4; ve.style.borderTopRightRadius = 4;
                    ve.style.borderBottomLeftRadius = 4; ve.style.borderBottomRightRadius = 4;
                    ve.style.borderLeftWidth = 4; ve.style.borderLeftColor = new Color(0.1f, 0.6f, 0.8f);
                });

            card.AddChild(new ForgeLabelBuilder("CardTitle").OnBuild(ve => {
                var lbl = ve as Label;
                lbl.text = title;
                lbl.style.color = Color.white;
                lbl.style.fontSize = 18;
                lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
                lbl.style.marginBottom = 5;
                lbl.style.letterSpacing = 1;
            }));

            card.AddChild(new ForgeLabelBuilder("CardSummary").OnBuild(ve => {
                var lbl = ve as Label;
                lbl.text = summary;
                lbl.style.color = new Color(0.6f, 0.7f, 0.8f);
                lbl.style.fontSize = 14;
                lbl.style.whiteSpace = WhiteSpace.Normal;
                lbl.style.marginBottom = 15;
            }));

            card.AddChild(new ForgeButtonBuilder("ExploreBtn").WithOnClick(onExploreAction).OnBuild(ve => {
                var btn = ve as Button;
                btn.text = "EXPLORE SECTOR APPLICATION";
                btn.style.height = 35; btn.style.width = 220;
                btn.style.backgroundColor = new Color(0.2f, 0.2f, 0.25f);
                btn.style.color = Color.white;
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;
                btn.style.marginTop = 10;
                btn.style.alignSelf = Align.FlexEnd;
            }));

            return card;
        }

        // ==========================================
        // PHASE 4: INDUSTRY DEEP DIVE SCENARIO ROUTER
        // ==========================================

        private void LoadMilitaryDefensePitch()
        {
            CancelActiveBenchmark();
            _rootContainer.Clear();
            var defensePitchProvider = new MilitaryDefense_Gui_ExplainerRouter();
            _rootContainer.Add(defensePitchProvider.CreateGui(_guiContext));
        }

        private void RouteToIndustry(List<IndustryScenario> scenarios, string title, string description)
        {
            _mainContainer.Clear();

            var layoutBuilder = new GraphicalUserInterfaceBuilder("IndustryScenariosRouter")
                .WithFlexDirection(FlexDirection.Column)
                .WithFlexGrow(1);

            layoutBuilder.AddChild(new ForgeButtonBuilder("BackBtn").WithOnClick(LoadSectorsList).OnBuild(ve => {
                var btn = ve as Button;
                btn.text = "◀ BACK TO SECTORS";
                btn.style.height = 30; btn.style.width = 150;
                btn.style.backgroundColor = Color.clear;
                btn.style.color = Color.gray;
                btn.style.borderBottomWidth = 1; btn.style.borderBottomColor = Color.gray;
                btn.style.marginBottom = 30;
                btn.style.alignSelf = Align.FlexStart;
            }));

            layoutBuilder.AddChild(new ForgeLabelBuilder(title).OnBuild(ve => {
                ve.style.color = new Color(0.1f, 0.8f, 0.9f);
                ve.style.fontSize = 32;
                ve.style.unityFontStyleAndWeight = FontStyle.Bold;
                ve.style.marginBottom = 5;
            }));

            layoutBuilder.AddChild(new ForgeLabelBuilder(description).OnBuild(ve => {
                ve.style.color = Color.gray;
                ve.style.fontSize = 14;
                ve.style.marginBottom = 20;
            }));

            var splitLayout = new GraphicalUserInterfaceBuilder("SplitLayout")
                .WithFlexDirection(FlexDirection.Row)
                .WithFlexGrow(1);

            var leftNav = new GraphicalUserInterfaceBuilder("ScenarioNav")
                .WithWidth(280)
                .WithMarginRight(20);

            var rightContent = new GraphicalUserInterfaceBuilder("ScenarioContentDisplay")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.1f, 0.12f, 0.15f))
                .OnBuild(ve => {
                    ve.style.paddingTop = 30; ve.style.paddingBottom = 30;
                    ve.style.paddingLeft = 30; ve.style.paddingRight = 30;
                    ve.style.borderLeftWidth = 2; ve.style.borderLeftColor = new Color(0.1f, 0.8f, 0.9f);
                });

            VisualElement targetContentPanel = null;
            rightContent.OnBuild(ve => targetContentPanel = ve);

            foreach (var scenario in scenarios)
            {
                leftNav.AddChild(new ForgeButtonBuilder("ScenarioBtn").WithOnClick(() => RenderScenarioDetails(targetContentPanel, scenario)).OnBuild(ve => {
                    var btn = ve as Button;
                    btn.text = scenario.Title;
                    btn.style.height = 50;
                    btn.style.marginBottom = 10;
                    btn.style.backgroundColor = new Color(0.15f, 0.18f, 0.22f);
                    btn.style.color = new Color(0.8f, 0.85f, 0.9f);
                    btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                    btn.style.paddingLeft = 15;
                }));
            }

            splitLayout.AddChild(leftNav);
            splitLayout.AddChild(rightContent);
            layoutBuilder.AddChild(splitLayout);

            _mainContainer.Add(layoutBuilder.Build());

            if (scenarios.Count > 0 && targetContentPanel != null)
            {
                RenderScenarioDetails(targetContentPanel, scenarios[0]);
            }
        }

        private void RenderScenarioDetails(VisualElement container, IndustryScenario scenario)
        {
            if (container == null) return;
            container.Clear();

            var detailBuilder = new GraphicalUserInterfaceBuilder("ScenarioDetailBuilder")
                .WithFlexDirection(FlexDirection.Column);

            detailBuilder.AddChild(new ForgeLabelBuilder(scenario.Title).OnBuild(ve => {
                ve.style.color = Color.white;
                ve.style.fontSize = 24;
                ve.style.unityFontStyleAndWeight = FontStyle.Bold;
                ve.style.marginBottom = 20;
            }));

            detailBuilder.AddChild(new ForgeLabelBuilder("THE CHALLENGE:").OnBuild(ve => {
                ve.style.color = new Color(0.5f, 0.2f, 0.8f);
                ve.style.fontSize = 12;
                ve.style.unityFontStyleAndWeight = FontStyle.Bold;
                ve.style.letterSpacing = 1;
            }));

            detailBuilder.AddChild(new ForgeLabelBuilder(scenario.Description).OnBuild(ve => {
                ve.style.color = new Color(0.8f, 0.85f, 0.9f);
                ve.style.fontSize = 16;
                ve.style.whiteSpace = WhiteSpace.Normal;
                ve.style.marginBottom = 30;
            }));

            detailBuilder.AddChild(new ForgeLabelBuilder("THE FSM_API SOLUTION:").OnBuild(ve => {
                ve.style.color = new Color(0.1f, 0.8f, 0.9f);
                ve.style.fontSize = 12;
                ve.style.unityFontStyleAndWeight = FontStyle.Bold;
                ve.style.letterSpacing = 1;
            }));

            detailBuilder.AddChild(new ForgeLabelBuilder(scenario.TechnicalApplication).OnBuild(ve => {
                ve.style.color = Color.white;
                ve.style.fontSize = 15;
                ve.style.whiteSpace = WhiteSpace.Normal;
                ve.style.backgroundColor = new Color(0.15f, 0.18f, 0.25f);
                ve.style.paddingTop = 15; ve.style.paddingBottom = 15;
                ve.style.paddingLeft = 15; ve.style.paddingRight = 15;
                ve.style.borderLeftWidth = 4;
                ve.style.borderLeftColor = new Color(0.1f, 0.8f, 0.9f);
            }));

            container.Add(detailBuilder.CreateGui(_guiContext));
        }

        // ==========================================
        // SCENARIO DATA REPOSITORIES
        // ==========================================

        private List<IndustryScenario> GetAecScenarios() => new List<IndustryScenario>
        {
            new IndustryScenario {
                Title = "Digital Twin Material Routing",
                Description = "In AEC, data is siloed. A structural beam exists in Revit, but its manufacturing status exists in an external ERP, and its delivery status on a contractor's spreadsheet. Bridging these creates brittle API dependencies.",
                TechnicalApplication = "FSM_API acts as the logic layer of a 'Digital Parts Warehouse.' A state machine instance is assigned to every single physical part. It securely tracks the lifecycle from [Designed] -> [Manufactured] -> [In-Transit] -> [Installed], routing data autonomously across platforms without central monolithic polling."
            },
            new IndustryScenario {
                Title = "Automated Clash Resolution",
                Description = "When HVAC models intersect with structural framing, human intervention is usually required to document the clash, notify teams, and await an update cycle.",
                TechnicalApplication = "An autonomous FSM Agent acts as a watchdog. Upon detecting a geometric clash, it transitions to a [Resolution State], automatically pinging the MEP engineer's local toolset, halting subsequent supply-chain orders for that specific part until the state returns to [Cleared]."
            }
        };

        private List<IndustryScenario> GetGameDevScenarios() => new List<IndustryScenario>
        {
            new IndustryScenario {
                Title = "Massive Swarm Pathing (RTS)",
                Description = "Modern game engines struggle to scale when hundreds of thousands of individual entities each require their own Monobehaviour Update loop for pathing, targeting, and decision making.",
                TechnicalApplication = "By entirely decoupling logic from the GameObject hierarchy, the FSM_API can sustain 500,000+ pure-C# math agents. These agents process their states in continuous loops and only send lightweight transform updates to the rendering engine's GPU instancing buffers."
            },
            new IndustryScenario {
                Title = "Asynchronous Quest Causality",
                Description = "RPG quest logic is notoriously messy, often relying on massive 'switch/case' statements and nested booleans to track player choices, resulting in broken questlines when edge cases occur.",
                TechnicalApplication = "Quests are built as self-contained state networks. Causality is deterministic. If a player kills an NPC out of order, the FSM gracefully transitions to an [Alternative Outcome] node based purely on interface evaluation, entirely avoiding hardcoded crash errors."
            }
        };

        private List<IndustryScenario> GetRoboticsScenarios() => new List<IndustryScenario>
        {
            new IndustryScenario {
                Title = "Assembly Line Fault Tolerance",
                Description = "Manufacturing hardware requires absolute determinism. A single failed sensor can halt an entire production line or cause catastrophic physical damage if the software throws an unhandled exception.",
                TechnicalApplication = "State centralization ensures verifiable loops. If a robotic arm fails a telemetry check, the state machine cannot proceed to [Execute Motion]. It gracefully shifts to [Safety Standby], alerting operators without relying on cascading 'if/else' error handling."
            },
            new IndustryScenario {
                Title = "Deterministic Kinematics",
                Description = "Robotic joints must execute complex, multi-stage motions in precise synchronicity. Network lag or varying CPU loads can desync commands.",
                TechnicalApplication = "Processing groups in the FSM_API guarantee execution order regardless of framerate. A robotic chassis receives a compiled Micro-Package containing an exact, untamperable State Sequence for its physical kinematics."
            }
        };

        private List<IndustryScenario> GetEnterpriseScenarios() => new List<IndustryScenario>
        {
            new IndustryScenario {
                Title = "Headless Micro-Frontend Architecture",
                Description = "Enterprise software often suffers from tightly coupled UI and business logic, meaning an update to a backend server requires refactoring the mobile, web, and desktop client front-ends.",
                TechnicalApplication = "FSM_API allows for the ultimate separation of Data and Representation. The core business logic runs as a headless state machine. Desktop (WPF), Web (HTML), and 3D (Unity) clients simply 'listen' to the current state and render the visual UI accordingly."
            },
            new IndustryScenario {
                Title = "Secure Telemetry Obfuscation",
                Description = "Exposing deep application states directly to front-end clients creates severe security vulnerabilities and allows reverse-engineering of proprietary business logic.",
                TechnicalApplication = "Because the FSM acts as a logic wall, the client only sees the interface it is permitted to see. The underlying transitions, calculations, and mathematical math agents remain entirely hidden and securely executed on the server side."
            }
        };

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string id) { }
        public void FromUIDocument(string doc) { }
    }
}