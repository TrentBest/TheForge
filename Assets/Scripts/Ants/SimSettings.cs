namespace Assets.Scripts.Ants
{
    // --- 1. DATA CONTEXTS & SETTINGS ---

    public class SimSettings
    {
        public int MaxPopulation = 500;
        public float AntBaseSpeed = 1.0f;
        public float WanderJitter = 15f;
        public int PheromoneDropAmount = 15;
        public float PheromoneDropChance = 0.6f;
        public int PheromoneDecayRate = 2;
        public float ChargeDecayRate = 0.25f;
    }
}