using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.GURPS
{
    /// <summary>
    /// The data container injected into the scene when the tool "Builds".
    /// </summary>
    public class DisadvantageManifest : MonoBehaviour
    {
        public List<GURPSDisadvantage> Traits = new List<GURPSDisadvantage>();
    }

    /// <summary>
    /// The Digital GURPS Disadvantages & Powers Library.
    /// Reforged to follow the Forge Protocol, utilizing Development GUIDs
    /// and Sovereign Memory binding.
    /// </summary>
    public class DisadvantagesAPI : IForgeBuilder, IGurpsApiProvider, IGuiProvider, IFilterWindowProvider
    {
        // --- IFORGEBUILDER ---
        public string ToolName => "Disadvantages & Powers Engine";
        public Type GetProductType() => typeof(DisadvantageManifest);
        public IGuiProvider GetGuiProvider() => this;

        // --- IDENTIFICATION (Development GUID Standard) ---
        private Guid _id = Guid.Parse("00000000-0000-0000-0000-000000001003");
        public Guid Id { get => _id; set => _id = value; }

        public ProviderType ProviderType => ProviderType.GurpsApi;
        public string ModuleName => "Disadvantages & Powers";
        public string Title => ToolName;
        public Vector2 position { get; set; }

        // --- INTERNAL STATE ---
        private List<GURPSDisadvantage> _disadvantages = new List<GURPSDisadvantage>();
        private GURPSDisadvantage _selectedTrait;
        private DataWarehouse _warehouse; // Roach Spray: Removed 'readonly' so Bind() can work
        private const string CacheKey = "GURPS_DISADVANTAGE_DATABASE";

        // --- UI CACHE ---
        private VisualElement _leftListPanel;
        private VisualElement _rightEditorPanel;

        public DisadvantagesAPI() { }

        public DisadvantagesAPI(DataWarehouse warehouse)
        {
            Bind(warehouse);
        }

        public void Initialize() => LoadFromCache();

        // --- LIFECYCLE & BINDING ---

        public void Bind(DataWarehouse warehouse)
        {
            if (warehouse == null)
            {
                Debug.LogWarning("[DisadvantagesAPI] Attempted to bind a null DataWarehouse. Aborting.");
                return;
            }

            _warehouse = warehouse;
            LoadFromCache();
            Debug.Log($"[DisadvantagesAPI] Bound to Sovereign Memory. {_disadvantages.Count} burdens loaded.");
        }

        // --- PERSISTENCE ---

        public void SaveToCache()
        {
            if (_warehouse == null) return;
            string json = JsonUtility.ToJson(new CacheWrapper { Disadvantages = _disadvantages }, true);
            _warehouse.StoreTemporary(CacheKey, json);
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                var cache = JsonUtility.FromJson<CacheWrapper>(json);
                if (cache?.Disadvantages != null && cache.Disadvantages.Count > 0)
                {
                    _disadvantages = cache.Disadvantages;
                    return;
                }
            }
            SeedStandardBurdens();
        }

        private void SeedStandardBurdens()
        {
            _disadvantages.Clear();
            // Roach Spray: Add directly to avoid boot-spamming the Save function
            _disadvantages.Add(new GURPSDisadvantage { Name = "Blindness", BaseCost = -50, Description = "Total loss of vision." });
            _disadvantages.Add(new GURPSDisadvantage { Name = "High Pain Threshold", BaseCost = 10, Description = "Ignore shock penalties." });
            SaveToCache();
        }

        // --- IFORGEBUILDER IMPLEMENTATION ---

        public object Build()
        {
            var go = new GameObject("GURPS_DisadvantageManifest");
            var manifest = go.AddComponent<DisadvantageManifest>();
            foreach (var d in _disadvantages) manifest.Traits.Add(d);
            return manifest;
        }

        // --- IFILTERWINDOWPROVIDER ---

        public void CreateComponentTree(List<ForgeFilterWindow.Element> tree)
        {
            tree.Add(new ForgeFilterWindow.GroupElement(0, "Trait Registry"));
            foreach (var d in _disadvantages)
            {
                // Roach Spray: Attach 'userData' for O(1) object retrieval instead of matching by string
                tree.Add(new ForgeFilterWindow.Element(1, d.Name) { userData = d });
            }
        }

        public bool GoToChild(ForgeFilterWindow.Element element, bool addIfComponent)
        {
            if (element.userData is GURPSDisadvantage target)
            {
                SelectTrait(target);
                return true; // Closes the search window on successful selection
            }
            return false;
        }

        // --- GUI GENERATION (Forge Protocol) ---

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("Disadvantages_Root")
                .WithFlexGrow(1f)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.09f));

            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithBackgroundColor(new Color(0.15f, 0.12f, 0.12f))
                .WithPadding(15f)
                .WithBorderColor(new Color(0.8f, 0.2f, 0.2f))
                .WithBorderWidth(0, 0, 3f, 0)
                .AddChild(new ForgeLabelBuilder("GURPS: BURDENS & POWERS")
                    .WithFontSize(20).WithFontStyle(FontStyle.Bold).WithColor(new Color(1.0f, 0.4f, 0.4f)))
            );

            rootBuilder.AddChild(new ForgeContainerBuilder("SplitBody")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithFlexGrow(1f)
                .AddChild(new ForgeContainerBuilder("ListPane")
                    .WithWidth(new StyleLength(Length.Percent(35f)))
                    .WithBorderColor(new Color(0.2f, 0.2f, 0.25f))
                    .WithBorderWidth(0, 1f, 0, 0)
                    .OnBuild(ve => _leftListPanel = ve))
                .AddChild(new ForgeContainerBuilder("EditorPane")
                    .WithFlexGrow(1f).WithPadding(20f)
                    .OnBuild(ve => _rightEditorPanel = ve))
            );

            var root = rootBuilder.Build();
            RefreshTraitList();
            if (_disadvantages.Count > 0) SelectTrait(_disadvantages[0]);

            return root;
        }

        private void RefreshTraitList()
        {
            if (_leftListPanel == null) return;
            _leftListPanel.Clear();

            var filterUI = new ForgeFilterWindowBuilder(this)
                .WithTitle("Burdens Search")
                .WithAccentColor(new Color(0.8f, 0.2f, 0.2f))
                .CreateGui(new GuiContext());

            filterUI.style.flexGrow = 1;
            _leftListPanel.Add(filterUI);

            _leftListPanel.Add(new ForgeButtonBuilder("➕ Log New Burden")
                .WithBackgroundColor(new Color(0.4f, 0.1f, 0.1f))
                .WithMarginTop(10f)
                .OnClick(() => {
                    var newTrait = new GURPSDisadvantage { Name = "New Burden" };
                    _disadvantages.Add(newTrait);
                    SaveToCache();
                    SelectTrait(newTrait);
                }).Build());
        }

        private void SelectTrait(GURPSDisadvantage trait)
        {
            if (trait == null || _rightEditorPanel == null) return;

            _selectedTrait = trait;
            RefreshTraitList(); // Refresh list to clear search filter and show new entries
            _rightEditorPanel.Clear();

            var editorContainer = new ForgeContainerBuilder("TraitEditor")
                .AddChild(new ForgeLabelBuilder("Burden Definition").WithFontSize(18).WithFontStyle(FontStyle.Bold).WithMarginBottom(15f))
                .AddChild(new ForgeTextFieldBuilder("Burden Name", trait.Name)
                    .OnValueChanged(evt => { trait.Name = evt.newValue; SaveToCache(); RefreshTraitList(); }))
                .AddChild(new ForgeTextFieldBuilder("Point Value", trait.BaseCost.ToString())
                    .OnValueChanged(evt => { if (int.TryParse(evt.newValue, out int c)) { trait.BaseCost = c; SaveToCache(); } }))
                .AddChild(new ForgeTextFieldBuilder("Description", trait.Description)
                    .AsMultiline(true)
                    .OnValueChanged(evt => { trait.Description = evt.newValue; SaveToCache(); }))

                // Added a Delete Button so CRUD is complete
                .AddChild(new ForgeButtonBuilder("🗑️ Delete Burden")
                    .WithBackgroundColor(new Color(0.5f, 0.1f, 0.1f))
                    .WithMarginTop(30f)
                    .OnClick(() => DeleteTrait(trait)));

            _rightEditorPanel.Add(editorContainer.Build());
        }

        private void DeleteTrait(GURPSDisadvantage trait)
        {
            if (_disadvantages.Remove(trait))
            {
                SaveToCache();
                _selectedTrait = null;
                _rightEditorPanel.Clear();
                RefreshTraitList();
            }
        }

        // --- FORGE GUI INTEGRATION STUBS ---

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { /* Handled by generic baker elsewhere if needed */ }
        public void FromUIDocument(string assetPath) { }

        // Internal wrapper required for flat JSON array serialization in Unity
        [Serializable]
        private class CacheWrapper { public List<GURPSDisadvantage> Disadvantages = new List<GURPSDisadvantage>(); }
    }
}