using TheSingularityWorkshop.Armada2525;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Armada2525_Gui_ResearchView : IGuiProvider
{
    public string Title => "SCIENCE DIRECTORATE";

    private Armada2525 _game;
    private GuiContext _lastCtx;
    private VisualElement _mainContentArea;

    // --- MOCK DATA STATE ---
    private float _totalScienceOutput = 12500f;
    private long _treasury = 45000;
    private float _globalMorale = 0.85f; // Too high for a revolution!
    private GameData.Government _currentGov = GameData.Government.Democracy;

    // Feature state
    private bool _isLobbyingActive = false; // Has the player bribed the directors this turn?

    private enum ResearchTab { MacroAreas, MicroFields, ActivePrototypes, Rollout }
    private ResearchTab _currentTab = ResearchTab.MicroFields; // Defaulting to Micro for demonstration

    // 1. MACRO: Areas of Study
    private Dictionary<string, float> _areaAllocations = new Dictionary<string, float>
    {
        { "Propulsion", 0.3f }, { "Energy", 0.4f }, { "Materials", 0.1f }, { "Computing", 0.1f }, { "Biology", 0.1f }
    };

    // 2. MICRO: Fields & Foci
    private class FieldDef
    {
        public string Name;
        public string ParentArea;
        public float Allocation;
        public bool IsUnlocked;
        public List<string> AvailableFoci;
        public string CurrentFocus;
    }

    private List<FieldDef> _fields;
    private Label _actionTooltipLabel;

    public Armada2525_Gui_ResearchView()
    {
        InitializeMockData();
    }

    public VisualElement CreateGui(GuiContext ctx)
    {
        _lastCtx = ctx;
        _game = UnityEngine.Object.FindAnyObjectByType<Armada2525>();

        var builder = new GraphicalUserInterfaceBuilder("Research_Root")
            .WithBackgroundColor(new Color(0.01f, 0.02f, 0.04f))
            .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
            .WithPercentSize(100, 100);

        // --- LEFT SIDEBAR: NAVIGATION & STATUS (25%) ---
        builder.AddChild(c => {
            var sidebar = new VisualElement
            {
                style = {
                    width = Length.Percent(25), height = Length.Percent(100), backgroundColor = new Color(0.05f, 0.07f, 0.1f),
                    borderRightWidth = 2, borderRightColor = Color.cyan, paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20
                }
            };

            sidebar.Add(new Button(() => _game?.SwitchGui("GalaxyView")) { text = "<< RETURN TO COMMAND", style = { backgroundColor = Color.clear, color = Color.gray, borderBottomWidth = 1, borderBottomColor = Color.gray, marginBottom = 20, height = 30 } });

            sidebar.Add(new Label("SCIENCE DIRECTORATE") { style = { color = Color.cyan, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
            sidebar.Add(new Label($"NET OUTPUT: {_totalScienceOutput:N0} SP/Turn") { style = { color = Color.green, fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });

            var govLabel = new Label($"GOVERNMENT: {_currentGov.ToString().ToUpper()}") { style = { color = Color.yellow, fontSize = 10, marginBottom = 5 } };
            sidebar.Add(govLabel);
            sidebar.Add(new Label($"TREASURY: {_treasury:N0} cr") { style = { color = Color.yellow, fontSize = 10, marginBottom = 20 } });

            sidebar.Add(new Label("DIRECTORATE TERMINALS") { style = { color = Color.gray, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
            sidebar.Add(CreateTabButton("MACRO: AREAS OF STUDY", ResearchTab.MacroAreas));
            sidebar.Add(CreateTabButton("MICRO: FIELDS & FOCI", ResearchTab.MicroFields));
            sidebar.Add(CreateTabButton("ACTIVE PROTOTYPES", ResearchTab.ActivePrototypes));
            sidebar.Add(CreateTabButton("INTEGRATION & ROLLOUT", ResearchTab.Rollout, true));

            return sidebar;
        });

        // --- RIGHT AREA: DYNAMIC CONTENT (75%) ---
        builder.AddChild(c => {
            var rightCol = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };
            _mainContentArea = new VisualElement { style = { flexGrow = 1, paddingTop = 20, paddingBottom = 20, paddingLeft = 30, paddingRight = 30 } };
            RenderCurrentTab();
            rightCol.Add(_mainContentArea);
            return rightCol;
        });

        return builder.Build();
    }

    // --------------------------------------------------------------------------------
    // TABS & RENDERING
    // --------------------------------------------------------------------------------

    private void RenderCurrentTab()
    {
        if (_mainContentArea == null) return;
        _mainContentArea.Clear();

        switch (_currentTab)
        {
            case ResearchTab.MacroAreas: RenderMacroAreas(); break;
            case ResearchTab.MicroFields: RenderMicroFields(); break;
            case ResearchTab.ActivePrototypes: _mainContentArea.Add(new Label("Active Projects View..")); break;
            case ResearchTab.Rollout: _mainContentArea.Add(new Label("Integration View..")); break;
        }
    }

    private void RenderMacroAreas()
    {
        _mainContentArea.Add(new Label("MACRO-ALLOCATION: AREAS OF STUDY") { style = { color = Color.white, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
        _mainContentArea.Add(new Label("Distribute your total Science Points (SP) across broad categories.") { style = { color = Color.gray, fontSize = 12, marginBottom = 30 } });

        var slidersContainer = new VisualElement { style = { width = 400 } };
        foreach (var area in _areaAllocations.Keys.ToList()) slidersContainer.Add(CreateAutoBalancingSlider(area, _areaAllocations, _totalScienceOutput));
        _mainContentArea.Add(slidersContainer);
    }

    private void RenderMicroFields()
    {
        _mainContentArea.Add(new Label("MICRO-ALLOCATION: FIELDS & FOCI") { style = { color = Color.white, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

        bool canMicroManage = _currentGov != GameData.Government.Democracy;

        // --- BUREAUCRACY LOCKOUT WITH ACTION BUTTONS ---
        if (!canMicroManage)
        {
            var lockBox = new VisualElement { style = { backgroundColor = new Color(0.15f, 0.05f, 0.05f), paddingLeft = 20, paddingRight = 20, paddingTop = 20, paddingBottom = 20, borderLeftWidth = 4, borderLeftColor = Color.red, marginBottom = 20 } };
            lockBox.Add(new Label("BUREAUCRATIC RESTRICTION") { style = { color = Color.red, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold } });
            lockBox.Add(new Label($"Your {_currentGov} delegates field research to civilian sectors. Budgets and Foci are selected automatically.") { style = { color = Color.white, fontSize = 12, marginTop = 5, whiteSpace = WhiteSpace.Normal } });

            var actionRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 15, alignItems = Align.Center } };

            // 1. DIMMED / UNAVAILABLE ACTION: Overthrow
            bool canRevolt = _globalMorale < 0.20f;
            var overthrowBtn = new Button(() => { if (!canRevolt) ShowRequirementsModal("Regime Change"); })
            {
                text = "INCITE REGIME CHANGE",
                style = {
                    backgroundColor = canRevolt ? new Color(0.6f, 0.1f, 0.1f) : new Color(0.2f, 0.1f, 0.1f),
                    color = canRevolt ? Color.white : new Color(0.5f, 0.5f, 0.5f),
                    borderTopColor = Color.black, borderBottomColor = Color.black, borderLeftColor = Color.black, borderRightColor = Color.black,
                    width = 200, height = 30, opacity = canRevolt ? 1f : 0.6f
                }
            };

            overthrowBtn.RegisterCallback<MouseEnterEvent>(e => _actionTooltipLabel.text = canRevolt ? "Click to access the Executive Government terminal and shift regimes." : $"UNAVAILABLE: Requires Global Morale below 20% (Current: {_globalMorale:P0}). The populace is too content to support a revolution.");
            overthrowBtn.RegisterCallback<MouseLeaveEvent>(e => _actionTooltipLabel.text = "");

            // 2. BRIBERY ACTION: Lobby Directors
            var bribeBtn = new Button(() => ProcessBribe())
            {
                text = _isLobbyingActive ? "DIRECTORS BRIBED" : "LOBBY DIRECTORS (10,000 cr)",
                style = {
                    backgroundColor = _isLobbyingActive ? new Color(0.1f, 0.4f, 0.1f) : new Color(0.1f, 0.2f, 0.3f),
                    color = Color.white, width = 220, height = 30, marginLeft = 15
                }
            };
            bribeBtn.SetEnabled(!_isLobbyingActive && _treasury >= 10000);

            bribeBtn.RegisterCallback<MouseEnterEvent>(e => _actionTooltipLabel.text = _isLobbyingActive ? "You have subverted the bureaucracy for this turn." : "Spend 10,000 credits from the Treasury to bypass civilian oversight and manually dictate Research Foci for 1 turn.");
            bribeBtn.RegisterCallback<MouseLeaveEvent>(e => _actionTooltipLabel.text = "");

            actionRow.Add(overthrowBtn);
            actionRow.Add(bribeBtn);
            lockBox.Add(actionRow);

            _actionTooltipLabel = new Label("") { style = { color = Color.yellow, fontSize = 11, marginTop = 10, unityFontStyleAndWeight = FontStyle.Italic } };
            lockBox.Add(_actionTooltipLabel);

            _mainContentArea.Add(lockBox);
        }

        // --- RENDER SLIDERS AND DROPDOWNS ---
        var scroll = new ScrollView { style = { flexGrow = 1 } };

        foreach (var area in _areaAllocations.Keys)
        {
            float areaBudget = _totalScienceOutput * _areaAllocations[area];
            if (areaBudget < 10) continue;

            var areaBox = new VisualElement { style = { backgroundColor = new Color(0.1f, 0.1f, 0.15f), paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15, marginBottom = 15, borderLeftWidth = 2, borderLeftColor = Color.cyan } };
            areaBox.Add(new Label($"{area.ToUpper()} (Budget: {areaBudget:N0} SP)") { style = { color = Color.cyan, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            var fieldsInArea = _fields.Where(f => f.ParentArea == area && f.IsUnlocked).ToList();
            var fieldDict = fieldsInArea.ToDictionary(f => f.Name, f => f.Allocation);

            foreach (var field in fieldsInArea)
            {
                var fieldRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 10, alignItems = Align.Center } };

                // Sliders are NEVER unlocked for Democracies unless you revolt. Bribery only unlocks Intent (Foci).
                var sliderBox = CreateAutoBalancingSlider(field.Name, fieldDict, areaBudget, canMicroManage);
                sliderBox.style.width = 300; sliderBox.style.marginRight = 20;

                var slider = sliderBox.Q<Slider>();
                if (slider != null) slider.RegisterValueChangedCallback(e => field.Allocation = fieldDict[field.Name]);
                fieldRow.Add(sliderBox);

                // Focus Dropdown - Enabled if you can micro, OR if you bribed them!
                bool canSetFocus = canMicroManage || _isLobbyingActive;

                var focusCol = new VisualElement { style = { flexGrow = 1 } };
                focusCol.Add(new Label(canSetFocus ? "Current Focus:" : "Civilian Selected Focus (Locked):") { style = { color = canSetFocus ? Color.white : Color.gray, fontSize = 9 } });

                var dropdown = new DropdownField(field.AvailableFoci, field.CurrentFocus);
                dropdown.SetEnabled(canSetFocus);
                if (!canSetFocus) dropdown.style.opacity = 0.5f; // Dimmed when locked

                dropdown.RegisterValueChangedCallback(e => field.CurrentFocus = e.newValue);
                dropdown.style.width = 200;
                focusCol.Add(dropdown);

                fieldRow.Add(focusCol);
                areaBox.Add(fieldRow);
            }
            scroll.Add(areaBox);
        }
        _mainContentArea.Add(scroll);
    }

    // --------------------------------------------------------------------------------
    // ACTIONS & LOGIC
    // --------------------------------------------------------------------------------

    private void ProcessBribe()
    {
        if (_treasury >= 10000)
        {
            Debug.Log("Treasury deducted 10,000 cr. Science Directors bribed.");
            _treasury -= 10000;
            _isLobbyingActive = true;

            // Re-render the tab so the dropdowns unlock immediately
            RenderCurrentTab();

            // Re-render the root so the sidebar treasury amount updates
            if (_mainContentArea.parent != null)
            {
                _mainContentArea.parent.parent.parent.Clear();
                CreateGui(_lastCtx);
            }
        }
    }

    private void ShowRequirementsModal(string attemptedAction)
    {
        Debug.Log($"Displaying modal for: How to unlock {attemptedAction}.");
        // Here you would inject a floating absolute UI element or switch to the Government view.
        // E.g. _game.SwitchGui("GovernmentView");
    }

    // --------------------------------------------------------------------------------
    // UTILS
    // --------------------------------------------------------------------------------

    private VisualElement CreateAutoBalancingSlider(string key, Dictionary<string, float> dict, float totalBudget, bool isEnabled = true)
    {
        var container = new VisualElement { style = { marginBottom = 15 } };
        var header = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };
        header.Add(new Label(key) { style = { color = isEnabled ? Color.white : Color.gray, fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold } });
        header.Add(new Label($"{(dict[key] * totalBudget):N0} SP ({dict[key]:P1})") { style = { color = isEnabled ? Color.cyan : new Color(0, 0.4f, 0.4f), fontSize = 12 } });
        container.Add(header);

        var slider = new Slider(0f, 1f) { value = dict[key] };
        slider.SetEnabled(isEnabled);

        slider.RegisterValueChangedCallback(evt => {
            if (!isEnabled) return;
            float delta = evt.newValue - dict[key];
            dict[key] = evt.newValue;

            var otherKeys = dict.Keys.Where(k => k != key).ToList();
            float sumOthers = otherKeys.Sum(k => dict[k]);

            if (sumOthers == 0 && delta > 0) { foreach (var k in otherKeys) dict[k] = 0f; dict[key] = 1.0f; }
            else
            {
                foreach (var k in otherKeys)
                {
                    dict[k] -= delta * (dict[k] / sumOthers);
                    if (dict[k] < 0) dict[k] = 0;
                }
            }
            float total = dict.Values.Sum();
            if (total > 0) { foreach (var k in dict.Keys.ToList()) dict[k] /= total; }
            RenderCurrentTab();
        });

        container.Add(slider);
        return container;
    }

    private VisualElement CreateTabButton(string text, ResearchTab targetTab, bool highlight = false)
    {
        return new Button(() => { _currentTab = targetTab; RenderCurrentTab(); })
        {
            text = text,
            style = {
                height = 40, marginBottom = 5, unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 15,
                backgroundColor = _currentTab == targetTab ? new Color(0, 0.4f, 0.6f) : new Color(0.1f, 0.1f, 0.15f),
                color = highlight ? Color.yellow : Color.white,
                borderLeftWidth = _currentTab == targetTab ? 4 : 0,
                borderLeftColor = highlight ? Color.yellow : Color.cyan
            }
        };
    }

    private void InitializeMockData()
    {
        _fields = new List<FieldDef>
        {
            new FieldDef { Name = "Sub-Light Drives", ParentArea = "Propulsion", Allocation = 0.5f, IsUnlocked = true, AvailableFoci = new List<string>{"Ion Thrusters", "Plasma Drives", "Impulse Engines"}, CurrentFocus = "Plasma Drives" },
            new FieldDef { Name = "Warp Dynamics", ParentArea = "Propulsion", Allocation = 0.5f, IsUnlocked = true, AvailableFoci = new List<string>{"Warp Bubble Gen", "Subspace Topology"}, CurrentFocus = "Warp Bubble Gen" },
            new FieldDef { Name = "Beam Weapons", ParentArea = "Energy", Allocation = 0.4f, IsUnlocked = true, AvailableFoci = new List<string>{"Heavy Lasers", "Phasers", "Disruptors"}, CurrentFocus = "Heavy Lasers" },
            new FieldDef { Name = "Shielding", ParentArea = "Energy", Allocation = 0.6f, IsUnlocked = true, AvailableFoci = new List<string>{"Deflectors", "Phase Shields"}, CurrentFocus = "Phase Shields" },
            new FieldDef { Name = "Armor Plating", ParentArea = "Materials", Allocation = 1.0f, IsUnlocked = true, AvailableFoci = new List<string>{"Titanium", "Tritanium", "Neutronium"}, CurrentFocus = "Tritanium" }
        };
    }

    public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
#if UNITY_EDITOR
    public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
}