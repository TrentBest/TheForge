using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding;
using Workshop.UI_And_Tools.Showcase;
using Workshop.Systems.MicroPackages;

namespace Workshop.Armada2525
{
    public class Armada2525_ShowcaseProvider : IShowcasePortal, IArchivalMetadata
    {
        // --- IGuiProvider & IShowcasePortal ---
        public string Title => "Armada 2525: Grand Strategy";
        public bool IsDiscoverable => true;
        public string Category => "PART IV: DIGITAL ARCHIVES & 4X MACRO-SIMULATION";
        public string Overview => "A functional reconstruction of early 4X grand strategy mechanics, featuring deterministic generation and simultaneous economy ticking.";
        public Color AccentColor => new Color(0.2f, 0.6f, 0.9f);
        public List<string> CoreTechnologies => new List<string> { "Digital Preservation", "Simultaneous Turn Resolution", "Deterministic Seeds" };
        public string TelemetryState => "ACCESSING ENCYCLOPEDIA GALACTICA";
        public bool IsUnderConstruction => false;

        // ====================================================================
        // 🏛️ IArchivalMetadata Implementation (The Queryable Ontology)
        // ====================================================================

        // --- MODERN ROUTING & STOREFRONT ---
        public string PackageId => "Workshop.Showcase.Armada2525";
        public string Version => "1.0.0";
        public string Author => "The Singularity Workshop";

        // Placed in The Armory because it orchestrates massive gameplay systems
        public ForgeSpatialZone SpatialZone => ForgeSpatialZone.GameplaySystems;

        public string ShortDescription => "A functional reconstruction of proto-4X grand strategy mechanics.";
        public string LongDescription => "Explores simultaneous turn resolution, deterministic stellar generation, and unmanaged memory pipelines to simulate macro-scale galactic empires without garbage collection overhead.";
        public string ThumbnailUrl => "Textures/Thumbnails/Archive_Armada2525";

        // --- HISTORICAL PRESERVATION ---
        public int ReleaseYear => 1991;
        public string OriginalAuthor => "Robert T. Smith";
        public string OriginalPublisher => "Interstel Corporation";
        public string OriginalPlatform => "MS-DOS";
        public string Era => "Proto-4X Grand Strategy";
        public string HistoricalSignificance =>
            "Before the term '4X' was coined, Armada 2525 defined the boundaries of grand strategy. " +
            "Released two years prior to Master of Orion, it utilized a highly sophisticated 'Simultaneous Turn Resolution' system, " +
            "where economic and military movements were plotted in secret and resolved mathematically. " +
            "This package preserves that algorithmic heritage by reforging it into a deterministic, unmanaged memory pipeline.";

        private IGuiRouter _globalForgeRouter;
        private ArmadaGalaxyContext _sharedGalaxyContext;

        public Armada2525_ShowcaseProvider() { }

        public void InjectRouter(IGuiRouter router) => _globalForgeRouter = router;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _sharedGalaxyContext = new ArmadaGalaxyContext { Name = "Sector_Alpha", UniversalSeed = 42 };

            var localRouter = new GuiFlowRouterBuilder("ArmadaAppRouter", "ArchiveIntro");
            var internalBridge = new ArmadaInternalRouter(localRouter, _globalForgeRouter);

            // Pass THIS provider (which is now IArchivalMetadata) into the UI
            localRouter.AddRoute("ArchiveIntro", r => new Armada_Gui_ArchiveIntro(internalBridge, this));
            localRouter.AddRoute("Bootloader", r => new Armada_Gui_Bootloader(internalBridge, _sharedGalaxyContext));
            localRouter.AddRoute("TacticalMap", r => new Armada2525_Gui_GridUniverse(_sharedGalaxyContext));

            localRouter.Build();
            return localRouter.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }

   }