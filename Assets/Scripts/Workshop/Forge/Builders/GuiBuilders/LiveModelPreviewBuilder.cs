using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class LiveModelPreviewBuilder : IGuiProvider
    {
        public string Title => "Live Preview";
        private GameObject _targetObject;
        private LiveModelPreviewContext _context;

        // Config Defaults
        private float _zoom = 1.5f;
        private float _pitch = 25f;
        private float _yaw = 45f;
        private bool _autoRotate = true;
        private float _rotationSpeed = 15f;
        private Color _backgroundColor = new Color(0.05f, 0.05f, 0.05f);
        private float _lightIntensity = 1.2f;
        private bool _mouseControl = true;
        private bool _multiAxis = false;
        private bool _showGizmos = false;

        private string _processGroup;
        private FSMHandle _fsmHandle;

        public LiveModelPreviewBuilder() { }
        public LiveModelPreviewBuilder(GameObject targetObject) { _targetObject = targetObject; }

        public LiveModelPreviewBuilder WithZoom(float zoom) { _zoom = zoom; return this; }
        public LiveModelPreviewBuilder WithPitch(float pitch) { _pitch = pitch; return this; }
        public LiveModelPreviewBuilder WithYaw(float yaw) { _yaw = yaw; return this; }
        public LiveModelPreviewBuilder WithAutoOrbit(Vector3 axis, float speed) { _autoRotate = true; _rotationSpeed = speed; return this; }
        public LiveModelPreviewBuilder WithBackgroundColor(Color color) { _backgroundColor = color; return this; }
        public LiveModelPreviewBuilder WithLightIntensity(float intensity) { _lightIntensity = intensity; return this; }
        public LiveModelPreviewBuilder WithMouseControl(bool enabled) { _mouseControl = enabled; return this; }
        public LiveModelPreviewBuilder WithMultiAxisRotation(bool enabled) { _multiAxis = enabled; return this; }
        public LiveModelPreviewBuilder WithGizmos(bool enabled) { _showGizmos = enabled; return this; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            Debug.Log("[LiveModelPreviewBuilder] === START CreateGui ===");

            if (_targetObject == null)
            {
                Debug.LogWarning("[LiveModelPreviewBuilder] ABORT: Target Object is NULL. Returning EmptyPreview.");
                return new GraphicalUserInterfaceBuilder("EmptyPreview")
                    .WithBackgroundColor(_backgroundColor)
                    .AddChild(new Label("NO TARGET OBJECT") { style = { color = Color.red } })
                    .Build();
            }

            Debug.Log($"[LiveModelPreviewBuilder] 1. Isolating Object '{_targetObject.name}' to Y:-10000");
            // 1. ISOLATE THE OBJECT (Your Epiphany!)
            // Move it far below the world map to prevent lighting/clipping bleed from the main scene
            _targetObject.transform.position = new Vector3(0, -10000, 0);

            Debug.Log("[LiveModelPreviewBuilder] 2. Pre-allocating RenderTexture.");
            // 2. PRE-ALLOCATE TEXTURE
            var rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            rt.Create();

            Debug.Log("[LiveModelPreviewBuilder] 3. Setting up FSM Context.");
            // 3. SETUP FSM CONTEXT
            _processGroup = "LivePreview_" + Guid.NewGuid().ToString().Substring(0, 6);
            _context = new LiveModelPreviewContext
            {
                TargetModel = _targetObject,
                RenderTexture = rt,
                Zoom = _zoom,
                Pitch = _pitch,
                Yaw = _yaw,
                AutoOrbitEnabled = _autoRotate,
                OrbitSpeed = _rotationSpeed,
                BackgroundColor = _backgroundColor,
                LightIntensity = _lightIntensity,
                MouseControlEnabled = _mouseControl,
                MultiAxisEnabled = _multiAxis,
                ShowGizmos = _showGizmos,
                Name = _processGroup
            };

            Debug.Log("[LiveModelPreviewBuilder] 4. Building UI via GraphicalUserInterfaceBuilder.");
            // 4. BUILD UI STRICTLY WITH BUILDERS
            var rootBuilder = new GraphicalUserInterfaceBuilder("PreviewRoot")
                .WithBackgroundColor(_backgroundColor)
                .OnBuild(ve =>
                {
                    Debug.Log($"[LiveModelPreviewBuilder] UI OnBuild triggered for {_processGroup}. Applying flexGrow = 1.");
                    ve.style.flexGrow = 1;
                    ve.schedule.Execute(() => ve.MarkDirtyRepaint()).Every(16);
                    if (_mouseControl)
                    {
                        ve.RegisterCallback<MouseDownEvent>(evt => {
                            _context.IsDragging = true; _context.LastMousePosition = evt.localMousePosition; ve.CaptureMouse();
                            Debug.Log($"[LiveModelPreviewBuilder] MouseDown captured on {_processGroup}");
                        });
                        ve.RegisterCallback<MouseUpEvent>(evt => {
                            _context.IsDragging = false; ve.ReleaseMouse();
                        });
                        ve.RegisterCallback<MouseMoveEvent>(evt => {
                            if (!_context.IsDragging) return;
                            Vector2 delta = evt.localMousePosition - _context.LastMousePosition;
                            _context.Yaw -= delta.x * 0.5f;
                            _context.Pitch = Mathf.Clamp(_context.Pitch - delta.y * 0.5f, -89f, 89f);
                            _context.LastMousePosition = evt.localMousePosition;
                        });
                        ve.RegisterCallback<WheelEvent>(evt => { _context.Zoom = Mathf.Clamp(_context.Zoom + (evt.delta.y * 0.1f), 0.5f, 10f); });
                    }

                    ve.RegisterCallback<DetachFromPanelEvent>(evt => Cleanup());
                })
                .AddChild(guiCtx => new ImageGuiBuilder(rt)
                    .WithScaleMode(ScaleMode.ScaleToFit)
                    .CreateGui(guiCtx)
                );

            Debug.Log("[LiveModelPreviewBuilder] 5. Validating FSM Definition.");
            // 5. FSM REGISTRATION
            if (!FSM_API.FSM_API.Interaction.Exists("LiveModelPreviewFSM"))
            {
                Debug.Log("[LiveModelPreviewBuilder] Definition 'LiveModelPreviewFSM' missing. Creating now.");
                FSM_API.FSM_API.Create.CreateFiniteStateMachine("LiveModelPreviewFSM", -1, _processGroup) // Use dynamic group
                    .State("Rendering", null, LiveModelPreviewFSM.OnRenderTick, null)
                    .WithInitialState("Rendering")
                    .BuildDefinition();
            }

            Debug.Log($"[LiveModelPreviewBuilder] Creating FSM Instance in group: {_processGroup}");
            // Create the instance in its unique processing group
            _fsmHandle = FSM_API.FSM_API.Create.CreateInstance("LiveModelPreviewFSM", _context, _processGroup);

            Debug.Log("[LiveModelPreviewBuilder] 6. Hooking into Pacemaker.");
            // 6. PACEMAKER HOOK
            // Tell the Runtime Integration to step this FSM on the Update loop
            if (TheSingularityWorkshop.FSM_API.Scripts.FSM_UnityIntegrationAdvanced.Instance != null)
            {
                TheSingularityWorkshop.FSM_API.Scripts.FSM_UnityIntegrationAdvanced.Instance.AddProcessingGroup("Update", _processGroup);
                Debug.Log($"[LiveModelPreviewBuilder] SUCCESS: Added '{_processGroup}' to Pacemaker Update loop.");
            }
            else
            {
                Debug.LogError("[LiveModelPreviewBuilder] PACEMAKER FAILURE: FSM_UnityIntegrationAdvanced.Instance is NULL! The render loop will not tick.");
            }
            
            Debug.Log("[LiveModelPreviewBuilder] === END CreateGui ===");
            return rootBuilder.Build();
        }

        private void Cleanup()
        {
            Debug.Log($"[LiveModelPreviewBuilder] Cleanup triggered for group: {_processGroup}");

            if (_context != null)
            {
                _context.IsValid = false;
                if (_context.PreviewCamera != null)
                {
                    Debug.Log("[LiveModelPreviewBuilder] Destroying Preview Camera.");
                    UnityEngine.Object.DestroyImmediate(_context.PreviewCamera.gameObject);
                }
                if (_context.RenderTexture != null)
                {
                    Debug.Log("[LiveModelPreviewBuilder] Releasing RenderTexture.");
                    _context.RenderTexture.Release();
                }
            }

            // Unhook from the Pacemaker
            if (TheSingularityWorkshop.FSM_API.Scripts.FSM_UnityIntegrationAdvanced.Instance != null)
            {
                Debug.Log($"[LiveModelPreviewBuilder] Removing '{_processGroup}' from Pacemaker.");
                TheSingularityWorkshop.FSM_API.Scripts.FSM_UnityIntegrationAdvanced.Instance.RemoveProcessingGroup("Update", _processGroup);
            }

            if (_fsmHandle != null)
            {
                Debug.Log("[LiveModelPreviewBuilder] Destroying FSM Instance.");
                FSM_API.FSM_API.Interaction.DestroyInstance(_fsmHandle);
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}