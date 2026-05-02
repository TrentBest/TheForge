using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.BardsTale.UI;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.BardsTale
{
    public class BardsTale_ExplorationGuiProvider : IGuiProvider
    {
        public string Title => "SKARA BRAE: EXPLORATION";
        private IGuiRouter _router;
        private BardsTaleExperienceContext _context;
        private VisualElement _mapContainer;

        public BardsTale_ExplorationGuiProvider(IGuiRouter router)
        {
            _router = router;
            _context = UnityEngine.Object.FindObjectOfType<BardsTaleExperienceContext>();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            return new ForgeSplitPanelBuilder(sidebarWidth: 400, Side.Right)
                .WithMain(BuildFirstPersonViewport(ctx))
                .WithSidebar(BuildGraphPaperMapper(ctx))
                .CreateGui(ctx);
        }

        private IGuiProvider BuildFirstPersonViewport(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("SkaraBraeView")
                .WithBackgroundColor(Color.black)
                .OnBuild(ve =>
                {
                    // 1. Instantiate the Viewport Provider
                    var viewport = new BardsTaleViewportProvider(_context);

                    // 2. Inject the real 3D Preview instead of the placeholder Label
                    ve.Add(viewport.CreateViewport());

                    // 3. Navigation Controls overlay
                    var controls = new VisualElement
                    {
                        style = {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.Center,
                position = Position.Absolute,
                bottom = 20, width = Length.Percent(100)
                    }
                    };
                    controls.Add(new ForgeButtonBuilder("TURN LEFT", () => RotatePlayer(-1)).Build());
                    controls.Add(new ForgeButtonBuilder("FORWARD", () => MovePlayer()).Build());
                    controls.Add(new ForgeButtonBuilder("TURN RIGHT", () => RotatePlayer(1)).Build());
                    ve.Add(controls);
                });
        }

        private IGuiProvider BuildGraphPaperMapper(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("Automapper")
                .WithPadding(15)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .AddHeader("DIGITAL GRAPH PAPER")
                .AddSeparator(Color.gray, 1)
                .OnBuild(ve =>
                {
                    _mapContainer = new VisualElement { style = { flexGrow = 1, marginTop = 10, alignItems = Align.Center, justifyContent = Justify.Center } };
                    RenderGraphPaper(ctx);
                    ve.Add(_mapContainer);
                })
                .AddChild(new ForgeButtonBuilder("CAMP (QUIT)", () => _router.NavigateTo("ExitToForge")).Build());
        }

        private void MovePlayer()
        {
            // FIX: Using local variable to avoid CS1612 struct copy modification error
            Vector2Int nextPos = _context.PlayerPosition + _context.PlayerDirection;
            nextPos.x = Mathf.Clamp(nextPos.x, 0, 29);
            nextPos.y = Mathf.Clamp(nextPos.y, 0, 29);

            _context.PlayerPosition = nextPos;
            _context.ExploredMap[nextPos.x, nextPos.y] = true;
            RefreshMap();
        }

        private void RotatePlayer(int dir)
        {
            if (dir > 0) _context.PlayerDirection = new Vector2Int(_context.PlayerDirection.y, -_context.PlayerDirection.x);
            else _context.PlayerDirection = new Vector2Int(-_context.PlayerDirection.y, _context.PlayerDirection.x);
            RefreshMap();
        }

        private void RefreshMap()
        {
            _mapContainer.Clear();
            RenderGraphPaper(new GuiContext());
        }

        private void RenderGraphPaper(GuiContext ctx)
        {
            float cellSize = 12f;
            var grid = new VisualElement { style = { width = 30 * cellSize, height = 30 * cellSize, flexDirection = FlexDirection.ColumnReverse } };

            for (int y = 0; y < 30; y++)
            {
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                for (int x = 0; x < 30; x++)
                {
                    var cell = new VisualElement();
                    cell.style.width = cellSize;
                    cell.style.height = cellSize;
                    cell.style.borderTopWidth = 0.5f; cell.style.borderBottomWidth = 0.5f;
                    cell.style.borderLeftWidth = 0.5f; cell.style.borderRightWidth = 0.5f;

                    var gridColor = new Color(0.2f, 0.2f, 0.2f);
                    cell.style.borderTopColor = gridColor; cell.style.borderBottomColor = gridColor;
                    cell.style.borderLeftColor = gridColor; cell.style.borderRightColor = gridColor;

                    if (_context.PlayerPosition.x == x && _context.PlayerPosition.y == y)
                        cell.style.backgroundColor = Color.yellow;
                    else if (_context.ExploredMap[x, y])
                        cell.style.backgroundColor = new Color(0.3f, 0.3f, 0.4f);

                    row.Add(cell);
                }
                grid.Add(row);
            }
            _mapContainer.Add(grid);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string p) { }
        public void FromUIDocument(string p) { }
    }
}