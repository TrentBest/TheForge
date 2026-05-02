using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Asteroids
{
    public class Asteroids_Gui_InGame : IGuiProvider
    {
        public string Title => "ASTEROIDS: ACTIVE DEPLOYMENT";

        private IGuiRouter _router;
        private GuiContext _guiContext;
        private AsteroidsContext _gameContext;
        private VisualElement _rootContainer;

        // State variables
        private bool _aiEngaged = false;
        private Label _dataExtractionLabel;

        // Instances for the 3D Mini-Fighters
        private List<GameObject> _lifeInstances = new List<GameObject>();

        // 1. Parameterless Constructor for Editor/Reflection
        public Asteroids_Gui_InGame() { }

        // 2. Injected Constructor for Alpha Router
        public Asteroids_Gui_InGame(IGuiRouter router)
        {
            _router = router;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _guiContext = ctx;
            _gameContext = UnityEngine.Object.FindObjectOfType<AsteroidsContext>();

            _rootContainer = new GraphicalUserInterfaceBuilder("InGame_HUD_Root")
                .WithPercentSize(100, 100)
                // Ensure the background is totally transparent so the game camera shows through!
                .WithBackgroundColor(Color.clear)
                .Build();

            // Register cleanup so we don't memory leak the mini-fighters
            _rootContainer.RegisterCallback<DetachFromPanelEvent>(evt => CleanUpInstances());

            Refresh();

            // Schedule a real-time HUD update loop (runs every 100ms)
            _rootContainer.schedule.Execute(UpdateHUD).Every(100);

            return _rootContainer;
        }

        private void Refresh()
        {
            CleanUpInstances();
            _rootContainer.Clear();

            // --- 1. TOP TACTICAL BAR (Lives & Score) ---
            var topBar = new GraphicalUserInterfaceBuilder("TopTacticalBar")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithPadding(20)
                .OnBuild(ve => {
                    ve.style.position = Position.Absolute;
                    ve.style.top = 0; ve.style.left = 0; ve.style.right = 0;
                    ve.style.backgroundColor = new Color(0.02f, 0.02f, 0.05f, 0.8f); // Semi-transparent dark blue
                    ve.style.borderBottomWidth = 2;
                    ve.style.borderBottomColor = new Color(0.4f, 0.8f, 1f, 0.5f); // Neon Cyan trim
                }).Build();

            // Left Side: Mini-Fighter Lives
            var livesContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            livesContainer.Add(new ForgeLabelBuilder("SYNC STATUS: ").WithColor(Color.gray).WithFontSize(10).WithFontStyle(FontStyle.Bold).CreateGui(_guiContext));

            int lives = _gameContext?.Score?.Lives ?? 3;
            GameObject activePrefab = GetActiveFighterPrefab();

            for (int i = 0; i < lives; i++)
            {
                if (activePrefab != null)
                {
                    var miniInst = CreateLifeInstance(activePrefab);
                    var preview = new LiveModelPreviewBuilder(miniInst)
                        .WithZoom(4.5f)
                        .WithPitch(-90f) // Exact Top-Down view
                        .WithMouseControl(false)
                        .WithBackgroundColor(Color.clear)
                        .Build();

                    preview.style.width = 35;
                    preview.style.height = 35;
                    preview.style.marginLeft = 5;

                    // Optional: Give it a little glow box
                    preview.style.backgroundColor = new Color(0.4f, 0.8f, 1f, 0.1f);
                    preview.style.borderTopLeftRadius = 5; preview.style.borderBottomRightRadius = 5;

                    livesContainer.Add(preview);
                }
                else
                {
                    // Fallback if no prefab is found
                    var backupIcon = new VisualElement { style = { width = 20, height = 20, backgroundColor = Color.cyan, marginLeft = 5 } };
                    livesContainer.Add(backupIcon);
                }
            }
            topBar.Add(livesContainer);

            // Right Side: Data Extracted (Score)
            var scoreContainer = new VisualElement { style = { flexDirection = FlexDirection.Column, alignItems = Align.FlexEnd } };
            scoreContainer.Add(new ForgeLabelBuilder("DATA EXTRACTED").WithColor(Color.gray).WithFontSize(10).WithFontStyle(FontStyle.Bold).CreateGui(_guiContext));

            // EXPLICIT CAST APPLIED HERE
            _dataExtractionLabel = (Label)new ForgeLabelBuilder(GetScoreString())
                .WithColor(new Color(0.8f, 0.4f, 1f)) // Deep Purple / Magenta
                .WithFontSize(24)
                .WithFontStyle(FontStyle.Bold)
                .CreateGui(_guiContext);
            _dataExtractionLabel.style.letterSpacing = 2;
            scoreContainer.Add(_dataExtractionLabel);

            topBar.Add(scoreContainer);
            _rootContainer.Add(topBar);

            // --- 2. BOTTOM CONTROL BAR (Engage AI) ---
            var bottomBar = new GraphicalUserInterfaceBuilder("BottomControlBar")
                .WithFlexLayout(FlexDirection.Row, Justify.Center, Align.FlexEnd)
                .WithPadding(20)
                .OnBuild(ve => {
                    ve.style.position = Position.Absolute;
                    ve.style.bottom = 0; ve.style.left = 0; ve.style.right = 0;
                    ve.pickingMode = PickingMode.Ignore; // Let clicks pass through the empty space to the game
                }).Build();

            var aiBtn = new ForgeButtonBuilder(_aiEngaged ? "AI CONTROL ACTIVE [ ABORT ]" : "ENGAGE NEURAL NETWORK", ToggleAI)
                .WithWidth(300).WithHeight(45)
                .WithBackgroundColor(_aiEngaged ? new Color(0.4f, 0.1f, 0.6f, 0.9f) : new Color(0.1f, 0.1f, 0.1f, 0.9f))
                .WithTextColor(_aiEngaged ? Color.white : new Color(0.4f, 0.8f, 1f))
                .WithFontStyle(FontStyle.Bold)
                .CreateGui(_guiContext);

            aiBtn.style.borderTopWidth = 2; aiBtn.style.borderBottomWidth = 2; aiBtn.style.borderLeftWidth = 2; aiBtn.style.borderRightWidth = 2;
            aiBtn.style.borderTopColor = _aiEngaged ? new Color(0.8f, 0.4f, 1f) : new Color(0.4f, 0.8f, 1f);
            aiBtn.style.borderBottomColor = aiBtn.style.borderTopColor;
            aiBtn.style.borderLeftColor = aiBtn.style.borderTopColor;
            aiBtn.style.borderRightColor = aiBtn.style.borderTopColor;

            // Optional: Escape route back to the menu
            var abortBtn = new ForgeButtonBuilder("EJECT", () => {
                _gameContext?.Status?.TransitionTo("Shutdown");
                _router?.NavigateTo("MainMenu");
            }).WithWidth(80).WithHeight(45).WithBackgroundColor(new Color(0.5f, 0.1f, 0.1f)).WithTextColor(Color.white).CreateGui(_guiContext);
            abortBtn.style.marginLeft = 15;

            bottomBar.Add(aiBtn);
            bottomBar.Add(abortBtn);
            _rootContainer.Add(bottomBar);

            // --- 3. THE NEURAL NETWORK OVERLAY ---
            if (_aiEngaged)
            {
                RenderNeuralNetworkOverlay();
            }
        }

        private void RenderNeuralNetworkOverlay()
        {
            var nnOverlay = new GraphicalUserInterfaceBuilder("NeuralNetTelemetry")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithPadding(20)
                .OnBuild(ve => {
                    ve.style.position = Position.Absolute;
                    ve.style.top = 80; ve.style.bottom = 80; ve.style.left = 20;
                    ve.style.width = 350;
                    ve.style.backgroundColor = new Color(0.05f, 0.02f, 0.08f, 0.85f); // Deep dark purple glass
                    ve.style.borderTopWidth = 1; ve.style.borderBottomWidth = 1; ve.style.borderLeftWidth = 1; ve.style.borderRightWidth = 1;
                    ve.style.borderTopColor = new Color(0.6f, 0.2f, 0.8f);
                    ve.style.borderBottomColor = new Color(0.6f, 0.2f, 0.8f);
                    ve.style.borderLeftColor = new Color(0.6f, 0.2f, 0.8f);
                    ve.style.borderRightColor = new Color(0.6f, 0.2f, 0.8f);
                    ve.style.borderTopLeftRadius = 10; ve.style.borderBottomRightRadius = 10;
                }).Build();

            // Header
            nnOverlay.Add(new ForgeLabelBuilder("HERMIT BRAIN TELEMETRY").WithColor(Color.white).WithFontStyle(FontStyle.Bold).WithFontSize(14).CreateGui(_guiContext));
            nnOverlay.Add(new ForgeLabelBuilder("GENERATION: 042 | FITNESS OPTIMIZING").WithColor(Color.gray).WithFontSize(10).CreateGui(_guiContext));

            // Spacer
            var line = new VisualElement { style = { height = 1, backgroundColor = new Color(0.6f, 0.2f, 0.8f, 0.5f), marginTop = 10, marginBottom = 10 } };
            nnOverlay.Add(line);

            // Placeholder for the Node Visualizer (To be replaced by your actual NN Node Drawer)
            var networkGraphSim = new VisualElement { style = { flexGrow = 1, backgroundColor = new Color(0, 0, 0, 0.5f), marginBottom = 10 } };
            networkGraphSim.Add(new Label("[ LIVE NEURAL TOPOLOGY ]") { style = { color = new Color(0.8f, 0.4f, 1f), unityTextAlign = TextAnchor.MiddleCenter, flexGrow = 1 } });
            nnOverlay.Add(networkGraphSim);

            // Sensory Input Logs
            var sensoryLog = new VisualElement { style = { height = 100, backgroundColor = new Color(0, 0, 0, 0.5f), paddingBottom = 5, paddingLeft = 5, paddingTop = 5 } };
            sensoryLog.Add(new Label("SENSORY INPUTS:") { style = { color = Color.gray, fontSize = 9 } });
            sensoryLog.Add(new Label("> IN_0: Nearest_Threat_Dist = 14.2m") { style = { color = Color.cyan, fontSize = 11 } });
            sensoryLog.Add(new Label("> IN_1: Nearest_Threat_Vel  = <0.4, -0.8>") { style = { color = Color.cyan, fontSize = 11 } });
            sensoryLog.Add(new Label("> OUT_0: Thrust_Vector      = <0.0, 1.0>") { style = { color = new Color(0.8f, 0.4f, 1f), fontSize = 11 } });
            sensoryLog.Add(new Label("> OUT_1: Weapon_State       = FIRE") { style = { color = new Color(0.8f, 0.4f, 1f), fontSize = 11 } });
            nnOverlay.Add(sensoryLog);

            _rootContainer.Add(nnOverlay);
        }

        private void ToggleAI()
        {
            _aiEngaged = !_aiEngaged;

            // TODO: In AsteroidsContext, we will add a boolean flag 'IsAiControlled'
            // if (_gameContext != null && _gameContext.Fighter != null)
            // {
            //      _gameContext.Fighter.IsAiControlled = _aiEngaged;
            // }

            ForgeLogger.Log($"[TACTICAL] AI Override status: {_aiEngaged}");
            Refresh();
        }

        private void UpdateHUD()
        {
            if (_dataExtractionLabel != null && _gameContext != null && _gameContext.Score != null)
            {
                // Update the score string rapidly
                _dataExtractionLabel.text = GetScoreString();
            }
        }

        private string GetScoreString()
        {
            int score = _gameContext?.Score?.CurrentScore ?? 0;
            return score.ToString("D6"); // Formats as 000000
        }

        // --- PREFAB & INSTANCE MANAGEMENT ---
        private GameObject GetActiveFighterPrefab()
        {
            if (_gameContext == null || _gameContext.Fighter == null || _gameContext.fighterPrefabs == null)
                return null;

            // Find the physical prefab that matches the string name we saved in the Hangar
            return _gameContext.fighterPrefabs.Find(p => p.name == _gameContext.Fighter.Name);
        }

        private GameObject CreateLifeInstance(GameObject prefab)
        {
            if (prefab == null) return null;
            var inst = UnityEngine.Object.Instantiate(prefab);
            inst.name = "[HUD_MINI_FIGHTER]";
            _lifeInstances.Add(inst);
            return inst;
        }

        private void CleanUpInstances()
        {
            foreach (var inst in _lifeInstances)
            {
                if (inst != null) UnityEngine.Object.DestroyImmediate(inst);
            }
            _lifeInstances.Clear();
        }

        // Standard builder accessors
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}