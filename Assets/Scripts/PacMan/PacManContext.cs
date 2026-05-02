using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using Workshop.Systems.FSMs.Contexts;

namespace Assets.Scripts.PacMan
{
    public enum GhostState { Chasing, Fleeing, Respawning }
    public enum GridTile { Empty, Wall, Pellet, PowerPellet, GhostHouse }

    public class PacManContext : IStateContext
    {
        public FSMHandle Handle { get; set; }

        // Maze Data
        public GridTile[,] MazeGrid { get; set; }

        // FIX: Added null checks to properties to prevent Editor crashes
        public int Width => MazeGrid != null ? MazeGrid.GetLength(0) : 0;
        public int Height => MazeGrid != null ? MazeGrid.GetLength(1) : 0;

        public Vector2Int PacManPosition { get; set; }
        public Vector2Int PacManSpawn { get; set; }
        public Vector2Int CurrentDirection { get; set; }
        public Vector2Int IntentDirection { get; set; }
        public int Score { get; set; }
        public int Lives { get; set; } = 3;

        public List<GhostContext> Ghosts { get; set; } = new();

        public bool IsEnergized { get; set; }
        public float EnergizerTimer { get; set; }
        public string ProcessingGroup => "PacMan_Simulation";

        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "PacMan_Simulation_Context";
    }

    public class GhostContext : IStateContext
    {
        public string Name { get; set; }
        public Vector2Int Position { get; set; }
        public Vector2Int SpawnPoint { get; set; }
        public GhostState CurrentBehavior { get; set; }
        public Color GhostColor { get; set; }
        public Vector2Int TargetTile { get; set; }

        public bool IsValid { get; set; } = true;
    }
}