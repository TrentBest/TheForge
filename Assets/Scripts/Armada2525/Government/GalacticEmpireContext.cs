using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.Armada2525.Economy;

namespace TheSingularityWorkshop.Armada2525.Government
{


    // --- MEDIA & PROPAGANDA ---
    public class MediaNetwork : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "Galactic News Network";
        public string Alignment { get; set; } = "Pro-Empress"; // Faction or Ideology they support
        public double Reach { get; set; } = 1000000; // Viewership
        public double OppositionFunding { get; set; } = 0; // Money the player has pumped in to skew them
    }

    // --- THE MASTER ROOT: THE EMPIRE ---
    public class GalacticEmpireContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string EmpressName { get; set; } = "Empress Valeria I";
        public double GlobalApprovalRating { get; set; } = 0.85f;

        public List<PoliticalFaction> MajorFactions { get; set; } = new List<PoliticalFaction>();

        // The Pillars
        public GalacticStockExchange Exchange { get; set; } = new GalacticStockExchange();
        public List<MediaNetwork> ActiveMedia { get; set; } = new List<MediaNetwork>();
        public List<MacroInfrastructure> ImperialLogistics { get; set; } = new List<MacroInfrastructure>();

        // Player's specific wallet inside this massive universe
        public double PlayerTreasury { get; set; } = 5000000;
        public string Name { get; set; } = "Emperium";
    }
}