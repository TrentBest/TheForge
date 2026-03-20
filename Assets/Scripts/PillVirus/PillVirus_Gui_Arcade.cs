using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.MicroPackages.Packages.FSM.HelloWorld;


#if UNITY_EDITOR
// The Nuclear Namespace we set up

#endif

// Explicitly stating we are using the runtime PillVirus namespace
using TheSingularityWorkshop.Sandbox.PillVirus;

namespace Singularity
{
    public class PillVirus_Gui_Arcade : IGuiProvider
    {
        public string Title => "CLINICAL TRIALS: CO-OP";

        private PillVirusContext _context;
        private VisualElement _rootContainer;
        private VisualElement[,] _uiGrid;

        public PillVirus_Gui_Arcade()
        {
            _context = new PillVirusContext(width: 12, height: 18, matchReq: 4, playerCount: 2);
            PillVirusLogic.InitializeFSM();

#if UNITY_EDITOR
           // FSM_EditorIntegrationAdvanced.AddProcessingGroup("EditorUpdate", "PillVirusGroup");
#endif
            global::TheSingularityWorkshop.FSM_API.FSM_API.Create.CreateInstance("PillVirusEngine", _context, "PillVirusGroup");
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _rootContainer = new VisualElement { style = { flexGrow = 1, backgroundColor = new Color(0.05f, 0.05f, 0.1f), alignItems = Align.Center, justifyContent = Justify.Center } };

            _rootContainer.focusable = true;
            _rootContainer.RegisterCallback<KeyDownEvent>(OnKeyDown);
            _rootContainer.schedule.Execute(() => _rootContainer.Focus());

            int cellSize = 30;
            int exactWidth = _context.Width * cellSize;
            int exactHeight = _context.Height * cellSize;

            var bottleView = new VisualElement
            {
                style = {
                    width = exactWidth,
                    height = exactHeight,
                    backgroundColor = new Color(0.1f, 0.1f, 0.15f),
                    borderBottomLeftRadius = 20, borderBottomRightRadius = 20,
                    borderTopWidth = 5, borderBottomWidth = 5, borderLeftWidth = 5, borderRightWidth = 5,
                    borderTopColor = new Color(0.6f, 0.8f, 0.9f), borderBottomColor = new Color(0.6f, 0.8f, 0.9f),
                    borderLeftColor = new Color(0.6f, 0.8f, 0.9f), borderRightColor = new Color(0.6f, 0.8f, 0.9f),
                    flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap
                }
            };

            _uiGrid = new VisualElement[_context.Width, _context.Height];

            for (int y = 0; y < _context.Height; y++)
            {
                for (int x = 0; x < _context.Width; x++)
                {
                    StyleBackgroundSize HelloWorldFriend = new StyleBackgroundSize(StyleKeyword.Auto);
                    var cell = new VisualElement
                    {
                        style = {
                            width = cellSize, height = cellSize,
                            borderTopWidth = 1, borderLeftWidth = 1,
                            borderTopColor = new Color(0,0,0,0.15f), borderLeftColor = new Color(0,0,0,0.15f),
                            backgroundSize = HelloWorldFriend
                            
                        }
                    };
                    _uiGrid[x, y] = cell;
                    bottleView.Add(cell);
                }
            }

            _rootContainer.Add(bottleView);

            _rootContainer.schedule.Execute(() => RenderGrid()).Every(16);

            return _rootContainer;
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            PillVirusLogic.HandleInput(_context, evt.keyCode);
        }

        private void RenderGrid()
        {
            for (int y = 0; y < _context.Height; y++)
            {
                for (int x = 0; x < _context.Width; x++)
                {
                    var cellData = _context.Board[x, y];
                    var uiElement = _uiGrid[x, y];

                    uiElement.style.backgroundColor = Color.clear;
                    uiElement.style.borderTopLeftRadius = 0; uiElement.style.borderTopRightRadius = 0;
                    uiElement.style.borderBottomLeftRadius = 0; uiElement.style.borderBottomRightRadius = 0;

                    // FIX: Bulletproof C# assignment to avoid the "unassigned local variable" compiler error
                    Color mappedColor = Color.clear;
                    bool hasColor = _context.ColorPalette.TryGetValue(cellData.ColorId, out mappedColor);

                    if (cellData.Type != CellType.Empty && hasColor)
                    {
                        uiElement.style.backgroundColor = mappedColor;

                        if (cellData.Type == CellType.Virus)
                        {
                            uiElement.style.borderTopLeftRadius = Length.Percent(50);
                            uiElement.style.borderTopRightRadius = Length.Percent(50);
                            uiElement.style.borderBottomLeftRadius = Length.Percent(50);
                            uiElement.style.borderBottomRightRadius = Length.Percent(50);
                        }
                    }
                }
            }

            foreach (var player in _context.ActivePlayers)
            {
                if (player.Y < _context.Height && player.X < _context.Width)
                {
                    _uiGrid[player.X, player.Y].style.backgroundColor = _context.ColorPalette[player.Color1];

                    if (player.IsHorizontal && player.X + 1 < _context.Width)
                        _uiGrid[player.X + 1, player.Y].style.backgroundColor = _context.ColorPalette[player.Color2];
                    else if (!player.IsHorizontal && player.Y + 1 < _context.Height)
                        _uiGrid[player.X, player.Y + 1].style.backgroundColor = _context.ColorPalette[player.Color2];
                }
            }
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
    }
}