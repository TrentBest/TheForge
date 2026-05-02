using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    /// <summary>
    /// Manages the Multiverse constants. 
    /// Arbitrates TL (Tech Level) and physical constants for the active simulation.
    /// </summary>
    public class UniverseAPI : IGurpsApiProvider, IFilterWindowProvider, IGuiProvider
    {
        public Guid Id { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000001010");
        public ProviderType ProviderType => ProviderType.GurpsApi;
        public string ModuleName => "Cosmology & Universes";

        private List<GURPSUniverse> _universes = new List<GURPSUniverse>();
        private int _activeSeed = 42;
        private readonly DataWarehouse _warehouse;
        private const string CacheKey = "GURPS_MULTIVERSE_STATE";

        public Vector2 position { get; set; }
        public string Title => "DURPS Multiverse Anchor";

        public UniverseAPI(DataWarehouse warehouse)
        {
            _warehouse = warehouse;
        }

        public UniverseAPI()
        {
            _warehouse = new DataWarehouse();
        }

        public void Initialize() => LoadFromCache();

        public List<GURPSUniverse> GetAll() => _universes.OrderBy(u => u.UniverseSeed).ToList();

        /// <summary>
        /// Returns the focused universe for the current simulation.
        /// [FIXED]: Resolved missing definition error.
        /// </summary>
        public GURPSUniverse GetActiveUniverse()
        {
            return _universes.FirstOrDefault(u => u.UniverseSeed == _activeSeed)
                ?? _universes.FirstOrDefault()
                ?? new GURPSUniverse();
        }

        public int GetTechLevel(int universeSeed, string category = "General")
        {
            var uni = _universes.FirstOrDefault(u => u.UniverseSeed == universeSeed);
            if (uni == null) return 3;

            return category.ToLower() switch
            {
                "medical" => uni.SpecificTechLevels.Medical,
                "transport" => uni.SpecificTechLevels.Transportation,
                "armory" => uni.SpecificTechLevels.Armory,
                "power" => uni.SpecificTechLevels.Power,
                _ => uni.TechLevel
            };
        }

        public void Register(GURPSUniverse uni)
        {
            _universes.RemoveAll(u => u.UniverseSeed == uni.UniverseSeed);
            _universes.Add(uni);
            SaveToCache();
        }

        public void SaveToCache()
        {
            if (_warehouse != null)
            {
                _warehouse.StoreTemporary(CacheKey, JsonUtility.ToJson(new CacheWrapper { Universes = _universes, ActiveSeed = _activeSeed }));
            }
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                var cache = JsonUtility.FromJson<CacheWrapper>(json);
                _universes = cache?.Universes ?? new List<GURPSUniverse>();
                _activeSeed = cache?.ActiveSeed ?? 42;
            }

            if (_universes.Count == 0) SeedStandardUniverses();
        }

        private void SeedStandardUniverses()
        {
            _universes.Clear();
            Register(new GURPSUniverse
            {
                Name = "Earth (Prime)",
                UniverseSeed = 42,
                TechLevel = 8,
                Description = "AEC Real-World Anchor for industrial extraction.",
                SpecificTechLevels = new GURPSUniverse.TechSplit { Medical = 8, Power = 9, Armory = 7, Transportation = 8 }
            });

            Register(new GURPSUniverse
            {
                Name = "Caldos Sector",
                UniverseSeed = 101,
                TechLevel = 10,
                Description = "High-tech star sector with strict physics.",
                SpecificTechLevels = new GURPSUniverse.TechSplit { Medical = 10, Power = 10, Armory = 9, Transportation = 11 }
            });

            SaveToCache();
        }

        public void CreateComponentTree(List<ForgeFilterWindow.Element> tree)
        {
            tree.Add(new ForgeFilterWindow.GroupElement(0, "Available Universes"));
            foreach (var uni in _universes)
            {
                tree.Add(new ForgeFilterWindow.Element(1, uni.Name) { userData = uni });
            }
        }

        public bool GoToChild(ForgeFilterWindow.Element element, bool addIfComponent)
        {
            if (element.userData is GURPSUniverse uni)
            {
                _activeSeed = uni.UniverseSeed;
                Debug.Log($"[DURPS] Multiverse Focus Shifted to: {uni.Name}");
                return true;
            }
            return false;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        public VisualElement CreateGui(GuiContext ctx)
        {
            var gui = new GraphicalUserInterfaceBuilder(ModuleName)
                .WithTitle(Title)
                .WithPadding(10);

            foreach (var uni in _universes)
            {
                gui.WithPanel($"Universe_{uni.UniverseSeed}")
                    .AddHeader($"{uni.Name} (TL {uni.TechLevel})", Color.yellow)
                    .EndPanel();
            }

            return gui.Build();
        }

        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
        public void Bind(DataWarehouse warehouse)
        {
            LoadFromCache();
            Debug.Log($"[DURPS] Universe Engine bound to Warehouse: {warehouse != null}");
        }

        [Serializable]
        private class CacheWrapper
        {
            public List<GURPSUniverse> Universes = new List<GURPSUniverse>();
            public int ActiveSeed;
        }
    }
}