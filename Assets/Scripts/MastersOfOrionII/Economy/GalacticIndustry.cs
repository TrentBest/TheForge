using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.MastersOfOrionII.Economy
{
    // --- 1. THE INDUSTRY (The Sector of the Market) ---
    public class GalacticIndustry : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "New Sector";
        public string Description { get; set; } = "Defines a boundary of transferable workforce skills.";

        // Defines how wild the stock swings are. (e.g. Rare Elements = 2.5, Basic Food = 0.5)
        public float VolatilityIndex { get; set; } = 1.0f;

        // High capital intensity means fewer, but much richer, firms spawn here.
        public float CapitalIntensity { get; set; } = 1.0f;
        public string Category { get; set; } = "Uncategorized";
    }
}