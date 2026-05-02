using System;
using System.Collections.Generic;
using UnityEngine;

namespace Workshop.GURPS
{
    [Serializable]
    public class GURPSUniverse
    {
        public string Name = "New Universe";
        public string Description = "A newly initialized shard of the multiverse.";
        public int UniverseSeed = 0;

        // --- Tier 1: Real-World Alignment ---
        public bool IsRealWorld => UniverseSeed == 42;

        public string Physics { get; internal set; } = "Standard Newtonian";

        // --- Tier 2: Physical Constants ---
        public float GravityG = 1.0f;
        public ManaLevel Mana = ManaLevel.Normal;
        public ControlRating ControlRating = ControlRating.CR3_Moderate; // Default ControlRating

        // --- Tier 3: Technology & Progression ---
        public int TechLevel = 3; // Overall TL
        public TechSplit SpecificTechLevels = new TechSplit();
        public int StartingWealth = 1000;

        // --- Tier 4: Content Filtering ---
        public List<string> IncludedRuleBooks = new List<string>();
        public List<string> ActiveFactions = new List<string>();

        [Serializable]
        public class TechSplit
        {
            public int Medical = 3;
            public int Transportation = 3;
            public int Armory = 3;
            public int Power = 3;
        }
    }
}