using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.Ants
{
    // --- 1. DATA CONTEXTS & SETTINGS ---

    //public class SimSettings
    //{
    //    public int MaxPopulation = 500;
    //    public float AntBaseSpeed = 1.0f;
    //    public float WanderJitter = 15f;
    //    public int PheromoneDropAmount = 15;
    //    public float PheromoneDropChance = 0.6f;
    //    public int PheromoneDecayRate = 2;

    //    // NEW: How fast the ant's internal scent gland empties while walking
    //    public float ChargeDecayRate = 0.25f;
    //}

    public class MacroAntContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; }

        public Vector2 Position;
        public float Heading;
        public float Speed;
        public int PauseTicks;

        // NEW: The ant's internal reservoir of scent.
        public float PheromoneCharge;

        public SimAntWorld World;
        public bool HasFood = false;
        public bool HasWater = false;

        public MacroAntContext(SimAntWorld world, float x, float y)
        {
            World = world;
            Position = new Vector2(x, y);
            Heading = UnityEngine.Random.Range(0f, 360f);
            Speed = 1.0f;
            PauseTicks = 0;
            PheromoneCharge = 255f; // Start with a full scent gland
        }
    }
}