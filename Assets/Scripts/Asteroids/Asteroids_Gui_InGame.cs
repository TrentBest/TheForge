#if UNITY_EDITOR
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Asteroids.Editor
{
    // Added DimensionSelect and split ActivePlay into 2D and 3D
    public enum InGameMenuState { Workshop, Settings, Hangar, ShipBuilder, DimensionSelect, ActivePlay2D, ActivePlay3D }

    public class Asteroids_Gui_InGame : IGuiProvider
    {
        public string Title => "ASTEROIDS: TACTICAL FORGE";

        private GuiContext _guiContext;
        private AsteroidsContext _gameContext;
        private VisualElement _rootContainer;
        private HangarTheme _theme = new HangarTheme();

        private InGameMenuState _currentState = InGameMenuState.Workshop;

        public VisualElement CreateGui(GuiContext context)
        {
            _guiContext = context;
            _gameContext = UnityEngine.Object.FindAnyObjectByType<AsteroidsContext>();
            _rootContainer = new VisualElement { style = { flexGrow = 1 } };
            Refresh();
            return _rootContainer;
        }

        private void Refresh()
        {
            _rootContainer.Clear();

            switch (_currentState)
            {
                case InGameMenuState.DimensionSelect: RenderDimensionSelect(); break;
                case InGameMenuState.Hangar: RenderHangarView(); break;
                case InGameMenuState.ShipBuilder: RenderShipBuilder(); break;
                case InGameMenuState.Settings: RenderSettingsPanel(); break;
                case InGameMenuState.ActivePlay2D: Render2DContextHUD(); break;
                case InGameMenuState.ActivePlay3D: Render3DCockpitHandoff(); break;
                case InGameMenuState.Workshop:
                default: RenderWorkshopMenu(); break;
            }
        }

        // --- 1. MAIN MENU ---
        private void RenderWorkshopMenu()
        {
            var mainView = new GraphicalUserInterfaceBuilder("Forge_Root")
                .WithBackgroundColor(_theme.BaseBackground)
                .WithFlexLayout(FlexDirection.Column, Justify.SpaceBetween, Align.Center)
                .WithPercentSize(100, 100)
                .WithPadding(40);

            var header = new Label("ORBITAL WORKSHOP") { style = { color = _theme.PrimaryAccent, fontSize = 48, unityFontStyleAndWeight = FontStyle.Bold, letterSpacing = 5, borderBottomWidth = 4, borderBottomColor = _theme.PrimaryAccent, paddingBottom = 10, marginTop = 20 } };
            mainView.AddChild(new GenericGuiProvider(() => header));

            var idleView = new VisualElement { style = { flexGrow = 1, justifyContent = Justify.Center, alignItems = Align.Center } };

            // ROUNTING UPDATED:
            // 1. Deploy takes us to the 2D/3D choice
            idleView.Add(CreateForgeButton("DEPLOY FIGHTER (START)", () => { _currentState = InGameMenuState.DimensionSelect; Refresh(); }, 350, 60, true));

            // 2. Schematics takes us to the Custom Ship Builder
            idleView.Add(CreateForgeButton("ACCESS SCHEMATICS (SHIPWRIGHT)", () => { _currentState = InGameMenuState.ShipBuilder; Refresh(); }, 350, 45));

            // 3. Settings moved here
            idleView.Add(CreateForgeButton("SYSTEM SETTINGS", () => { _currentState = InGameMenuState.Settings; Refresh(); }, 350, 45));

            mainView.AddChild(new GenericGuiProvider(() => idleView));
            _rootContainer.Add(mainView.Build());
        }

        // --- 2. DIMENSION SELECTOR ---
        private void RenderDimensionSelect()
        {
            var view = new GraphicalUserInterfaceBuilder("DimSelect_Root")
                .WithBackgroundColor(_theme.BaseBackground)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .WithPercentSize(100, 100);

            view.AddChild(new Label("SELECT COMBAT DIMENSION") { style = { color = _theme.TitleText, fontSize = 36, marginBottom = 40, unityFontStyleAndWeight = FontStyle.Bold } });

            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            // 2D Path -> Straight to the game
            row.Add(CreateForgeButton("2D CLASSIC ARCADE\n(Top-Down Vector)", () => {
                Debug.Log("[FSM] Loading 2D Asteroids Context..");
                _currentState = InGameMenuState.ActivePlay2D;
                Refresh();
            }, 250, 80, true));

            row.Add(new VisualElement { style = { width = 40 } }); // Spacer

            // 3D Path -> To the Armory Hologram Hangar
            row.Add(CreateForgeButton("3D TACTICAL SIM\n(First-Person Newtonian)", () => {
                Debug.Log("[FSM] Loading 3D Hangar Armory Terminal..");
                _currentState = InGameMenuState.Hangar;
                Refresh();
            }, 250, 80, true));

            view.AddChild(new GenericGuiProvider(() => row));
            view.AddChild(new GenericGuiProvider(() => CreateForgeButton("<< ABORT SEQUENCE", () => { _currentState = InGameMenuState.Workshop; Refresh(); }, 200, 40)));

            _rootContainer.Add(view.Build());
        }

        // --- 3. ARMORY HANGAR (3D Selection) ---
        private void RenderHangarView()
        {
            var hangarGui = new Asteroids_Gui_Hangar(_gameContext,
                onBack: () => { _currentState = InGameMenuState.DimensionSelect; Refresh(); },

                // When they hit LAUNCH in the Armory, we transition to the physical 3D world
                onLaunch: () => { _currentState = InGameMenuState.ActivePlay3D; Refresh(); },

                // We keep a shortcut to the Shipwright here too, just like in the menu
                onOpenBuilder: () => { _currentState = InGameMenuState.ShipBuilder; Refresh(); },
                _theme
            );
            _rootContainer.Add(hangarGui.CreateGui(_guiContext));
        }

        // --- 4. SHIPWRIGHT (Custom Builder) ---
        private void RenderShipBuilder()
        {
            var builderGui = new Asteroids_Gui_ShipBuilder(_gameContext,
                onBack: () => { _currentState = InGameMenuState.Workshop; Refresh(); },
                onSave: (newShipObject) => {
                    _gameContext.fighterPrefabs.Add(newShipObject);
                    _currentState = InGameMenuState.Hangar; // Automatically take them to the Hangar to see their new ship
                    Refresh();
                },
                _theme
            );
            _rootContainer.Add(builderGui.CreateGui(_guiContext));
        }

        // --- 5A. 2D HUD ---
        private void Render2DContextHUD()
        {
            var hudView = new GraphicalUserInterfaceBuilder("HUD_Root_2D")
                .WithBackgroundColor(Color.clear)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.FlexStart)
                .WithPercentSize(100, 100)
                .WithPadding(20);

            var menuBtn = CreateForgeButton("<< ABORT TO WORKSHOP", () => { _currentState = InGameMenuState.Workshop; Refresh(); }, 220, 40, true);
            hudView.AddChild(new GenericGuiProvider(() => menuBtn));

            var scoreBox = new VisualElement { style = { alignItems = Align.FlexEnd } };
            scoreBox.Add(new Label("SCORE: 000000") { style = { color = _theme.PrimaryAccent, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold } });
            hudView.AddChild(new GenericGuiProvider(() => scoreBox));

            _rootContainer.Add(hudView.Build());
        }

        // --- 5B. 3D COCKPIT HANDOFF ---
        private void Render3DCockpitHandoff()
        {
            // The UI effectively disappears here (Color.clear) 
            // In a real implementation, this UI element would invoke your FSM_API to trigger 
            // the "Spawn Gantry -> Move Player Camera -> Boot Cockpit InWorldGui" state sequence.

            var hudView = new GraphicalUserInterfaceBuilder("HUD_Root_3D")
                .WithBackgroundColor(Color.clear)
                .WithPercentSize(100, 100)
                .WithPadding(20);

            // A tiny emergency abort button in the corner, everything else is handled by the 3D Cockpit
            var menuBtn = CreateForgeButton("EJECT (DEV)", () => { _currentState = InGameMenuState.Workshop; Refresh(); }, 150, 30);
            hudView.AddChild(new GenericGuiProvider(() => menuBtn));

            _rootContainer.Add(hudView.Build());
        }

        // --- SETTINGS & UTILS ---
        private void RenderSettingsPanel() { /* Same as before, just route Back to InGameMenuState.Workshop */ }

        private Button CreateForgeButton(string text, Action onClick, float width, float height, bool isPrimary = false)
        {
            return new Button(onClick) { text = text, style = { width = width, height = height, marginTop = 10, backgroundColor = isPrimary ? _theme.PrimaryAccent : _theme.BaseBackground, color = isPrimary ? Color.black : _theme.TitleText, unityFontStyleAndWeight = FontStyle.Bold, fontSize = isPrimary ? 16 : 12, borderTopWidth = 2, borderBottomWidth = 4, borderLeftWidth = 2, borderRightWidth = 2, borderTopColor = _theme.BorderDark, borderLeftColor = _theme.BorderDark, borderBottomColor = Color.black, borderRightColor = Color.black } };
        }
        private Foldout CreateForgeFoldout(string title, VisualElement content) { /* Same as before */ return new Foldout(); }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}
#endif