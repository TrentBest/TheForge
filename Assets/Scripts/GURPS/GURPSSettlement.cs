using System;
using UnityEngine;

namespace Workshop.GURPS
{
    [Serializable]
    public class GURPSSettlement
    {
        public string Name = "New Settlement";
        public int Population = 1000;
        public int TechLevel = 3;
        public int ControlRating = 3;
        public WealthTier EconomyBase = WealthTier.Average;

        // Social metrics for success roll modifiers
        public int AppearanceModifier = 0;
        public int HygieneModifier = 0;

        public GURPSSettlement() { }

        public GURPSSettlement(string name, int tl, int cr)
        {
            Name = name;
            TechLevel = tl;
            ControlRating = cr;
        }
    }
}