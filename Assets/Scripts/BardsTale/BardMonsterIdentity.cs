using Assets.Scripts.GURPS;
using UnityEngine;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.BardsTale
{
    public class BardMonsterIdentity : IAvatarIdentity, IArchivalMetadata
    {
        // --- IAvatarIdentity ---
        public string UniqueId { get; set; }
        public string DisplayName { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public GameObject PreviewPrefab { get; set; }
        public Color BrandColor => Color.red;

        // --- GURPS Data (The "Rolling" Engine) ---
        public int ST { get; set; }
        public int DX { get; set; }
        public int IQ { get; set; }
        public int HT { get; set; }

        // ====================================================================
        // 🏛️ IArchivalMetadata Implementation (The Queryable Ontology)
        // ====================================================================

        // --- MODERN ROUTING & STOREFRONT ---
        // By defaulting to these values, we ensure the UI can always render the card
        public string PackageId { get; set; } = "Workshop.Archive.BardsTale.Bestiary";
        public string Version { get; set; } = "1.0.0";
        public string Author { get; set; } = "The Singularity Workshop";

        // Monsters and Actors belong in the Foundry (Physical Assets)
        public ForgeSpatialZone SpatialZone { get; set; } = ForgeSpatialZone.PhysicalAssets;

        // We can dynamically map the modern descriptions to the legacy identity data
        public string ShortDescription => Subtitle;
        public string LongDescription => Description;
        public string ThumbnailUrl { get; set; } = "Textures/Thumbnails/Archive_BardsTale_Monster";

        // --- HISTORICAL PRESERVATION ---
        public int ReleaseYear => 1985;
        public string OriginalAuthor => "Michael Cranford";
        public string OriginalPublisher => "Interplay Productions";
        public string OriginalPlatform => "Apple II / Commodore 64";
        public string Era => "The Golden Age of CRPGs";
        public string HistoricalSignificance { get; set; } = "A foundational entity in early CRPG grid-based dungeon crawlers.";
        public Color AccentColor => new Color(0.8f, 0.1f, 0.1f); // Classic bloody red
    }

    /// <summary>
    /// The Static Foundry Registry. 
    /// These are the raw archival blueprints ready to be instantiated by the FSM/GURPS engine.
    /// </summary>
    public static class BardsTale_MonsterRegistry
    {
        public static BardMonsterIdentity Kobold = new BardMonsterIdentity
        {
            UniqueId = "kobold",
            DisplayName = "Kobold",
            Subtitle = "Small Dungeon Dweller",
            ST = 8,
            DX = 11,
            IQ = 7,
            HT = 10,
            Description = "A small, reptilian humanoid that thrives in the dark corners of Skara Brae.",
            HistoricalSignificance = "The 'Kobold' represents the standard low-level dungeon fodder that defined early RPG balance."
        };

        public static BardMonsterIdentity MadDog = new BardMonsterIdentity
        {
            UniqueId = "mad_dog",
            DisplayName = "Mad Dog",
            Subtitle = "Rabid Stray",
            ST = 9,
            DX = 12,
            IQ = 4,
            HT = 11,
            Description = "A feral, starving hound roaming the city streets, highly aggressive but easily dispatched.",
            HistoricalSignificance = "Often the very first enemy encountered by a new party outside the Adventurer's Guild."
        };

        public static BardMonsterIdentity Skeleton = new BardMonsterIdentity
        {
            UniqueId = "skeleton",
            DisplayName = "Skeleton",
            Subtitle = "Animated Bones",
            ST = 10,
            DX = 10,
            IQ = 0, // Mindless undead
            HT = 12, // Hard to kill with piercing weapons
            Description = "The magically animated remains of a fallen warrior, clattering forward with rusted weapons.",
            HistoricalSignificance = "Introduced players to the concept of weapon resistances, requiring blunt force rather than arrows."
        };
    }
}