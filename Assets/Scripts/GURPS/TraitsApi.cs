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
    /// The Traits Library for DURPS. 
    /// Manages the database of Advantages and Disadvantages.
    /// Integrated with DataWarehouse for persistent streaming.
    /// </summary>
    public class TraitsApi : IGurpsApiProvider, IFilterWindowProvider
    {
        public int Id => 1002;
        public ProviderType ProviderType => ProviderType.GurpsApi;
        public string ModuleName => "Advantages & Disadvantages";

        private List<GURPSTrait> _traits = new List<GURPSTrait>();
        private readonly DataWarehouse _warehouse;
        private const string CacheKey = "GURPS_TRAITS_DATABASE";

        // --- IGurpsApiProvider Implementation ---
        public Vector2 position { get; set; }
        private Guid _guid = Guid.NewGuid();
        Guid IGurpsApiProvider.Id { get => _guid; set => _guid = value; }

        public string Title => ModuleName;
        public TraitsApi() { _warehouse = new DataWarehouse(); }
        public TraitsApi(DataWarehouse warehouse)
        {
            _warehouse = warehouse;
        }

        public void Initialize() => LoadFromCache();

        // --- CORE API LOGIC ---

        public List<GURPSTrait> GetAll() => _traits;

        public GURPSTrait GetByName(string name) =>
            _traits.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public void Register(GURPSTrait trait)
        {
            _traits.RemoveAll(t => t.Name == trait.Name && t.SourceBookName == trait.SourceBookName);
            _traits.Add(trait);
            SaveToCache();
        }

        public void Remove(string traitId)
        {
            _traits.RemoveAll(t => t.Id == traitId);
            SaveToCache();
        }

        // --- PERSISTENCE: DATA WAREHOUSE INTEGRATION ---

        public void SaveToCache()
        {
            if (_warehouse == null) return;
            try
            {
                // We use a specialized wrapper to handle polymorphism in JsonUtility
                string json = JsonUtility.ToJson(new CacheWrapper { Traits = SerializeTraits() }, true);
                _warehouse.StoreTemporary(CacheKey, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DURPS] Traits Save Error: {e.Message}");
            }
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                try
                {
                    var cache = JsonUtility.FromJson<CacheWrapper>(json);
                    if (cache != null && cache.Traits != null)
                    {
                        _traits = DeserializeTraits(cache.Traits);
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DURPS] Traits Hydration Error: {e.Message}");
                }
            }

            SeedStandardAdvantages();
        }

        private void SeedStandardAdvantages()
        {
            _traits.Clear();

            // Advantages
            Register(new GURPSAdvantage { Name = "Combat Reflexes", BaseCost = 15, Category = AdvantageCategory.Mental, Description = "+1 to Active Defenses, +2 to Fright Checks." });
            Register(new GURPSAdvantage { Name = "High Pain Threshold", BaseCost = 10, Category = AdvantageCategory.Physical, Description = "Ignore shock penalties." });
            Register(new GURPSAdvantage { Name = "Luck", BaseCost = 15, Category = AdvantageCategory.Mental, Description = "Reroll one feat every hour." });

            // Disadvantages
            Register(new GURPSDisadvantage { Name = "Blindness", BaseCost = -50, Category = AdvantageCategory.Physical, Description = "Total loss of vision." });
            Register(new GURPSDisadvantage { Name = "Honesty", BaseCost = -10, Category = AdvantageCategory.Mental, Description = "Must obey laws; never lie." });

            SaveToCache();
        }

        // --- POLYMORPHIC JSON HELPERS ---
        // JsonUtility doesn't handle abstract lists well, so we wrap them for storage
        private List<TraitContainer> SerializeTraits() =>
            _traits.Select(t => new TraitContainer { TypeName = t.GetType().AssemblyQualifiedName, JsonData = JsonUtility.ToJson(t) }).ToList();

        private List<GURPSTrait> DeserializeTraits(List<TraitContainer> containers)
        {
            var list = new List<GURPSTrait>();
            foreach (var c in containers)
            {
                var type = Type.GetType(c.TypeName);
                if (type != null) list.Add((GURPSTrait)JsonUtility.FromJson(c.JsonData, type));
            }
            return list;
        }

        // --- IFilterWindowProvider Implementation ---

        public void CreateComponentTree(List<ForgeFilterWindow.Element> tree)
        {
            tree.Add(new ForgeFilterWindow.GroupElement(0, "Traits Library"));

            // Level 1: Split by Advantage vs Disadvantage
            var typeGroups = _traits.GroupBy(t => t is GURPSAdvantage ? "Advantages" : "Disadvantages");

            foreach (var typeGroup in typeGroups)
            {
                tree.Add(new ForgeFilterWindow.GroupElement(1, typeGroup.Key));

                // Level 2: Sub-group by Category (Physical, Mental, etc.)
                var categoryGroups = typeGroup.GroupBy(t => t.Category);
                foreach (var catGroup in categoryGroups)
                {
                    tree.Add(new ForgeFilterWindow.GroupElement(2, catGroup.Key.ToString()));

                    foreach (var trait in catGroup)
                    {
                        // Level 3: The actual selectable item
                        tree.Add(new ForgeFilterWindow.Element(3, trait.Name) { userData = trait });
                    }
                }
            }
        }

        public bool GoToChild(ForgeFilterWindow.Element element, bool addIfComponent)
        {
            if (element.level == 3 && element.userData is GURPSTrait trait)
            {
                Debug.Log($"[DURPS] Trait Context Focused: {trait.Name} ({trait.BaseCost} pts)");
                return true;
            }
            return false;
        }

        // --- THE FIX: Wrap Editor-Only Code ---
#if UNITY_EDITOR
        public void CreateComponentTree(List<UnityEditor.Rendering.FilterWindow.Element> tree)
        {
            tree.Add(new UnityEditor.Rendering.FilterWindow.GroupElement(0, "Traits Library (Legacy)"));
        }

        public bool GoToChild(UnityEditor.Rendering.FilterWindow.Element element, bool addIfComponent) => false;
#endif

        public Action<VisualElement> GetGuiBuilder()
        {
            return (root) => root.Add(CreateGui(new GuiContext()));
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var gui = new GraphicalUserInterfaceBuilder(ModuleName)
                .WithTitle(Title)
                .WithPadding(10)
                .WithAutoGrow()
                .WithScrollable(true);

            // Action bar for library-wide operations
            gui.WithPanel("LibraryControls")
                .WithFlexDirection(FlexDirection.Row)
                .AddButton("Refresh Cache", LoadFromCache)
                .AddButton("Seed Defaults", () => { SeedStandardAdvantages(); })
                .EndPanel();

            gui.AddSeparator(Color.gray, 1);

            // Categorized List
            var categories = _traits.GroupBy(t => t.Category);
            foreach (var category in categories)
            {
                gui.AddHeader($"{category.Key} Traits", Color.cyan);

                foreach (var trait in category)
                {
                    bool isAdvantage = trait is GURPSAdvantage;
                    Color typeColor = isAdvantage ? Color.green : Color.red;

                    gui.WithPanel($"Trait_{trait.Name}")
                        .WithMarginBottom(4)
                        .WithPadding(5)
                        .WithBorderWidth(1)
                        .WithBorderColor(new Color(0.3f, 0.3f, 0.3f, 1f))
                        .WithFlexDirection(FlexDirection.Row)
                        // LINT FIX: Replaced raw Labels with ForgeLabelBuilder
                        .AddChild(new ForgeLabelBuilder($"[{trait.BaseCost} pts]")
                            .WithWidth(60)
                            .WithColor(typeColor)
                            .WithBold())
                        .AddChild(new ForgeLabelBuilder(trait.Name)
                            .WithFlexGrow(1)
                            .WithBold())
                        .AddButton("Remove", () => Remove(trait.Id))
                        .EndPanel();
                }
            }

            return gui.Build();
        }

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(new GuiContext());
            GraphicalUserInterfaceBuilder.ConvertToUIDocument(root, assetPath);
        }
#endif

        public void FromUIDocument(string assetPath)
        {
            Debug.Log($"[DURPS] Library GUI Hydrated from {assetPath}");
        }

        public void Bind(DataWarehouse warehouse)
        {
            throw new NotImplementedException();
        }

        [Serializable]
        private class CacheWrapper { public List<TraitContainer> Traits; }

        [Serializable]
        private class TraitContainer { public string TypeName; public string JsonData; }
    }
}