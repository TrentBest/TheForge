using System;
using System.Collections.Generic;

namespace Assets.Scripts.MastersOfOrionII
{
    // ----------------------------------------------------------------------
    // 4. RESEARCH & TECH
    // ----------------------------------------------------------------------
    [Serializable]
    public class ResearchState
    {
        // Unlocked Tech IDs
        public HashSet<string> UnlockedTechs = new HashSet<string>();

        // Current Focus
        public string CurrentProjectID;
        public float CurrentProgress;
        public float ProjectCost;

        // Spending Allocation
        public float ResearchBudgetPct; // % of Empire income to Science
    }
}
