using Assets.Scripts.GURPS;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.BardsTale.UI;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Showcase;

namespace Workshop.BardsTale
{
    public partial class BardsTale_ShowcaseProvider : IShowcasePortal, IArchivalMetadata
    {
        // --- IGuiProvider & IShowcasePortal ---
        public string Title => "The Bard's Tale: Algorithmic Archaeology";
        public bool IsDiscoverable => true;
        public string Category => "PART VI: RPG ARCHIVES";
        public string Overview => "A reconstruction of the 1985 dungeon-crawling classic, focusing on grid-based navigation and state-driven mapping.";
        public Color AccentColor => new Color(0.6f, 0.1f, 0.1f); // Blood Red
        public List<string> CoreTechnologies => new List<string> { "Grid Navigation", "Automapping", "Party FSM" };
        public string TelemetryState => "RENDERING SKARA BRAE";
        public bool IsUnderConstruction => false;

        // ====================================================================
        // 🏛️ IArchivalMetadata Implementation (The Queryable Ontology)
        // ====================================================================

        // --- MODERN ROUTING & STOREFRONT ---
        public string PackageId => "Workshop.Showcase.BardsTale";
        public string Version => "1.0.0";
        public string Author => "The Singularity Workshop";

        // This package orchestrates complex RPG systems, so it belongs in The Armory
        public ForgeSpatialZone SpatialZone => ForgeSpatialZone.GameplaySystems;

        public string ShortDescription => Overview;
        public string LongDescription => HistoricalSignificance;
        public string ThumbnailUrl => "Textures/Thumbnails/Archive_BardsTale_SkaraBrae";

        // --- HISTORICAL PRESERVATION ---
        public int ReleaseYear => 1985;
        public string OriginalAuthor => "Michael Cranford";
        public string OriginalPublisher => "Interplay Productions";
        public string OriginalPlatform => "Apple II / Commodore 64";
        public string Era => "The Golden Age of CRPGs";
        public string HistoricalSignificance => "The Bard's Tale established the blueprint for first-person party RPGs, " +
            "introducing animated enemy portraits and the complex party-management FSM that defined the genre.";

        private IGuiRouter _globalForgeRouter;
        private BardsTaleExperienceContext _sharedContext;

        public BardsTale_ShowcaseProvider() { }

        public void InjectRouter(IGuiRouter router) => _globalForgeRouter = router;

        private List<IAvatarIdentity> GetClassIdentityList()
        {
            return new List<IAvatarIdentity>
            {
                new BardClassIdentity("Warrior", "Frontline Protector", "Master of blades and heavy plate armor.", Color.gray),
                new BardClassIdentity("Paladin", "Holy Crusader", "A divine warrior with high resistance to magic.", Color.yellow),
                new BardClassIdentity("Bard", "Keeper of Lore", "Uses mystical songs to buff the party and heal wounds.", new Color(0.8f, 0.4f, 0.1f))
            };
        }

        private class BardClassIdentity : IAvatarIdentity
        {
            public string UniqueId { get; }
            public string DisplayName { get; }
            public string Subtitle { get; }
            public string Description { get; }
            public GameObject PreviewPrefab { get; }
            public Color BrandColor { get; }

            public BardClassIdentity(string n, string s, string d, Color c)
            {
                UniqueId = n.ToLower(); DisplayName = n; Subtitle = s; Description = d; BrandColor = c;
            }
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _sharedContext = UnityEngine.Object.FindAnyObjectByType<BardsTaleExperienceContext>();
            if (_sharedContext == null)
            {
                var go = new GameObject("[BARDS_TALE_CONTEXT]");
                _sharedContext = go.AddComponent<BardsTaleExperienceContext>();

                // --- GURPS INITIALIZATION ---
                _sharedContext.ActiveParty = new GURPS_Party();
                // You can expand this to load saved party members from the DataWarehouse later

                _sharedContext.Initialize();
            }

            var localRouter = new GuiFlowRouterBuilder("BardsTaleAppRouter", "ArchiveIntro");
            var internalBridge = new BardsTaleInternalRouter(localRouter, _globalForgeRouter);

            // Register Internal Routes
            localRouter.AddRoute("ArchiveIntro", r => new BardsTale_Gui_ArchivePlaque(internalBridge, this));
            localRouter.AddRoute("Guild", r =>
                new Forge_Hangar("ADVENTURER'S GUILD")
                    .WithTheme(AccentColor, new Color(0.1f, 0.05f, 0.05f), Color.black)
                    .OnConfirm(item => internalBridge.NavigateTo("Exploration"))
                    .OnBack(() => internalBridge.NavigateTo("ArchiveIntro")));

            localRouter.AddRoute("Exploration", r => new BardsTale_ExplorationGuiProvider(internalBridge));

            localRouter.Build();
            return localRouter.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string p) { }
        public void FromUIDocument(string p) { }
    }

   
    
   
    
}