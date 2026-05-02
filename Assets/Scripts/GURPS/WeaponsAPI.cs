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
    /// The Armory Database for DURPS.
    /// Manages GURPS weapon data and provides the Forge interface for equipment selection.
    /// </summary>
    public class WeaponsAPI : IGurpsApiProvider, IFilterWindowProvider, IGuiProvider
    {
        public int Id => 1007;
        public ProviderType ProviderType => ProviderType.GurpsApi;
        public string ModuleName => "Armory Database";

        private List<GURPSWeapon> _weapons = new List<GURPSWeapon>();
        private readonly DataWarehouse _warehouse;
        private const string CacheKey = "GURPS_WEAPONS_DATABASE";

        // --- IGurpsApiProvider Implementation ---
        public Vector2 position { get; set; }
        private Guid _guid = Guid.NewGuid();
        Guid IGurpsApiProvider.Id { get => _guid; set => _guid = value; }

        public string Title => "DURPS Armory Database";

        public WeaponsAPI(DataWarehouse warehouse)
        {
            _warehouse = warehouse;
        }

        public WeaponsAPI()
        {
        }

        public void Initialize() => LoadFromCache();

        // --- CORE API LOGIC ---

        public List<GURPSWeapon> GetAll() => _weapons.OrderBy(w => w.Name).ToList();

        public GURPSWeapon GetExact(string name, string sourceBook) =>
            _weapons.FirstOrDefault(w => w.Name == name && w.SourceBookName == sourceBook);

        public void Register(GURPSWeapon weapon)
        {
            _weapons.RemoveAll(w => w.Name == weapon.Name && w.SourceBookName == weapon.SourceBookName);
            _weapons.Add(weapon);
            SaveToCache();
        }

        public void UnRegister(GURPSWeapon weapon)
        {
            _weapons.RemoveAll(w => w.Name == weapon.Name && w.SourceBookName == weapon.SourceBookName);
            SaveToCache();
        }

        // --- PERSISTENCE: DATA WAREHOUSE INTEGRATION ---

        public void SaveToCache()
        {
            if (_warehouse == null) return;
            try
            {
                string json = JsonUtility.ToJson(new CacheWrapper { Weapons = _weapons }, true);
                _warehouse.StoreTemporary(CacheKey, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DURPS] Weapons Save Error: {e.Message}");
            }
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                try
                {
                    var cache = JsonUtility.FromJson<CacheWrapper>(json);
                    if (cache != null && cache.Weapons != null)
                    {
                        _weapons = cache.Weapons;
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DURPS] Weapons Hydration Error: {e.Message}");
                }
            }

            SeedStandardWeapons();
        }

        /// <summary>
        /// Populates the database with standard GURPS weapon entries across various Tech Levels.
        /// </summary>
        private void SeedStandardWeapons()
        {
            _weapons.Clear();

            // Low-Tech / Modern
            Register(new GURPSWeapon
            {
                Name = "Broadsword",
                Category = WeaponCategory.Melee,
                DamageString = "sw+1 cut",
                Type = DamageType.Cutting,
                Cost = 500,
                SourceBookName = "GURPS Basic Set",
                TechLevel = 3
            });

            Register(new GURPSWeapon
            {
                Name = "9mm Auto Pistol",
                Category = WeaponCategory.RangedKinetic,
                DamageString = "2d-1 pi",
                Type = DamageType.Piercing,
                Cost = 200,
                SourceBookName = "GURPS Basic Set",
                TechLevel = 7,
                Accuracy = "3",
                Range = "150/1900",
                RateOfFire = "3",
                Recoil = "2"
            });

            // Ultra-Tech
            Register(new GURPSWeapon
            {
                Name = "Laser Pistol",
                Category = WeaponCategory.RangedEnergy,
                DamageString = "3d(2) burn",
                Type = DamageType.Burning,
                ArmorDivisor = 2,
                TechLevel = 9,
                Cost = 250,
                SourceBookName = "GURPS Ultra-Tech",
                Accuracy = "6",
                Range = "300/900",
                RateOfFire = "10",
                Recoil = "1"
            });

            // Space (Ship Scale)
            Register(new GURPSWeapon
            {
                Name = "Anti-Matter Missile",
                Category = WeaponCategory.ShipScale,
                DamageString = "6dx100 ex",
                Type = DamageType.Crushing,
                Cost = 50000,
                SourceBookName = "GURPS Space",
                TechLevel = 12
            });

            SaveToCache();
        }

        // --- IFilterWindowProvider Implementation ---

        public void CreateComponentTree(List<ForgeFilterWindow.Element> tree)
        {
            tree.Add(new ForgeFilterWindow.GroupElement(0, "Armory Database"));

            // Level 1: Group by Source Book
            var bookGroups = _weapons.GroupBy(w => w.SourceBookName);
            foreach (var book in bookGroups)
            {
                tree.Add(new ForgeFilterWindow.GroupElement(1, book.Key));

                // Level 2: Group by Category
                var categoryGroups = book.GroupBy(w => w.Category);
                foreach (var cat in categoryGroups)
                {
                    tree.Add(new ForgeFilterWindow.GroupElement(2, cat.Key.ToString()));

                    foreach (var weapon in cat)
                    {
                        // Level 3: Selectable Weapon
                        tree.Add(new ForgeFilterWindow.Element(3, weapon.Name) { userData = weapon });
                    }
                }
            }
        }

        public bool GoToChild(ForgeFilterWindow.Element element, bool addIfComponent)
        {
            if (element.level == 3 && element.userData is GURPSWeapon weapon)
            {
                Debug.Log($"[DURPS] Armory Focus: {weapon.Name} (TL{weapon.TechLevel})");
                return true;
            }
            return false;
        }

        // --- IGuiProvider Implementation ---

        public VisualElement CreateGui(GuiContext ctx)
        {
            var gui = new GraphicalUserInterfaceBuilder(ModuleName)
                .WithTitle(Title)
                .WithPadding(10)
                .WithAutoGrow()
                .WithScrollable(true);

            gui.WithPanel("ArmoryControls")
                .WithFlexDirection(FlexDirection.Row)
                .AddButton("Reload from Warehouse", LoadFromCache)
                .AddButton("Seed Standard Weapons", SeedStandardWeapons)
                .EndPanel();

            gui.AddSeparator(Color.gray, 1);

            var categoryGroups = _weapons.GroupBy(w => w.Category);
            foreach (var category in categoryGroups)
            {
                gui.AddHeader($"{category.Key} Weapons", Color.yellow);

                foreach (var weapon in category)
                {
                    gui.WithPanel($"Weapon_{weapon.Name}")
                        .WithMarginBottom(6)
                        .WithPadding(8)
                        .WithBorderWidth(1)
                        .WithBorderColor(new Color(0.4f, 0.4f, 0.4f, 1f))
                        .WithFlexDirection(FlexDirection.Row)
                        .WithAlignItems(Align.Center);

                    // LINT FIX: Replaced raw Labels with ForgeLabelBuilder
                    gui.AddChild(new ForgeLabelBuilder($"TL{weapon.TechLevel}")
                        .WithWidth(35)
                        .WithFontSize(10)
                        .WithOpacity(0.7f));

                    gui.AddChild(new ForgeLabelBuilder(weapon.Name)
                        .WithFlexGrow(1)
                        .WithBold());

                    gui.AddChild(new ForgeLabelBuilder($"${weapon.Cost}")
                        .WithWidth(60)
                        .WithTextAlign(TextAnchor.MiddleRight));

                    gui.WithPanel("Stats")
                        .WithMarginLeft(10)
                        .WithFlexDirection(FlexDirection.Row)
                        .AddChild(new ForgeLabelBuilder($"Dmg: {weapon.DamageString}")
                            .WithFontSize(11)
                            .WithMarginRight(5));

                    if (weapon.Category != WeaponCategory.Melee)
                    {
                        gui.AddChild(new ForgeLabelBuilder($"Acc: {weapon.Accuracy} | Rng: {weapon.Range}")
                            .WithFontSize(11)
                            .WithOpacity(0.8f));
                    }

                    gui.EndPanel();

                    gui.AddButton("Details", () => Debug.Log($"[Armory] Inspecting {weapon.Name} from {weapon.SourceBookName}"));

                    gui.EndPanel();
                }
            }

            return gui.Build();
        }

        public Action<VisualElement> GetGuiBuilder()
        {
            return (root) => root.Add(CreateGui(new GuiContext()));
        }

        // --- Legacy & Serialization ---

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(new GuiContext());
            GraphicalUserInterfaceBuilder.ConvertToUIDocument(root, assetPath);
        }
#endif

        public void FromUIDocument(string assetPath) => Debug.Log($"[WeaponsAPI] Armory hydrated from {assetPath}");

        // --- THE FIX: Wrap Editor-Only Code ---
#if UNITY_EDITOR
        public void CreateComponentTree(List<UnityEditor.Rendering.FilterWindow.Element> tree)
        {
            tree.Add(new UnityEditor.Rendering.FilterWindow.GroupElement(0, "Armory (Legacy)"));
        }

        public bool GoToChild(UnityEditor.Rendering.FilterWindow.Element element, bool addIfComponent) => false;
#endif

        public void Bind(DataWarehouse warehouse)
        {
            throw new NotImplementedException();
        }

        [Serializable]
        private class CacheWrapper { public List<GURPSWeapon> Weapons = new List<GURPSWeapon>(); }
    }
}