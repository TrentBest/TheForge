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
    // A data component for scene-level manifestation of the economy state
    public class EconomyManifest : MonoBehaviour
    {
        public List<GURPSWealthLevel> WealthLevels = new List<GURPSWealthLevel>();
        public List<GURPSTradeGood> Commodities = new List<GURPSTradeGood>();
    }

    public class EconomyAPI : IForgeBuilder, IGurpsApiProvider, IGuiProvider, IFilterWindowProvider
    {
        // --- IFORGEBUILDER IMPLEMENTATION ---
        public string ToolName => "Economic & Trade Matrix";
        public Type GetProductType() => typeof(EconomyManifest);
        public IGuiProvider GetGuiProvider() => this;

        // --- IGURPSAPIPROVIDER IMPLEMENTATION ---
        public int Id => 1014;
        public ProviderType ProviderType => ProviderType.GurpsApi;
        public string ModuleName => "Economic & Trade Matrix";
        public string Title => ToolName;

        public Vector2 position { get; set; }
        private Guid _guid = Guid.NewGuid();
        Guid IGurpsApiProvider.Id { get => _guid; set => _guid = value; }

        // --- INTERNAL STATE ---
        private List<GURPSWealthLevel> _wealthLevels = new List<GURPSWealthLevel>();
        private List<GURPSTradeGood> _commodities = new List<GURPSTradeGood>();
        private int _globalTechFloor = 0;

        private object _selectedObject; // Polymorphic selection for the editor
        private readonly DataWarehouse _warehouse;
        private const string CacheKey = "GURPS_ECONOMY_DATABASE";

        private VisualElement _leftListPanel;
        private VisualElement _rightEditorPanel;

        public EconomyAPI() { _warehouse = new DataWarehouse(); Initialize(); }
        public EconomyAPI(DataWarehouse warehouse)
        {
            _warehouse = warehouse;
            Initialize();
        }

        public void Initialize() => LoadFromCache();

        /// <summary>
        /// Sets the minimum technological requirement for trade and wealth.
        /// [FIXED]: Resolved missing definition error.
        /// </summary>
        public void SetGlobalTechFloor(int level)
        {
            _globalTechFloor = Mathf.Max(0, level);
            Debug.Log($"[DURPS] Global Economic Tech Floor set to: {level}");
            SaveToCache();
        }

        public void SaveToCache()
        {
            if (_warehouse == null) return;
            string json = JsonUtility.ToJson(new CacheWrapper { WealthLevels = _wealthLevels, Commodities = _commodities, TechFloor = _globalTechFloor }, true);
            _warehouse.StoreTemporary(CacheKey, json);
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                var cache = JsonUtility.FromJson<CacheWrapper>(json);
                _wealthLevels = cache?.WealthLevels ?? new List<GURPSWealthLevel>();
                _commodities = cache?.Commodities ?? new List<GURPSTradeGood>();
                _globalTechFloor = cache?.TechFloor ?? 0;
            }
            if (_wealthLevels.Count == 0) SeedStandardEconomy();
        }

        private void SeedStandardEconomy()
        {
            _wealthLevels.Clear();
            _wealthLevels.Add(new GURPSWealthLevel { Name = "Dead Broke", Multiplier = 0f, Cost = -25 });
            _wealthLevels.Add(new GURPSWealthLevel { Name = "Average", Multiplier = 1f, Cost = 0 });
            _wealthLevels.Add(new GURPSWealthLevel { Name = "Wealthy", Multiplier = 5f, Cost = 20 });

            _commodities.Add(new GURPSTradeGood { Name = "Industrial Isotopes", BaseValue = 5000, TechLevelReq = 8 });
            _commodities.Add(new GURPSTradeGood { Name = "Grain", BaseValue = 10, TechLevelReq = 0 });

            SaveToCache();
        }

        public object Build()
        {
            var go = new GameObject("GURPS_EconomyNode");
            var manifest = go.AddComponent<EconomyManifest>();
            manifest.WealthLevels.AddRange(_wealthLevels);
            manifest.Commodities.AddRange(_commodities);
            return manifest;
        }

        // --- IFILTERWINDOWPROVIDER (Strict Forge Implementation) ---
        public void CreateComponentTree(List<ForgeFilterWindow.Element> tree)
        {
            tree.Add(new ForgeFilterWindow.GroupElement(0, "Economy & Trade"));

            tree.Add(new ForgeFilterWindow.GroupElement(1, "Wealth Levels"));
            foreach (var w in _wealthLevels)
                tree.Add(new ForgeFilterWindow.Element(2, w.Name) { userData = w });

            tree.Add(new ForgeFilterWindow.GroupElement(1, "Trade Commodities"));
            foreach (var c in _commodities)
                tree.Add(new ForgeFilterWindow.Element(2, c.Name) { userData = c });
        }

        public bool GoToChild(ForgeFilterWindow.Element element, bool addIfComponent)
        {
            if (element is ForgeFilterWindow.GroupElement) return false;

            if (element.userData != null)
            {
                _selectedObject = element.userData;
                RefreshEditor();
                return true;
            }
            return false;
        }

        // --- GUI GENERATION (Forge Container/Component Pattern) ---
        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("Economy_Root")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.09f));

            // Header - High-Tech Gold/Amber for Economy
            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithBackgroundColor(new Color(0.15f, 0.14f, 0.1f))
                .WithPadding(15)
                .WithBorderColor(new Color(0.8f, 0.6f, 0.2f))
                .WithBorderWidth(0, 0, 3, 0)
                .AddChild(new ForgeLabelBuilder("GURPS: ECONOMIC & TRADE MATRIX")
                    .WithFontSize(20).WithFontStyle(FontStyle.Bold).WithColor(new Color(1.0f, 0.8f, 0.4f)))
            );

            rootBuilder.AddChild(new ForgeContainerBuilder("SplitBody")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithFlexGrow(1)
                .AddChild(new ForgeContainerBuilder("ListPane")
                    .WithWidth(new StyleLength(Length.Percent(35)))
                    .WithBorderColor(new Color(0.2f, 0.2f, 0.25f))
                    .WithBorderWidth(0, 1, 0, 0)
                    .OnBuild(ve => _leftListPanel = ve))
                .AddChild(new ForgeContainerBuilder("EditorPane")
                    .WithFlexGrow(1).WithPadding(20)
                    .OnBuild(ve => _rightEditorPanel = ve))
            );

            var root = rootBuilder.Build();
            RefreshList();
            return root;
        }

        private void RefreshList()
        {
            if (_leftListPanel == null) return;
            _leftListPanel.Clear();

            var filterUI = new ForgeFilterWindowBuilder(this)
                .WithTitle("Market Search")
                .WithAccentColor(new Color(0.8f, 0.6f, 0.2f))
                .CreateGui(new GuiContext());

            filterUI.style.flexGrow = 1;
            _leftListPanel.Add(filterUI);

            // Quick Add Actions
            var actionRow = new ForgeContainerBuilder("Actions")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithMarginTop(10)
                .AddChild(new ForgeButtonBuilder("+ Wealth")
                    .WithBackgroundColor(new Color(0.2f, 0.3f, 0.2f))
                    .WithFlexGrow(1)
                    .OnClick(() => {
                        var nw = new GURPSWealthLevel { Name = "New Tier" };
                        _wealthLevels.Add(nw); SaveToCache(); RefreshList();
                    }))
                .AddChild(new ForgeButtonBuilder("+ Commodity")
                    .WithBackgroundColor(new Color(0.2f, 0.2f, 0.3f))
                    .WithFlexGrow(1).WithMarginLeft(5)
                    .OnClick(() => {
                        var nc = new GURPSTradeGood { Name = "New Good" };
                        _commodities.Add(nc); SaveToCache(); RefreshList();
                    }));

            _leftListPanel.Add(actionRow.Build());
        }

        private void RefreshEditor()
        {
            if (_rightEditorPanel == null || _selectedObject == null) return;
            _rightEditorPanel.Clear();

            var editor = new ForgeContainerBuilder("MatrixEditor");

            if (_selectedObject is GURPSWealthLevel w)
            {
                editor.AddChild(new ForgeLabelBuilder("Wealth Level Configuration").WithFontSize(18).WithFontStyle(FontStyle.Bold).WithMarginBottom(15))
                      .AddChild(new ForgeTextFieldBuilder("Designation", w.Name).OnValueChanged(v => { w.Name = v.newValue; SaveToCache(); RefreshList(); }))
                      .AddChild(new ForgeTextFieldBuilder("Cost (Points)", w.Cost.ToString()).OnValueChanged(v => { if (int.TryParse(v.newValue, out int r)) w.Cost = r; SaveToCache(); }))
                      .AddChild(new ForgeTextFieldBuilder("Multiplier (xBase)", w.Multiplier.ToString()).OnValueChanged(v => { if (float.TryParse(v.newValue, out float r)) w.Multiplier = r; SaveToCache(); }));
            }
            else if (_selectedObject is GURPSTradeGood c)
            {
                editor.AddChild(new ForgeLabelBuilder("Commodity Definition").WithFontSize(18).WithFontStyle(FontStyle.Bold).WithMarginBottom(15))
                      .AddChild(new ForgeTextFieldBuilder("Resource Name", c.Name).OnValueChanged(v => { c.Name = v.newValue; SaveToCache(); RefreshList(); }))
                      .AddChild(new ForgeTextFieldBuilder("Base Value (¤)", c.BaseValue.ToString()).OnValueChanged(v => { if (int.TryParse(v.newValue, out int r)) c.BaseValue = r; SaveToCache(); }))
                      .AddChild(new ForgeTextFieldBuilder("Min Tech Level", c.TechLevelReq.ToString()).OnValueChanged(v => { if (int.TryParse(v.newValue, out int r)) c.TechLevelReq = r; SaveToCache(); }));
            }

            _rightEditorPanel.Add(editor.Build());
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }

        public void Bind(DataWarehouse warehouse)
        {
            Initialize();
            Debug.Log($"[DURPS] Economy Engine bound to Warehouse: {warehouse != null}");
        }

        [Serializable]
        private class CacheWrapper
        {
            public List<GURPSWealthLevel> WealthLevels = new List<GURPSWealthLevel>();
            public List<GURPSTradeGood> Commodities = new List<GURPSTradeGood>();
            public int TechFloor;
        }
    }

    [Serializable]
    public class GURPSWealthLevel { public string Name; public float Multiplier; public int Cost; }

    [Serializable]
    public class GURPSTradeGood { public string Name; public int BaseValue; public int TechLevelReq; }
}