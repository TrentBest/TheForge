using System.Collections.Generic;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Armada2525.Corsair
{
    public class CorsairLairContext : IStateContext
    {
        public string Name { get; set; } = "CorsairHollow_Alpha";
        public bool IsValid { get; set; } = true;

        // --- BASE STATS ---
        public int AsteroidSizeLevel { get; set; } = 1; // 1 = Tiny, 3 = Massive
        public int TotalCredits { get; set; } = 5000;
        public int TotalPowerGenerated { get; set; } = 100;
        public int TotalPowerConsumed { get; set; } = 20;
        public float GlobalHeat { get; set; } = 0f; // 0 to 100. At 100, the Empire attacks.

        // --- THE MINION SWARM ---
        public int TotalMinions { get; set; } = 250;
        public int MaxMinionCapacity { get; set; } = 300; // Dictated by Barracks built

        // Link to the GPU Compute Shader data (Derived from your AntFarm)
        public ComputeBuffer MinionDataBuffer { get; set; }

        // --- THE GRID (Multi-level Voxel Data) ---
        // A simple representation: Level -> (X,Y) -> VoxelState (Solid, Mined, Room)
        public Dictionary<int, CorsairGridLevel> Levels { get; set; } = new Dictionary<int, CorsairGridLevel>();

        // --- MACRO STRATEGY ---
        public List<PirateOperation> ActiveOperations { get; set; } = new List<PirateOperation>();
    }

    public class CorsairGridLevel
    {
        public int ZLevel { get; set; } // 0 is surface, -1, -2 are deeper
        public int Width { get; set; }
        public int Height { get; set; }
        public int[] VoxelData { get; set; } // 0 = Solid Rock, 1 = Cleared, 2 = Built Room
    }

    public class PirateOperation
    {
        public string TargetTradeRouteId { get; set; }
        public int AssignedShips { get; set; }
        public float IncomePerTick { get; set; }
        public float HeatGeneratedPerTick { get; set; }
    }
}