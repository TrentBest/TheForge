using System;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API.Scripts;


#if UNITY_EDITOR
using UnityEditor.UIElements;

#endif

namespace TheSingularityWorkshop.Forge
{
    public enum DisplayPreset
    {
        Custom,
        Wristwatch,   // ~0.04m (1.5" Diagonal)
        Phone,        // ~0.07m (6" Diagonal)
        Tablet,       // ~0.20m (10" Diagonal)
        Monitor24,    // ~0.53m (24" Diagonal)
        Monitor34UW,  // ~0.80m (34" Ultrawide)
        TV55,         // ~1.22m (55" Diagonal)
        TV85,         // ~1.90m (85" Diagonal)
        Wall,         // 3.00m  (120" Projector / LED Wall)
        BillboardSm,  // 3.40m  (Junior Poster)
        BillboardMed, // 6.70m  (Urban Poster)
        BillboardLg   // 14.6m  (Highway Bulletin)
    }

    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(InWorldGuiBuilder))]
    public class ForgeDisplay : MonoBehaviour, IStateContext
    {

        [Header("Configuration")]
        public DisplayPreset Preset = DisplayPreset.Custom;
        public Vector2 PhysicalSize = new Vector2(1.2f, 0.7f); // Default to TV55
        [Range(500, 4000)] public int PixelsPerMeter = 2000;

        [Header("Interaction Settings")]
        public bool IsLocked = false;
        public float GripThickness = 0.05f; // Size of the move bar

        [Header("Hardware References")]
        public Transform SlateRoot;
        public Transform BezelRoot;

        // Generated Handles
        private Transform _handlesRoot;
        private BoxCollider _moveHandle;
        private BoxCollider _resizeHandle;

        private InWorldGuiBuilder _guiRenderer;
        private BoxCollider _mainCollider;

        // IStateContext
        public string Name { get; set; }
        public bool IsValid { get; set; } = true;
        public FSMHandle Status { get; private set; }

        private void OnEnable() // Changed from Awake/Start to OnEnable for Editor lifecycle reliability
        {
            _guiRenderer = GetComponent<InWorldGuiBuilder>();
            _mainCollider = GetComponent<BoxCollider>();

            if (string.IsNullOrEmpty(Name)) Name = $"Display_{Guid.NewGuid().ToString().Substring(0, 4)}";

            GenerateInteractionHandles();
            InitializeBehaviorFsm();

            // Ensure visual state is correct on load/compile
            ApplyPreset(Preset);
        }

       
        public void ApplyPreset(DisplayPreset preset)
        {
            Preset = preset;
            switch (preset)
            {
                case DisplayPreset.Wristwatch: Resize(0.04f, 0.04f); break;
                case DisplayPreset.Phone: Resize(0.07f, 0.15f); break;
                case DisplayPreset.Tablet: Resize(0.25f, 0.18f); break;
                case DisplayPreset.Monitor24: Resize(0.53f, 0.30f); break;
                case DisplayPreset.Monitor34UW: Resize(0.80f, 0.34f); break;
                case DisplayPreset.TV55: Resize(1.22f, 0.69f); break;
                case DisplayPreset.TV85: Resize(1.90f, 1.05f); break;
                case DisplayPreset.Wall: Resize(2.65f, 1.50f); break;
                case DisplayPreset.BillboardSm: Resize(3.40f, 1.80f); break;
                case DisplayPreset.BillboardMed: Resize(6.70f, 3.60f); break;
                case DisplayPreset.BillboardLg: Resize(14.60f, 4.30f); break;
            }
        }

        public void Resize(float width, float height)
        {
            PhysicalSize = new Vector2(Mathf.Max(0.02f, width), Mathf.Max(0.02f, height));
            RefreshPhysicalGeometry();

            // Re-configure renderer if initialized
            if (_guiRenderer != null)
            {
                int targetWidth = Mathf.RoundToInt(PhysicalSize.x * PixelsPerMeter);
                _guiRenderer.SetResolutionSettings(PhysicalSize, targetWidth);
            }
        }

        public void SetManipulationMode(bool enabled)
        {
            if (_handlesRoot) _handlesRoot.gameObject.SetActive(enabled);
        }

        public void Initialize(IGuiProvider directProvider)
        {
            Name = $"Display_{directProvider.Title}";
            ConfigureRenderer();
            _guiRenderer.Initialize(directProvider);
            _guiRenderer.Build();
        }

        public void Initialize(IForgeBuilder tool)
        {
            Name = tool != null ? $"Display_{tool.ToolName}" : $"Display_Empty";
            IGuiProvider contentProvider = tool?.GetGuiProvider() ?? new StandbyProvider(tool?.ToolName ?? "Standby");

            ConfigureRenderer();
            _guiRenderer.Initialize(contentProvider);
            _guiRenderer.Build();
        }



        private void ConfigureRenderer()
        {
            int targetWidth = Mathf.RoundToInt(PhysicalSize.x * PixelsPerMeter);
            _guiRenderer.SetResolutionSettings(PhysicalSize, targetWidth);
        }

        private void RefreshPhysicalGeometry()
        {
            if (SlateRoot) SlateRoot.localScale = new Vector3(PhysicalSize.x, PhysicalSize.y, 1f);
            if (BezelRoot) BezelRoot.localScale = new Vector3(PhysicalSize.x, PhysicalSize.y, 1f);
            if (_mainCollider) _mainCollider.size = new Vector3(PhysicalSize.x, PhysicalSize.y, 0.02f);
            UpdateHandleGeometry();
        }

        private void GenerateInteractionHandles()
        {
            // Only generate if missing or destroyed
            if (_handlesRoot != null) return;

            // (Keep generation logic exactly the same as your file)
            _handlesRoot = new GameObject("InteractionHandles").transform;
            _handlesRoot.SetParent(transform, false);

            // A. Move Handle
            var moveGo = new GameObject("Handle_Move");
            moveGo.transform.SetParent(_handlesRoot, false);
            moveGo.layer = gameObject.layer;
            _moveHandle = moveGo.AddComponent<BoxCollider>();
            var moveVis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            moveVis.transform.SetParent(moveGo.transform, false);
            DestroyImmediate(moveVis.GetComponent<Collider>());
            moveVis.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            moveVis.GetComponent<Renderer>().sharedMaterial.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

            // B. Resize Handle
            var resizeGo = new GameObject("Handle_Resize");
            resizeGo.transform.SetParent(_handlesRoot, false);
            resizeGo.layer = gameObject.layer;
            _resizeHandle = resizeGo.AddComponent<BoxCollider>();
            var resizeVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            resizeVis.transform.SetParent(resizeGo.transform, false);
            DestroyImmediate(resizeVis.GetComponent<Collider>());
            resizeVis.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            resizeVis.GetComponent<Renderer>().sharedMaterial.color = new Color(0f, 0.6f, 1f, 0.8f);

            _handlesRoot.gameObject.SetActive(false);
            UpdateHandleGeometry();
        }

        private void UpdateHandleGeometry()
        {
            if (!_moveHandle) return;
            _moveHandle.transform.localPosition = new Vector3(0, (PhysicalSize.y / 2) + (GripThickness / 2), 0);
            _moveHandle.size = new Vector3(PhysicalSize.x, GripThickness, GripThickness);
            if (_moveHandle.transform.childCount > 0) _moveHandle.transform.GetChild(0).localScale = _moveHandle.size;

            _resizeHandle.transform.localPosition = new Vector3((PhysicalSize.x / 2), -(PhysicalSize.y / 2), 0);
            _resizeHandle.size = Vector3.one * (GripThickness * 2);
            if (_resizeHandle.transform.childCount > 0) _resizeHandle.transform.GetChild(0).localScale = _resizeHandle.size;
        }

        private void InitializeBehaviorFsm()
        {
            // Register to "Panels" so the Editor Integration drives it
            if ( !FSM_API.FSM_API.Interaction.Exists("ForgeDisplay_Behavior"))
            {
                FSM_API.FSM_API.Create.CreateFiniteStateMachine("ForgeDisplay_Behavior", -1, "Panels")
                    .State("Initialization", OnEnterInitialization, null, null)
                    .State("Idle", null, null, null)
                    .BuildDefinition();
            }
            Status = FSM_API.FSM_API.Create.CreateInstance("ForgeDisplay_Behavior", this, "Panels");
        }

        private void OnEnterInitialization(IStateContext context)
        {
            if (context is ForgeDisplay display)
            {
                //we need to register the
            }
        }

        private class StandbyProvider : IGuiProvider
        {
            private string _title;
            public string Title => _title;
            public StandbyProvider(string title) { _title = title; }
            public Action<VisualElement> GetGuiBuilder() => null;
            public VisualElement CreateGui(GuiContext ctx) => new Label(_title);

            public void ToUIDocument(string assetPath)
            {
                throw new NotImplementedException();
            }

            public void FromUIDocument(string assetPath)
            {
                throw new NotImplementedException();
            }
        }
    }
}