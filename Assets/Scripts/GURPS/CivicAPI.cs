using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    [Serializable]
    public class CivicManifest { public List<GURPSSettlement> Settlements = new List<GURPSSettlement>(); }

    public class CivicAPI : IGurpsApiProvider, IGuiProvider
    {
        public string ModuleName => "Civic Infrastructure";
        public string Title => "URBAN ARCHITECT";
        public Guid Id { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000001012");

        private List<GURPSSettlement> _settlements = new List<GURPSSettlement>();
        private DataWarehouse _warehouse;
        private const string CacheKey = "GURPS_CIVIC_DB";

        public CivicAPI()
        {
            _warehouse = new DataWarehouse();
            Initialize();
        }

        public CivicAPI(DataWarehouse warehouse)
        {
            Bind(warehouse);
        }

        public void Bind(DataWarehouse warehouse)
        {
            _warehouse = warehouse;
            Initialize();
        }

        public void Initialize()
        {
            LoadFromCache();
            if (_settlements.Count == 0) SeedStandardSettlements();
        }

        private void SeedStandardSettlements()
        {
            _settlements.Add(new GURPSSettlement("Oakhaven", 3, 3));
            _settlements.Add(new GURPSSettlement("Iron-City Prime", 8, 5));
            SaveToCache();
        }

        public void SaveToCache()
        {
            if (_warehouse == null) return;
            string json = JsonUtility.ToJson(new CivicManifest { Settlements = _settlements });
            _warehouse.StoreTemporary(CacheKey, json);
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                var manifest = JsonUtility.FromJson<CivicManifest>(json);
                _settlements = manifest?.Settlements ?? new List<GURPSSettlement>();
            }
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var gui = new GraphicalUserInterfaceBuilder(Title)
                .WithPadding(15)
                .AddHeader("CHARTERED SETTLEMENTS", new Color(0.4f, 0.9f, 0.7f));

            foreach (var settlement in _settlements)
            {
                gui.AddChild(new ForgeLabelBuilder($"{settlement.Name} (TL{settlement.TechLevel})")
                    .WithColor(Color.white));
            }

            return gui.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string p) { }
        public void FromUIDocument(string p) { }
    }
}