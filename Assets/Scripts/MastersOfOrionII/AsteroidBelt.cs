namespace Assets.Scripts.MastersOfOrionII
{
    /// <summary>
    /// Sources of raw materials but no population.
    /// </summary>
    public struct AsteroidBelt
    {
        public int Id;
        public float DistanceFromStarAU;
        public float Density; // How hard is it to navigate ships through?
        public ResourceRichness MineralRichness;
        public bool HasRareIsotopes; // Special strategic resource?
    }
}
