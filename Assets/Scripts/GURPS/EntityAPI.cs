using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    [Serializable]
    public class EntityManifest
    {
        public List<GURPSNPCTemplate> Templates = new List<GURPSNPCTemplate>();
    }

    public class EntityAPI : IGurpsApiProvider, IGuiProvider, IFilterWindowProvider
    {
        public string Title => "ENTITY & POPULATION MATRIX";
        public string ModuleName => "Entity Engine";
        public Guid Id { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000001019");

        private List<GURPSNPCTemplate> _templates = new List<GURPSNPCTemplate>();
        private DataWarehouse _warehouse;
        private const string CacheKey = "GURPS_ENTITY_DATABASE";

        public EntityAPI()
        {
            _warehouse = new DataWarehouse();
            Initialize();
        }

        public EntityAPI(DataWarehouse warehouse)
        {
            Bind(warehouse);
        }

        public void Bind(DataWarehouse warehouse)
        {
            _warehouse = warehouse;
            Initialize();
            Debug.Log($"[DURPS] EntityAPI Bound. Memory Shovel active.");
        }

        public void Initialize()
        {
            LoadFromCache();
            if (_templates.Count == 0) SeedStandardNPCs();
        }

        public void SaveToCache()
        {
            if (_warehouse == null) return;
            _warehouse.StoreTemporary(CacheKey, JsonUtility.ToJson(new EntityManifest { Templates = _templates }));
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                var manifest = JsonUtility.FromJson<EntityManifest>(json);
                _templates = manifest?.Templates ?? new List<GURPSNPCTemplate>();
            }
        }

        private void SeedStandardNPCs()
        {
            _templates.Add(new GURPSNPCTemplate("Default Citizen", 25));
            _templates.Add(new GURPSNPCTemplate("Elite Guard", 100) { ST = 13, DX = 12, HT = 12 });
            _templates.Add(new GURPSNPCTemplate("Master Sage", 150) { IQ = 15 });
            SaveToCache();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder(Title)
                .WithPadding(20)
                .AddHeader("POPULATION BLUEPRINTS", Color.cyan)
                .Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void CreateComponentTree(List<ForgeFilterWindow.Element> tree) { }
        public bool GoToChild(ForgeFilterWindow.Element element, bool add) => false;
        public void ToUIDocument(string p) { }
        public void FromUIDocument(string p) { }
    }
}