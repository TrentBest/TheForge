using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    /// <summary>
    /// Manages physical regions, biomes, and environmental hazards.
    /// Pure POCO implementation for Sovereign Rule Hosting.
    /// </summary>
    [Serializable]
    public class GURPSRegion
    {
        public string Name = "New Region";
        public string BiomeType = "Wilderness";
        public float HazardRating = 0.0f;
    }

    public class GeographyAPI : IGurpsApiProvider, IGuiProvider
    {
        public string ModuleName => "Global Geography & Hazards";
        public string Title => "MAP ARCHITECT";
        public Guid Id { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000001015");

        private List<GURPSRegion> _regions = new List<GURPSRegion>();
        private DataWarehouse _warehouse;
        private const string CacheKey = "GURPS_GEOGRAPHY_DB";

        // --- CONSTRUCTORS (Pillar 8: Context-First) ---

        public GeographyAPI()
        {
            _warehouse = new DataWarehouse();
            Initialize();
        }

        public GeographyAPI(DataWarehouse warehouse)
        {
            Bind(warehouse);
        }

        // --- IGURPSAPIPROVIDER IMPLEMENTATION ---

        public void Initialize()
        {
            LoadFromCache();
            if (_regions.Count == 0) SeedStandardRegions();
        }

        public void Bind(DataWarehouse warehouse)
        {
            _warehouse = warehouse;
            Initialize();
        }

        public void SaveToCache()
        {
            if (_warehouse == null) return;
            string json = JsonUtility.ToJson(new CacheWrapper { Regions = _regions });
            _warehouse.StoreTemporary(CacheKey, json);
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                var cache = JsonUtility.FromJson<CacheWrapper>(json);
                _regions = cache?.Regions ?? new List<GURPSRegion>();
            }
        }

        private void SeedStandardRegions()
        {
            _regions.Clear();
            _regions.Add(new GURPSRegion { Name = "Sovereign Wastes", BiomeType = "Radioactive Desert", HazardRating = 8.5f });
            _regions.Add(new GURPSRegion { Name = "Greenvale Shallows", BiomeType = "Swamp", HazardRating = 2.0f });
            SaveToCache();
        }

        // --- IGUI-PROVIDER IMPLEMENTATION (Pillar 1: Fluent Builders) ---

        public VisualElement CreateGui(GuiContext ctx)
        {
            var gui = new GraphicalUserInterfaceBuilder(ModuleName)
                .WithPadding(10)
                .AddChild(new ForgeLabelBuilder("REGION REGISTRY")
                    .WithBold()
                    .WithColor(Color.green)
                    .Build());

            foreach (var region in _regions)
            {
                gui.WithPanel(region.Name)
                    .WithMarginBottom(5)
                    .WithBorderWidth(1)
                    .WithBorderColor(new Color(0.2f, 0.4f, 0.2f))
                    .AddChild(new ForgeLabelBuilder(region.Name).WithBold())
                    .AddChild(new ForgeLabelBuilder($"Biome: {region.BiomeType}").WithFontSize(10))
                    .AddChild(new ForgeLabelBuilder($"Hazard: {region.HazardRating}").WithColor(Color.red))
                    .EndPanel();
            }

            return gui.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }

        [Serializable]
        private class CacheWrapper { public List<GURPSRegion> Regions = new List<GURPSRegion>(); }
    }
}