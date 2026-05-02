using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.PacMan
{
    public class PacMan_Gui_Main : IGuiProvider
    {
        public string Title => "PAC-MAN ARCADE";
        private PacManContext _context;
        private VisualElement[,] _tileMap;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("PacManRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(Color.black)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .Build();

            // 1. Initialize Context if missing (Editor Safety)
            if (_context == null)
            {
                _context = new PacManContext();
                // FIX: Ensure MazeGrid is not null for the layout calculations
                _context.MazeGrid = new GridTile[1, 1];
            }

            // 2. Setup the Grid Viewport
            var mazeContainer = new VisualElement { name = "MazeViewport" };
            mazeContainer.style.width = _context.Width * 20;
            mazeContainer.style.height = _context.Height * 20;
            mazeContainer.style.flexWrap = Wrap.Wrap;
            root.Add(mazeContainer);

            InitializeTileMap(mazeContainer);

            root.schedule.Execute(() => {
                // FIX: Ensure the FSM exists before ticking to prevent Editor log spam
                if (FSM_API.Interaction.Exists("PacMan_Core", "PacMan_Simulation"))
                {
                    FSM_API.Interaction.Update("PacMan_Simulation");
                }

                UpdateVisuals();
            }).Every(16);

            return root;
        }

        private void InitializeTileMap(VisualElement container)
        {
            // FIX: Added guard against 0-sized grids
            int w = Mathf.Max(10, _context.Width);
            int h = Mathf.Max(10, _context.Height);

            _tileMap = new VisualElement[w, h];
            for (int y = h - 1; y >= 0; y--)
            {
                for (int x = 0; x < w; x++)
                {
                    var tile = new VisualElement { style = { width = 20, height = 20 } };
                    container.Add(tile);
                    _tileMap[x, y] = tile;
                }
            }
        }

        private void UpdateVisuals()
        {
            // FIX: Ensure tilemap and grid are ready before drawing
            if (_tileMap == null || _context.MazeGrid == null) return;

            for (int x = 0; x < _context.Width; x++)
            {
                for (int y = 0; y < _context.Height; y++)
                {
                    var tile = _tileMap[x, y];
                    var type = _context.MazeGrid[x, y];

                    tile.style.backgroundColor = type switch
                    {
                        GridTile.Wall => Color.blue,
                        GridTile.Pellet => Color.white,
                        GridTile.PowerPellet => Color.yellow,
                        _ => Color.clear
                    };

                    if (new Vector2Int(x, y) == _context.PacManPosition)
                        tile.style.backgroundColor = Color.yellow;
                }
            }
        }

        public System.Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}