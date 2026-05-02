using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.GURPS
{
    public class TechnologyLevelApi : IGurpsApiProvider, IGuiProvider
    {
        public int Id => 1004;
        public ProviderType ProviderType => ProviderType.GurpsApi;
        public string ModuleName => "TechLevels & Progress";
        public string Title => "TECH REGISTRY";

        private List<TechnologyLevel> _techLevels = new List<TechnologyLevel>();
        private readonly DataWarehouse _warehouse;
        private string CachePath => Application.persistentDataPath + "/DGURPS_TechLevels.json";

        // Interface Properties
        public Vector2 position { get; set; }
        private Guid _guid = Guid.NewGuid();
        Guid IGurpsApiProvider.Id { get => _guid; set => _guid = value; }

        public TechnologyLevelApi(DataWarehouse warehouse)
        {
            _warehouse = warehouse;
        }

        public TechnologyLevelApi()
        {
            _warehouse = new DataWarehouse();
        }

        public void Initialize() => LoadFromCache();

        // --- CORE API METHODS ---

        public List<TechnologyLevel> GetAll() =>
            _techLevels.OrderBy(tl => tl.Level).ThenBy(tl => tl.DivergentOffset).ToList();

        public TechnologyLevel GetByLevel(int level, int divergent = 0) =>
            _techLevels.FirstOrDefault(tl => tl.Level == level && tl.DivergentOffset == divergent);

        // --- FORGE GUI (Kill the Stubs) ---

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("Tech_Root")
                .WithDirection(FlexDirection.Row)
                .WithFlexGrow(1f)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.04f));

            // Navigation Pane (Registry)
            var listPane = new ForgeContainerBuilder("Nav")
                .WithWidth(new StyleLength(Length.Percent(30f)))
                .WithPadding(10f)
                .WithBorderWidth(0, 1f, 0, 0)
                .WithBorderColor(new Color(0.2f, 0.5f, 0.2f, 0.5f));

            listPane.AddChild(new DynamicGuiProvider(c => {
                var scroll = new ScrollView();
                foreach (var tl in GetAll())
                {
                    var captured = tl;
                    scroll.Add(new ForgeButtonBuilder(tl.GetFormattedTL())
                        .WithMarginBottom(2f)
                        .OnClick(() => { /* Logic to refresh a detail panel */ }).Build());
                }
                return scroll;
            }));

            rootBuilder.AddChild(listPane);
            return rootBuilder.Build();
        }

        // --- PERSISTENCE ---

        public void SaveToCache()
        {
            try
            {
                string json = JsonUtility.ToJson(new CacheWrapper { Levels = _techLevels }, true);
                File.WriteAllText(CachePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DGURPS] TechLevel Save Error: {e.Message}");
            }
        }

        public void LoadFromCache()
        {
            if (File.Exists(CachePath))
            {
                try
                {
                    var cache = JsonUtility.FromJson<CacheWrapper>(File.ReadAllText(CachePath));
                    _techLevels = cache?.Levels ?? new List<TechnologyLevel>();
                    if (_techLevels.Count > 0) return;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DGURPS] TechLevel Load Error: {e.Message}");
                }
            }
            SeedStandardTechLevels();
        }

        private void SeedStandardTechLevels()
        {
            _techLevels.Clear();
            _techLevels.Add(new TechnologyLevel(0, "Stone Age", 0.05f));
            _techLevels.Add(new TechnologyLevel(8, "Digital Age", 1.5f));
            SaveToCache();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "TechLevel_Snapshot");
        public void FromUIDocument(string path) { }

        public void Bind(DataWarehouse warehouse)
        {
            throw new NotImplementedException();
        }

        internal GURPSTechDefinition GetByTL(int tl)
        {
            throw new NotImplementedException();
        }

        internal void Register(TechnologyLevel tl)
        {
            throw new NotImplementedException();
        }

        internal void UnRegister(TechnologyLevel tl)
        {
            throw new NotImplementedException();
        }

        [Serializable]
        private class CacheWrapper { public List<TechnologyLevel> Levels = new List<TechnologyLevel>(); }
    }
}