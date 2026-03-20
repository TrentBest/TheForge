using TheSingularityWorkshop.Builders;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    [ExecuteAlways]
    [RequireComponent(typeof(UIDocument))]
    public class InWorldGuiBuilder : MonoBehaviour, IStateContext
    {
        [Header("Configuration")]
        public PanelSettings CustomSettingsTemplate;
        [SerializeField] private InWorldGuiDisplaySettings _settings = new InWorldGuiDisplaySettings();
        [HideInInspector] public string selectedProviderTypeName;

        // IStateContext
        public string Name { get; set; }
        public bool IsValid { get; set; } = true;
        public FSMHandle Status { get; private set; }

        private IGuiProvider _provider;
        private UIDocument _uiDocument;
        private RenderTexture _renderTexture;
        private Material _screenMaterial;
        private Renderer _screenRenderer;

        private void OnEnable()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (string.IsNullOrEmpty(Name)) Name = $"GuiOS_{Guid.NewGuid().ToString().Substring(0, 4)}";

            if (!string.IsNullOrEmpty(selectedProviderTypeName) && _provider == null)
                ResolveProviderFromType(selectedProviderTypeName);

            // Initialize FSM regardless of Play Mode (Editor Kernel drives it now)
            InitializeFsm();

            // Force a fresh build to recover from Domain Reloads
            Build();
        }

        public void SetResolutionSettings(Vector2 physicalSizeMeters, int textureWidthPx)
        {
            _settings.physicalSize = physicalSizeMeters;
            _settings.targetTextureWidth = textureWidthPx;
            if (_uiDocument != null) ConfigureProjection();
        }

        public InWorldGuiBuilder Initialize(IGuiProvider provider)
        {
            _provider = provider;
            return this;
        }

        public void Build()
        {
            if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();
            ConfigureProjection();

            if (_provider != null)
            {
                var ctx = new GuiContext
                {
                    Name = gameObject.name,
                    Log = Debug.Log,
                    EditMode = false, // Always False for In-World
                    Controls = new RuntimeControlFactory()
                };

                var root = _provider.CreateGui(ctx);
                if (root != null)
                {
                    _uiDocument.rootVisualElement.Clear();
                    _uiDocument.rootVisualElement.Add(root);
                    ctx.OnBuilt?.Invoke(root);
                    root.MarkDirtyRepaint();
                }
            }
        }

        /// <summary>
        /// Call this to force the UI to repaint immediately.
        /// </summary>
        public void MarkDirty()
        {
            if (_uiDocument != null && _uiDocument.rootVisualElement != null)
            {
                _uiDocument.rootVisualElement.MarkDirtyRepaint();
            }
        }

        private void ConfigureProjection()
        {
            EnsurePanelSettings();
            var ps = _uiDocument.panelSettings;
            if (ps == null) return;

            Vector2 finalPhysicalSize = _settings.physicalSize;
            int texWidth = _settings.targetTextureWidth;

            // Aspect Ratio
            float aspectRatio = finalPhysicalSize.x / Mathf.Max(0.001f, finalPhysicalSize.y);

            // A. TEXTURE RESOLUTION (Sharpness)
            // Keep this high (e.g. 2048+) so the pixels are crisp
            int texHeight = Mathf.RoundToInt(texWidth / aspectRatio);

            // B. REFERENCE RESOLUTION (Layout Size)
            // We scale the "Virtual Screen" down to make elements look bigger.
            // If uiScale is 2.0, virtual width is 960px. A 20px font acts like a 40px font.
            float baseWidth = 1920f;
            float virtualWidth = baseWidth / Mathf.Max(0.1f, _settings.uiScale);
            int refWidth = Mathf.RoundToInt(virtualWidth);
            int refHeight = Mathf.RoundToInt(virtualWidth / aspectRatio);

            EnsureRenderTexture(texWidth, texHeight);

            ps.targetTexture = _renderTexture;
            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            ps.match = 0.5f; // Balance width/height scaling

            // Apply the virtual size
            ps.referenceResolution = new Vector2Int(refWidth, refHeight);
            ps.clearColor = true;

            ConfigureHardware();
        }

        private void EnsurePanelSettings()
        {
            if (_uiDocument.panelSettings != null && _uiDocument.panelSettings.name != "InWorldPanelSettings") return;
            if (CustomSettingsTemplate != null)
            {
                _uiDocument.panelSettings = Instantiate(CustomSettingsTemplate);
                return;
            }
            _uiDocument.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        }

        private void ConfigureHardware()
        {
            _screenRenderer = GetComponent<Renderer>();
            if (_screenRenderer == null)
            {
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.SetParent(this.transform, false);
                quad.name = "Generated_Screen_Surface";
                _screenRenderer = quad.GetComponent<MeshRenderer>();
                DestroyImmediate(quad.GetComponent<Collider>());
                quad.AddComponent<BoxCollider>();
            }

            if (_screenMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Transparent");
                _screenMaterial = new Material(shader);
                _screenRenderer.sharedMaterial = _screenMaterial;
            }

            if (_renderTexture != null)
            {
                if (_screenMaterial.HasProperty("_BaseMap")) _screenMaterial.SetTexture("_BaseMap", _renderTexture);
                else _screenMaterial.mainTexture = _renderTexture;
            }
        }

        private void EnsureRenderTexture(int width, int height)
        {
            // If dimensions match and texture exists, do nothing
            if (_renderTexture != null && _renderTexture.IsCreated() &&
                _renderTexture.width == width && _renderTexture.height == height)
                return;

            if (_renderTexture != null) _renderTexture.Release();

            _renderTexture = new RenderTexture(width, height, 24)
            {
                name = $"{name}_GuiTexture",
              
                useMipMap = false,
                filterMode = FilterMode.Bilinear, // Bilinear keeps edges cleaner than Trilinear for UI
                anisoLevel = 0 // Aniso doesn't help much without mips
            };
            _renderTexture.Create();
        }

        private void InitializeFsm()
        {
            string fsmId = "InWorldGui_OS";
            string pathway = Application.isPlaying ? "Panels" : "EditorUpdate"; // Use the Editor Kernel pathway if not playing

            if ( !FSM_API.FSM_API.Interaction.Exists(fsmId))
            {
                FSM_API.FSM_API.Create.CreateFiniteStateMachine(fsmId, -1, pathway)
                    .State("Running", ctx => { /* Logic */ }, null, null)
                    .BuildDefinition();
            }
            Status = FSM_API.FSM_API.Create.CreateInstance(fsmId, this, pathway);
        }

        public void ResolveProviderFromType(string typeName)
        {
            selectedProviderTypeName = typeName;
            var type = System.Type.GetType(typeName) ?? System.Type.GetType($"TheSingularityWorkshop.{typeName}");
            if (type != null)
            {
                try { _provider = (IGuiProvider)Activator.CreateInstance(type); } catch { }
            }
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                // 1. Force the UI Toolkit to repaint every frame the Editor ticks
                _uiDocument.rootVisualElement?.MarkDirtyRepaint();

                // 2. Self-healing: If the texture was released by the GPU, rebuild
                if (_renderTexture == null || !_renderTexture.IsCreated())
                {
                    Build();
                }
            }
        }

        private void OnDestroy()
        {
            IsValid = false;
            if (_renderTexture != null) _renderTexture.Release();
        }
    }
}