using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Math;
using Workshop.Core.Memory;
using Workshop.GURPS;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    public enum SettlementScale
    {
        Outpost,        // 10s
        Hamlet,         // 100s
        Village,        // 1,000s
        Town,           // 10,000s
        City,           // 100,000s
        Metropolis,     // 1,000,000s
        Megalopolis,    // 10,000,000s - 100,000,000s
        Arcology,       // 1,000,000,000s (Dense hyper-structure)
        Ecumenopolis    // 1,000,000,000,000s+ (Planet-wide city)
    }

    public class PopulationCenterData
    {
        public string Id = Guid.NewGuid().ToString();
        public string Name = "New Settlement";
        public SettlementScale Scale = SettlementScale.Village;
        public AstroInt Population = new AstroInt(0, 0, 0, 500);
        public string PrimaryFocus = "Balanced"; // e.g., Industrial, Agricultural, Research
        public Color FactionColor = Color.white;
        public Vector2 PlanetaryCoordinates = Vector2.zero; // Lat / Lon
        public bool IsCapital = false;

        public SettlementEnvironmentData EnvData { get;  set; }
    }
    public enum SettlementAdaptation
    {
        Native,             // Breathes the air, walks the ground (e.g., Earth cities)
        SealedDome,         // Contained atmosphere on hostile surface (e.g., Mars colony)
        Subterranean,       // Dug into the crust for shielding/heat (e.g., Asteroid bases)
        AtmosphericFloat,   // Cloud cities (e.g., Bespin)
        OrbitalTether,      // Space elevator terminus, straddling space and atmosphere
        Aquatic,             // Underwater or floating on hostile oceans
        Terrestrial
    }

    public class SettlementEnvironmentData
    {
        public int TechLevel = 8; // GURPS standard TL scale (0 to 12+)
        public SettlementAdaptation Adaptation = SettlementAdaptation.Native;
        public float EcologicalFootprint = 25f; // 0 (Symbiotic) to 100 (Toxic Wasteland)
        public float TerraformingProgress = 0f; // 0 (Hostile) to 100 (Paradise)

        // Inherited/Read-only from the planetary chunk it sits on
        public string LocalBiomeName = "Temperate Plains";
        public float LocalGravityG = 1.0f;
    }

    public class GameMastersCompanion_Gui_PopulationForge : IGuiProvider
    {
        [Serializable]
        private class SettlementCacheWrapper
        {
            public List<PopulationCenterData> Items = new List<PopulationCenterData>();
        }
        public string Title => "GM COMPANION: POPULATION FORGE";
        private GuiContext _lastCtx;
        private string _processGroup;
        private const string CACHE_KEY = "CACHE_POPULATION_FORGE_DATA";

        // The master list of all settlements on the current world
        private List<PopulationCenterData> _settlements = new List<PopulationCenterData>();

        private VisualElement _previewContainer;
        private VisualElement _crudArea;

        // ==========================================
        // 1. SETUP PHASE (Cache Retrieval / Default Generation)
        // ==========================================
        private void Setup(GuiContext ctx)
        {
            _lastCtx = ctx;
            _processGroup = "PopForge_" + Guid.NewGuid().ToString().Substring(0, 6);

            // Reach out to other editor's data via DataWarehouse
            if (ctx.TryGetService<DataWarehouse>(out var warehouse) &&
                warehouse.TryRetrieveTemporary(CACHE_KEY, out string json))
            {
                // Restore from Cache
                var wrapper = JsonUtility.FromJson<SettlementCacheWrapper>(json);
                _settlements = wrapper != null ? wrapper.Items : new List<PopulationCenterData>();
                Debug.Log($"[Population Forge] Setup: Restored {_settlements.Count} settlements from cache.");
            }
            else
            {
                // No cache found. Build default scenario for them to CRUD on.
                _settlements = new List<PopulationCenterData>();
                GenerateRandomNetwork();
                Debug.Log("[Population Forge] Setup: Built default settlement network.");
            }
        }

        // ==========================================
        // 2. EXECUTE PHASE (GUI Construction)
        // ==========================================
        public VisualElement CreateGui(GuiContext ctx)
        {
            Setup(ctx); // Initialize State

            var splitPanel = new ForgeSplitPanelBuilder(sidebarWidth: 450);

            splitPanel.WithSidebar(new GraphicalUserInterfaceBuilder("Population_Sidebar")
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .AddChild(new GraphicalUserInterfaceBuilder("ForgeControls")
                    .WithPadding(15).WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                    .WithBorderBottomWidth(2).WithBorderBottomColor(new Color(0.8f, 0.6f, 0.2f))
                    .AddChild(new Label("CIVILIZATION REGISTRY") { style = { color = new Color(1f, 0.8f, 0.4f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } })
                    .AddButton("🌍 AUTO-GENERATE NETWORK", () => GenerateRandomNetwork())
                    .Build())
                .AddChild(new GraphicalUserInterfaceBuilder("DataHub")
                    .WithPadding(15).WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                    .OnBuild(ve => {
                        ve.style.flexGrow = 1;
                        _crudArea = new VisualElement { style = { flexGrow = 1 } };
                        _crudArea.Add(BuildSettlementCRUD());
                        ve.Add(_crudArea);
                    })
                    .Build())
            );

            splitPanel.WithMain(new GraphicalUserInterfaceBuilder("MapViewport")
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f))
                .AddChild(new Label("World Map / Topology Preview goes here...") { style = { color = Color.gray, unityTextAlign = TextAnchor.MiddleCenter, flexGrow = 1 } })
            );

            VisualElement root = splitPanel.CreateGui(ctx);

            // Bind the Takedown lifecycle to the root's detachment!
            root.RegisterCallback<DetachFromPanelEvent>(evt => Takedown());

            return root;
        }

        // ==========================================
        // 3. TAKEDOWN PHASE (Cleanup & Caching)
        // ==========================================
        private void Takedown()
        {
            // Save the exact user state back to the warehouse
            if (_lastCtx != null && _lastCtx.TryGetService<DataWarehouse>(out var warehouse))
            {
                var wrapper = new SettlementCacheWrapper { Items = _settlements };
                warehouse.StoreTemporary(CACHE_KEY, JsonUtility.ToJson(wrapper));
                Debug.Log($"[Population Forge] Takedown: Cached {_settlements.Count} settlements securely.");
            }

            Dispose(); // Fire the standard interface cleanup
        }

        public void Dispose()
        {
            // Null out heavy UI references to help the Garbage Collector
            if (_crudArea != null) _crudArea.Clear();
            _crudArea = null;
            _previewContainer = null;

            // If you bind any FSMs to this panel in the future, Destroy/Unregister them here using _processGroup!
            // FSM_API.Interaction.DestroyInstancesInGroup(_processGroup);
        }

        // --- SUB-BUILDERS (Identical to previous logic) ---

        private VisualElement BuildSettlementCRUD()
        {
            var crud = new CRUD_Builder<PopulationCenterData>("SETTLEMENTS", () => _settlements,
                s => $"{(s.IsCapital ? "👑 " : "⌂ ")}{s.Name}", BuildSettlementEditorForm,
                s => { if (!_settlements.Contains(s)) _settlements.Add(s); },
                s => { _settlements.Remove(s); }
            );
            crud.Style.ListWidth = 140f;
            crud.Style.AccentColor = new Color(0.8f, 0.6f, 0.2f);
            return crud.CreateGui(_lastCtx);
        }

        private VisualElement BuildSettlementEditorForm(PopulationCenterData center)
        {
            return new GraphicalUserInterfaceBuilder("SettlementForm")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .AddStringData("Name", center.Name, v => center.Name = v)
                .AddToggleData("Planetary Capital", center.IsCapital, v => center.IsCapital = v)
                .OnBuild(ve => ve.Add(new VisualElement { style = { height = 8, backgroundColor = center.FactionColor, marginTop = 5, marginBottom = 15, borderTopLeftRadius = 4, borderTopRightRadius = 4, borderBottomLeftRadius = 4, borderBottomRightRadius = 4 } }))
                .AddChild(new Label("DEMOGRAPHICS") { style = { color = new Color(0.5f, 0.8f, 1f), marginTop = 10, marginBottom = 5, unityFontStyleAndWeight = FontStyle.Bold } })
                .OnBuild(ve => {
                    var scaleField = new EnumField("Scale", center.Scale);
                    scaleField.RegisterValueChangedCallback(evt => {
                        center.Scale = (SettlementScale)evt.newValue;
                        center.Population = GetBaselinePopulation(center.Scale);
                        RefreshCRUD();
                    });
                    ve.Add(scaleField);

                    // AstroInt Provider Injection
                    var astroProvider = new AstroIntProvider("TOTAL POPULATION", center.Population, newPop => center.Population = newPop);
                    ve.Add(astroProvider.CreateGui(_lastCtx));
                })
                .AddChild(new Label("INFRASTRUCTURE") { style = { color = new Color(0.5f, 0.8f, 1f), marginTop = 20, marginBottom = 5, unityFontStyleAndWeight = FontStyle.Bold } })
                .AddStringData("Primary Focus", center.PrimaryFocus, v => center.PrimaryFocus = v)
                .Build();
        }

        private AstroInt GetBaselinePopulation(SettlementScale scale)
        {
            switch (scale)
            {
                case SettlementScale.Outpost: return new AstroInt(0, 0, 0, (uint)UnityEngine.Random.Range(10, 50));
                case SettlementScale.Hamlet: return new AstroInt(0, 0, 0, (uint)UnityEngine.Random.Range(50, 200));
                case SettlementScale.Village: return new AstroInt(0, 0, 0, (uint)UnityEngine.Random.Range(200, 2000));
                case SettlementScale.Town: return new AstroInt(0, 0, 0, (uint)UnityEngine.Random.Range(2000, 25000));
                case SettlementScale.City: return new AstroInt(0, 0, 0, (uint)UnityEngine.Random.Range(25000, 500000));
                case SettlementScale.Metropolis: return new AstroInt(0, 0, 0, (uint)UnityEngine.Random.Range(1000000, 8000000));
                case SettlementScale.Megalopolis: return new AstroInt(0, 0, 0, (uint)UnityEngine.Random.Range(15000000, 80000000));
                case SettlementScale.Arcology: return new AstroInt(0, 0, 0, (uint)UnityEngine.Random.Range(200000000, 900000000));
                case SettlementScale.Ecumenopolis: return new AstroInt(0, 0, (uint)UnityEngine.Random.Range(1000, 9000), 0);
                default: return new AstroInt(0, 0, 0, 0);
            }
        }

        private void GenerateRandomNetwork()
        {
            _settlements.Clear();
            _settlements.Add(new PopulationCenterData { Name = "Prime Capitol", Scale = SettlementScale.Metropolis, Population = GetBaselinePopulation(SettlementScale.Metropolis), IsCapital = true, FactionColor = Color.cyan });
            for (int i = 0; i < 3; i++) _settlements.Add(new PopulationCenterData { Name = $"City Alpha-{i}", Scale = SettlementScale.City, Population = GetBaselinePopulation(SettlementScale.City), FactionColor = Color.cyan });
            for (int i = 0; i < 8; i++) _settlements.Add(new PopulationCenterData { Name = $"Sector {i} Settlement", Scale = SettlementScale.Town, Population = GetBaselinePopulation(SettlementScale.Town), FactionColor = Color.white });
            RefreshCRUD();
        }

        private void RefreshCRUD()
        {
            if (_crudArea != null) { _crudArea.Clear(); _crudArea.Add(BuildSettlementCRUD()); }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}