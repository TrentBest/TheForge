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
    /// The Logistics & Transportation Registry.
    /// Manages vehicle blueprints for freight, combat, and exploration.
    /// Reforged to follow the Forge Protocol and kill lambda ambiguity.
    /// </summary>
    public class LogisticsAPI : IGurpsApiProvider, IFilterWindowProvider
    {
        public int Id => 1013;
        public ProviderType ProviderType => ProviderType.GurpsApi;
        public string ModuleName => "Logistics & Transportation";

        private List<GURPSVehicleTemplate> _templates = new List<GURPSVehicleTemplate>();
        private readonly DataWarehouse _warehouse;
        private const string CacheKey = "GURPS_LOGISTICS_DATABASE";

        public Vector2 position { get; set; }
        private Guid _guid = Guid.NewGuid();
        Guid IGurpsApiProvider.Id { get => _guid; set => _guid = value; }

        public string Title => "Vehicle Logistics Manifest";

        // UI State
        private GURPSVehicleTemplate _selectedTemplate;
        private ScrollView _leftListPanel;
        private VisualElement _rightEditorPanel;

        public LogisticsAPI() { _warehouse = new DataWarehouse(); Initialize(); }

        public LogisticsAPI(DataWarehouse warehouse)
        {
            _warehouse = warehouse;
            Initialize();
        }

        public void Initialize() => LoadFromCache();

        // --- PERSISTENCE ---

        public void SaveToCache()
        {
            if (_warehouse == null) return;
            // FIXED: Now uses the internal CacheWrapper defined below
            string json = JsonUtility.ToJson(new CacheWrapper { Templates = _templates }, true);
            _warehouse.StoreTemporary(CacheKey, json);
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                // FIXED: Now uses the internal CacheWrapper defined below
                var cache = JsonUtility.FromJson<CacheWrapper>(json);
                _templates = cache?.Templates ?? new List<GURPSVehicleTemplate>();
            }
            if (_templates.Count == 0) SeedStandardVehicles();
        }

        private void SeedStandardVehicles()
        {
            _templates.Add(new GURPSVehicleTemplate { Name = "Bulk Hauler", MaxVelocity = 25, FuelCapacity = 5000, CargoCapacityTons = 500 });
            _templates.Add(new GURPSVehicleTemplate { Name = "Interceptor", MaxVelocity = 120, FuelCapacity = 1000, CargoCapacityTons = 2 });
            SaveToCache();
        }

        // --- GUI GENERATION LAYER ---

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("LogisticsAPI_Root")
                .WithFlexGrow(1f)
                .WithBackgroundColor(new Color(0.08f, 0.09f, 0.08f)); // Industrial Green-Black

            // Header Ribbon
            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithBackgroundColor(new Color(0.12f, 0.15f, 0.12f))
                .WithPadding(15f)
                .WithBorderColor(new Color(0.4f, 0.6f, 0.4f))
                .WithBorderWidth(0, 0, 1f, 0)
                .AddChild(new ForgeLabelBuilder(Title).WithFontSize(18).WithBold().WithColor(new Color(0.6f, 1.0f, 0.6f)))
            );

            // Split Layout
            var splitBody = new ForgeContainerBuilder("SplitBody")
                .WithDirection(FlexDirection.Row)
                .WithFlexGrow(1f);

            // LEFT PANE: Vehicle Registry
            var listPane = new ForgeContainerBuilder("ListPane")
                .WithWidth(new StyleLength(Length.Percent(30f)))
                .WithBorderColor(new Color(0.2f, 0.25f, 0.2f))
                .WithBorderWidth(0, 1f, 0, 0)
                .WithPadding(10f)
                .AddChild(new DynamicGuiProvider(c => {
                    var svBuilder = new ForgeScrollViewBuilder().WithMode(ScrollViewMode.Vertical);
                    var sv = svBuilder.CreateGui(c) as ScrollView;
                    sv.style.flexGrow = 1;
                    _leftListPanel = sv;
                    return sv;
                }))
                .AddChild(new ForgeButtonBuilder("➕ New Vehicle Blueprint")
                    .WithBackgroundColor(new Color(0.1f, 0.4f, 0.2f))
                    .WithMarginTop(10f)
                    .OnClick(() => {
                        var newTpl = new GURPSVehicleTemplate { Name = "New Hull Design" };
                        _templates.Add(newTpl);
                        SaveToCache();
                        SelectTemplate(newTpl);
                    }));

            // RIGHT PANE: Hull Architect
            var editorPane = new ForgeContainerBuilder("EditorPane")
                .WithFlexGrow(1f)
                .WithPadding(20f)
                .OnBuild(ve => _rightEditorPanel = ve);

            splitBody.AddChild(listPane).AddChild(editorPane);
            rootBuilder.AddChild(splitBody);

            var root = rootBuilder.Build();
            RefreshVehicleList();
            if (_templates.Count > 0) SelectTemplate(_templates[0]);

            return root;
        }

        private void RefreshVehicleList()
        {
            if (_leftListPanel == null) return;
            _leftListPanel.Clear();

            foreach (var template in _templates)
            {
                var t = template;
                var isSelected = t == _selectedTemplate;
                var btnColor = isSelected ? new Color(0.3f, 0.4f, 0.3f) : new Color(0.15f, 0.18f, 0.15f);

                _leftListPanel.Add(new ForgeButtonBuilder(t.Name)
                    .OnClick(() => SelectTemplate(t))
                    .WithBackgroundColor(btnColor)
                    .WithMarginBottom(4f)
                    .WithBorderRadius(3f)
                    .Build());
            }
        }

        private void SelectTemplate(GURPSVehicleTemplate template)
        {
            _selectedTemplate = template;
            RefreshVehicleList();

            if (_rightEditorPanel == null || template == null) return;
            _rightEditorPanel.Clear();

            var editor = new ForgeContainerBuilder("HullArchitect")
                .AddChild(new ForgeLabelBuilder($"ARCHITECT: {template.Name}").WithFontSize(18).WithBold().WithMarginBottom(15f))
                .AddChild(new ForgeTextFieldBuilder("Hull Designation", template.Name)
                    .OnValueChanged(v => { template.Name = v.newValue; RefreshVehicleList(); }))
                .AddSeparator(new Color(0.2f, 0.3f, 0.2f), 1f)
                .AddChild(new ForgeTextFieldBuilder("Max Velocity", template.MaxVelocity.ToString())
                    .OnValueChanged(v => { if (float.TryParse(v.newValue, out float r)) template.MaxVelocity = r; }))
                .AddChild(new ForgeTextFieldBuilder("Fuel Capacity", template.FuelCapacity.ToString())
                    .OnValueChanged(v => { if (float.TryParse(v.newValue, out float r)) template.FuelCapacity = r; }))
                .AddChild(new ForgeTextFieldBuilder("Cargo (Tons)", template.CargoCapacityTons.ToString())
                    .OnValueChanged(v => { if (int.TryParse(v.newValue, out int r)) template.CargoCapacityTons = r; }))
                .AddChild(new ForgeButtonBuilder("🗑 Scuttle Design")
                    .WithBackgroundColor(new Color(0.5f, 0.1f, 0.1f))
                    .WithMarginTop(20f)
                    .OnClick(() => {
                        _templates.Remove(template);
                        SaveToCache();
                        SelectTemplate(_templates.Count > 0 ? _templates[0] : null);
                    }));

            _rightEditorPanel.Add(editor.Build());
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var snapshotRoot = CreateGui(new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? "LogisticsForge_Snapshot" : System.IO.Path.GetFileNameWithoutExtension(assetPath);
            WorkshopUxmlBaker.Bake(snapshotRoot, fileName);
        }

        public void FromUIDocument(string assetPath) { }

        // --- FILTER WINDOW ---

        public void CreateComponentTree(List<ForgeFilterWindow.Element> tree)
        {
            tree.Add(new ForgeFilterWindow.GroupElement(0, "Vehicle Blueprints"));
            foreach (var t in _templates)
                tree.Add(new ForgeFilterWindow.Element(1, t.Name) { userData = t });
        }

        public bool GoToChild(ForgeFilterWindow.Element element, bool addIfComponent)
        {
            if (element.userData is GURPSVehicleTemplate target)
            {
                SelectTemplate(target);
                return true;
            }
            return false;
        }

        // --- THE FIX: Wrap Editor-Only Code ---
#if UNITY_EDITOR
        public void CreateComponentTree(List<UnityEditor.Rendering.FilterWindow.Element> tree) { }
        public bool GoToChild(UnityEditor.Rendering.FilterWindow.Element element, bool addIfComponent) => false;
#endif

        public void Bind(DataWarehouse warehouse)
        {
            throw new NotImplementedException();
        }

        // =================================================================================
        // INTERNAL DATA WRAPPER
        // =================================================================================

        [Serializable]
        private class CacheWrapper
        {
            public List<GURPSVehicleTemplate> Templates = new List<GURPSVehicleTemplate>();
        }
    }
}