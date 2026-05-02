using System;
using System.Collections.Generic;

namespace Assets.Scripts.MastersOfOrionII
{
    // ----------------------------------------------------------------------
    // 5. LEADERS & ADVISORS
    // ----------------------------------------------------------------------
    [Serializable]
    public class Leader
    {
        public int Id;
        public string Name;
        public LeaderType Type;      // Governor, Admiral, Scientist
        public int Level;            // 1-10
        public int AssignedTargetId; // ColonyID (if Governor) or FleetID (if Admiral)

        // Traits (e.g., "LogisticsExpert", "Charismatic")
        public List<string> Traits = new List<string>();
    }
}
