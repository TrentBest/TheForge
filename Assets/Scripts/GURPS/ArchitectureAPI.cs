using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.GURPS
{
    public class ArchitectureAPI : IGurpsApiProvider, IFilterWindowProvider
    {
        public int Id => 1018;
        public ProviderType ProviderType => ProviderType.GurpsApi;
        public string ModuleName => "Architectural Forge";

        private List<GURPSBuildingTemplate> _templates = new List<GURPSBuildingTemplate>();
        private DataWarehouse _warehouse; // Removed readonly to allow Binding
        private const string CacheKey = "GURPS_ARCH_DATABASE";

        public Vector2 position { get; set; }
        private Guid _guid = Guid.NewGuid();
        Guid IGurpsApiProvider.Id { get => _guid; set => _guid = value; }

        public string Title => "Architectural Forge Database";

        // UI State
        private GURPSBuildingTemplate _selectedTemplate;
        private ScrollView _leftListPanel;
        private VisualElement _rightEditorPanel;

        // Default Constructor for safe instantiation
        public ArchitectureAPI() { }

        public ArchitectureAPI(DataWarehouse warehouse)
        {
            _warehouse = warehouse;
            Initialize();
        }

        public void Initialize()
        {
            if (_warehouse == null)
            {
                Debug.LogWarning($"[{ModuleName}] Initialized without a Warehouse. Persistence disabled until Bind().");
                return;
            }
            LoadFromCache();
        }

        public void Bind(DataWarehouse warehouse)
        {
            _warehouse = warehouse;
            // Immediate data ingestion upon binding if not already initialized
            if (_templates.Count == 0) LoadFromCache();
        }

        public void SaveToCache()
        {
            if (_warehouse == null) return;
            string json = JsonUtility.ToJson(new CacheWrapper { Templates = _templates }, true);
            _warehouse.StoreTemporary(CacheKey, json);
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                var cache = JsonUtility.FromJson<CacheWrapper>(json);
                _templates = cache?.Templates ?? new List<GURPSBuildingTemplate>();
            }

            // Safe fallback to seed data if the database is empty
            if (_templates.Count == 0) SeedStandardArch();
        }

        private void SeedStandardArch()
        {
            _templates.Clear();
            _templates.Add(new GURPSBuildingTemplate { Name = "Fortified Outpost", TechLevel = 7, DR = 100, HP = 5000, Purpose = "Military" });
            _templates.Add(new GURPSBuildingTemplate { Name = "Habitation Shanty", TechLevel = 3, DR = 2, HP = 50, Purpose = "Residential" });
            SaveToCache();
        }

        internal void Register(GURPSBuildingTemplate template)
        {
            if (template != null && !_templates.Contains(template))
            {
                _templates.Add(template);
                SaveToCache();
                RefreshTemplateList();
            }
        }

        // =================================================================================
        // GUI GENERATION LAYER (Pure Forge Protocol)
        // =================================================================================

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("ArchitectureAPI_Root")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.09f));

            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithPadding(15f)
                .WithBorderColor(new Color(0.2f, 0.2f, 0.25f))
                .WithBorderWidth(0, 0, 1, 0)
                .AddChild(new ForgeLabelBuilder(Title).WithFontSize(18).WithBold().WithColor(new Color(0.0f, 0.9f, 1.0f)))
            );

            rootBuilder.AddChild(new ForgeContainerBuilder("SplitBody")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithFlexGrow(1)
                .AddChild(new ForgeContainerBuilder("ListPane")
                    .WithWidth(new StyleLength(Length.Percent(30f)))
                    .WithBorderColor(new Color(0.2f, 0.2f, 0.25f))
                    .WithBorderWidth(0, 1, 0, 0)
                    .WithPadding(10f)
                    .AddChild(new DynamicGuiProvider(c => {
                        var svBuilder = new ForgeScrollViewBuilder().WithMode(ScrollViewMode.Vertical);
                        var sv = svBuilder.CreateGui(c) as ScrollView;
                        sv.style.flexGrow = 1;
                        _leftListPanel = sv;
                        return sv;
                    }))
                    .AddChild(new ForgeButtonBuilder("➕ Add Blueprint")
                        .WithBackgroundColor(new Color(0.1f, 0.5f, 0.2f))
                        .WithMarginTop(10f)
                        .OnClick(() => {
                            var newTpl = new GURPSBuildingTemplate { Name = "New Blueprint" };
                            Register(newTpl);
                            SelectTemplate(newTpl);
                        }))
                )
                .AddChild(new ForgeContainerBuilder("EditorPane")
                    .WithFlexGrow(1)
                    .WithPadding(20f)
                    .OnBuild(ve => _rightEditorPanel = ve))
            );

            var root = rootBuilder.Build();
            RefreshTemplateList();
            if (_templates.Count > 0) SelectTemplate(_templates[0]);

            return root;
        }

        private void RefreshTemplateList()
        {
            if (_leftListPanel == null) return;
            _leftListPanel.Clear();

            foreach (var template in _templates)
            {
                var capturedTemplate = template;
                var isSelected = capturedTemplate == _selectedTemplate;
                var btnColor = isSelected ? new Color(0.3f, 0.3f, 0.4f) : new Color(0.15f, 0.15f, 0.18f);

                _leftListPanel.Add(new ForgeButtonBuilder(capturedTemplate.Name)
                    .OnClick(() => SelectTemplate(capturedTemplate))
                    .WithBackgroundColor(btnColor)
                    .WithMarginBottom(4f)
                    .WithBorderRadius(3f)
                    .Build());
            }
        }

        private void SelectTemplate(GURPSBuildingTemplate template)
        {
            _selectedTemplate = template;
            RefreshTemplateList();

            if (_rightEditorPanel == null) return;
            _rightEditorPanel.Clear();
            if (template == null) return;

            var editor = new ForgeContainerBuilder("TemplateEditor")
                .WithFlexGrow(1)
                .AddChild(new ForgeLabelBuilder($"Editing: {template.Name}").WithFontSize(16).WithBold().WithMarginBottom(15f))
                .AddChild(new ForgeTextFieldBuilder("Blueprint Name", template.Name)
                    .OnValueChanged(evt => { template.Name = evt.newValue; RefreshTemplateList(); })
                    .WithMarginBottom(10f))
                .AddChild(new ForgeTextFieldBuilder("Primary Purpose", template.Purpose)
                    .OnValueChanged(evt => template.Purpose = evt.newValue)
                    .WithMarginBottom(10f))
                .AddChild(new ForgeTextFieldBuilder("Tech Level (TL)", template.TechLevel.ToString())
                    .OnValueChanged(evt => { if (int.TryParse(evt.newValue, out int v)) template.TechLevel = v; })
                    .WithMarginBottom(10f))
                .AddChild(new ForgeTextFieldBuilder("Structural Hit Points (HP)", template.HP.ToString())
                    .OnValueChanged(evt => { if (int.TryParse(evt.newValue, out int v)) template.HP = v; })
                    .WithMarginBottom(10f))
                .AddChild(new ForgeTextFieldBuilder("Square Footage", template.SquareFootage.ToString())
                    .OnValueChanged(evt => { if (float.TryParse(evt.newValue, out float v)) template.SquareFootage = v; })
                    .WithMarginBottom(30f))
                .AddChild(new ForgeContainerBuilder("ActionBar")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .AddChild(new ForgeButtonBuilder("💾 Save Database")
                        .WithBackgroundColor(new Color(0.1f, 0.4f, 0.6f))
                        .WithWidth(150f)
                        .OnClick(SaveToCache))
                    .AddChild(new ForgeButtonBuilder("🗑 Delete")
                        .WithBackgroundColor(new Color(0.6f, 0.1f, 0.1f))
                        .WithWidth(140f)
                        .OnClick(() => {
                            _templates.Remove(template);
                            SaveToCache();
                            SelectTemplate(_templates.Count > 0 ? _templates[0] : null);
                        }))
                );

            _rightEditorPanel.Add(editor.Build());
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var snapshotRoot = CreateGui(new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? "ArchitectureForge_Snapshot" : System.IO.Path.GetFileNameWithoutExtension(assetPath);
            WorkshopUxmlBaker.Bake(snapshotRoot, fileName);
        }

        public void FromUIDocument(string assetPath) { }

        // =================================================================================
        // FILTER WINDOW (Forge Compliant)
        // =================================================================================

        public void CreateComponentTree(List<ForgeFilterWindow.Element> tree)
        {
            tree.Add(new ForgeFilterWindow.GroupElement(0, "Architecture Blueprints"));
            foreach (var t in _templates)
                tree.Add(new ForgeFilterWindow.Element(1, t.Name) { userData = t });
        }

        public bool GoToChild(ForgeFilterWindow.Element element, bool addIfComponent)
        {
            if (element is ForgeFilterWindow.GroupElement) return false;
            if (element.userData is GURPSBuildingTemplate target)
            {
                SelectTemplate(target);
                return true;
            }
            return false;
        }

        // --- THE FIX: Wrap Editor-Only Code ---
#if UNITY_EDITOR
        // Legacy stubs
        public void CreateComponentTree(List<UnityEditor.Rendering.FilterWindow.Element> tree) { }
        public bool GoToChild(UnityEditor.Rendering.FilterWindow.Element element, bool addIfComponent) => false;
#endif

        [Serializable]
        private class CacheWrapper { public List<GURPSBuildingTemplate> Templates = new List<GURPSBuildingTemplate>(); }
    }
}