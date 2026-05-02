using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.CorsairsInSpace.Data
{
    /// <summary>
    /// A generalized "Widget" representing a massive astronomical body (Asteroid, Moon, etc.).
    /// Replaces monolithic planet classes to ensure ECS-like memory safety.
    /// </summary>
    public class CosmicCellContext : IStateContext
    {
        public string Name { get; set; } = "Uncharted Cosmic Cell";
        public bool IsValid { get; set; } = true;

        // Macro coordinates in the raWWar / Armada Galaxy
        public int SectorId { get; set; }
        public long GalacticX { get; set; }
        public long GalacticY { get; set; }

        // Physical properties for the Architect
        public float MassKilotons { get; set; }
        public int DiameterMeters { get; set; }

        // Difficulty modifiers based on Sector Proximity (0 = Capital, 7 = Fringes)
        public float BaseHeatModifier => SectorId == 0 ? 2.5f : (1.0f - (SectorId * 0.1f));

        // The generalized data dictionary (Your "UltimateClass" concept)
        // Stores modular data like "MineralComposition", "AmbientRadiation", etc.
        public Dictionary<string, object> CellData { get; set; } = new Dictionary<string, object>();

        public CosmicCellContext(int sectorId, int diameterMeters)
        {
            SectorId = sectorId;
            DiameterMeters = diameterMeters;
            MassKilotons = (diameterMeters * diameterMeters) * 1.5f; // Rough heuristic

            // Seed initial data
            CellData.Add("MineralRichness", sectorId > 4 ? "High" : "Depleted");
            CellData.Add("AuthorityPresence", sectorId == 0 ? "Extreme" : "Minimal");
        }
    }
}