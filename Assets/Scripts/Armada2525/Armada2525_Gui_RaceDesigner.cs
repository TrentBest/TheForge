using TheSingularityWorkshop.Armada2525;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Armada2525_Gui_RaceDesigner : IGuiProvider
{
    public string Title => "GENETIC SEQUENCER";

    private Armada2525_Gui_GameBuilder _orchestrator;
    private VisualElement _root;
    private VisualElement _mainContent;
    private VisualElement _portraitView;
    private GuiContext _lastCtx;

    // --- STATE ---
    private int _dnaPoints = 150;
    private int _maxPoints = 150;
    private RaceData _currentRace;
    private List<string> _evolutionaryHistory = new List<string>(); // Tracks the "Path"

    // --- PRESETS ---
    private List<RaceData> _presets;
    private Action onSaveAndLaunchClicked;
    private Action onBackClicked;

    public Armada2525_Gui_RaceDesigner(Armada2525_Gui_GameBuilder owner)
    {
        _orchestrator = owner;
        _presets = GeneratePresets();
        _currentRace = new RaceData { Name = "New Species", Description = "A blank slate awaiting evolutionary direction." };
    }

    // Default constructor for interface compliance / standalone testing
    public Armada2525_Gui_RaceDesigner() : this(new Armada2525_Gui_GameBuilder()) { }

    public Armada2525_Gui_RaceDesigner(Action onSaveAndLaunchClicked, Action onBackClicked)
    {
        this.onSaveAndLaunchClicked = onSaveAndLaunchClicked;
        this.onBackClicked = onBackClicked;
    }

    public VisualElement CreateGui(GuiContext ctx)
    {
        _lastCtx = ctx;
        var builder = new GraphicalUserInterfaceBuilder("RaceDesigner_Root")
            .WithPercentSize(100, 100)
            .WithBackgroundColor(new Color(0.02f, 0.02f, 0.05f)) // Deep Void
            .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch);

        // --- 1. LEFT COLUMN: THE SPECIMEN TANK (Visuals) ---
        builder.AddChild(c => {
            var tank = new VisualElement
            {
                style = {
                    width = Length.Percent(35),
                    borderRightWidth = 2, borderRightColor = Color.cyan,
                    backgroundColor = new Color(0,0,0,0.5f),
                    alignItems = Align.Center, justifyContent = Justify.Center
                }
            };

            // The "Portrait" - In a real game, this would be a 3D RenderTexture
            _portraitView = new VisualElement
            {
                style = {
                    width = 250, height = 350,
                    backgroundColor = new Color(0.1f, 0.1f, 0.1f),
                    borderTopLeftRadius = 10, borderTopRightRadius = 10,
                    borderBottomLeftRadius = 10, borderBottomRightRadius = 10,
                    borderLeftWidth = 1, borderRightWidth = 1, borderTopWidth = 1, borderBottomWidth = 1,
                    borderLeftColor = Color.white, borderRightColor = Color.white, borderTopColor = Color.white, borderBottomColor = Color.white
                }
            };
            tank.Add(_portraitView);

            // Race Name Input
            var nameField = new TextField { value = _currentRace.Name, style = { marginTop = 20, width = 250, fontSize = 18, unityTextAlign = TextAnchor.MiddleCenter } };
            nameField.RegisterValueChangedCallback(e => _currentRace.Name = e.newValue);
            tank.Add(nameField);

            // DNA Budget Meter
            tank.Add(CreateDNAMeter());

            return tank;
        });

        // --- 2. RIGHT COLUMN: THE SEQUENCER (Controls) ---
        builder.AddChild(c => {
            var controls = new VisualElement { style = { flexGrow = 1, paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20 } };

            // Header
            controls.Add(new Label("EVOLUTIONARY PATHWAY") { style = { fontSize = 24, color = Color.green, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            // Tabs / Stages
            var tabs = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 20, borderBottomWidth = 1, borderBottomColor = Color.gray } };
            tabs.Add(CreateTab("ARCHETYPE", () => RenderArchetypeSelector()));
            tabs.Add(CreateTab("BIOLOGY", () => RenderBiologyEditor()));
            tabs.Add(CreateTab("SOCIOLOGY", () => RenderSociologyEditor()));
            controls.Add(tabs);

            _mainContent = new VisualElement { style = { flexGrow = 1 } };
            controls.Add(_mainContent);

            // Footer Actions
            var footer = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd, marginTop = 20 } };
            footer.Add(new Button(() => LoadPresetMenu()) { text = "LOAD PRESET", style = { marginRight = 10, backgroundColor = new Color(0.3f, 0.3f, 0.3f) } });
            footer.Add(new Button(() => CommitRace())
            {
                text = "FINALIZE GENOME",
                style = { height = 40, backgroundColor = new Color(0, 0.5f, 0.2f), unityFontStyleAndWeight = FontStyle.Bold, color = Color.white }
            });
            controls.Add(footer);

            // Initialize default view
            RenderArchetypeSelector();

            return controls;
        });

        _root = builder.Build();
        UpdatePortrait(); // Initial draw
        return _root;
    }

    // ==========================================================================================
    // STAGE 1: ARCHETYPE (The "Cell Stage" Equivalent)
    // Determines base metabolism and huge bonuses/maluses.
    // ==========================================================================================
    private void RenderArchetypeSelector()
    {
        _mainContent.Clear();
        _mainContent.Add(new Label("SELECT METABOLIC BASE") { style = { color = Color.gray, marginBottom = 10 } });

        var grid = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };

        // CARBON (Standard)
        grid.Add(CreateArchetypeCard("CARBON BASED", "Adaptable. The galactic standard.", 0,
            () => SetArchetype(RaceArchetype.Carbon)));

        // SILICON (Slow, Tough)
        grid.Add(CreateArchetypeCard("SILICON BASED", "Consumes minerals for food. Immune to radiation. Slow reproduction.", 40,
            () => SetArchetype(RaceArchetype.Silicon)));

        // SYNTHETIC (Machine)
        grid.Add(CreateArchetypeCard("SYNTHETIC", "Immortal leaders. Does not breathe. Requires Industrial maintenance.", 80,
            () => SetArchetype(RaceArchetype.Synthetic)));

        // ENERGY (Spiritual/Weird)
        grid.Add(CreateArchetypeCard("ENERGY BEING", "Incorporeal. Ships require no fuel but massive shielding. Low population cap.", 100,
            () => SetArchetype(RaceArchetype.Energy)));

        _mainContent.Add(grid);
    }

    private VisualElement CreateArchetypeCard(string title, string desc, int cost, Action onSelect)
    {
        bool isSelected = _currentRace.Archetype.ToString().ToUpper() == title.Split(' ')[0];

        var card = new Button(onSelect)
        {
            style = {
                width = Length.Percent(48), height = 100, marginBottom = 10, marginRight = 5,
                backgroundColor = isSelected ? new Color(0, 0.3f, 0.5f) : new Color(0.1f, 0.1f, 0.15f),
                borderLeftColor = isSelected ? Color.cyan : Color.gray, borderLeftWidth = 4,
                paddingTop = 10, paddingBottom = 10,paddingRight = 10, paddingLeft = 10, justifyContent = Justify.Center
            }
        };

        card.Add(new Label(title) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 14 } });
        card.Add(new Label(desc) { style = { color = new Color(1, 1, 1, 0.7f), fontSize = 10, whiteSpace = WhiteSpace.Normal } });
        card.Add(new Label($"Cost: {cost} DNA") { style = { color = cost > _maxPoints ? Color.red : Color.yellow, fontSize = 10, alignSelf = Align.FlexEnd } });

        return card;
    }

    // ==========================================================================================
    // STAGE 2: BIOLOGY (The "Creature Stage" Equivalent)
    // Physical traits. Available options depend on Archetype.
    // ==========================================================================================
    private void RenderBiologyEditor()
    {
        _mainContent.Clear();
        _mainContent.Add(new Label("PHYSICAL ADAPTATIONS") { style = { color = Color.gray, marginBottom = 10 } });

        var scroll = new ScrollView();

        // 1. Environmental Traits
        scroll.Add(new Label("ENVIRONMENTAL") { style = { color = Color.cyan, marginTop = 10 } });

        // AQUATIC: Only if Carbon or Silicon
        if (_currentRace.Archetype == RaceArchetype.Carbon || _currentRace.Archetype == RaceArchetype.Silicon)
            scroll.Add(CreateTraitToggle("Aquatic", "Ocean worlds are ideal. Desert worlds are uninhabitable.", 10, "IsAquatic"));
        else
            scroll.Add(CreateGhostTrait("Aquatic", "Requires Biological Body"));

        // VACUUM TOLERANT
        if (_currentRace.Archetype == RaceArchetype.Synthetic || _currentRace.Archetype == RaceArchetype.Energy)
            scroll.Add(CreateStandardTrait("Vacuum Native", "Does not require atmosphere.", 0)); // Built-in
        else
            scroll.Add(CreateTraitToggle("Vacuum Tolerant", "Can survive low atmosphere.", 30, "VacuumTolerant"));

        // 2. Physical Prowess
        scroll.Add(new Label("MORPHOLOGY") { style = { color = Color.cyan, marginTop = 15 } });
        scroll.Add(CreateTraitToggle("Cybernetic Implants", "Increases worker efficiency +20%.", 40, "Cybernetic"));
        scroll.Add(CreateTraitToggle("Hive Mind", "No morale penalties. No independent thought.", 60, "HiveMind"));
        scroll.Add(CreateTraitToggle("Four Arms", "Ground combat +50%.", 20, "FourArms"));

        _mainContent.Add(scroll);
    }

    // ==========================================================================================
    // STAGE 3: SOCIOLOGY (The "Civ Stage" Equivalent)
    // Government and Ethics. Gated by Biological choices.
    // ==========================================================================================
    private void RenderSociologyEditor()
    {
        _mainContent.Clear();
        _mainContent.Add(new Label("SOCIETAL DOCTRINE") { style = { color = Color.gray, marginBottom = 10 } });

        // Example of "Evolutionary Lock"
        // If you picked "Hive Mind" in Biology, you cannot pick "Democracy".
        bool isHive = _currentRace.Traits.Contains("HiveMind");

        var scroll = new ScrollView();

        // GOVERNMENT
        scroll.Add(new Label("GOVERNMENT TYPE") { style = { color = Color.cyan } });

        if (isHive)
        {
            scroll.Add(CreateGhostTrait("Representative Democracy", "Incompatible with Hive Mind Biology"));
            scroll.Add(CreateStandardTrait("Gestalt Consciousness", "The only path for the swarm.", 0));
        }
        else
        {
            scroll.Add(CreateTraitToggle("Representative Democracy", "Morale +10%. Slower decisions.", 10, "Democracy"));
            scroll.Add(CreateTraitToggle("Military Dictatorship", "Ship building +20%. Lower Morale.", 10, "Dictatorship"));
            scroll.Add(CreateTraitToggle("Technocracy", "Research +20%.", 20, "Technocracy"));
        }

        // HISTORY
        scroll.Add(new Label("HISTORICAL ORIGIN") { style = { color = Color.cyan, marginTop = 15 } });
        scroll.Add(CreateTraitToggle("War Torn", "Start with veteran ships. Start with fewer population.", -10, "WarTorn"));
        scroll.Add(CreateTraitToggle("Post-Scarcity", "Start with extra credits. High maintenance costs.", 30, "RichStart"));

        _mainContent.Add(scroll);
    }

    // ==========================================================================================
    // LOGIC & DATA MANGEMENT
    // ==========================================================================================

    private void SetArchetype(RaceArchetype type)
    {
        _currentRace.Archetype = type;
        // Reset points logic based on base cost of archetype
        int cost = type switch { RaceArchetype.Carbon => 0, RaceArchetype.Silicon => 40, RaceArchetype.Synthetic => 80, RaceArchetype.Energy => 100, _ => 0 };
        _dnaPoints = _maxPoints - cost;
        _currentRace.Traits.Clear(); // Reset traits on base change
        UpdatePortrait();
        RenderArchetypeSelector(); // Refresh UI
    }

    private void ToggleTrait(string id, int cost)
    {
        if (_currentRace.Traits.Contains(id))
        {
            _currentRace.Traits.Remove(id);
            _dnaPoints += cost;
        }
        else
        {
            if (_dnaPoints >= cost)
            {
                _currentRace.Traits.Add(id);
                _dnaPoints -= cost;
            }
        }
        // Refresh specific parts or whole view
        // In a real UI Toolkit setup, we'd bind values, but here we redraw for simplicity
        if (_mainContent.childCount > 0)
        {
            // Hacky refresh of current tab
            // ideally we store _currentTab state
        }
        UpdatePortrait(); // Update visuals based on new traits
    }

    private void UpdatePortrait()
    {
        _portraitView.Clear();
        // Dynamic generation of "face" based on traits
        // This is a programmatic art placeholder

        var face = new VisualElement { style = { width = 100, height = 100, backgroundColor = GetSkinColor(), alignSelf = Align.Center, top = 50,
                borderBottomLeftRadius = 50, borderBottomRightRadius = 50, borderTopLeftRadius = 50, borderTopRightRadius = 50 } };

        // Eyes
        int eyeCount = _currentRace.Traits.Contains("FourArms") ? 4 : 2; // Arbitrary correlation for fun
        if (_currentRace.Archetype == RaceArchetype.Silicon) eyeCount = 1; // Cyclops

        for (int i = 0; i < eyeCount; i++)
        {
            face.Add(new VisualElement { style = { width = 20, height = 20, backgroundColor = Color.black, position = Position.Absolute, left = 20 + (i * 20), top = 30 } });
        }

        // Cybernetics
        if (_currentRace.Traits.Contains("Cybernetic"))
        {
            face.Add(new VisualElement { style = { width = 40, height = 40, borderTopColor = Color.red, borderRightColor = Color.red, borderBottomColor = Color.red, borderLeftColor = Color.red,
                    borderTopWidth = 2, borderRightWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, position = Position.Absolute, left = 60, top = 10, backgroundColor = new Color(0, 0, 0, 0) } }); // Borg eye
        }

        // Aquatic
        if (_currentRace.Traits.Contains("IsAquatic"))
        {
            face.style.borderBottomLeftRadius = 0; face.style.borderBottomRightRadius = 0; // Squid shape?
        }

        _portraitView.Add(face);

        // Archetype Label below portrait
        _portraitView.Add(new Label(_currentRace.Archetype.ToString().ToUpper()) { style = { alignSelf = Align.Center, marginTop = 120, color = Color.gray } });
    }

    private Color GetSkinColor()
    {
        return _currentRace.Archetype switch
        {
            RaceArchetype.Carbon => new Color(0.8f, 0.6f, 0.4f),
            RaceArchetype.Silicon => new Color(0.5f, 0.5f, 0.5f),
            RaceArchetype.Synthetic => new Color(0.8f, 0.8f, 0.9f),
            RaceArchetype.Energy => new Color(0.2f, 0.8f, 1.0f, 0.5f), // Transparent
            _ => Color.white
        };
    }

    // --- UI HELPERS ---

    private VisualElement CreateTraitToggle(string name, string desc, int cost, string id)
    {
        bool hasTrait = _currentRace.Traits.Contains(id);
        var box = new Button(() => ToggleTrait(id, cost))
        {
            style = {
                flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween,
                backgroundColor = hasTrait ? new Color(0, 0.4f, 0.2f) : new Color(0.2f, 0.2f, 0.2f),
                paddingTop = 8, paddingBottom = 8, paddingLeft = 8, paddingRight = 8, marginBottom = 4, borderLeftWidth = 3, borderLeftColor = hasTrait ? Color.green : Color.gray
            }
        };

        var left = new VisualElement();
        left.Add(new Label(name) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });
        left.Add(new Label(desc) { style = { color = Color.gray, fontSize = 10 } });
        box.Add(left);

        box.Add(new Label(cost.ToString()) { style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold } });

        return box;
    }

    private VisualElement CreateStandardTrait(string name, string desc, int cost)
    {
        // Non-interactive trait (e.g., inherent to archetype)
        var box = new VisualElement
        {
            style = {
                flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween,
                backgroundColor = new Color(0.1f, 0.1f, 0.1f), opacity = 0.8f,
                paddingTop = 8, paddingRight = 8, paddingLeft = 8, paddingBottom = 8,  marginBottom = 4
            }
        };
        var left = new VisualElement();
        left.Add(new Label(name + " (Inherent)") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold } });
        left.Add(new Label(desc) { style = { color = Color.gray, fontSize = 10 } });
        box.Add(left);
        return box;
    }

    private VisualElement CreateGhostTrait(string name, string reason)
    {
        var box = new VisualElement
        {
            style = {
                flexDirection = FlexDirection.Row, alignItems = Align.Center,
                backgroundColor = new Color(0.1f, 0, 0, 0.5f), paddingTop = 8, paddingBottom = 8, paddingLeft = 8, paddingRight = 8, marginBottom = 4
            }
        };
        box.Add(new Label($"LOCKED: {name}") { style = { color = Color.gray, flexGrow = 1 } });
        box.Add(new Label(reason.ToUpper()) { style = { color = Color.red, fontSize = 9 } });
        return box;
    }

    private VisualElement CreateTab(string text, Action onClick)
    {
        return new Button(onClick)
        {
            text = text,
            style = { flexGrow = 1, height = 40, backgroundColor = Color.clear, borderBottomWidth = 0, color = Color.white }
        };
    }

    private VisualElement CreateDNAMeter()
    {
        var root = new VisualElement { style = { marginTop = 20, width = Length.Percent(100) } };
        root.Add(new Label($"GENETIC STABILITY: {_dnaPoints}/{_maxPoints}") { style = { alignSelf = Align.Center, color = Color.yellow } });
        var bar = new VisualElement { style = { height = 10, backgroundColor = Color.black, marginTop = 5 } };
        // In a real update loop we'd set the width percent here
        return root;
    }

    // --- PRESETS & FINALIZATION ---

    private void LoadPresetMenu()
    {
        // Pop up a modal or switch view to list _presets
        Debug.Log("Open Preset List..");
    }

    private void CommitRace()
    {
        Debug.Log($"Race Created: {_currentRace.Name} ({_currentRace.Archetype})");
        // Inject into GameData via Orchestrator
        // _orchestrator.SetPlayerRace(_currentRace);
        _orchestrator.FinishAndLaunch(); // Go to loading screen
    }

    private List<RaceData> GeneratePresets()
    {
        return new List<RaceData> {
            new RaceData { Name = "Terran", Archetype = RaceArchetype.Carbon, Traits = new List<string> { "Democracy", "Diplomatic" } },
            new RaceData { Name = "Xenon Hive", Archetype = RaceArchetype.Silicon, Traits = new List<string> { "HiveMind", "WarTorn" } }
        };
    }

    // --- DATA STRUCTURES (Internal for now, move to DataModels.cs later) ---
    public class RaceData
    {
        public string Name;
        public string Description;
        public RaceArchetype Archetype;
        public List<string> Traits = new List<string>();
        public Color PrimaryColor;
    }

    public enum RaceArchetype { Carbon, Silicon, Synthetic, Energy }

    public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

#if UNITY_EDITOR
    public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
    public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
}