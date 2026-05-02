using System;
using System.Collections.Generic;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.GURPS
{
    /// <summary>
    /// Represents a GURPS Rulebook or Supplement.
    /// Acts as a manifest for which traits, skills, and tech are available in a campaign.
    /// This class serves as the IStateContext for the "BookLifecycle" FSM.
    /// </summary>
    [Serializable]
    public class GURPSBook : IRuleBook, IStateContext
    {
        // --- 1. Serialized Backing Fields (Persistence) ---
        [SerializeField] private string _name = "New Book";
        [SerializeField] private string _description = "";
        [SerializeField] private int _specificityTier = 1;
        [SerializeField] private int _loadOrder = 0;
        [SerializeField] private List<string> _targetUniverseSeeds = new List<string>();
        [SerializeField] private bool _definesNewUniverse = false;

        // --- 2. IRuleBook Implementation ---
        public string Name { get => _name; set => _name = value; }
        public string Description { get => _description; set => _description = value; }

        /// <summary>
        /// Tier 1: Core (Basic Set), Tier 2: Genre (Magic/High-Tech), 
        /// Tier 3: World (Infinite Worlds), Tier 4: Adventure.
        /// </summary>
        public int SpecificityTier { get => _specificityTier; set => _specificityTier = value; }
        public int LoadOrder { get => _loadOrder; set => _loadOrder = value; }

        public List<string> TargetUniverseSeeds => _targetUniverseSeeds;
        public bool DefinesNewUniverse { get => _definesNewUniverse; set => _definesNewUniverse = value; }

        // --- 3. IStateContext Implementation ---
        /// <summary>
        /// When false, the FSM(s) operating on this context will be ignored (no-ops).
        /// Set to true by the BooksAPI once indexing and validation are complete.
        /// </summary>
        public bool IsValid { get; set; } = false;

        // --- 4. Content & Metadata ---
        public string Category = "Uncategorized";
        public bool IsCoreBook => SpecificityTier == 1;

        public int PageCount { get; internal set; }

        // Content Manifests: Lists of IDs provided by this book to the engine.
        public List<string> IncludedTraitIds = new List<string>();
        public List<string> IncludedSkillIds = new List<string>();
        public List<string> IncludedWeaponIds = new List<string>();

        public string CoverImagePath = "";

        /// <summary>
        /// High-level content providers exposed by this book (e.g., Magic, Psionics, Tech).
        /// Used for systemic cross-referencing without requiring books to be GurpsApiProviders.
        /// </summary>
        public Dictionary<string, object> ContentProviders = new Dictionary<string, object>();

        public Texture2D GetCoverImage()
        {
            if (!string.IsNullOrEmpty(CoverImagePath))
                return Resources.Load<Texture2D>(CoverImagePath);

            return Resources.Load<Texture2D>($"Covers/{Name}");
        }
    }
}