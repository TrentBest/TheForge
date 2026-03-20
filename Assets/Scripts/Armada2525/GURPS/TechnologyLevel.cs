using System;
using System.Collections.Generic;
using System.Linq;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    [Serializable]
    public class TechCategoryOffset
    {
        public string CategoryName;
        public int Offset;

        public TechCategoryOffset(string name, int offset)
        {
            CategoryName = name;
            Offset = offset;
        }
    }

    [Serializable]
    public class TechnologyLevel
    {
        public int Level;
        public int DivergentOffset = 0;
        public string Name = "New Tech Level";
        public string Description = "";
        public float StartingWealthMultiplier = 1.0f;
        public string SourceBookName = "GURPS Basic Set";

        public List<TechCategoryOffset> CategoryOffsets = new List<TechCategoryOffset>();

        public TechnologyLevel() { }

        public TechnologyLevel(int level, string name, float wealthMult = 1.0f, int divergentOffset = 0)
        {
            Level = level; Name = name; StartingWealthMultiplier = wealthMult; DivergentOffset = divergentOffset;
        }

        public string GetFormattedTL() => DivergentOffset > 0 ? $"TL {Level}+{DivergentOffset}" : $"TL {Level}";
        public override string ToString() => $"{GetFormattedTL()} - {Name}";

        // UPDATED: Dynamically fetches from the API instead of hardcoding strings
        public void EnsureStandardCategories()
        {
            // Failsafe in case this is called before DGURPS boot sequence finishes
            if (DigitalGenericUniversalRolePlayingSystem.TechCategories == null) return;

            var globalCategories = DigitalGenericUniversalRolePlayingSystem.TechCategories.GetAll();

            foreach (var cat in globalCategories)
            {
                if (!CategoryOffsets.Any(c => c.CategoryName == cat.Name))
                {
                    CategoryOffsets.Add(new TechCategoryOffset(cat.Name, 0));
                }
            }
        }
    }
}