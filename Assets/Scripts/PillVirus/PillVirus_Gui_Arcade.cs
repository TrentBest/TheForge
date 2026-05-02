using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.PillVirus
{
    /// <summary>
    /// Arcade View for Pill Virus. 
    /// Refactored for Pillar 1 (Builders) and Pillar 2 (Context).
    /// </summary>
    public class PillVirus_Gui_Arcade : IGuiProvider
    {
        public string Title => "CLINICAL TRIALS: CO-OP";

        private PillVirusContext _ctx;
        private IGuiRouter _router;
        private FSMHandle _fsmHandle;
        private string _processGroup = "PillVirusUpdate";

        // UI references (Maintained for the Render loop)
        private VisualElement _cabinet;
        private readonly List<VisualElement[,]> _bottleGrids = new();
        private Label _status;
        private VisualElement _nextColor1Cell;
        private VisualElement _nextColor2Cell;
        private VisualElement _gameOverScreen;

        /// <summary>
        /// Default Constructor for Reflection Safety.
        /// Assumes a safe/standby state.
        /// </summary>
        public PillVirus_Gui_Arcade()
        {
            _ctx = new PillVirusContext(); // Safe default
        }

        public PillVirus_Gui_Arcade(IGuiRouter r) : this()
        {
            _router = r;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Rule 8: Context-First Initialization
            // Attempt to resolve existing session data from Global Sovereign Memory
            if (ctx.DataWarehouse != null && ctx.DataWarehouse.TryGetAsset<PillVirusContext>("ActiveSession", out var sessionCtx))
            {
                _ctx = sessionCtx;
            }

            // Initialize FSM if not running
            if (_fsmHandle == null)
            {
                PillVirusLogic.InitializeFSM();
                _fsmHandle = FSM_API.Create.CreateInstance("PillVirusEngine", _ctx, _processGroup);
            }

            // Root Layout via Builder
            var rootBuilder = new ForgeContainerBuilder("ArcadeRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.01f, 0.01f, 0.02f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            // THE CABINET (Multiplayer Grid)
            var cabinetBuilder = new ForgeContainerBuilder("Cabinet")
                .WithDirection(FlexDirection.Row)
                .WithPadding(20);

            _bottleGrids.Clear();
            for (int p = 0; p < _ctx.PlayerCount; p++)
            {
                cabinetBuilder.AddChild(new DynamicGuiProvider(CreateBottle(p)));
            }

            var root = rootBuilder.Build();
            _cabinet = cabinetBuilder.Build();
            root.Add(_cabinet);

            // Status Bar
            var statusBuilder = new ForgeLabelBuilder("SYSTEM READY")
                .WithFontSize(24)
                .WithColor(Color.cyan)
                .WithMarginTop(20);

            _status = statusBuilder.Build() as Label;
            root.Add(_status);

            // Game Over Overlay
            _gameOverScreen = BuildGameOverOverlay();
            root.Add(_gameOverScreen);

            // Input Management
            root.focusable = true;
            root.RegisterCallback<KeyDownEvent>(e => PillVirusLogic.HandleInput(_ctx, e.keyCode, true));
            root.RegisterCallback<KeyUpEvent>(e => PillVirusLogic.HandleInput(_ctx, e.keyCode, false));

            // Heartbeat
            root.schedule.Execute(() => {
                FSM_API.Interaction.Update(_processGroup);
                Render();
            }).Every(16);

            return root;
        }

        private VisualElement CreateBottle(int playerId)
        {
            var bottleGrid = new ForgeGridGuiBuilder($"Bottle_{playerId}", _ctx.Height, _ctx.Width);
            var uiGrid = new VisualElement[_ctx.Width, _ctx.Height];

            for (int y = 0; y < _ctx.Height; y++)
            {
                for (int x = 0; x < _ctx.Width; x++)
                {
                    // Create cell using Container Builder
                    var cell = new ForgeContainerBuilder()
                        .WithWidth(25).WithHeight(25)
                        .Build();

                    uiGrid[x, y] = cell;
                    // FIX: Wrap VisualElement in DynamicGuiProvider to satisfy IGuiProvider requirement
                    bottleGrid.SetCell(y, x, new DynamicGuiProvider(cell));
                }
            }

            _bottleGrids.Add(uiGrid);

            return new ForgeContainerBuilder($"Player_{playerId}_Frame")
                .WithDirection(FlexDirection.Row)
                .WithAlignItems(Align.FlexEnd)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.1f))
                .WithBorderWidth(6, 6, 10, 6)
                .WithBorderColor(new Color(0.4f, 0.6f, 1f))
                .AddChild(bottleGrid)
                .Build();
        }

        private VisualElement BuildGameOverOverlay()
        {
            var overlay = new ForgeContainerBuilder("GameOver")
                .WithPosition(Position.Absolute)
                .WithBackgroundColor(new Color(0, 0, 0, 0.8f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .WithFlexGrow(1);

            var returnBtn = new ForgeButtonBuilder("RETURN TO MENU", () => {
                FSM_API.Interaction.DestroyInstance(_fsmHandle);
                FSM_API.Interaction.DestroyFiniteStateMachine("PillVirusEngine", _processGroup);
                _router?.NavigateTo("MainMenu");
            }).WithFontSize(24).WithPadding(20);

            overlay.AddChild(returnBtn);

            var element = overlay.Build();
            element.style.top = 0; element.style.bottom = 0;
            element.style.left = 0; element.style.right = 0;
            element.style.display = DisplayStyle.None;
            return element;
        }

        private void Render()
        {
            if (_ctx.IsGameOver || _ctx.IsVictory)
            {
                _status.text = _ctx.IsVictory ? "VICTORY" : "DEFEAT";
                _gameOverScreen.style.display = DisplayStyle.Flex;
            }

            // Sync Visual Grid with Context Board
            for (int pIdx = 0; pIdx < _bottleGrids.Count; pIdx++)
            {
                var grid = _bottleGrids[pIdx];
                for (int y = 0; y < _ctx.Height; y++)
                    for (int x = 0; x < _ctx.Width; x++)
                        ApplyPillStyle(grid[x, y], _ctx.Board[x, y], x, y);
            }
        }

        private void ApplyPillStyle(VisualElement ve, GridCell data, int x, int y)
        {
            ve.style.backgroundColor = (data.ColorId > 0) ? _ctx.ColorPalette[data.ColorId] : Color.clear;
            // ... (Rounding logic remains same, but utilizes VisualElement style properties)
        }

        public Action<VisualElement> GetGuiBuilder() => r => r.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string p) { }
        public void ToUIDocument(string p) { }
    }
}