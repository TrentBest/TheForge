using TheSingularityWorkshop.Armada2525;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Armada2525_Gui_ShipYardView : IGuiProvider
{
    public string Title => "ORBITAL SHIPYARD COMMAND";

    private Colony _colony;
    private Armada2525 _game;
    private GuiContext _lastCtx;

    private VisualElement _mainContentArea;
    private enum ShipyardTab { Management, Docks, Logistics, Workforce }
    private ShipyardTab _currentTab = ShipyardTab.Management;

    // --- MOCK SIMULATION DATA ---
    private string _directorName = "Director Vance (AI Assigned)";
    private int _shipyardTier = 1; // Tier 1 = 4 Hardpoints, Tier 2 = 8, Tier 3 = 12
    private long _treasury = 250000;

    // The Unified Hardpoint System
    public enum ModuleType { Empty, Dock, Warehouse, Habitat, Manufactory }

    private class Hardpoint
    {
        public int SlotId;
        public ModuleType Type;
        public int Level; // 1 to ShipyardTier
        public string Name;
        public string Status; // Idle, Building, Full, etc.
        public Color ThemeColor;
    }

    private List<Hardpoint> _hardpoints;
    private Dictionary<string, int> _storedParts = new Dictionary<string, int> { { "Hull Plates", 4500 }, { "Drive Coils", 1200 } };
    private float _shiftRotation = 1f;

    public void Initialize(Colony colony)
    {
        _colony = colony;
        InitializeMockData();
    }

    public VisualElement CreateGui(GuiContext ctx)
    {
        _lastCtx = ctx;
        _game = UnityEngine.Object.FindAnyObjectByType<Armada2525>();
        if (_hardpoints == null) InitializeMockData();

        var builder = new GraphicalUserInterfaceBuilder("Shipyard_Root")
            .WithBackgroundColor(new Color(0.01f, 0.03f, 0.05f, 0.98f))
            .WithPercentSize(100, 100)
            .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

        // --- TOP BAR ---
        builder.AddChild(c => {
            var topBar = new VisualElement { style = { height = 80, flexDirection = FlexDirection.Row, backgroundColor = new Color(0.05f, 0.07f, 0.1f), borderBottomWidth = 2, borderBottomColor = Color.cyan, paddingLeft = 20, paddingRight = 20, alignItems = Align.Center } };

            var titleBox = new VisualElement { style = { flexGrow = 1 } };
            titleBox.Add(new Label($"ORBITAL SHIPYARD // {_colony?.Name ?? "SIRIUS PRIME"}") { style = { color = Color.cyan, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold } });
            titleBox.Add(new Label($"Director: {_directorName} | Base Tier: {_shipyardTier} ({_hardpoints.Count} Hardpoints)") { style = { color = Color.gray, fontSize = 12 } });
            topBar.Add(titleBox);

            topBar.Add(new Label($"TREASURY: {_treasury:N0} cr") { style = { color = Color.yellow, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, marginRight = 20 } });

            var exitBtn = new Button(() => _game?.SwitchGui("PlanetView")) { text = "EXIT TO ORBIT", style = { height = 40, width = 120, backgroundColor = new Color(0.4f, 0.1f, 0.1f), color = Color.white } };
            topBar.Add(exitBtn);

            return topBar;
        });

        // --- MAIN WORKSPACE ---
        builder.AddChild(c => {
            var workspace = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row } };

            // LEFT SIDEBAR
            var sidebar = new VisualElement { style = { width = 250, backgroundColor = new Color(0.03f, 0.05f, 0.08f), borderRightWidth = 2, borderRightColor = Color.cyan, paddingTop = 20 } };
            sidebar.Add(new Label("FACILITY TERMINALS") { style = { color = Color.gray, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, marginLeft = 15, marginBottom = 10 } });

            sidebar.Add(CreateTabButton("OVERALL MANAGEMENT", ShipyardTab.Management));
            sidebar.Add(CreateTabButton("DRYDOCKS & ASSEMBLY", ShipyardTab.Docks));
            sidebar.Add(CreateTabButton("LOGISTICS & STORAGE", ShipyardTab.Logistics));
            sidebar.Add(CreateTabButton("WORKFORCE & HABITATS", ShipyardTab.Workforce));

            workspace.Add(sidebar);

            // RIGHT CONTENT AREA
            _mainContentArea = new VisualElement { style = { flexGrow = 1, paddingLeft = 30, paddingTop = 30, paddingRight = 30, paddingBottom = 30 } };
            RenderCurrentTab();
            workspace.Add(_mainContentArea);

            return workspace;
        });

        return builder.Build();
    }

    private void RenderCurrentTab()
    {
        if (_mainContentArea == null) return;
        _mainContentArea.Clear();

        switch (_currentTab)
        {
            case ShipyardTab.Management: RenderManagementTab(); break;
            case ShipyardTab.Docks: RenderDocksTab(); break;
            case ShipyardTab.Logistics: RenderLogisticsTab(); break;
            case ShipyardTab.Workforce: RenderWorkforceTab(); break;
        }
    }

    // --- PILLAR 0: OVERALL MANAGEMENT (THE NEW CORE) ---
    private void RenderManagementTab()
    {
        _mainContentArea.Add(new Label("FACILITY ALLOCATION & EXPANSION") { style = { color = Color.white, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
        _mainContentArea.Add(new Label("Allocate the station's structural hardpoints. Balance construction docks, parts storage, crew habitats, and manufactories. Space is highly restricted.") { style = { color = Color.gray, fontSize = 12, marginBottom = 20 } });

        var layout = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };

        // LEFT: Hardpoint Grid
        var gridPane = new VisualElement { style = { flexGrow = 1, paddingRight = 20 } };
        var grid = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };

        foreach (var hp in _hardpoints)
        {
            var slot = new VisualElement
            {
                style = {
                width = 150, height = 150, backgroundColor = hp.Type == ModuleType.Empty ? new Color(0.05f, 0.05f, 0.05f) : new Color(0.1f, 0.1f, 0.15f),
                marginRight = 10, marginBottom = 10, paddingLeft=10, paddingTop=10, paddingRight=10, paddingBottom=10,
                borderTopWidth=2, borderBottomWidth=2, borderLeftWidth=2, borderRightWidth=2, borderTopColor=hp.ThemeColor, borderBottomColor=hp.ThemeColor, borderLeftColor=hp.ThemeColor, borderRightColor=hp.ThemeColor
            }
            };

            slot.Add(new Label($"HARDPOINT 0{hp.SlotId}") { style = { color = Color.gray, fontSize = 10, marginBottom = 5 } });

            if (hp.Type == ModuleType.Empty)
            {
                slot.Add(new Label("VACANT") { style = { color = Color.gray, flexGrow = 1, unityTextAlign = TextAnchor.MiddleCenter } });

                // Build Options Dropdown Simulation
                slot.Add(new Button(() => InstallModule(hp, ModuleType.Dock)) { text = "Build DOCK", style = { fontSize = 9, height = 20, backgroundColor = new Color(0, 0.3f, 0.4f) } });
                slot.Add(new Button(() => InstallModule(hp, ModuleType.Warehouse)) { text = "Build STORAGE", style = { fontSize = 9, height = 20, backgroundColor = new Color(0.4f, 0.4f, 0) } });
                slot.Add(new Button(() => InstallModule(hp, ModuleType.Habitat)) { text = "Build HABITAT", style = { fontSize = 9, height = 20, backgroundColor = new Color(0, 0.4f, 0) } });
                slot.Add(new Button(() => InstallModule(hp, ModuleType.Manufactory)) { text = "Build FACTORY", style = { fontSize = 9, height = 20, backgroundColor = new Color(0.4f, 0.2f, 0.6f) } });
            }
            else
            {
                slot.Add(new Label(hp.Name) { style = { color = hp.ThemeColor, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, whiteSpace = WhiteSpace.Normal, marginBottom = 5 } });
                slot.Add(new Label($"Tier {hp.Level} {hp.Type}") { style = { color = Color.white, fontSize = 10, marginBottom = 5 } });
                slot.Add(new Label(hp.Status) { style = { color = Color.gray, fontSize = 9, whiteSpace = WhiteSpace.Normal } });

                var spacer = new VisualElement { style = { flexGrow = 1 } };
                slot.Add(spacer);

                var actions = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };
                if (hp.Level < _shipyardTier) actions.Add(new Button(() => Debug.Log("Upgrading Module")) { text = "UPGRADE", style = { fontSize = 9, height = 20, flexGrow = 1, marginRight = 2, backgroundColor = new Color(0.2f, 0.4f, 0.2f) } });
                actions.Add(new Button(() => DemolishModule(hp)) { text = "SCRAP", style = { fontSize = 9, height = 20, flexGrow = 1, backgroundColor = new Color(0.4f, 0.1f, 0.1f) } });
                slot.Add(actions);
            }
            grid.Add(slot);
        }
        gridPane.Add(grid);
        layout.Add(gridPane);

        // RIGHT: Shipyard Expansion (Retrofit vs Build)
        var expansionPane = new VisualElement { style = { width = 300, backgroundColor = new Color(0.05f, 0.05f, 0.08f), paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15, borderLeftWidth = 2, borderLeftColor = Color.cyan } };
        expansionPane.Add(new Label("STATION EXPANSION") { style = { color = Color.white, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } });

        int nextTier = _shipyardTier + 1;
        if (_shipyardTier < 4)
        {
            expansionPane.Add(new Label($"Current Tier: {_shipyardTier}") { style = { color = Color.cyan, marginBottom = 5 } });
            expansionPane.Add(new Label($"Hardpoints: {_hardpoints.Count} / {nextTier * 4}") { style = { color = Color.white, marginBottom = 15 } });

            var retroBox = new VisualElement { style = { backgroundColor = new Color(0.2f, 0.1f, 0.1f), paddingLeft = 10, paddingTop = 10, paddingBottom = 10, paddingRight = 10, borderLeftWidth = 3, borderLeftColor = Color.red, marginBottom = 20 } };
            retroBox.Add(new Label("RETROFIT WARNING") { style = { color = Color.red, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 10, marginBottom = 5 } });
            retroBox.Add(new Label("Upgrading an existing shipyard adds structural stress. It is 30% more expensive than constructing a new Shipyard of the target tier from scratch.") { style = { color = Color.gray, fontSize = 9, whiteSpace = WhiteSpace.Normal } });
            expansionPane.Add(retroBox);

            long upgradeCost = nextTier * 50000;
            expansionPane.Add(new Button(() => UpgradeShipyardTier()) { text = $"EXPAND TO TIER {nextTier}\n{upgradeCost:N0} cr", style = { height = 50, backgroundColor = new Color(0.1f, 0.3f, 0.4f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });
        }
        else
        {
            expansionPane.Add(new Label("MAXIMUM SUPER-STRUCTURE REACHED") { style = { color = Color.green, unityFontStyleAndWeight = FontStyle.Bold } });
        }

        layout.Add(expansionPane);
        _mainContentArea.Add(layout);
    }

    // --- PILLAR 1: DOCKS (Filtered for Docks & Manufactories) ---
    private void RenderDocksTab()
    {
        _mainContentArea.Add(new Label("DRYDOCKS & MANUFACTORIES") { style = { color = Color.white, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });

        var activeDocks = _hardpoints.Where(h => h.Type == ModuleType.Dock || h.Type == ModuleType.Manufactory).ToList();

        if (activeDocks.Count == 0)
        {
            _mainContentArea.Add(new Label("NO ASSEMBLY DOCKS INSTALLED. Allocate hardpoints in Overall Management.") { style = { color = Color.red } });
            return;
        }

        var scroll = new ScrollView { style = { flexGrow = 1 } };
        foreach (var hp in activeDocks)
        {
            var card = new VisualElement { style = { flexDirection = FlexDirection.Row, backgroundColor = new Color(0.1f, 0.1f, 0.15f), marginBottom = 10, paddingLeft = 15, paddingTop = 15, paddingBottom = 15, borderLeftWidth = 4, borderLeftColor = hp.ThemeColor } };
            card.Add(new Label($"[0{hp.SlotId}] {hp.Name} (Tier {hp.Level}) - {hp.Status}") { style = { color = Color.white, flexGrow = 1 } });

            if (hp.Type == ModuleType.Manufactory)
                card.Add(new Button(() => Debug.Log("Open Production Manifest")) { text = "SET PRODUCTION", style = { height = 30 } });
            else
                card.Add(new Button(() => Debug.Log("Open Dock Orders")) { text = "MANAGE ORDERS", style = { height = 30 } });

            scroll.Add(card);
        }
        _mainContentArea.Add(scroll);
    }

    // --- PILLAR 2: LOGISTICS (Filtered for Warehouses) ---
    private void RenderLogisticsTab()
    {
        _mainContentArea.Add(new Label("LOGISTICS & STORAGE") { style = { color = Color.yellow, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });
        // (Implementation similar to previous, but iterating over _hardpoints where Type == Warehouse)
        _mainContentArea.Add(new Label($"Total Storage Modules: {_hardpoints.Count(h => h.Type == ModuleType.Warehouse)}") { style = { color = Color.gray } });
    }

    // --- PILLAR 3: WORKFORCE (Filtered for Habitats) ---
    private void RenderWorkforceTab()
    {
        _mainContentArea.Add(new Label("WORKFORCE & HABITATS") { style = { color = Color.green, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });
        // (Implementation similar to previous, iterating over _hardpoints where Type == Habitat)
        _mainContentArea.Add(new Label($"Total Habitat Modules: {_hardpoints.Count(h => h.Type == ModuleType.Habitat)}") { style = { color = Color.gray } });
    }

    // --------------------------------------------------------------------------------
    // ACTIONS & DATA
    // --------------------------------------------------------------------------------

    private void InstallModule(Hardpoint hp, ModuleType type)
    {
        hp.Type = type;
        hp.Level = 1;
        hp.Status = "Idle / Empty";

        switch (type)
        {
            case ModuleType.Dock: hp.Name = "Light Assembly Dock"; hp.ThemeColor = Color.cyan; break;
            case ModuleType.Warehouse: hp.Name = "Parts Depot"; hp.ThemeColor = Color.yellow; break;
            case ModuleType.Habitat: hp.Name = "Worker Quarters"; hp.ThemeColor = Color.green; break;
            case ModuleType.Manufactory: hp.Name = "Industrial Forge"; hp.ThemeColor = new Color(0.6f, 0.2f, 0.8f); break;
        }
        RenderCurrentTab();
    }

    private void DemolishModule(Hardpoint hp)
    {
        hp.Type = ModuleType.Empty;
        hp.Name = "";
        hp.Status = "";
        hp.Level = 0;
        hp.ThemeColor = new Color(0.2f, 0.2f, 0.2f);
        RenderCurrentTab();
    }

    private void UpgradeShipyardTier()
    {
        long cost = (_shipyardTier + 1) * 50000;
        if (_treasury >= cost && _shipyardTier < 4)
        {
            _treasury -= cost;
            _shipyardTier++;

            // Add 4 new empty hardpoints
            for (int i = 0; i < 4; i++)
            {
                _hardpoints.Add(new Hardpoint
                {
                    SlotId = _hardpoints.Count + 1,
                    Type = ModuleType.Empty,
                    ThemeColor = new Color(0.2f, 0.2f, 0.2f)
                });
            }

            // Force total UI refresh to update top bar
            if (_mainContentArea.parent != null)
            {
                _mainContentArea.parent.parent.parent.Clear();
                CreateGui(_lastCtx);
            }
        }
    }

    private Button CreateTabButton(string text, ShipyardTab tab)
    {
        return new Button(() => { _currentTab = tab; RenderCurrentTab(); })
        {
            text = text,
            style = {
                height = 50, marginBottom = 5, unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 20, borderLeftWidth = _currentTab == tab ? 4 : 0,
                backgroundColor = _currentTab == tab ? new Color(0, 0.3f, 0.5f) : new Color(0.1f, 0.1f, 0.15f),
                color = Color.white, borderLeftColor = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold
            }
        };
    }

    private void InitializeMockData()
    {
        _hardpoints = new List<Hardpoint>();
        int maxHps = _shipyardTier * 4;

        for (int i = 1; i <= maxHps; i++)
        {
            var hp = new Hardpoint { SlotId = i, Level = 1 };
            if (i == 1) { hp.Type = ModuleType.Dock; hp.Name = "Light Assembly Dock"; hp.ThemeColor = Color.cyan; hp.Status = "Idle"; }
            else if (i == 2) { hp.Type = ModuleType.Warehouse; hp.Name = "Parts Depot"; hp.ThemeColor = Color.yellow; hp.Status = "45% Full"; }
            else if (i == 3) { hp.Type = ModuleType.Habitat; hp.Name = "Worker Quarters"; hp.ThemeColor = Color.green; hp.Status = "Nominal"; }
            else { hp.Type = ModuleType.Empty; hp.Name = ""; hp.ThemeColor = new Color(0.2f, 0.2f, 0.2f); }

            _hardpoints.Add(hp);
        }
    }

    public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
#if UNITY_EDITOR
    public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
}