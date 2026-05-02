using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.GURPS
{
    [Serializable]
    public class SocietalManifest { public List<GURPSFaction> Factions = new List<GURPSFaction>(); }

    public class SocietalAPI : IGuiProvider, IGurpsApiProvider
    {
        public string Title => "SOCIETAL MATRIX";
        public string ModuleName => "Societal Engine";
        public Guid Id { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000001018");

        private VisualElement _detailPanel;
        private List<GURPSFaction> _factions = new List<GURPSFaction>();
        private DataWarehouse _warehouse;
        private const string CacheKey = "GURPS_SOCIETAL_DB";

        public SocietalAPI()
        {
            _warehouse = new DataWarehouse();
            Initialize();
        }

        public SocietalAPI(DataWarehouse warehouse)
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
            if (_factions.Count == 0) SeedStandardFactions();
        }

        private void SeedStandardFactions()
        {
            _factions.Add(new GURPSFaction { Name = "Sovereign Union", TechLevel = 10, ControlRating = 4 });
            _factions.Add(new GURPSFaction { Name = "Free-Trade Syndicate", TechLevel = 8, ControlRating = 1 });
            SaveToCache();
        }

        public void SaveToCache()
        {
            if (_warehouse == null) return;
            string json = JsonUtility.ToJson(new SocietalManifest { Factions = _factions });
            _warehouse.StoreTemporary(CacheKey, json);
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                var manifest = JsonUtility.FromJson<SocietalManifest>(json);
                _factions = manifest?.Factions ?? new List<GURPSFaction>();
            }
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("Societal_Root")
                .WithDirection(FlexDirection.Row)
                .WithFlexGrow(1f)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.05f));

            var listPane = new ForgeContainerBuilder("Nav")
                .WithWidth(new StyleLength(Length.Percent(30f)))
                .WithPadding(10f)
                .WithBorderWidth(0, 1f, 0, 0)
                .WithBorderColor(new Color(0.3f, 0.3f, 0.4f))
                .AddChild(new ForgeLabelBuilder("SYSTEM FACTIONS").WithBold().WithColor(Color.cyan));

            listPane.AddChild(new DynamicGuiProvider(c => {
                var scroll = new ScrollView();
                foreach (var f in _factions)
                {
                    var captured = f;
                    scroll.Add(new ForgeButtonBuilder(f.Name)
                        .OnClick(() => ManifestInspector(captured))
                        .Build());
                }
                return scroll;
            }));

            var inspectorPane = new ForgeContainerBuilder("Inspector")
                .WithFlexGrow(1f)
                .WithPadding(20f)
                .OnBuild(ve => _detailPanel = ve);

            rootBuilder.AddChild(listPane).AddChild(inspectorPane);

            if (_factions.Count > 0) ManifestInspector(_factions[0]);

            return rootBuilder.Build();
        }

        private void ManifestInspector(GURPSFaction faction)
        {
            if (_detailPanel == null) return;
            _detailPanel.Clear();

            var content = new ForgeContainerBuilder("Content")
                .AddChild(new ForgeLabelBuilder(faction.Name.ToUpper())
                    .WithFontSize(26).WithBold().WithColor(Color.cyan))
                .AddChild(new ForgeLabelBuilder($"{faction.Jurisdiction} // {faction.BaseWealth}")
                    .WithColor(Color.gray).WithMarginBottom(15f));

            var vectorGrid = new ForgeContainerBuilder("Vectors")
                .WithDirection(FlexDirection.Row)
                .AddChild(CreateVector("TL", $"LEVEL {faction.TechLevel}", Color.green))
                .AddChild(CreateVector("CR", $"RATING {faction.ControlRating}", Color.white))
                .AddChild(CreateVector("RECOG", $"{faction.RecognitionScore}/18", Color.yellow));

            content.AddChild(vectorGrid);
            _detailPanel.Add(content.Build());
        }

        private IGuiProvider CreateVector(string label, string val, Color c) =>
            new ForgeContainerBuilder(label)
                .WithPadding(12f).WithMarginRight(10f)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.18f))
                .AddChild(new ForgeLabelBuilder(label).WithFontSize(9).WithColor(Color.gray))
                .AddChild(new ForgeLabelBuilder(val).WithFontSize(14).WithBold().WithColor(c));

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}