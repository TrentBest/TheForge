using System;
using System.Collections.Generic;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    [Serializable]
    public class GURPSUniverse
    {
        public string Name = "New Universe";
        public string Description = "";

        // The unique seed (e.g., 42 for Real World)
        public int UniverseSeed = 0;

        // Ontology Layer 1: Real-World vs Simulated
        public bool IsRealWorld => UniverseSeed == 42;

        // Settings that ripple down to the Campaign and Item editors
        public int DefaultTechLevel = 3;
        public string PhysicsProfile = "Standard"; // Mana level, Gravity, etc.

        // Content filtering: What books/items are natively available here
        public List<string> IncludedRuleBooks = new List<string>();
    }
}