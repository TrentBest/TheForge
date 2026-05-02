using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Assets.Scripts.MastersOfOrionII
{
    /// <summary>
    /// The Multi-Phase Orchestrator for Game Initialization.
    /// Manages the transition from cosmic constants to biological traits.
    /// </summary>
    public class MastersOfOrionII_Gui_GameBuilder : IGuiProvider
    {
        public string Title => "UNIVERSE ARCHITECT";

        // --- SESSION DATA ---
        public GalaxyConfigData GalaxyConfig { get; private set; } = new GalaxyConfigData();
        public RaceConfigData RaceConfig { get; private set; } = new RaceConfigData();

        // --- FSM STATE ---
        private enum BuilderPhase { Galaxy, Race, LaunchReady }
        private BuilderPhase _currentPhase = BuilderPhase.Galaxy;

        // --- SUB-PROVIDERS ---
        private readonly MastersOfOrionII_Gui_GalaxyConfig _galaxyGui;
        private readonly MastersOfOrionII_Gui_RaceDesigner _raceGui;

        private VisualElement _contentArea;
        private MastersOfOrionII_Game _gameInstance;

        public MastersOfOrionII_Gui_GameBuilder()
        {
            _galaxyGui = new MastersOfOrionII_Gui_GalaxyConfig(this);
            _raceGui = new MastersOfOrionII_Gui_RaceDesigner(this);
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _gameInstance = Object.FindAnyObjectByType<MastersOfOrionII_Game>();

            var rootBuilder = new ForgeContainerBuilder("GameBuilder_Root")
                .WithFlexGrow(1f)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.05f)); // Deep Space Blue

            // 1. Progress Breadcrumb Header
            rootBuilder.AddChild(new ForgeContainerBuilder("Breadcrumbs")
                .WithDirection(FlexDirection.Row).WithPadding(15f).WithBackgroundColor(new Color(0.05f, 0.05f, 0.1f))
                .AddChild(CreateBreadcrumb("GALAXY", BuilderPhase.Galaxy))
                .AddSeparator(Color.cyan, 2f)
                .AddChild(CreateBreadcrumb("RACE", BuilderPhase.Race))
                .AddSeparator(Color.gray, 2f)
                .AddChild(CreateBreadcrumb("LAUNCH", BuilderPhase.LaunchReady))
            );

            // 2. Main Content Viewport
            rootBuilder.AddChild(new ForgeContainerBuilder("Viewport")
                .WithFlexGrow(1f)
                // FIX: Solving lambda conversion via DynamicGuiProvider
                .AddChild(new DynamicGuiProvider(c => {
                    _contentArea = new VisualElement { style = { flexGrow = 1 } };
                    RefreshView();
                    return _contentArea;
                }))
            );

            // 3. Navigation Footer
            rootBuilder.AddChild(new ForgeContainerBuilder("Footer")
                .WithDirection(FlexDirection.Row).WithJustifyContent(Justify.SpaceBetween).WithPadding(15f)
                .AddChild(new ForgeButtonBuilder("PREVIOUS")
                    .WithWidth(120f).WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f))
                    .OnClick(NavigateBack))
                .AddChild(new ForgeButtonBuilder(_currentPhase == BuilderPhase.Race ? "LAUNCH UNIVERSE" : "NEXT")
                    .WithWidth(180f).WithBackgroundColor(new Color(0.1f, 0.4f, 0.8f))
                    .OnClick(NavigateNext))
            );

            return rootBuilder.Build();
        }

        private IGuiProvider CreateBreadcrumb(string label, BuilderPhase target)
        {
            bool isActive = _currentPhase == target;
            return new ForgeLabelBuilder(label)
                .WithColor(isActive ? Color.cyan : Color.gray)
                .WithBold()
                .WithMarginLeft(20f).WithMarginRight(20f);
        }

        private void RefreshView()
        {
            if (_contentArea == null) return;
            _contentArea.Clear();

            switch (_currentPhase)
            {
                case BuilderPhase.Galaxy:
                    _contentArea.Add(_galaxyGui.CreateGui(new GuiContext()));
                    break;
                case BuilderPhase.Race:
                    _contentArea.Add(_raceGui.CreateGui(new GuiContext()));
                    break;
                case BuilderPhase.LaunchReady:
                    _contentArea.Add(new ForgeLabelBuilder("ALL SYSTEMS GO. CLICK LAUNCH TO BEGIN.").WithFontSize(24).WithBold().Build());
                    break;
            }
        }

        // --- NAVIGATION LOGIC ---

        private void NavigateNext()
        {
            if (_currentPhase == BuilderPhase.Galaxy)
            {
                _currentPhase = BuilderPhase.Race;
                RaceConfig.TotalPoints = GalaxyConfig.CalculatePoints();
                RefreshView();
            }
            else if (_currentPhase == BuilderPhase.Race)
            {
                FinishAndLaunch();
            }
        }

        private void NavigateBack()
        {
            if (_currentPhase == BuilderPhase.Race)
            {
                _currentPhase = BuilderPhase.Galaxy;
                RefreshView();
            }
        }

        public void FinishAndLaunch()
        {
            Debug.Log("[OrionBuilder] Seeding Universe...");
            UniverseGenerator.StartGeneration(_gameInstance, GalaxyConfig, RaceConfig);
            _gameInstance.LaunchGame(); // Triggers FSM transition to StarMap_Strategic
        }

        public System.Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "GameBuilder_Export");
        public void FromUIDocument(string path) { }

        internal object AdvanceToRace()
        {
            throw new System.NotImplementedException();
        }
    }
}