using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_API.Scripts;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.GURPS;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    /// <summary>
    /// The Master Experience Orchestrator.
    /// Manages the global state machine, GUI manifestation, and the underlying data matrix.
    /// Reforged to eliminate stubs and align with the Singularity Forge Protocol.
    /// </summary>
    public class MastersOfOrionII_Game : MonoBehaviour, IInExperienceGame, IStateContext
    {
        [Header("FSM Configuration")]
        public string fsmName = "MastersOfOrionII_GlobalState";
        public string processingGroup = "MastersOfOrionII_Game::GlobalSystems";

        // --- CORE ENGINE ---
        private FSMHandle _gameFsmHandle;
        public bool IsValid { get; set; } = false;
        public string Name { get; set; }

        public Dictionary<string, string> ProcessingGroups { get; } = new Dictionary<string, string>();
        public Dictionary<string, IGuiProvider> GUI = new Dictionary<string, IGuiProvider>();

        // The Active UI Manifestation
        public IGuiProvider CurrentGUI { get; private set; }

        // The "God Mode" Data Matrix
        public GameData Data { get; set; }
        public DataWarehouse Warehouse { get; internal set; }

        private void Awake()
        {
            Name = string.IsNullOrEmpty(name) ? "MOO2_Orchestrator" : name;
            Data = new GameData();

            // Register the global heartbeat group
            var integration = FindAnyObjectByType<FSM_UnityIntegrationAdvanced>();
            if (integration != null)
            {
                integration.AddProcessingGroup("Update", processingGroup);
                ProcessingGroups["Update"] = processingGroup;
            }
        }

        private void Start()
        {
            InitializeGameFSM();
            IsValid = true;
        }

        /// <summary>
        /// Defines the high-level transitions for the experience lifecycle.
        /// </summary>
        private void InitializeGameFSM()
        {
            if (!FSM_API.Interaction.Exists(fsmName, processingGroup))
            {
                FSM_API.Create.CreateFiniteStateMachine(fsmName, processRate: 0, processingGroup: processingGroup)
                    .State("Initialization", OnEnterInitialization, null, null)
                    .State("MainMenu", OnEnterMainMenu, null, null)
                    .State("GameSetup", OnEnterGameSetup, null, null)
                    .State("StarMap_Strategic", OnEnterStarMap, null, null)

                    // Transitions: Managed via manual triggers or data flags
                    .Transition("Initialization", "MainMenu", (ctx) => true)
                    .Transition("MainMenu", "GameSetup", (ctx) => _transitionToSetup)
                    .Transition("GameSetup", "StarMap_Strategic", (ctx) => _transitionToGame)

                    .BuildDefinition();
            }

            _gameFsmHandle = FSM_API.Create.CreateInstance(fsmName, this, processingGroup);
        }

        // --- TRANSITION FLAGS ---
        private bool _transitionToSetup = false;
        private bool _transitionToGame = false;

        // --- FSM STATE CALLBACKS ---

        private void OnEnterInitialization(IStateContext context)
        {
            // Self-referential initialization of GUI providers
            // Using the Reforged providers we created in previous cycles
            GUI["MainMenu"] = new MastersOfOrionII_Gui_MainMenu();
            GUI["GameSetup"] = new MastersOfOrionII_Gui_GameBuilder();
            // GUI["Logistics"] = new LogisticsAPI(Data.Warehouse); // Example link

            SwitchGui("MainMenu");
        }

        private void OnEnterMainMenu(IStateContext context)
        {
            _transitionToSetup = false;
            _transitionToGame = false;
            SwitchGui("MainMenu");
        }

        private void OnEnterGameSetup(IStateContext context)
        {
            SwitchGui("GameSetup");
            // UniverseGenerator.StartGeneration(this); // Trigger background proc
        }

        private void OnEnterStarMap(IStateContext context)
        {
            SwitchGui("GalaxyView");
        }

        // --- FORGE GUI ORCHESTRATION ---

        public void SwitchGui(string key)
        {
            if (GUI.TryGetValue(key, out var provider))
            {
                CurrentGUI = provider;

                // Manifest the UI into the world using the Forge Builder
                var builder = GetComponent<InWorldGuiBuilder>();
                if (builder != null)
                {
                    builder.Initialize(CurrentGUI).Build();
                }

                Debug.Log($"[{Name}] UI Context Switched: {key}");
            }
        }

        // --- PUBLIC API ---

        public void StartNewGameFlow() => _transitionToSetup = true;
        public void LaunchGame() => _transitionToGame = true;

        /// <summary>
        /// Manifests a state change across the quantum bridge.
        /// Fixed: Now correctly routes to the FSM Handle.
        /// </summary>
        public object TransitionToState(string targetState)
        {
            if (_gameFsmHandle != null)
            {
                _gameFsmHandle.TransitionTo(targetState);
                return true;
            }
            return false;
        }

        // --- INTERFACE COMPLIANCE ---
        public FSMHandle InjectionAs(ExperienceInjectionContext context, IInputBridge inputBridge) => _gameFsmHandle;
        public List<InputRequirement> GetRequiredInputs() => new List<InputRequirement>();
    }
}