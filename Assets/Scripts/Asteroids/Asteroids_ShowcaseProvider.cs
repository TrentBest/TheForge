using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Showcase;
using Workshop.Systems.MicroPackages;

namespace Workshop.Asteroids
{
    public partial class Asteroids_ShowcaseProvider : IShowcasePortal, IArchivalMetadata
    {
        public string Title => "Asteroids: Vector Reconstruction";

        // --- IShowcasePortal Hooks ---
        public bool IsDiscoverable => true;
        public string Category => "PART V: ALGORITHMIC ARCHAEOLOGY";
        public string Overview => "A functional reconstruction of the 1979 vector-physics classic, optimized for unmanaged memory.";
        public Color AccentColor => Color.white;
        public List<string> CoreTechnologies => new List<string> { "Vector Projection", "Deterministic Wraparound", "Entity FSM Composition" };
        public string TelemetryState => "CALIBRATING VECTOR BUFFERS";
        public bool IsUnderConstruction => false;

        // ====================================================================
        // 🏛️ IArchivalMetadata Implementation (The Queryable Ontology)
        // ====================================================================

        // --- MODERN ROUTING & STOREFRONT ---
        public string PackageId => "Workshop.Showcase.Asteroids";
        public string Version => "1.0.0";
        public string Author => "The Singularity Workshop";

        // Pure physics and coordinate-wrapped logic belongs in The Cortex
        public ForgeSpatialZone SpatialZone => ForgeSpatialZone.LogicAndCompute;

        public string ShortDescription => Overview;
        public string LongDescription => HistoricalSignificance;
        public string ThumbnailUrl => "Textures/Thumbnails/Archive_Asteroids_Vector";

        // --- HISTORICAL PRESERVATION ---
        public int ReleaseYear => 1979;
        public string OriginalAuthor => "Lyle Rains, Ed Logg";
        public string OriginalPublisher => "Atari Inc.";
        public string OriginalPlatform => "Vector Display Terminal";
        public string Era => "The Vector Renaissance";
        public string HistoricalSignificance => "Asteroids revolutionized high-precision vector graphics and established the bedrock for modern simulation logic.";

        private IGuiRouter _globalForgeRouter;
        private AsteroidsContext _sharedContext;

        public Asteroids_ShowcaseProvider() { }

        public void InjectRouter(IGuiRouter router) => _globalForgeRouter = router;

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. Establish the Single Source of Truth for Asteroids
            _sharedContext = UnityEngine.Object.FindObjectOfType<AsteroidsContext>();
            if (_sharedContext == null)
            {
                var go = new GameObject("[ASTEROIDS_CONTEXT]");
                _sharedContext = go.AddComponent<AsteroidsContext>();
            }

            // 2. Initialize the Local Sandbox Router
            var localRouter = new GuiFlowRouterBuilder("AsteroidsAppRouter", "ArchiveIntro");
            var internalBridge = new AsteroidsInternalRouter(localRouter, _globalForgeRouter);

            // 3. Register Internal Routes
            // Using your standard historical plaque UI for the intro
            localRouter.AddRoute("ArchiveIntro", r => new Archive_Gui_HistoricalPlaque(internalBridge, this));

            // Using the generalized Forge_Hangar as a fluent provider
            localRouter.AddRoute("Hangar", r =>
                new Forge_Hangar("FIGHTER REQUISITION")
                    .WithRegistry(GetShipIdentityList())
                    .OnConfirm(item => internalBridge.NavigateTo("CombatSim"))
                    .OnBack(() => internalBridge.NavigateTo("ArchiveIntro")));

            // Match constructor to your current simulation view signature
            localRouter.AddRoute("CombatSim", r => new Asteroids_Gui_InGame(internalBridge));

            localRouter.Build();
            return localRouter.CreateGui(ctx);
        }

        private List<IAvatarIdentity> GetShipIdentityList()
        {
            // Maps your Asteroids fighters to the generalized Hangar Registry
            var list = new List<IAvatarIdentity>();
            // Logic to fetch fighter definitions from _sharedContext or resources goes here
            return list;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }

    public class Forge_Hangar : IGuiProvider
    {
        public Forge_Hangar(string title) { }
        public Forge_Hangar WithRegistry(List<IAvatarIdentity> list) => this;
        public Forge_Hangar OnConfirm(Action<IAvatarIdentity> action) => this;
        public Forge_Hangar OnBack(Action action) => this;
        public string Title => "Hangar";
        public VisualElement CreateGui(GuiContext ctx) => new VisualElement();
        public Action<VisualElement> GetGuiBuilder() => r => { };
        public void ToUIDocument(string p) { }
        public void FromUIDocument(string p) { }
    }
}