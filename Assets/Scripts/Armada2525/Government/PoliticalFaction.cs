using System.Collections.Generic;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Armada2525.Government
{

    // --- THE UPGRADED FACTION ---
    public class PoliticalFaction : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "New Faction";
        public string LeaderName { get; set; } = "Unknown Leader";

        // Flavor & Aesthetics
        public Color FactionColor { get; set; } = Color.white;
        public string InsigniaID { get; set; } = "default_logo";

        // The Deep Data
        public FactionIdeology CoreIdeology { get; set; }
        public FactionDoctrine PrimaryDoctrine { get; set; }

        public double PoliticalCapital { get; set; } = 0;
        public bool IsPlayerFaction { get; set; } = false;

        public PoliticalFaction ParentFaction { get; set; }

        public List<PoliticalFaction> GetSubFactions(List<PoliticalFaction> globalDatabase)
        {
            return globalDatabase.FindAll(f => f.ParentFaction == this);
        }
    }
}