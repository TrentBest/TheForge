// File: Assets/Scripts/Workshop/Forge/TheForge.cs
using TheSingularityWorkshop.Forge.Builders;
using TheSingularityWorkshop.Forge.Builders.Casting;
using TheSingularityWorkshop.Forge.Builders.CauseEffect;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.Staging;
using TheSingularityWorkshop.Forge.Builders.Timing;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace TheSingularityWorkshop.Forge
{
    public class TheForge : MonoBehaviour, IStateContext
    {
        [Header("Configuration")]
        public GameObject DisplayPrefab;
        public float SpawnDistance = 1.5f;

        [Header("Tools")]
        public CastBuilder CastTool;
        public StageBuilder StageTool;
        public TimeBuilder TimeTool;
        public CauseEffectBuilder LogicTool;
        public CauseEffectBuilder SystemTool;

        [SerializeField] private bool _isEstablished = false;
        
        public bool IsValid { get; set; } = false;
        public string Name { get; set; } = "TheForge";
        public FSMHandle Status { get; private set; }

        private List<ForgeDisplay> _activeDisplays = new List<ForgeDisplay>();
        private ExperienceContext _context;
        private List<ForgeDisplay> _allDisplays = new List<ForgeDisplay>();

        void Awake()
        {
            // Auto-Discovery
            if (!CastTool) CastTool = GetIntrinsicTool<CastBuilder>();
            if (!StageTool) StageTool = GetIntrinsicTool<StageBuilder>();
            if (!TimeTool) TimeTool = GetIntrinsicTool<TimeBuilder>();
            if (!LogicTool) LogicTool = GetIntrinsicTool<CauseEffectBuilder>();
            if (!SystemTool) SystemTool = GetIntrinsicTool<CauseEffectBuilder>();

            _context = FindAnyObjectByType<ExperienceContext>();
            InitializeFsm();
        }

        private T GetIntrinsicTool<T>() where T : Component => GetComponentInChildren<T>(true);

        private void InitializeFsm()
        {
            if ( !FSM_API.FSM_API.Interaction.Exists("TheForge_OS"))
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
           
            foreach(var d in _activeDisplays) if(d) Destroy(d.gameObject);
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
            // Simple layout logic: Spacing out 3 panels
            // We no longer track 'angles', just relative positions
            Vector3 center = transform.position + (transform.forward * SpawnDistance);
            Vector3 left = center - (transform.right * 1.0f);
            Vector3 right = center + (transform.right * 1.0f);

            SpawnTool(CastTool, left, "Display_Cast");
            SpawnTool(StageTool, center, "Display_Stage");
            SpawnTool(TimeTool, right, "Display_Time");
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