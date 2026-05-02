using System;
using System.Linq;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Core;


namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    [ExecuteAlways]
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(BoxCollider))] // Critical for raycast interaction
    public class InWorldGuiBuilder : MonoBehaviour, IStateContext
    {
        [Header("Configuration")]
        public PanelSettings CustomSettingsTemplate;
        [SerializeField] private InWorldGuiDisplaySettings _settings = new InWorldGuiDisplaySettings();

        [Tooltip("Type the name of any IGuiProvider class. We will find it.")]
        public string selectedProviderTypeName;

        // IStateContext
        public string Name { get; set; }
        public bool IsValid { get; set; } = true;
        public FSMHandle Status { get; private set; }

        private IGuiProvider _provider;
        private UIDocument _uiDocument;
        private RenderTexture _renderTexture;
        private Material _screenMaterial;
        private Renderer _screenRenderer;

        public UIDocument Document => _uiDocument; // Exposed for the Interactor

        // THE FIX: Added IsInitialized property for ForgeDisplay orchestration
        public bool IsInitialized => _provider != null;

        private void OnEnable()
        {
            Debug.Log($"[InWorldGui] 🟢 OnEnable triggered for {gameObject.name}");

            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null) Debug.LogError($"[InWorldGui] ❌ Missing UIDocument on {gameObject.name}");

            if (string.IsNullOrEmpty(Name))
            {
                Name = $"GuiOS_{Guid.NewGuid().ToString().Substring(0, 4)}";
                Debug.Log($"[InWorldGui] Assigned new OS Name: {Name}");
            }

            if (!string.IsNullOrEmpty(selectedProviderTypeName) && _provider == null)
            {
                Debug.Log($"[InWorldGui] Provider is null. Attempting to resolve: {selectedProviderTypeName}");
                ResolveProviderFromType(selectedProviderTypeName);
            }

            Debug.Log("[InWorldGui] Initializing FSM...");
            InitializeFsm();

            // Hook into your custom Forge Domain Reload lifecycle
            ForgeDomainConductor.OnBeforeDomainReload -= HandleBeforeReload;
            ForgeDomainConductor.OnBeforeDomainReload += HandleBeforeReload;

            ForgeDomainConductor.OnAfterDomainReload -= HandleAfterReload;
            ForgeDomainConductor.OnAfterDomainReload += HandleAfterReload;
        }

        private void Start()
        {
            // Catches normal initializations (Scene Open, entering Play Mode without reload)
            if (_uiDocument != null && _uiDocument.rootVisualElement != null && _uiDocument.rootVisualElement.childCount == 0)
            {
                Debug.Log("[InWorldGui] Forcing initial Build() from Start...");
                Build();
            }
        }

        private void OnDisable()
        {
            // Always clean up your static event subscriptions
            ForgeDomainConductor.OnBeforeDomainReload -= HandleBeforeReload;
            ForgeDomainConductor.OnAfterDomainReload -= HandleAfterReload;
        }

        public void SetResolutionSettings(Vector2 physicalSizeMeters, int textureWidthPx)
        {
            Debug.Log($"[InWorldGui] SetResolutionSettings called: Physical {physicalSizeMeters}, TexWidth {textureWidthPx}");
            _settings.physicalSize = physicalSizeMeters;
            _settings.targetTextureWidth = textureWidthPx;
            if (_uiDocument != null) ConfigureProjection();
        }

        public InWorldGuiBuilder Initialize(IGuiProvider provider)
        {
            Debug.Log($"[InWorldGui] Initialize injected with provider: {provider?.Title ?? "NULL"}");
            _provider = provider;
            return this;
        }

        public void Build()
        {
            Debug.Log("[InWorldGui] 🏗️ Build() Initiated...");
            if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();

            ConfigureProjection();

            if (_provider != null)
            {
                Debug.Log($"[InWorldGui] Creating GUI from provider: {_provider.Title}");
                var ctx = new GuiContext
                {
                    Name = gameObject.name,
                    Log = (msg) => Debug.Log($"[GuiContext] {msg}"), // Route internal UI logs
                    EditMode = false, // Always False for In-World
                    Controls = new RuntimeControlFactory()
                };

                var root = _provider.CreateGui(ctx);
                if (root != null)
                {
                    Debug.Log("[InWorldGui] Root VisualElement created successfully. Attaching to UIDocument.");

                    // Null check protects against out-of-order execution
                    if (_uiDocument.rootVisualElement == null)
                    {
                        Debug.LogWarning("[InWorldGui] ⚠️ UIDocument.rootVisualElement is not ready yet. Aborting build.");
                        return;
                    }

                    _uiDocument.rootVisualElement.Clear();
                    _uiDocument.rootVisualElement.Add(root);
                    ctx.OnBuilt?.Invoke(root);
                    root.MarkDirtyRepaint();
                }
                else
                {
                    Debug.LogWarning("[InWorldGui] Provider returned a NULL VisualElement.");
                }
            }
            else
            {
                Debug.LogWarning("[InWorldGui] ⚠️ Build aborted: No provider is assigned.");
            }
        }

        public void MarkDirty()
        {
            if (_uiDocument != null && _uiDocument.rootVisualElement != null)
            {
                _uiDocument.rootVisualElement.MarkDirtyRepaint();
            }
        }

        private void ConfigureProjection()
        {
            Debug.Log("[InWorldGui] 📽️ Configuring Projection...");
            EnsurePanelSettings();
            var ps = _uiDocument.panelSettings;
            if (ps == null)
            {
                Debug.LogError("[InWorldGui] ❌ PanelSettings is null after EnsurePanelSettings!");
                return;
            }

            Vector2 finalPhysicalSize = _settings.physicalSize;
            int texWidth = _settings.targetTextureWidth;

            // Aspect Ratio
            float aspectRatio = finalPhysicalSize.x / Mathf.Max(0.001f, finalPhysicalSize.y);
            Debug.Log($"[InWorldGui] Calculated Aspect Ratio: {aspectRatio} (Physical: {finalPhysicalSize})");

            // A. TEXTURE RESOLUTION (Sharpness)
            int texHeight = Mathf.RoundToInt(texWidth / aspectRatio);
            Debug.Log($"[InWorldGui] Target RenderTexture Resolution: {texWidth}x{texHeight}");

            // B. REFERENCE RESOLUTION (Layout Size)
            float baseWidth = 1920f;
            float virtualWidth = baseWidth / Mathf.Max(0.1f, _settings.uiScale);
            int refWidth = Mathf.RoundToInt(virtualWidth);
            int refHeight = Mathf.RoundToInt(virtualWidth / aspectRatio);
            Debug.Log($"[InWorldGui] Target Reference Resolution (Virtual Space): {refWidth}x{refHeight}");

            EnsureRenderTexture(texWidth, texHeight);

            ps.targetTexture = _renderTexture;
            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            ps.match = 0.5f;
            ps.referenceResolution = new Vector2Int(refWidth, refHeight);
            ps.clearColor = true;

            ConfigureHardware();
        }

        private void EnsurePanelSettings()
        {
            if (_uiDocument.panelSettings != null && _uiDocument.panelSettings.name != "InWorldPanelSettings")
            {
                Debug.Log("[InWorldGui] Using existing Custom PanelSettings.");
                return;
            }

            if (CustomSettingsTemplate != null)
            {
                Debug.Log("[InWorldGui] Instantiating PanelSettings from Custom Template.");
                _uiDocument.panelSettings = Instantiate(CustomSettingsTemplate);
                return;
            }

            Debug.Log("[InWorldGui] Creating generic instance of PanelSettings.");
            _uiDocument.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        }

        private void ConfigureHardware()
        {
            Debug.Log("[InWorldGui] 📺 Configuring Hardware (Mesh/Material)...");
            _screenRenderer = GetComponent<Renderer>();
            if (_screenRenderer == null)
            {
                Debug.LogWarning("[InWorldGui] No Renderer found. Generating screen Quad...");
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.SetParent(this.transform, false);
                quad.name = "Generated_Screen_Surface";
                _screenRenderer = quad.GetComponent<MeshRenderer>();

                // Replace default mesh collider with BoxCollider for better UI Raycasting
                if (Application.isPlaying) Destroy(quad.GetComponent<Collider>());
                else DestroyImmediate(quad.GetComponent<Collider>());

                quad.AddComponent<BoxCollider>();
            }
            else if (GetComponent<Collider>() == null)
            {
                gameObject.AddComponent<BoxCollider>();
            }

            if (_screenMaterial == null)
            {
                Debug.Log("[InWorldGui] Creating new Material for Screen...");
                var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Transparent");
                _screenMaterial = new Material(shader);
                _screenRenderer.sharedMaterial = _screenMaterial;
            }

            if (_renderTexture != null)
            {
                Debug.Log("[InWorldGui] Binding RenderTexture to Material...");
                if (_screenMaterial.HasProperty("_BaseMap")) _screenMaterial.SetTexture("_BaseMap", _renderTexture);
                else _screenMaterial.mainTexture = _renderTexture;
            }
            else
            {
                Debug.LogError("[InWorldGui] ❌ Cannot bind material: RenderTexture is null.");
            }
        }

        private void EnsureRenderTexture(int width, int height)
        {
            if (_renderTexture != null && _renderTexture.IsCreated() &&
                _renderTexture.width == width && _renderTexture.height == height)
            {
                Debug.Log("[InWorldGui] RenderTexture already matches target specs. Skipping creation.");
                return;
            }

            Debug.Log($"[InWorldGui] Creating new RenderTexture ({width}x{height})...");
            if (_renderTexture != null) _renderTexture.Release();

            _renderTexture = new RenderTexture(width, height, 24)
            {
                name = $"{name}_GuiTexture",
                useMipMap = false,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 0
            };
            _renderTexture.Create();
        }

        private void InitializeFsm()
        {
            string fsmId = "InWorldGui_OS";
            string pathway = Application.isPlaying ? "Panels" : "EditorUpdate";

            Debug.Log($"[InWorldGui] Checking FSM Ecosystem for '{fsmId}' on pathway '{pathway}'...");
            if (!FSM_API.Interaction.Exists(fsmId, pathway))
            {
                Debug.Log($"[InWorldGui] FSM Definition not found. Creating '{fsmId}'...");
                FSM_API.Create.CreateFiniteStateMachine(fsmId, -1, pathway)
                    .State("Running", ctx => { /* Logic */ }, null, null)
                    .BuildDefinition();
            }
            else
            {
                Debug.Log($"[InWorldGui] FSM Definition '{fsmId}' already exists.");
            }

            Status = FSM_API.Create.CreateInstance(fsmId, this, pathway);
            Debug.Log($"[InWorldGui] FSM Instance Attached. Status Valid: {Status.IsValid}");
        }

        public void ResolveProviderFromType(string searchName)
        {
            Debug.Log($"[InWorldGui] Hunting across assemblies for IGuiProvider matching: '{searchName}'");
            selectedProviderTypeName = searchName;

            Type foundType = null;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var asm in assemblies)
            {
                // Find any class that implements IGuiProvider and matches the name (ignoring case)
                foundType = asm.GetTypes().FirstOrDefault(t =>
                    typeof(IGuiProvider).IsAssignableFrom(t) &&
                    !t.IsInterface &&
                    !t.IsAbstract &&
                    t.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase));

                if (foundType != null) break;
            }

            if (foundType != null)
            {
                try
                {
                    _provider = (IGuiProvider)Activator.CreateInstance(foundType);
                    Debug.Log($"[InWorldGui] 🎯 Success! Instantiated provider: {foundType.FullName}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[InWorldGui] ❌ Failed to instantiate {foundType.Name}: {ex.Message}");
                }
            }
            else
            {
                Debug.LogError($"[InWorldGui] ❌ Could not find any IGuiProvider named: {searchName}. Did you type the name correctly?");
            }
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                _uiDocument.rootVisualElement?.MarkDirtyRepaint();

                // Self-healing: If the texture was released by the GPU, rebuild
                if (_renderTexture == null || !_renderTexture.IsCreated())
                {
                    Debug.LogWarning("[InWorldGui] 🚨 GPU dropped the RenderTexture (Domain Reload?). Forcing Rebuild.");
                    Build();
                }
            }
        }

        private void OnDestroy()
        {
            Debug.Log($"[InWorldGui] 🛑 OnDestroy called for {gameObject.name}. Releasing assets.");
            IsValid = false;
            if (_renderTexture != null) _renderTexture.Release();
        }

        private void HandleBeforeReload()
        {
            Debug.Log("[InWorldGui] Domain Reload impending. Safely suspending GUI...");

            // Optional: If your FSM needs to gracefully pause, register it with the manager here:
            // ForgeDomainReloadConductor.RegisterSuspendTask(Status);

            if (_uiDocument != null && _uiDocument.rootVisualElement != null)
            {
                _uiDocument.rootVisualElement.Clear();
            }
        }

        private void HandleAfterReload()
        {
            Debug.Log("[InWorldGui] Domain Reload complete. Rebuilding GUI...");

            // Optional: Register the resume task with your manager here if needed
            // ForgeDomainReloadConductor.RegisterResumeTask(Status);

            Build();
        }
    }
}