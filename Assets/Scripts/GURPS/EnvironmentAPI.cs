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
    // A data component for scene-level manifestation of environmental parameters
    public class EnvironmentManifest : MonoBehaviour
    {
        public List<GURPSEnvironment> ActiveProfiles = new List<GURPSEnvironment>();
    }

    [Serializable]
    public class GURPSEnvironment
    {
        public string Id = Guid.NewGuid().ToString();
        public string Name = "New Climate Profile";
        public float Gravity = 1.0f; // Gs
        public string Atmosphere = "Standard Oxygen-Nitrogen";
        public float Pressure = 1.0f; // Atmospheres
        public int TemperatureBase = 70; // Fahrenheit
        public int RadiationLevel = 0; // Rads/Hour
    }

    public class EnvironmentAPI : IForgeBuilder, IGurpsApiProvider, IGuiProvider, IFilterWindowProvider
    {
        // --- IFORGEBUILDER IMPLEMENTATION ---
        public string ToolName => "Environmental Physics Architect";
        public Type GetProductType() => typeof(EnvironmentManifest);
        public IGuiProvider GetGuiProvider() => this;

        // --- IGURPSAPIPROVIDER IMPLEMENTATION ---
        public int Id => 1017;
        public ProviderType ProviderType => ProviderType.GurpsApi;
        public string ModuleName => "Environmental Physics";
        public string Title => ToolName;
        public EnvironmentAPI() { _warehouse = new DataWarehouse(); Initialize(); }
        public Vector2 position { get; set; }
        private Guid _guid = Guid.NewGuid();
        Guid IGurpsApiProvider.Id { get => _guid; set => _guid = value; }

        // --- INTERNAL STATE ---
        private List<GURPSEnvironment> _environments = new List<GURPSEnvironment>();
        private GURPSEnvironment _selectedEnvironment;
        private readonly DataWarehouse _warehouse;
        private const string CacheKey = "GURPS_ENVIRONMENT_DATABASE";

        private VisualElement _leftListPanel;
        private VisualElement _rightEditorPanel;

        public EnvironmentAPI(DataWarehouse warehouse)
        {
            _warehouse = warehouse;
            Initialize();
        }

        public void Initialize() => LoadFromCache();

        public void SaveToCache()
        {
            if (_warehouse == null) return;
            string json = JsonUtility.ToJson(new CacheWrapper { Environments = _environments }, true);
            _warehouse.StoreTemporary(CacheKey, json);
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                var cache = JsonUtility.FromJson<CacheWrapper>(json);
                _environments = cache?.Environments ?? new List<GURPSEnvironment>();
            }
            if (_environments.Count == 0) SeedStandardEnvironments();
        }

        private void SeedStandardEnvironments()
        {
            _environments.Clear();
            _environments.Add(new GURPSEnvironment { Name = "Standard Terrestrial", Gravity = 1.0f, Atmosphere = "Oxygen-Nitrogen", Pressure = 1.0f });
            _environments.Add(new GURPSEnvironment { Name = "Vacuum (Deep Space)", Gravity = 0f, Atmosphere = "None", Pressure = 0f });
            _environments.Add(new GURPSEnvironment { Name = "Heavy Venusian", Gravity = 0.9f, Atmosphere = "Corrosive", Pressure = 90.0f });
            SaveToCache();
        }

        public object Build()
        {
            var go = new GameObject("GURPS_EnvironmentNode");
            var manifest = go.AddComponent<EnvironmentManifest>();
            // Deep copy to ensure scene data is decoupled from cached API data
            foreach (var env in _environments)
            {
                manifest.ActiveProfiles.Add(new GURPSEnvironment
                {
                    Name = env.Name,
                    Gravity = env.Gravity,
                    Atmosphere = env.Atmosphere,
                    Pressure = env.Pressure,
                    TemperatureBase = env.TemperatureBase,
                    RadiationLevel = env.RadiationLevel
                });
            }
            return manifest;
        }

        // --- IFILTERWINDOWPROVIDER (Forge Implementation) ---
        public void CreateComponentTree(List<ForgeFilterWindow.Element> tree)
        {
            tree.Add(new ForgeFilterWindow.GroupElement(0, "Environment Profiles"));
            foreach (var env in _environments)
            {
                tree.Add(new ForgeFilterWindow.Element(1, env.Name) { userData = env });
            }
        }

        public bool GoToChild(ForgeFilterWindow.Element element, bool addIfComponent)
        {
            if (element is ForgeFilterWindow.GroupElement) return false;
            if (element.userData is GURPSEnvironment target)
            {
                SelectEnvironment(target);
                return true;
            }
            return false;
        }

        // --- THE FIX: Wrap Editor-Only Code ---
#if UNITY_EDITOR
        public bool GoToChild(UnityEditor.Rendering.FilterWindow.Element element, bool addIfComponent)
        {
            if (element is UnityEditor.Rendering.FilterWindow.GroupElement) return false;
            var target = _environments.FirstOrDefault(e => e.Name == element.name);
            if (target != null)
            {
                SelectEnvironment(target);
                return true;
            }
            return false;
        }
#endif

        // --- GUI GENERATION (Forge Container/Component Pattern) ---
        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("Environment_Root")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.09f));

            // Header Section
            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithBackgroundColor(new Color(0.1f, 0.15f, 0.15f))
                .WithPadding(15)
                .WithBorderColor(new Color(0.2f, 0.8f, 0.8f))
                .WithBorderWidth(0, 0, 3, 0)
                .AddChild(new ForgeLabelBuilder("GURPS: ENVIRONMENTAL PHYSICS")
                    .WithFontSize(20).WithFontStyle(FontStyle.Bold).WithColor(new Color(0.4f, 1.0f, 0.9f)))
            );

            // Body Section
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
            if (_environments.Count > 0) SelectEnvironment(_environments[0]);

            return root;
        }

        private void RefreshList()
        {
            if (_leftListPanel == null) return;
            _leftListPanel.Clear();

            // Unified Forge Filter
            var filterUI = new ForgeFilterWindowBuilder(this)
                .WithTitle("Atmospheric Registry")
                .WithAccentColor(new Color(0.2f, 0.8f, 0.8f))
                .CreateGui(new GuiContext());

            filterUI.style.flexGrow = 1;
            _leftListPanel.Add(filterUI);

            _leftListPanel.Add(new ForgeButtonBuilder("➕ Log New Profile")
                .WithBackgroundColor(new Color(0.1f, 0.4f, 0.4f))
                .WithMarginTop(10)
                .OnClick(() => {
                    var newEnv = new GURPSEnvironment { Name = $"Sector {_environments.Count + 1}" };
                    _environments.Add(newEnv);
                    SaveToCache();
                    SelectEnvironment(newEnv);
                }).Build());
        }

        private void SelectEnvironment(GURPSEnvironment env)
        {
            _selectedEnvironment = env;
            RefreshList();

            if (_rightEditorPanel == null) return;
            _rightEditorPanel.Clear();

            var editor = new ForgeContainerBuilder("EnvironmentEditor")
                .AddChild(new ForgeLabelBuilder("Climate Profile Configuration").WithFontSize(18).WithFontStyle(FontStyle.Bold).WithMarginBottom(15))
                .AddChild(new ForgeTextFieldBuilder("Profile Designation", env.Name)
                    .OnValueChanged(v => { env.Name = v.newValue; SaveToCache(); RefreshList(); }))

                .AddChild(new ForgeContainerBuilder("PhysicsGroup")
                    .WithMarginTop(10).WithPadding(10).WithBackgroundColor(new Color(0.12f, 0.12f, 0.12f))
                    .AddChild(new ForgeTextFieldBuilder("Gravity (G)", env.Gravity.ToString("F2"))
                        .OnValueChanged(v => { if (float.TryParse(v.newValue, out float r)) env.Gravity = r; SaveToCache(); }))
                    .AddChild(new ForgeTextFieldBuilder("Atmospheric Comp.", env.Atmosphere)
                        .OnValueChanged(v => { env.Atmosphere = v.newValue; SaveToCache(); }))
                    .AddChild(new ForgeTextFieldBuilder("Pressure (Atm)", env.Pressure.ToString("F2"))
                        .OnValueChanged(v => { if (float.TryParse(v.newValue, out float r)) env.Pressure = r; SaveToCache(); }))
                    .AddChild(new ForgeTextFieldBuilder("Temp (Base F)", env.TemperatureBase.ToString())
                        .OnValueChanged(v => { if (int.TryParse(v.newValue, out int r)) env.TemperatureBase = r; SaveToCache(); }))
                    .AddChild(new ForgeTextFieldBuilder("Radiation (Rads/h)", env.RadiationLevel.ToString())
                        .OnValueChanged(v => { if (int.TryParse(v.newValue, out int r)) env.RadiationLevel = r; SaveToCache(); }))
                );

            _rightEditorPanel.Add(editor.Build());
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var snapshotRoot = CreateGui(new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? "EnvironmentAPI_Snapshot" : System.IO.Path.GetFileNameWithoutExtension(assetPath);
            WorkshopUxmlBaker.Bake(snapshotRoot, fileName);
        }

        public void FromUIDocument(string assetPath)
        {
            Debug.LogWarning("[EnvironmentAPI] FromUIDocument is not supported for this dynamically generated matrix.");
        }

        public void Bind(DataWarehouse warehouse)
        {
            
        }

        [Serializable]
        private class CacheWrapper { public List<GURPSEnvironment> Environments = new List<GURPSEnvironment>(); }
    }
}