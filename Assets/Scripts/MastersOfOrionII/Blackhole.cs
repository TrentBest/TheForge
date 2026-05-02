namespace Assets.Scripts.MastersOfOrionII
{
    /// <summary>
    /// Dangerous terrain or high-science locations.
    /// </summary>
    public struct Blackhole
    {
        public int Id;
        public float EventHorizonRadiusAU;
        public float AccretionDiskRadiation; // Damage per second to ships
        public bool LeadsToUnknown; // Maybe it's a wormhole in disguise?
    }
}
