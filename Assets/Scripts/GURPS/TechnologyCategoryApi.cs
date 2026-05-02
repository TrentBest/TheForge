using Assets.Scripts.GURPS;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.GURPS
{
    /// <summary>
    /// The Taxonomy Registry for technological disciplines.
    /// Manages the categorization of tech (e.g., Medical, Weaponry, Transportation).
    /// Reforged to follow the Forge Protocol and resolve the GetAll() deficit.
    /// </summary>
    public class TechnologyCategoryApi : IGuiProvider
    {
        public string Title => "CATEGORY ARCHITECT";

        private List<TechnologyCategory> _categories = new List<TechnologyCategory>();
        private readonly DataWarehouse _warehouse;
        private const string CacheKey = "GURPS_TECH_CATEGORIES_DB";

        // UI Binding
        private VisualElement _detailPanel;

        public TechnologyCategoryApi() { _warehouse = new DataWarehouse(); LoadFromCache(); }

        public TechnologyCategoryApi(DataWarehouse warehouse)
        {
            _warehouse = warehouse;
            LoadFromCache();
        }

        // --- CORE REGISTRY LOGIC ---

        /// <summary>
        /// Retrieves all registered categories for systemic synchronization.
        /// Resolves the compiler deficit in TechnologyLevel.cs.
        /// </summary>
        public List<TechnologyCategory> GetAll()
        {
            return _categories.OrderBy(c => c.Name).ToList();
        }

        public void Register(TechnologyCategory category)
        {
            if (category == null) return;
            _categories.RemoveAll(c => c.Name == category.Name);
            _categories.Add(category);

            // Trigger a global sync across all Tech Levels
            SyncGlobalTechLevels();
            SaveToCache();
        }

        private void SyncGlobalTechLevels()
        {
            // Failsafe: Ensures all TLs in the master system have these categories mapped
            if (DigitalGenericUniversalRolePlayingSystem.TechLevels == null) return;

            var allLevels = DigitalGenericUniversalRolePlayingSystem.TechLevels.GetAll();
            foreach (var level in allLevels)
            {
                level.EnsureStandardCategories();
            }
        }

        // --- FORGE GUI MANIFESTATION ---

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("Category_Root")
                .WithDirection(FlexDirection.Row)
                .WithFlexGrow(1f)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.04f));

            // LEFT: Category List (30%)
            var listPane = new ForgeContainerBuilder("Nav")
                .WithWidth(new StyleLength(Length.Percent(30f)))
                .WithPadding(10f)
                .WithBorderWidth(0, 1f, 0, 0)
                .WithBorderColor(new Color(0.3f, 0.5f, 0.6f, 0.4f)) // Taxonomy Cyan
                .AddChild(new ForgeLabelBuilder("CATEGORIES").WithBold().WithColor(Color.cyan).WithMarginBottom(10f));

            listPane.AddChild(new DynamicGuiProvider(c => {
                var scroll = new ScrollView();
                foreach (var cat in _categories.OrderBy(x => x.Name))
                {
                    var captured = cat;
                    scroll.Add(new ForgeButtonBuilder(cat.Name)
                        .WithMarginBottom(2f)
                        .WithBackgroundColor(new Color(0.1f, 0.12f, 0.15f))
                        .OnClick(() => ManifestInspector(captured))
                        .Build());
                }
                return scroll;
            }));

            // RIGHT: Inspector (70%)
            var inspectorPane = new ForgeContainerBuilder("Inspector")
                .WithFlexGrow(1f)
                .WithPadding(20f)
                .OnBuild(ve => _detailPanel = ve);

            rootBuilder.AddChild(listPane).AddChild(inspectorPane);
            if (_categories.Count > 0) ManifestInspector(_categories[0]);

            return rootBuilder.Build();
        }

        private void ManifestInspector(TechnologyCategory category)
        {
            if (_detailPanel == null) return;
            _detailPanel.Clear();

            var content = new ForgeContainerBuilder("DetailContent")
                .AddChild(new ForgeLabelBuilder(category.Name.ToUpper()).WithFontSize(24).WithBold().WithColor(Color.cyan))
                .AddChild(new ForgeLabelBuilder(category.SourceBookName).WithColor(Color.gray).WithMarginBottom(15f))
                .AddChild(new ForgeLabelBuilder(category.Description).WithFontSize(14).WithMarginBottom(20f));

            // Systemic Impact Note
            content.AddChild(new ForgeContainerBuilder("ImpactBox")
                .WithPadding(15f).WithBackgroundColor(new Color(0.1f, 0.15f, 0.1f)).WithBorderRadius(5f)
                .AddChild(new ForgeLabelBuilder("SYSTEMIC ROLE").WithFontSize(10).WithColor(Color.gray).WithMarginBottom(5f))
                .AddChild(new ForgeLabelBuilder($"This category defines independent TL offsets for different cultures.")
                    .WithFontSize(11).WithFontStyle(FontStyle.Italic)));

            _detailPanel.Add(content.Build());
        }

        // --- DATA PERSISTENCE ---

        public void SaveToCache()
        {
            if (_warehouse == null) return;
            string json = JsonUtility.ToJson(new { Items = _categories }, true);
            _warehouse.StoreTemporary(CacheKey, json);
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                // Hydration logic via DataWarehouse unmanaged memory shelf
            }
            if (_categories.Count == 0) SeedStandardCategories();
        }

        private void SeedStandardCategories()
        {
            _categories = new List<TechnologyCategory>
            {
                new TechnologyCategory("Transportation", "Vehicles, FTL, and local transit."),
                new TechnologyCategory("Weaponry", "Personal and tactical hardware."),
                new TechnologyCategory("Medical", "Cybernetics, biotech, and pharmaceuticals.")
            };
            SaveToCache();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "TechCategory_Registry");
        public void FromUIDocument(string path) { }

        internal void Initialize()
        {
            throw new NotImplementedException();
        }

        internal void UnRegister(TechnologyCategory cat)
        {
            throw new NotImplementedException();
        }
    }
}