using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;
// Resolve ambiguity by aliasing the Workshop ForgeFilterWindow
using ForgeFilterWindow = Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.ForgeFilterWindow;

namespace Workshop.GURPS
{
    /// <summary>
    /// The Campaign Manager / Quantum Orchestrator.
    /// Acts as the high-level gatekeeper, determining which content manifests in the world.
    /// Reforged to follow the Forge Protocol and kill lambda ambiguity.
    /// </summary>
    public class CampaignAPI : IGurpsApiProvider, IFilterWindowProvider
    {
        public int Id => 1016;
        public ProviderType ProviderType => ProviderType.GurpsApi;
        public string ModuleName => "Campaign Manager";

        private List<string> _activeBookNames = new List<string>();
        private DataWarehouse _warehouse;
        private const string CacheKey = "GURPS_CAMPAIGN_ACTIVE_BOOKS";

        public Vector2 position { get; set; }
        private Guid _guid = Guid.NewGuid();
        Guid IGurpsApiProvider.Id { get => _guid; set => _guid = value; }

        public string Title => "Campaign Quantum Orchestrator";

        // GUI State
        private ScrollView _activeBooksListPanel;
        private string _pendingBookActivation = "";

        public CampaignAPI()
        {

        }

        public CampaignAPI(DataWarehouse warehouse) => _warehouse = warehouse;

        public void Initialize()
        {
            if (_warehouse == null)
            {
                Debug.LogWarning($"[{ModuleName}] Initialized without a Warehouse. Persistence disabled.");
                return;
            }
            LoadFromCache();
        }

        // --- THE QUANTUM FILTER LOGIC ---

        public bool IsContentAllowed(string sourceBook)
        {
            // Sandbox Mode: If no restrictions are set, everything is permitted.
            if (_activeBookNames.Count == 0) return true;
            return _activeBookNames.Any(b => b.Equals(sourceBook, StringComparison.OrdinalIgnoreCase));
        }

        public void ActivateBook(string name)
        {
            if (!string.IsNullOrWhiteSpace(name) && !_activeBookNames.Contains(name))
            {
                _activeBookNames.Add(name);
                SaveToCache();
                RefreshActiveBooksList();
            }
        }

        public void DeactivateBook(string name)
        {
            if (_activeBookNames.Remove(name))
            {
                SaveToCache();
                RefreshActiveBooksList();
            }
        }

        // --- PERSISTENCE ---

        public void SaveToCache()
        {
            if (_warehouse == null) return;
            string json = JsonUtility.ToJson(new CacheWrapper { ActiveBooks = _activeBookNames });
            _warehouse.StoreTemporary(CacheKey, json);
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                var cache = JsonUtility.FromJson<CacheWrapper>(json);
                _activeBookNames = cache?.ActiveBooks ?? new List<string>();
            }
        }

        // =================================================================================
        // GUI GENERATION LAYER (Forge Protocol compliant)
        // =================================================================================

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("CampaignAPI_Root")
                .WithFlexGrow(1f)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.09f));

            // Header Section
            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithPadding(15f)
                .WithBorderColor(new Color(0.2f, 0.2f, 0.25f))
                .WithBorderWidth(0, 0, 1f, 0)
                .AddChild(new ForgeLabelBuilder(Title).WithFontSize(18).WithBold().WithColor(new Color(0.9f, 0.4f, 0.8f)))
                .AddChild(new ForgeLabelBuilder("Global filtering scope for rulesets and databases.").WithColor(Color.gray).WithFontSize(11))
            );

            var bodyBuilder = new ForgeContainerBuilder("BodyLayer").WithFlexGrow(1f).WithPadding(20f);

            // Active Scope ScrollView
            bodyBuilder.AddChild(new ForgeContainerBuilder("ListContainer")
                .WithFlexGrow(1f)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithBorderWidth(1f)
                .WithBorderRadius(5f)
                .WithPadding(10f)
                // FIX: Explicitly using DynamicGuiProvider to resolve Lambda ambiguity
                .AddChild(new DynamicGuiProvider(c => {
                    var svBuilder = new ForgeScrollViewBuilder().WithMode(ScrollViewMode.Vertical);
                    var sv = svBuilder.CreateGui(c) as ScrollView;
                    sv.style.flexGrow = 1;
                    _activeBooksListPanel = sv;
                    return sv;
                }))
            );

            // Manual Injection UI
            bodyBuilder.AddChild(new ForgeContainerBuilder("InjectionRow")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithMarginTop(15f).WithPadding(10f).WithBorderRadius(5f)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .AddChild(new ForgeTextFieldBuilder("Ruleset Name", "")
                    .WithFlexGrow(1f).WithMarginRight(10f)
                    .OnValueChanged(evt => _pendingBookActivation = evt.newValue))
                .AddChild(new ForgeButtonBuilder("Activate")
                    .WithBackgroundColor(new Color(0.1f, 0.5f, 0.2f))
                    .WithWidth(100f)
                    .OnClick(() => ActivateBook(_pendingBookActivation)))
            );

            rootBuilder.AddChild(bodyBuilder);

            var root = rootBuilder.Build();
            RefreshActiveBooksList();
            return root;
        }

        private void RefreshActiveBooksList()
        {
            if (_activeBooksListPanel == null) return;
            _activeBooksListPanel.Clear();

            if (_activeBookNames.Count == 0)
            {
                _activeBooksListPanel.Add(new ForgeLabelBuilder("Sandbox Mode: All content allowed.")
                    .WithColor(Color.gray).WithFontStyle(FontStyle.Italic).WithMarginTop(10f).Build());
                return;
            }

            foreach (var bookName in _activeBookNames)
            {
                var n = bookName;
                var row = new ForgeContainerBuilder($"Row_{n}")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                    .WithPadding(8f).WithMarginBottom(4f).WithBorderRadius(3f)
                    .AddChild(new ForgeLabelBuilder(n).WithColor(new Color(0.0f, 0.9f, 1.0f)).WithBold())
                    .AddChild(new ForgeButtonBuilder("Deactivate")
                        .WithBackgroundColor(new Color(0.6f, 0.1f, 0.1f))
                        .WithWidth(90f)
                        .OnClick(() => DeactivateBook(n)));

                _activeBooksListPanel.Add(row.Build());
            }
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "CampaignSnapshot");
        public void FromUIDocument(string path) { }

        // =================================================================================
        // FILTER WINDOW IMPLEMENTATION
        // =================================================================================

        public void CreateComponentTree(List<ForgeFilterWindow.Element> tree)
        {
            tree.Add(new ForgeFilterWindow.GroupElement(0, "Active Campaign Scope"));
            foreach (var book in _activeBookNames) tree.Add(new ForgeFilterWindow.Element(1, book));
        }

        public bool GoToChild(ForgeFilterWindow.Element element, bool add) => false;

        // --- THE FIX: Wrap Editor-Only Code ---
#if UNITY_EDITOR
        // Redirects for legacy FilterWindow stubs
        public void CreateComponentTree(List<UnityEditor.Rendering.FilterWindow.Element> tree) { }
        public bool GoToChild(UnityEditor.Rendering.FilterWindow.Element element, bool addIfComponent) => false;
#endif

        public void Bind(DataWarehouse warehouse) => _warehouse = warehouse;

        [Serializable]
        private class CacheWrapper { public List<string> ActiveBooks = new List<string>(); }
    }
}