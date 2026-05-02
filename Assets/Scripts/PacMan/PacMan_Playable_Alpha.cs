using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.PacMan;
using TheSingularityWorkshop.FSM_API;

public class PacMan_Playable_Alpha : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    private PacManContext _context;

    void Start()
    {
        // 1. Build FSM Definition
        PacManFsmBuilder.BuildDefinition();

        // 2. Initialize Data Context
        _context = CreateDefaultLevel();

        // 3. Create FSM Instance and link to Context
        var handle = FSM_API.Create.CreateInstance("PacMan_Core", _context, "PacMan_Simulation");
        _context.Handle = handle;

        // 4. Bootstrap UI
        var arcadeGui = new PacMan_Gui_Main(); // Injected with _context in real runtime
        _uiDocument.rootVisualElement.Add(arcadeGui.CreateGui(new Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.GuiContext()));

        handle.TransitionTo("Ready");
    }

    private PacManContext CreateDefaultLevel()
    {
        int w = 15, h = 15;
        var ctx = new PacManContext { MazeGrid = new GridTile[w, h] };

        // Simple Border generation (Empire Pattern)
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (x == 0 || x == w - 1 || y == 0 || y == h - 1) ctx.MazeGrid[x, y] = GridTile.Wall;
                else ctx.MazeGrid[x, y] = GridTile.Pellet;
            }

        // Procedural "Block" Walls
        ctx.MazeGrid[2, 2] = GridTile.Wall; ctx.MazeGrid[2, 3] = GridTile.Wall;
        ctx.MazeGrid[12, 12] = GridTile.Wall; ctx.MazeGrid[12, 11] = GridTile.Wall;

        // Setup Spawn Points
        ctx.PacManSpawn = new Vector2Int(7, 1);
        ctx.PacManPosition = ctx.PacManSpawn;
        ctx.MazeGrid[7, 7] = GridTile.PowerPellet;

        ctx.Ghosts.Add(new GhostContext { Name = "Blinky", SpawnPoint = new Vector2Int(7, 8), Position = new Vector2Int(7, 8), GhostColor = Color.red });

        return ctx;
    }
}