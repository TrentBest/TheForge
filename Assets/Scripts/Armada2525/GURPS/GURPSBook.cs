using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    [Serializable]
    public class GURPSBook : IRuleBook
    {
        // 1. SERIALIZED BACKING FIELDS (This is what JsonUtility saves to disk)
        [SerializeField] private string _name = "New Book";
        [SerializeField] private string _description = "";
        [SerializeField] private int _specificityTier = 1;
        [SerializeField] private int _loadOrder = 0;

        // 2. INTERFACE PROPERTIES (This satisfies IRuleBook and routes to the fields)
        public string Name { get => _name; set => _name = value; }
        public string Description { get => _description; set => _description = value; }
        public int SpecificityTier { get => _specificityTier; set => _specificityTier = value; }
        public int LoadOrder { get => _loadOrder; set => _loadOrder = value; }

        // 3. STANDARD FIELDS (JsonUtility saves these automatically)
        public string Category = "Uncategorized";
        public int PageCount = 0;
        public string CoverImagePath = "";

        
        public List<string> TargetUniverseSeeds { get; } = new List<string>();

        public bool DefinesNewUniverse { get; set; } = false;

        // Inside GURPSBook.cs
        public Texture2D GetCoverImage()
        {
            // If the user has defined a specific path in the Book Editor, use it
            if (!string.IsNullOrEmpty(CoverImagePath))
            {
                return UnityEngine.Resources.Load<Texture2D>(CoverImagePath);
            }

            // Fallback: Try guessing by name if path is empty
            if (!string.IsNullOrEmpty(Name))
            {
                return UnityEngine.Resources.Load<Texture2D>($"Covers/{Name}");
            }

            return null;
        }
    }
}