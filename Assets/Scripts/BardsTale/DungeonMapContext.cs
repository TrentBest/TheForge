using UnityEngine;
using System.Collections.Generic;

namespace Workshop.BardsTale.World
{
    public enum TileFeature { None, Wall, Door, SecretDoor, StairsUp, StairsDown }

    [System.Serializable]
    public class DungeonCell
    {
        public Vector2Int Coordinates;
        // Bitmask or simple bools for the 4 cardinal walls
        public TileFeature NorthWall;
        public TileFeature EastWall;
        public TileFeature SouthWall;
        public TileFeature WestWall;
        public string SpecialEventId; // For "A voice yells: 'TURN BACK!'"
    }

    /// <summary>
    /// Stores the layout for a single dungeon level, intended for DataWarehouse persistence.
    /// </summary>
    public class DungeonMapContext
    {
        public string MapId { get; set; } = "MangarTower_Lvl1";
        public int Width { get; set; } = 20;
        public int Height { get; set; } = 20;

        // Using a Dictionary for sparse maps or a flat array for performance
        public Dictionary<Vector2Int, DungeonCell> Cells { get; set; } = new Dictionary<Vector2Int, DungeonCell>();

        public TileFeature GetWall(Vector2Int pos, Vector2Int direction)
        {
            if (!Cells.TryGetValue(pos, out var cell)) return TileFeature.Wall; // Edge of the world

            if (direction == Vector2Int.up) return cell.NorthWall;
            if (direction == Vector2Int.right) return cell.EastWall;
            if (direction == Vector2Int.down) return cell.SouthWall;
            if (direction == Vector2Int.left) return cell.WestWall;

            return TileFeature.Wall;
        }
    }
}