using TheSingularityWorkshop.Armada2525;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_API.Scripts;
using UnityEngine;

public class Armada2525 : MonoBehaviour, IInExperienceGame
{
    [Header("FSM Configuration")]
    public string fsmName = "Armada2525_GlobalState";
    public string processingGroup = "Armada2525::GlobalSystems";

    private FSMHandle gameFsmHandle;
    public bool IsValid { get; set; } = false;
    public string Name { get; set; }

    public Dictionary<string, string> ProcessingGroups { get; } = new Dictionary<string, string>();
    public Dictionary<string, IGuiProvider> GUI = new Dictionary<string, IGuiProvider>();

    // The Active UI
    public IGuiProvider CurrentGUI { get; private set; }

    // The Data Model
    public GameData Data { get; internal set; }

    private void Awake()
    {
        Name = name;
        Data = new GameData(); // Initialize Data Container
        InitializeGameFSM();
    }

    private void Start()
    {
        gameFsmHandle = FSM_API.Create.CreateInstance(fsmName, this, processingGroup);
        IsValid = true;
    }

    private void InitializeGameFSM()
    {
        if ( !FSM_API.Interaction.Exists(fsmName, processingGroup))
        {
            var integration = FindAnyObjectByType<FSM_UnityIntegrationAdvanced>();
            if (integration != null) integration.AddProcessingGroup("Update", processingGroup);
            ProcessingGroups.TryAdd("Update", processingGroup);

            FSM_API.Create.CreateFiniteStateMachine(fsmName, -1, processingGroup)
                .State("Initialization", OnEnterInitialization, null, null)
                .State("MainMenu", OnEnterMainMenu, null, null)
                .State("GameSetup", OnEnterGameSetup, null, null) 
                .State("StarMap_Strategic", OnEnterStarMap, null, null)

                // Transitions
                .Transition("Initialization", "MainMenu", (ctx) => true)
                .Transition("MainMenu", "GameSetup", (ctx) => _transitionToSetup)
                .Transition("GameSetup", "StarMap_Strategic", (ctx) => _transitionToGame)

                .BuildDefinition();
        }
        gameFsmHandle = FSM_API.Create.CreateInstance(fsmName, this, processingGroup);
    }

    // --- TRANSITION FLAGS ---
    private bool _transitionToSetup = false;
    private bool _transitionToGame = false;

    // --- STATE CALLBACKS ---

    private void OnEnterInitialization(IStateContext context)
    {
        if (context is Armada2525 game)
        {
            // Register all GUIs
            game.GUI["MainMenu"] = new Armada2525_Gui_MainMenu();
            game.GUI["GameSetup"] = new Armada2525_Gui_GameBuilder();
            game.GUI["ColonyView"] = new Armada2525_Gui_ColonyView();
            game.GUI["StarView"] = new Armada2525_Gui_StarSystemView();
            game.GUI["GalaxyView"] = new Armada2525_Gui_GalaxyView();
            game.GUI["Settings"] = new Armada2525_Gui_Settings();
            game.GUI["TransportTycoon"] = new Armada2525_Gui_TransportTycoon();

            // Auto-move to Main Menu
            // (In a real FSM, we'd use a transition, but for init we can force it)
            game.SwitchGui("MainMenu");
        }
    }

    private void OnEnterMainMenu(IStateContext context)
    {
        if (context is Armada2525 game)
        {
            game._transitionToSetup = false; // Reset flags
            game.SwitchGui("MainMenu");
        }
    }

    private void OnEnterGameSetup(IStateContext context)
    {
        if (context is Armada2525 game)
        {
            game.SwitchGui("GameSetup");
            // Start the background generator immediately!
            // The UI will show the "Fluff" while this runs.
            //UniverseGenerator.StartGeneration(game);
        }
    }

    private void OnEnterStarMap(IStateContext context)
    {
        if (context is Armada2525 game)
        {
            game.SwitchGui("GalaxyView");
        }
    }

    // --- HELPER ---
    public void SwitchGui(string key)
    {
        if (GUI.ContainsKey(key))
        {
            CurrentGUI = GUI[key];
            // If using InWorldGuiBuilder, you need to tell it to rebuild:
            var builder = GetComponent<InWorldGuiBuilder>();
            if (builder != null) builder.Initialize(CurrentGUI).Build();
        }
    }

    // --- API ---
    public void StartNewGameFlow()
    {
        _transitionToSetup = true; // Triggers transition to "GameSetup"
    }

    public void LaunchGame()
    {
        _transitionToGame = true; // Triggers transition to "StarMap_Strategic"
    }

    // .. Interface implementation stubs ..
    public FSMHandle InjectionAs(ExperienceInjectionContext context, IInputBridge inputBridge) => gameFsmHandle;
    public List<InputRequirement> GetRequiredInputs() => new List<InputRequirement>();

    internal object TransitionToState(string v)
    {
        throw new NotImplementedException();
    }
}