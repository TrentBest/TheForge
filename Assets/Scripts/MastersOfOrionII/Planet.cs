namespace Assets.Scripts.MastersOfOrionII
{
    /// <summary>
    /// The Atomic Planet Data.
    /// Reforged as a blittable value type for high-performance simulation.
    /// Identity is handled via NameId lookup in the DataWarehouse.
    /// </summary>
    [System.Serializable]
    public struct Planet
    {
        public int Id;
        public uint Seed;
        public int NameId;         // The "Key" to the string in the registry
        public int SystemIndex;

        // Classification
        public PlanetType Type;

        // Orbital Physics
        public float SemiMajorAxisAU;
        public float OrbitalPeriodDays;

        // Surface Physics
        public float RadiusEarth;
        public float GravityG;
        public float SurfaceTempK;

        // Habitability
        public AtmosphereComposition Atmosphere;
        public float AtmosphereDensity;
        public float HydrospherePct;

        // Economy / 4X Stats
        public ResourceRichness MineralRichness;
        public float BioDiversity;
        public float EnergyPotential;

        public int MaxPopulationMillions;
    }
}