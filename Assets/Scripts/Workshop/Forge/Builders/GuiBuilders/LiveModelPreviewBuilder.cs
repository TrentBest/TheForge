using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Runtime;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.Memory; // Required for ForgeDiegeticTerminal

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class LiveModelPreviewBuilder : IGuiProvider, IDisposable
    {
        public string Title => "Live Model Preview";

        private GameObject _targetObject;
        private GameObject _previewModel;
        private Camera _previewCamera;
        private RenderTexture _renderTexture;
        private RuntimeViewGizmo _viewGizmo;

        private float _zoom = 5f;
        private float _pitch = 30f;
        private float _yaw = 45f;
        private bool _autoRotate = false;
        private float _rotationSpeed = 10f;
        private Color _backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        private float _lightIntensity = 1.0f;
        private bool _mouseControl = true;
        private bool _multiAxis = true;
        private bool _showGizmos = true;

        private LiveModelPreviewContext _context;
        private string _processGroup;
        private FSMHandle _fsmHandle;

        // Model boundaries
        private Vector3 _modelCenter;
        private float _modelSize = 1f;

        public LiveModelPreviewBuilder(GameObject targetModel)
        {
            _targetObject = targetModel;
        }

        public LiveModelPreviewBuilder WithBackgroundColor(Color color) { _backgroundColor = color; return this; }
        public LiveModelPreviewBuilder WithAutoRotate(bool autoRotate, float speed = 10f) { _autoRotate = autoRotate; _rotationSpeed = speed; return this; }
        //public LiveModelPreviewBuilder WithMouseControl(bool enabled) { _mouseControl = enabled; return this; }
        public LiveModelPreviewBuilder WithGizmos(bool show) { _showGizmos = show; return this; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            if (_targetObject == null)
            {
                return new GraphicalUserInterfaceBuilder("EmptyPreview")
                    .WithBackgroundColor(_backgroundColor)
                    .AddChild(new Label("NO TARGET OBJECT") { style = { color = Color.red } })
                    .Build();
            }

            // --- State Restoration Pass ---
            string cacheKey = $"LMP_STATE_{_targetObject.name}";
            if (ctx.Services.TryGetValue(typeof(DataWarehouse), out var dwObj))
            {
                var dw = dwObj as DataWarehouse;
                if (dw != null && dw.TryRetrieveTemporary(cacheKey, out string json))
                {
                    var cached = JsonUtility.FromJson<LiveModelPreviewContext>(json);
                    _zoom = cached.Zoom;
                    _pitch = cached.Pitch;
                    _yaw = cached.Yaw;
                }
            }

            var rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            rt.Create();

            _processGroup = "LivePreview_" + Guid.NewGuid().ToString().Substring(0, 6);

            // 1. Clean up potential old ghosts before starting
            DestroyGhostObjects();

            // 2. Set up the Scene Components
            SetupPreviewScene(_targetObject, rt);

            // --- DIEGETIC STAGING LOGIC ---
            // Determine if we are in Deep Space or physically in-world on a Forge
            if (ctx.TryGetService<ForgeDiegeticTerminal>(out var terminal) && terminal.FabricationPad != null)
            {
                // WE ARE DIEGETIC: Stage physically on the terminal table
                Vector3 padPos = terminal.FabricationPad.position;
                if (_previewModel != null)
                {
                    _previewModel.transform.position = padPos;
                    // Auto-scale large models down to fit on the fabrication pad
                    float maxPadSize = 2f; // e.g., 2 meter hologram limit
                    if (_modelSize > maxPadSize)
                    {
                        float scaleFactor = maxPadSize / _modelSize;
                        _previewModel.transform.localScale = Vector3.one * scaleFactor;
                    }
                }
                if (_previewCamera != null)
                {
                    // Camera offsets based on the pad, not deep space
                    _previewCamera.transform.position = padPos + (Vector3.back * _zoom);
                }

                // Optional: Swap background to clear if projecting a hologram
                _backgroundColor = Color.clear;
            }
            else if (ctx.TryGetService<Cast.Actor>(out var player))
            {
                // WE ARE SCREEN-SPACE: Relational Deep Space Staging
                Vector3 guiBase = player.transform.position - (player.transform.forward * 500f);
                float sideOffset = (_processGroup.GetHashCode() % 2 == 0) ? 500f : -500f;
                Vector3 stagingPos = guiBase + (player.transform.right * sideOffset);

                if (_previewModel != null) _previewModel.transform.position = stagingPos;
                if (_previewCamera != null) _previewCamera.transform.position = stagingPos + (Vector3.back * _zoom);
            }

            // 3. Initialize the Gizmo Overlay (if requested)
            if (_showGizmos)
            {
                _viewGizmo = new RuntimeViewGizmo(ViewGizmoStyle.AutodeskCube);
            }

            _context = new LiveModelPreviewContext
            {
                TargetModel = _previewModel,
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

            var rootBuilder = new GraphicalUserInterfaceBuilder("PreviewRoot")
                .WithBackgroundColor(_backgroundColor)
                .OnBuild(ve =>
                {
                    ve.style.flexGrow = 1;

                    // Main Tick Loop - Guarantee Render
                    ve.schedule.Execute(() => {
                        ve.MarkDirtyRepaint();
                        UpdateCameraPosition();

                        if (_previewCamera != null && _viewGizmo != null)
                        {
                            _viewGizmo.SyncRotation(_previewCamera.transform.rotation);
                        }
                    }).Every(16);

                    if (_mouseControl)
                    {
                        ve.RegisterCallback<MouseDownEvent>(evt => {
                            _context.IsDragging = true; _context.LastMousePosition = evt.localMousePosition; ve.CaptureMouse();
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
                        ve.RegisterCallback<WheelEvent>(evt => {
                            _context.Zoom = Mathf.Clamp(_context.Zoom + (evt.delta.y * 0.05f), 0.1f, 10f);
                        });
                    }

                    // --- Automatic Serialization on Detach ---
                    ve.RegisterCallback<DetachFromPanelEvent>(evt => {
                        if (ctx.TryGetService<DataWarehouse>(out var warehouse))
                        {
                            warehouse.StoreTemporary(cacheKey, JsonUtility.ToJson(_context));
                        }
                        Dispose();
                    });
                });

            var previewImage = new Image { image = rt, scaleMode = ScaleMode.ScaleToFit, style = { flexGrow = 1 } };
            if (_viewGizmo != null) previewImage.Add(_viewGizmo.GizmoUIElement);
            rootBuilder.AddChild(previewImage);

            if (!FSM_API.FSM_API.Interaction.Exists("LiveModelPreviewFSM"))
            {
                FSM_API.FSM_API.Create.CreateFiniteStateMachine("LiveModelPreviewFSM", -1, _processGroup)
                    .State("Rendering", null, LiveModelPreviewFSM.OnRenderTick, null)
                    .WithInitialState("Rendering")
                    .BuildDefinition();
            }

            _fsmHandle = FSM_API.FSM_API.Create.CreateInstance("LiveModelPreviewFSM", _context, _processGroup);

            if (TheSingularityWorkshop.FSM_API.Scripts.FSM_UnityIntegrationAdvanced.Instance != null)
            {
                TheSingularityWorkshop.FSM_API.Scripts.FSM_UnityIntegrationAdvanced.Instance.AddProcessingGroup("Update", _processGroup);
            }

            return rootBuilder.Build();
        }
        public float ZoomFactor { get; private set; } = 1f;
        public bool AutoOrbitEnabled { get; private set; } = false;
        public Vector3 OrbitAxis { get; private set; } = Vector3.up;
        public float OrbitSpeed { get; private set; } = 10f;
        public bool MouseControlEnabled { get; private set; } = false;

        // --- FLUENT BUILDER METHODS ---

        /// <summary>
        /// Adjusts the starting distance or FOV of the preview camera.
        /// </summary>
        public LiveModelPreviewBuilder WithZoom(float zoom)
        {
            ZoomFactor = zoom;
            // TODO: Apply this to your generated preview camera's fieldOfView, 
            // or use it as a multiplier for the camera's negative Z offset.
            return this;
        }

        /// <summary>
        /// Triggers an automatic rotation around the target model.
        /// </summary>
        public LiveModelPreviewBuilder WithAutoOrbit(Vector3 axis, float speed)
        {
            AutoOrbitEnabled = true;
            OrbitAxis = axis;
            OrbitSpeed = speed;
            // TODO: Your LiveModelPreviewFSM or UI Toolkit scheduler will need to read these 
            // properties to rotate the camera/model per tick.
            return this;
        }

        /// <summary>
        /// Enables mouse drag rotation and scroll wheel zooming.
        /// </summary>
        public LiveModelPreviewBuilder WithMouseControl(bool enabled)
        {
            MouseControlEnabled = enabled;
            // TODO: Pass this flag into your LiveModelPreviewContext so the FSM 
            // knows whether to consume and process MouseMove/PointerDown events.
            return this;
        }
        private void SetupPreviewScene(GameObject original, RenderTexture rt)
        {
            // Duplicate model for isolated rendering
            _previewModel = GameObject.Instantiate(original);
            _previewModel.name = original.name + "_Ghost";

            // Strip harmful scripts from ghost
            foreach (var comp in _previewModel.GetComponentsInChildren<MonoBehaviour>())
            {
                UnityEngine.Object.DestroyImmediate(comp);
            }

            CalculateModelBounds();

            // Setup dedicated camera
            var camObj = new GameObject("LMP_Camera_" + _processGroup);
            _previewCamera = camObj.AddComponent<Camera>();
            _previewCamera.targetTexture = rt;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = _backgroundColor;
            _previewCamera.cullingMask = 1 << 31; // Render only the Preview Layer

            // Assign ghost to preview layer
            SetLayerRecursive(_previewModel, 31);

            // Add lighting
            var lightObj = new GameObject("LMP_Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = _lightIntensity;
            lightObj.transform.SetParent(camObj.transform);

            EditorStagingManager.RegisterGhost(_previewModel);
            EditorStagingManager.RegisterGhost(camObj);
        }

        private void CalculateModelBounds()
        {
            if (_previewModel == null) return;
            var renderers = _previewModel.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            Bounds b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);

            _modelCenter = b.center;
            _modelSize = b.size.magnitude;
            _zoom = _modelSize * 1.5f;
        }

        private void UpdateCameraPosition()
        {
            if (_previewCamera == null || _previewModel == null) return;

            if (_autoRotate) _yaw += _rotationSpeed * Time.deltaTime;

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);
            Vector3 position = rotation * new Vector3(0, 0, -_zoom) + _previewModel.transform.position;

            _previewCamera.transform.position = position;
            _previewCamera.transform.LookAt(_previewModel.transform.position);
        }

        private void SetLayerRecursive(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform) SetLayerRecursive(child.gameObject, layer);
        }

        private void DestroyGhostObjects()
        {
            if (_previewModel != null) UnityEngine.Object.DestroyImmediate(_previewModel);
            if (_previewCamera != null) UnityEngine.Object.DestroyImmediate(_previewCamera.gameObject);
        }

        public void Dispose()
        {
            DestroyGhostObjects();
            if (_renderTexture != null) _renderTexture.Release();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string id) { }
        public void FromUIDocument(string doc) { }

        public LiveModelPreviewBuilder WithPitch(float v)
        {

            return this;
        }

        public LiveModelPreviewBuilder WithYaw(float v)
        {

            return this;
        }
    }
}