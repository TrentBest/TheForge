// File: Assets/Scripts/Workshop/Forge/TheForge.cs
using System.Collections.Generic;
using TheSingularityWorkshop.Forge.Builders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace TheSingularityWorkshop.Forge
{
    public class TheForge : MonoBehaviour, IStateContext
    {
        [Header("Configuration")]
        public GameObject DisplayPrefab;
        public float SpawnDistance = 1.5f;

        // Dynamic registry for all available tooling
        public Dictionary<string, IForgeBuilder> tooling = new Dictionary<string, IForgeBuilder>();

        [SerializeField] private bool _isEstablished = false;

        public bool IsValid { get; set; } = false;
        public string Name { get; set; } = "TheForge";
        public FSMHandle Status { get; private set; }

        private List<ForgeDisplay> _activeDisplays = new List<ForgeDisplay>();
        private ExperienceContext _context;

        void Awake()
        {
            // Auto-Discovery: Find all IForgeBuilders attached to this or its children
            var intrinsicTools = GetComponentsInChildren<IForgeBuilder>(true);
            foreach (var tool in intrinsicTools)
            {
                if (tool != null && !string.IsNullOrEmpty(tool.ToolName) && !tooling.ContainsKey(tool.ToolName))
                {
                    tooling.Add(tool.ToolName, tool);
                }
            }

            _context = FindAnyObjectByType<ExperienceContext>();
            InitializeFsm();
        }

        private void InitializeFsm()
        {
            if (!FSM_API.FSM_API.Interaction.Exists("TheForge_OS"))
            {
                FSM_API.FSM_API.Create.CreateFiniteStateMachine("TheForge_OS", -1, "Workshop")
                    .State("Boot", OnBoot, null, null)
                    .State("Questionnaire", OnQuestionnaireEnter, null, null)
                    .State("Running", OnRunningEnter, null, null)
                    .Transition("Boot", "Questionnaire", ctx => IsExperienceReady() && IsGenesisMode())
                    .Transition("Boot", "Running", ctx => IsExperienceReady() && !IsGenesisMode())
                    .Transition("Questionnaire", "Running", ctx => _isEstablished)
                    .BuildDefinition();
            }
            Status = FSM_API.FSM_API.Create.CreateInstance("TheForge_OS", this, "Workshop");
            IsValid = true;
        }

        private bool IsExperienceReady() => _context && _context.IsValid && _context.GetCurrentExperience() != null;
        private bool IsGenesisMode() => _context.GetCurrentExperience()?.Id == "Genesis_Workshop";

        private void OnBoot(IStateContext ctx) { if (!_context) _context = FindAnyObjectByType<ExperienceContext>(); }

        private void OnQuestionnaireEnter(IStateContext ctx)
        {
            Debug.Log("[TheForge] Entering Genesis Mode. Spawning Questionnaire..");
            SpawnInitialQuestionnaire();
        }

        private void OnRunningEnter(IStateContext ctx)
        {
            Debug.Log("[TheForge] Entering Running Mode. Spawning Workspace..");

            foreach (var d in _activeDisplays) if (d) Destroy(d.gameObject);
            _activeDisplays.Clear();

            SpawnDefaultWorkshopLayout();
        }

        private void SpawnInitialQuestionnaire()
        {
            var provider = new QuestionnaireProvider(() => SetEstablished(true));

            // 1. Calculate Spawn Position (Directly in front of Forge/User)
            Vector3 spawnPos = transform.position + (transform.forward * SpawnDistance);
            spawnPos.y += 0.2f; // Slight eye-level adjustment

            // 2. Spawn & Resize
            var display = SpawnRawDisplay(spawnPos, "Display_Genesis");
            if (display)
            {
                // FORCE THE 72" SIZE
                display.Resize(1.6f, 0.9f);
                display.Initialize(provider);
            }
        }

        private void SpawnDefaultWorkshopLayout()
        {
            int toolCount = tooling.Count;
            if (toolCount == 0) return;

            Vector3 center = transform.position + (transform.forward * SpawnDistance);

            // Dynamically calculate spacing to center the group of tools
            float spacing = 1.0f;
            float startOffset = -((toolCount - 1) * spacing) / 2f;

            int index = 0;
            foreach (var kvp in tooling)
            {
                string toolName = kvp.Key;
                IForgeBuilder tool = kvp.Value;

                // Position them in a linear array. 
                // Next step for the OS: Curve this into a cylindrical projection around the user.
                Vector3 pos = center + (transform.right * (startOffset + (index * spacing)));

                SpawnTool(tool, pos, $"Display_{toolName}");
                index++;
            }
        }

        private void SpawnTool(IForgeBuilder tool, Vector3 position, string name)
        {
            var display = SpawnRawDisplay(position, name);
            if (display)
            {
                // Standard Tool Size
                display.Resize(0.8f, 0.6f);
                display.Initialize(tool);
            }
        }

        private ForgeDisplay SpawnRawDisplay(Vector3 worldPos, string name)
        {
            if (!DisplayPrefab) return null;

            // Look at the Forge center (User)
            Quaternion rotation = Quaternion.LookRotation(transform.position - worldPos);
            rotation *= Quaternion.Euler(0, 180, 0); // Correct for back-face

            var go = Instantiate(DisplayPrefab, worldPos, rotation, transform);
            go.name = name;

            var display = go.GetComponent<ForgeDisplay>();
            if (display) _activeDisplays.Add(display);

            return display;
        }

        public void SetEstablished(bool state) => _isEstablished = state;
    }
}