namespace Assets.Scripts.MastersOfOrionII
{
    /// <summary>
    /// Represents the entire container.
    /// </summary>
    public struct Galaxy
    {
        public int Seed; // The Master Seed
        public int RadiusInSectors; // Size of the map
        public int AgeInBillionsYears; // Affects star metallicity (resource abundance)
    }
}
