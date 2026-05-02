using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_InGameHUD : IGuiProvider
    {
        public string Title => "MOO2 COMMAND INTERFACE";

        private GuiContext _lastCtx;
        private VisualElement _centerViewport;

        // --- STATE ---
        private string _currentStardate;
        private int _credits;
        private int _commandPoints;

        // --- ROUTING ACTIONS ---
        public Action OnQuitToMainMenu { get; set; }

        // REQUIRED: Parameterless Constructor for Reflection
        public MastersOfOrionII_Gui_InGameHUD()
        {
            AllocateSafeState();
        }

        // Convenience Constructor for Manual Routing
        public MastersOfOrionII_Gui_InGameHUD(Action onQuitToMainMenu)
        {
            OnQuitToMainMenu = onQuitToMainMenu;
            AllocateSafeState();
        }

        // Guarantees the object is completely safe to render even if injected completely empty
        private void AllocateSafeState()
        {
            _currentStardate = string.IsNullOrEmpty(_currentStardate) ? "3500.0" : _currentStardate;
            _credits = _credits == 0 ? 50 : _credits;
            _commandPoints = _commandPoints == 0 ? 10 : _commandPoints;

            // Safe fallback for delegates to prevent null reference crashes if not injected
            OnQuitToMainMenu ??= () => Debug.LogWarning($"[{Title}] OnQuitToMainMenu triggered, but no delegate was provided.");
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var root = new GraphicalUserInterfaceBuilder("MoO2_InGameHUD")
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.01f, 0.01f, 0.02f))
                .WithFlexLayout(FlexDirection.Column, Justify.SpaceBetween, Align.Stretch)
                .Build();

            // --- 1. TOP NAV BAR ---
            var topBar = new GraphicalUserInterfaceBuilder("TopNavBar")
                .WithBackgroundColor(new Color(0.1f, 0.15f, 0.2f, 0.9f))
                .WithFlexLayout(FlexDirection.Row, Justify.Center, Align.Center)
                .Build();

            topBar.style.height = 40;
            topBar.style.borderBottomWidth = 2;
            topBar.style.borderBottomColor = new Color(0, 0.5f, 0.8f);

            topBar.Add(CreateNavBtn("GAME", () => OnQuitToMainMenu?.Invoke()));
            topBar.Add(CreateNavBtn("DESIGN", () => LoadView(new DummyView("SHIP DESIGNER"))));
            topBar.Add(CreateNavBtn("FLEET", () => LoadView(new DummyView("FLEET COMMAND"))));
            topBar.Add(CreateNavBtn("MAP", () => LoadView(new MastersOfOrionII_Gui_GalaxyMap())));
            topBar.Add(CreateNavBtn("RACES", () => LoadView(new DummyView("DIPLOMACY"))));
            topBar.Add(CreateNavBtn("PLANETS", () => LoadView(new DummyView("COLONY ROSTER"))));
            topBar.Add(CreateNavBtn("TECH", () => LoadView(new DummyView("RESEARCH MATRIX"))));

            root.Add(topBar);

            // --- 2. CENTER VIEWPORT ---
            _centerViewport = new GraphicalUserInterfaceBuilder("CenterViewport").Build();
            _centerViewport.style.flexGrow = 1;
            _centerViewport.style.position = Position.Relative;
            root.Add(_centerViewport);

            // --- 3. BOTTOM BAR (TURN & STATS) ---
            var bottomBar = new GraphicalUserInterfaceBuilder("BottomStatusBar")
                .WithBackgroundColor(new Color(0.05f, 0.08f, 0.1f, 0.95f))
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithPadding(10)
                .Build();

            bottomBar.style.height = 60;
            bottomBar.style.borderTopWidth = 2;
            bottomBar.style.borderTopColor = new Color(0, 0.5f, 0.8f);

            var statsGroup = new GraphicalUserInterfaceBuilder("StatsGroup")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .Build();

            statsGroup.Add(new Label($"BC: {_credits}") { style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold, marginRight = 20, fontSize = 16 } });
            statsGroup.Add(new Label($"CP: {_commandPoints}") { style = { color = Color.green, unityFontStyleAndWeight = FontStyle.Bold, marginRight = 20, fontSize = 16 } });
            statsGroup.Add(new Label($"STARDATE: {_currentStardate}") { style = { color = Color.cyan, fontSize = 16 } });
            bottomBar.Add(statsGroup);

            var turnBtn = new Button(() => AdvanceTurn())
            {
                text = "TURN",
                style = {
                    backgroundColor = new Color(0.6f, 0.1f, 0.1f),
                    color = Color.white,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 20,
                    width = 120, height = 40,
                    borderTopLeftRadius = 5, borderTopRightRadius = 5, borderBottomLeftRadius = 5, borderBottomRightRadius = 5
                }
            };
            bottomBar.Add(turnBtn);

            root.Add(bottomBar);

            // Safe Default Launch State
            LoadView(new MastersOfOrionII_Gui_GalaxyMap());

            return root;
        }

        private VisualElement CreateNavBtn(string text, Action onClick)
        {
            return new Button(onClick)
            {
                text = text,
                style = {
                    backgroundColor = Color.clear,
                    color = Color.white,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    borderTopWidth = 0, borderBottomWidth = 0, borderLeftWidth = 0, borderRightWidth = 0,
                    marginLeft = 10, marginRight = 10, fontSize = 14
                }
            };
        }

        private void LoadView(IGuiProvider viewProvider)
        {
            if (_centerViewport == null) return;
            _centerViewport.Clear();
            _centerViewport.Add(viewProvider.CreateGui(new GuiContext()));
        }

        private void AdvanceTurn()
        {
            Debug.Log("[MoO2 Engine] Processing End of Turn...");
            if (float.TryParse(_currentStardate, out float date))
            {
                _currentStardate = (date + 0.1f).ToString("F1");
            }
            CreateGui(_lastCtx);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }

    // STRICT COMPLIANCE: Safe State Dummy View
    public class DummyView : IGuiProvider
    {
        public string Title { get; private set; }

        // REQUIRED: Parameterless constructor
        public DummyView()
        {
            AllocateSafeState();
        }

        public DummyView(string title)
        {
            Title = title;
            AllocateSafeState();
        }

        private void AllocateSafeState()
        {
            Title = string.IsNullOrEmpty(Title) ? "UNALLOCATED_CONSTRUCT" : Title;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder($"Dummy_{Title}")
                .WithPercentSize(100, 100)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .AddChild(new Label($"[{Title} UNDER CONSTRUCTION]") { style = { color = Color.gray, fontSize = 24 } })
                .Build();
        }
        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string p) { }
        public void FromUIDocument(string p) { }
    }
}