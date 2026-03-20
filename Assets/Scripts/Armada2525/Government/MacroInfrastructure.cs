using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Armada2525.Government
{
    // --- LOGISTICS & INFRASTRUCTURE ---
    public class MacroInfrastructure : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "Core Sector Freight";
        public string SectorType { get; set; } = "Logistics"; // Freight, Mining, Construction
        public long ActiveVessels { get; set; } = 15000;
        public double DailyTonnage { get; set; } = 5000000;
    }
}