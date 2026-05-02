using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    [Serializable]
    public class GURPSHeroArchetype
    {
        public string Name = "New Archetype";
        public int PointTotal = 100;
        public int BaseST = 10;
        public int BaseDX = 10;
        public int BaseIQ = 10;
        public int BaseHT = 10;
        public List<string> PrimarySkills = new List<string>();
        public List<string> StartingTraits = new List<string>();
    }

    /// <summary>
    /// The Hero Archetype API manages the "Class" templates for GURPS-driven experiences.
    /// Provides the data backend for Warriors, Rogues, and Wizards.
    /// </summary>
    public class HeroClassAPI : IGurpsApiProvider, IGuiProvider
    {
        public string ModuleName => "Hero Archetypes";
        public string Title => "ARCHETYPE FORGE";
        public Guid Id { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000001020");

        private List<GURPSHeroArchetype> _archetypes = new List<GURPSHeroArchetype>();
        private DataWarehouse _warehouse;
        private const string CacheKey = "GURPS_HERO_ARCHETYPES";

        // --- FORGE TIME CONSTRUCTOR ---
        public HeroClassAPI()
        {
            _warehouse = new DataWarehouse(); // Default to internal memory for Forge Chronos
            Initialize();
        }

        public HeroClassAPI(DataWarehouse warehouse)
        {
            _warehouse = warehouse;
            Initialize();
        }

        public void Initialize() => LoadFromCache();

        public void Bind(DataWarehouse warehouse)
        {
            _warehouse = warehouse;
            LoadFromCache();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var gui = new GraphicalUserInterfaceBuilder(ModuleName)
                .WithPadding(15)
                .AddHeader("HERO ARCHETYPES", Color.yellow);

            foreach (var archetype in _archetypes)
            {
                gui.WithPanel($"Archetype_{archetype.Name}")
                    .WithBorderWidth(1).WithBorderColor(new Color(0.4f, 0.4f, 0.2f))
                    .WithMarginBottom(5)
                    .AddChild(new ForgeLabelBuilder($"{archetype.Name} ({archetype.PointTotal} pts)").WithBold())
                    .AddChild(new ForgeLabelBuilder($"ST:{archetype.BaseST} DX:{archetype.BaseDX} IQ:{archetype.BaseIQ} HT:{archetype.BaseHT}").WithFontSize(10))
                    .EndPanel();
            }

            gui.AddButton("CREATE NEW ARCHETYPE", () => {
                _archetypes.Add(new GURPSHeroArchetype { Name = "Warrior" });
                SaveToCache();
            });

            return gui.Build();
        }

        public void SaveToCache()
        {
            if (_warehouse == null) return;
            _warehouse.StoreTemporary(CacheKey, JsonUtility.ToJson(new { Items = _archetypes }));
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                // Logic to hydrate archetypes from JSON
            }
            if (_archetypes.Count == 0) SeedDefaults();
        }

        private void SeedDefaults()
        {
            _archetypes.Add(new GURPSHeroArchetype { Name = "Sovereign Warrior", BaseST = 12, BaseHT = 12 });
            _archetypes.Add(new GURPSHeroArchetype { Name = "Clandestine Rogue", BaseDX = 13, BaseIQ = 11 });
            _archetypes.Add(new GURPSHeroArchetype { Name = "Techno-Wizard", BaseIQ = 14, BaseDX = 11 });
            SaveToCache();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string p) { }
        public void FromUIDocument(string p) { }
    }
}