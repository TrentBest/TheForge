using System;
using System.Collections.Generic;
using System.Linq;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;


using UnityEngine.UIElements;
using TheSingularityWorkshop.Armada2525.GURPS;

public class ExperienceContext : MonoBehaviour, IStateContext
{
    public string ManifestName = "";
    public List<Sense> Senses { get; private set; } = new List<Sense>();
    public bool IsValid { get; set; } = false;
    public string Name { get => name; set => name = value; }
    public FSMHandle Status { get; private set; }

    // --- NEW: ONTOLOGY ANCHORS FOR THE ACTIVE EXPERIENCE ---
    public string SpatialScale { get; set; } = "Standard";
    public string MechanicsEngine { get; set; } = "None";
    public GURPSUniverse GoverningUniverse { get; set; }

    // --- HOOK FOR THE PEDESTAL UI ---
    public Action<VisualElement> OnPedestalUIUpdated;

    private ExperienceBuilder_OLD _builder;
    private Experience _currentExperience;

    // FSM handles
    private FSMHandle _questionnaireStatus;

    // simple state flags used by transitions
    private bool _loaded = false;
    private bool _initialized = false;
    private bool _shouldShutdown = false;
    private bool _needsQuestionnaire = false;
    private bool _questionnaireComplete = false;

    private void Awake()
    {
        _builder = new ExperienceBuilder_OLD();

        // 1. MAIN EXPERIENCE FSM
        FSM_API.Create.CreateFiniteStateMachine("ExperienceFSM", -1, "Update")
            .State("Loading", OnEnterLoading, null, null)
            .State("Initializing", OnEnterInitializing, null, null)
            .State("Acting", OnEnterActing, OnUpdateActing, OnExtActing)
            .State("Shutdown", OnEnterShutdown, null, null)
            .Transition("Loading", "Initializing", Loaded)
            .Transition("Initializing", "Acting", Initialized)
            .Transition("Acting", "Shutdown", ShouldShutdown)
            .BuildDefinition();

        Status = FSM_API.Create.CreateInstance("ExperienceFSM", this, "Update");

        // 2. CONSTRUCT QUESTIONNAIRE FSM (The GM's Tooling)
        BuildQuestionnaireFSM();

        IsValid = true;
    }

    private void BuildQuestionnaireFSM()
    {
        FSM_API.Create.CreateFiniteStateMachine("ConstructQuestionnaireFSM", -1, "Update")
            .State("Ask_Scale", AskScale_Enter, null, null)
            .State("Ask_Mechanics", AskMechanics_Enter, null, null)
            .State("Manifest_Reality", ManifestReality_Enter, null, null)

            // Wait until the user has set the Scale, then move to Mechanics
            .Transition("Ask_Scale", "Ask_Mechanics", ctx => SpatialScale != "Unknown")
            // Wait until they set the Engine, then Manifest
            .Transition("Ask_Mechanics", "Manifest_Reality", ctx => MechanicsEngine != "None")
            // Flag complete once Manifested
            .Transition("Manifest_Reality", "Done", ctx => _questionnaireComplete)
            .BuildDefinition();
    }

    private void OnEnterLoading(IStateContext context)
    {
        _loaded = false;
        _initialized = false;
        _shouldShutdown = false;
        _needsQuestionnaire = false;
        Senses.Clear();

        try
        {
            _currentExperience = _builder.Load(string.IsNullOrWhiteSpace(ManifestName) ? null : ManifestName);

            if (_currentExperience != null)
            {
                FinalizeLoad();
            }
        }
        catch (System.IO.FileNotFoundException)
        {
            if (string.IsNullOrWhiteSpace(ManifestName))
            {
                Debug.Log("ExperienceContext: No external manifest found. initializing 'Genesis' (Default Workshop).");
                _currentExperience = CreateGenesisExperience();
                _needsQuestionnaire = true; // Trigger the pedestal!
                FinalizeLoad();
            }
            else
            {
                Debug.LogError($"ExperienceContext: Explicit manifest '{ManifestName}' not found.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"ExperienceContext: Critical error loading manifest: {ex}");
        }
    }

    private Experience CreateGenesisExperience()
    {
        SpatialScale = "Unknown";
        MechanicsEngine = "None";

        return new Experience
        {
            Id = "Genesis_Workshop",
            Name = "The Forge Construct",
            Description = "The white void. Awaiting ontological definition.",
            Senses = new List<SenseDescriptor>()
        };
    }

    public Experience GetCurrentExperience() => _currentExperience;

    private void FinalizeLoad()
    {
        if (_currentExperience.Senses != null)
            Senses.AddRange(_currentExperience.Senses.Select(sd => sd.Sense));

        _loaded = true;
    }

    private void OnEnterInitializing(IStateContext context)
    {
        _initialized = false;
        _initialized = true;
    }

    private void OnEnterActing(IStateContext context)
    {
        Debug.Log("ExperienceContext: Entering Acting state.");

        // If we loaded Genesis, boot up the Questionnaire FSM on the pedestal
        if (_needsQuestionnaire)
        {
            Debug.Log("ExperienceContext: Waking the Pedestal FSM..");
            _questionnaireStatus = FSM_API.Create.CreateInstance("ConstructQuestionnaireFSM", this, "Update");
        }
    }

    private void OnUpdateActing(IStateContext context)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _shouldShutdown = true;
        }

        // Keep the questionnaire pumping if it's active
        if (_needsQuestionnaire && !_questionnaireComplete)
        {
            // Note: Assuming your API requires manual pumping of sub-FSMs if not globally registered.
            // If CreateInstance automatically registers it to a global update loop, you can omit this.
        }
    }

    private void OnExtActing(IStateContext context) { }

    private void OnEnterShutdown(IStateContext context)
    {
        Senses.Clear();
        _currentExperience = null;
        _loaded = false;
        _initialized = false;
        _shouldShutdown = false;
        _needsQuestionnaire = false;
    }

    private bool Loaded(IStateContext context) => _loaded;
    private bool Initialized(IStateContext context) => _initialized;
    private bool ShouldShutdown(IStateContext context) => _shouldShutdown;

    // =========================================================================
    // THE CONSTRUCT QUESTIONNAIRE LOGIC (Ontology Layers 4 & 1)
    // =========================================================================

    private void AskScale_Enter(IStateContext ctx)
    {
        var options = new Dictionary<string, Action> {
            { "Sub-Atomic (Quantum Realm)", () => SpatialScale = "SubAtomic" },
            { "Microscopic (Insect Scale)", () => SpatialScale = "Microscopic" },
            { "Miniature (Mouse/Fraggle Scale)", () => SpatialScale = "Miniature" },
            { "Human Standard (Mansion/Room)", () => SpatialScale = "Standard" },
            { "Gigantic (Giant's Castle)", () => SpatialScale = "Gigantic" },
            { "Planetary Region", () => SpatialScale = "Regional" },
            { "Interplanetary System", () => SpatialScale = "System" }
        };

        PresentQuestion("At what spatial scale will this experience exist?", options);
    }

    private void AskMechanics_Enter(IStateContext ctx)
    {
        var options = new Dictionary<string, Action> {
            { "The GURPS Engine (RPG Simulation)", () => MechanicsEngine = "GURPS" },
            { "Real-World AEC Physics (Architecture)", () => MechanicsEngine = "AEC" },
            { "raWWar Tactical System", () => MechanicsEngine = "raWWar" },
            { "Custom / Narrative", () => MechanicsEngine = "Custom" }
        };

        PresentQuestion($"You have chosen a {SpatialScale} scale. What laws govern this space?", options);
    }

    private void ManifestReality_Enter(IStateContext ctx)
    {
        Debug.Log($"[THE FORGE] Manifesting a {SpatialScale} reality powered by {MechanicsEngine}.");

        // --- THE HANDOFF ---
        // If they chose GURPS, we initialize a new universe and put the Universe Editor on the Pedestal
        if (MechanicsEngine == "GURPS")
        {
            GoverningUniverse = new GURPSUniverse
            {
                UniverseSeed = UnityEngine.Random.Range(1000, 9999), // Give it a random ID
                Name = $"New {SpatialScale} Universe",
                Description = $"A newly forged reality at the {SpatialScale} scale."
            };

            // 1. Ensure the core system is booted so we can register the data
            DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem();

            // 2. Register this new universe into the API so the Editor can see it
            DigitalGenericUniversalRolePlayingSystem.Universes.Register(GoverningUniverse);

            // 3. Create the GUI using the standard GuiContext
            var universeEditor = new Armada2525_Gui_GURPS_UniverseEditor();
            var guiContext = new TheSingularityWorkshop.Forge.Builders.GuiBuilders.GuiContext();

            OnPedestalUIUpdated?.Invoke(universeEditor.CreateGui(guiContext));
        }
        else
        {
            // Placeholder for other rulesets
            var blankUi = new VisualElement();
            blankUi.Add(new Label("REALITY MANIFESTED.") { style = { fontSize = 24, color = Color.green } });
            OnPedestalUIUpdated?.Invoke(blankUi);
        }

        _questionnaireComplete = true;
    }

    // Helper to generate the UI and push it to the pedestal screen
    private void PresentQuestion(string prompt, Dictionary<string, Action> options)
    {
        var container = new VisualElement { style = { paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20, alignItems = Align.Center } };

        container.Add(new Label(prompt)
        {
            style = { fontSize = 20, color = Color.cyan, marginBottom = 20, unityFontStyleAndWeight = FontStyle.Bold, whiteSpace = WhiteSpace.Normal, unityTextAlign = TextAnchor.MiddleCenter }
        });

        foreach (var kvp in options)
        {
            var btn = new Button(kvp.Value)
            {
                text = kvp.Key,
                style = { width = 300, height = 40, marginBottom = 10, backgroundColor = new Color(0.15f, 0.15f, 0.15f), color = Color.white }
            };
            // Hover effect
            btn.RegisterCallback<MouseEnterEvent>(e => btn.style.backgroundColor = new Color(0.3f, 0.5f, 0.7f));
            btn.RegisterCallback<MouseLeaveEvent>(e => btn.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f));
            container.Add(btn);
        }

        OnPedestalUIUpdated?.Invoke(container);
    }
}

public enum Sense
{
    Vision = 0,
    Audio = 1,
    Touch = 2,
    Smell = 3,
    Taste = 4,
    Custom = 5,
    Custom01 = 6,
    Custom02 = 7,
    Custom03 = 8,
    Custom04 = 9,
    Custom05 = 10,
    Custom06 = 11,
    Custom07 = 12,
    Custom08 = 13,
    Custom09 = 14,
    Custom10 = 15,
}