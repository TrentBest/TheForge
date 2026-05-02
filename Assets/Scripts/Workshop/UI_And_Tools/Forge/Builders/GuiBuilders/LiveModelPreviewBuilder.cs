using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.Gameplay.Cast;
using Workshop.GURPS;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class LiveModelPreviewBuilder : IGuiProvider, IDisposable
    {
        public string Title => "Live Model Preview";

        private GameObject _targetObject;
        private GameObject _previewModel;
        private Camera _previewCamera;
        private RenderTexture _renderTexture;
        private RuntimeViewGizmo _viewGizmo;
        private Action<GameObject, string> _onPreviewInit;
        private Action<GameObject, string> _onPreviewDestroy;
        private bool _showControls = false;
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
        private bool _autoZoom = false;
        private float _zoomSpeed = -1f;
        private LiveModelPreviewContext _context;
        private string _processGroup;
        private FSMHandle _fsmHandle;
        private readonly List<Action<VisualElement>> _onBuildActions = new();

        // Hierarchy Management
        private GameObject _lmpGlobalRoot;
        private GameObject _sessionRoot;

        // Model boundaries
        private Vector3 _modelCenter;
        private float _modelSize = 1f;
        private Vector3 _centerOffset;
        private bool _useOriginalModel = false;
        private Vector3 _cameraOffset;
        private Vector2 _size;
        private bool _tourMode;

        // --- NEW MODES ---
        private Vector3? _manualCameraPosition;
        private bool _isGlobalGpuMode = false;

        public float ZoomFactor { get; private set; } = 1f;
        public bool AutoOrbitEnabled { get; private set; } = false;
        public Vector3 OrbitAxis { get; private set; } = Vector3.up;
        public float OrbitSpeed { get; private set; } = 10f;
        public bool MouseControlEnabled { get; private set; } = true;

        public LiveModelPreviewBuilder(GameObject targetModel)
        {
            _targetObject = targetModel;
        }

        // Tells the LMPB not to panic if the target object is null, we just want its camera!
        public LiveModelPreviewBuilder AsGlobalGpuViewer()
        {
            _isGlobalGpuMode = true;
            return this;
        }

        public LiveModelPreviewBuilder WithCameraPosition(Vector3 position)
        {
            _manualCameraPosition = position;
            return this;
        }

        public LiveModelPreviewBuilder WithPreviewLifecycle(Action<GameObject, string> onInit, Action<GameObject, string> onDestroy = null)
        {
            _onPreviewInit = onInit; _onPreviewDestroy = onDestroy; return this;
        }

        public LiveModelPreviewBuilder WithControls(bool showControls = true) { _showControls = showControls; return this; }
        public LiveModelPreviewBuilder WithBackgroundColor(Color color) { _backgroundColor = color; return this; }

        public LiveModelPreviewBuilder WithAutoRotate(bool autoRotate, float speed = 10f)
        {
            _autoRotate = autoRotate; _rotationSpeed = speed; AutoOrbitEnabled = autoRotate; OrbitSpeed = speed; return this;
        }

        public LiveModelPreviewBuilder WithGizmos(bool show) { _showGizmos = show; return this; }
        public LiveModelPreviewBuilder WithAutoZoom(bool autoZoom, float speed = -1f) { _autoZoom = autoZoom; _zoomSpeed = speed; return this; }

        public LiveModelPreviewBuilder WithOriginalModel(bool useOriginal = true)
        {
            _useOriginalModel = useOriginal; return this;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // If we aren't in Global GPU mode, we strictly require a target GameObject
            if (_targetObject == null && !_isGlobalGpuMode)
            {
                return new GraphicalUserInterfaceBuilder("EmptyPreview")
                    .WithBackgroundColor(_backgroundColor)
                    .AddChild(new ForgeLabelBuilder("NO TARGET OBJECT").WithColor(Color.red).CreateGui(ctx))
                    .Build();
            }

            string cacheKey = _targetObject != null ? $"LMP_STATE_{_targetObject.name}" : "LMP_STATE_GLOBAL";

            if (ctx.Services.TryGetValue(typeof(DataWarehouse), out var dwObj))
            {
                var dw = dwObj as DataWarehouse;
                if (dw != null && dw.TryRetrieveTemporary(cacheKey, out string json))
                {
                    var cached = JsonUtility.FromJson<LiveModelPreviewContext>(json);
                    _zoom = cached.Zoom; _pitch = cached.Pitch; _yaw = cached.Yaw;
                }
            }

            var rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            rt.Create();
            _renderTexture = rt;

            _processGroup = "LivePreview_" + Guid.NewGuid().ToString().Substring(0, 6);

            DestroyGhostObjects();
            SetupPreviewScene(_targetObject, rt);

            if (!_useOriginalModel && !_isGlobalGpuMode)
            {
                if (ctx.TryGetService<ForgeDiegeticTerminal>(out var terminal) && terminal.FabricationPad != null)
                {
                    Vector3 padPos = terminal.FabricationPad.position;
                    if (_previewModel != null)
                    {
                        _previewModel.transform.position = padPos;
                        float maxPadSize = 2f;
                        if (_modelSize > maxPadSize)
                        {
                            float scaleFactor = maxPadSize / _modelSize;
                            _previewModel.transform.localScale = Vector3.one * scaleFactor;
                        }
                    }
                    if (_previewCamera != null) _previewCamera.transform.position = padPos + (Vector3.back * _zoom);
                    _backgroundColor = Color.clear;
                }
                else if (ctx.TryGetService<Actor>(out var player))
                {
                    Vector3 guiBase = player.transform.position - (player.transform.forward * 500f);
                    float sideOffset = (_processGroup.GetHashCode() % 2 == 0) ? 500f : -500f;
                    Vector3 stagingPos = guiBase + (player.transform.right * sideOffset);
                    if (_previewModel != null) _previewModel.transform.position = stagingPos;
                    if (_previewCamera != null) _previewCamera.transform.position = stagingPos + (Vector3.back * _zoom);
                }
            }

            if (_showGizmos) _viewGizmo = new RuntimeViewGizmo(ViewGizmoStyle.AutodeskCube);

            _context = new LiveModelPreviewContext
            {
                TargetModel = _previewModel,
                RenderTexture = rt,
                Zoom = _zoom,
                Pitch = _pitch,
                Yaw = _yaw,
                PreviewCamera = _previewCamera,
                UseOriginalModel = _useOriginalModel,
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
                .WithAutoGrow(true)
                .OnBuild(ve =>
                {
                    ve.schedule.Execute(() => {
                        ve.MarkDirtyRepaint();
                        UpdateCameraPosition();

                        if (_previewCamera != null && _viewGizmo != null)
                            _viewGizmo.SyncRotation(_previewCamera.transform.rotation);
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
                            _yaw = _context.Yaw; _pitch = _context.Pitch;
                        });
                        ve.RegisterCallback<WheelEvent>(evt => {
                            _context.Zoom = Mathf.Clamp(_context.Zoom + (evt.delta.y * 0.05f), 0.1f, 100f);
                            _zoom = _context.Zoom;
                        });
                    }

                    ve.RegisterCallback<DetachFromPanelEvent>(evt => {
                        if (ctx.TryGetService<DataWarehouse>(out var warehouse))
                            warehouse.StoreTemporary(cacheKey, JsonUtility.ToJson(_context));
                        Dispose();
                    });
                });

            var previewImage = new Image { image = rt, scaleMode = ScaleMode.ScaleToFit, style = { flexGrow = 1 } };
            if (_viewGizmo != null) previewImage.Add(_viewGizmo.GizmoUIElement);
            rootBuilder.AddChild(previewImage);

            if (!FSM_API.Interaction.Exists("LiveModelPreviewFSM", _processGroup))
            {
                FSM_API.Create.CreateFiniteStateMachine("LiveModelPreviewFSM", -1, _processGroup)
                    .State("Priming", null, LiveModelPreviewFSM.OnPrimeTick, null)
                    .State("Rendering", null, LiveModelPreviewFSM.OnRenderTick, null)
                    .Transition("Priming", "Rendering", transitionCtx => true)
                    .WithInitialState("Priming")
                    .BuildDefinition();
            }

            _fsmHandle = FSM_API.Create.CreateInstance("LiveModelPreviewFSM", _context, _processGroup);

            if (TheSingularityWorkshop.FSM_API.Scripts.FSM_UnityIntegrationAdvanced.Instance != null)
            {
                TheSingularityWorkshop.FSM_API.Scripts.FSM_UnityIntegrationAdvanced.Instance.AddProcessingGroup("Update", _processGroup);
            }

            if (_showControls)
            {
                var controlBar = new GraphicalUserInterfaceBuilder("LMP_Controls")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f, 0.8f))
                    .WithPadding(8).WithBorderRadius(6)
                    .AddToggleData("Auto-Orbit", _autoRotate, v => { _autoRotate = v; _context.AutoOrbitEnabled = v; AutoOrbitEnabled = v; })
                    .AddSliderData("Orbit Speed", 0f, 50f, _rotationSpeed, v => { _rotationSpeed = v; _context.OrbitSpeed = v; OrbitSpeed = v; })
                    .OnBuild(ve => { ve.style.position = Position.Absolute; ve.style.bottom = 10; ve.style.left = 10; ve.style.right = 10; });

                controlBar.GetGuiBuilder().Invoke(previewImage);
            }

            var finalVisualElement = rootBuilder.Build();
            foreach (var action in _onBuildActions) action?.Invoke(finalVisualElement);
            return finalVisualElement;
        }

        public LiveModelPreviewBuilder WithZoom(float zoom) { ZoomFactor = zoom; _zoom = zoom; return this; }
        public LiveModelPreviewBuilder WithAutoOrbit(Vector3 axis, float speed) { AutoOrbitEnabled = true; OrbitAxis = axis.normalized; OrbitSpeed = speed; _autoRotate = true; _rotationSpeed = speed; return this; }
        public LiveModelPreviewBuilder WithMouseControl(bool enabled) { MouseControlEnabled = enabled; _mouseControl = enabled; return this; }
        public LiveModelPreviewBuilder WithPitch(float v) { _pitch = v; return this; }
        public LiveModelPreviewBuilder WithYaw(float v) { _yaw = v; return this; }

        public VisualElement Build() => CreateGui(new GuiContext());
        public Camera GetCamera() => _previewCamera;

        private void SetupPreviewScene(GameObject original, RenderTexture rt)
        {
            _lmpGlobalRoot = GameObject.Find("LMP_Root");
            if (_lmpGlobalRoot == null) _lmpGlobalRoot = new GameObject("LMP_Root");

            string targetName = original != null ? original.name : "Global_GPU_View";
            _sessionRoot = new GameObject($"LivePreview_{targetName}_{_processGroup}");
            _sessionRoot.transform.SetParent(_lmpGlobalRoot.transform);

            if (_useOriginalModel && original != null)
            {
                _previewModel = original;
            }
            else if (original != null)
            {
                // RIP TO PIECES: Strip out all physics and MonoBehaviour logic to make it a pure visual "Ghost"
                _previewModel = GameObject.Instantiate(original);
                _previewModel.name = original.name + "_Ghost";
                _previewModel.hideFlags = HideFlags.HideAndDontSave;
                _previewModel.SetActive(true);

                foreach (var comp in _previewModel.GetComponentsInChildren<MonoBehaviour>()) UnityEngine.Object.DestroyImmediate(comp);
                foreach (var rb in _previewModel.GetComponentsInChildren<Rigidbody>()) UnityEngine.Object.DestroyImmediate(rb);
                foreach (var col in _previewModel.GetComponentsInChildren<Collider>()) UnityEngine.Object.DestroyImmediate(col);

                SetLayerRecursive(_previewModel, 31);
                _previewModel.transform.SetParent(_sessionRoot.transform);
            }

            CalculateModelBounds();

            if (_onPreviewInit != null && _previewModel != null)
                _onPreviewInit.Invoke(_previewModel, _processGroup);

            var camObj = new GameObject("LMP_Camera_" + _processGroup);
            camObj.transform.SetParent(_sessionRoot.transform);

            _previewCamera = camObj.AddComponent<Camera>();
            _previewCamera.targetTexture = rt;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = _backgroundColor;

            // =======================================================
            // CAMERA CULLING MASKS
            // =======================================================
            if (_isGlobalGpuMode)
            {
                // Global Mode sees the Default Layer (Where the GPU Swarm is rendered)
                _previewCamera.cullingMask = (1 << 0);
            }
            else if (_useOriginalModel)
            {
                int dynamicMask = 0;
                foreach (var renderer in original.GetComponentsInChildren<Renderer>())
                    dynamicMask |= (1 << renderer.gameObject.layer);
                if (dynamicMask == 0) dynamicMask = 1 << original.layer;
                _previewCamera.cullingMask = dynamicMask;
            }
            else
            {
                // Standard Item Viewer Mode ONLY looks at the Ghost Layer (31)
                _previewCamera.cullingMask = 1 << 31;
            }

            var lightObj = new GameObject("LMP_Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = _lightIntensity;
            lightObj.transform.SetParent(camObj.transform);
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
            _centerOffset = _modelCenter - _previewModel.transform.position;

            if (ZoomFactor == 1f) _zoom = _modelSize * 1.5f;
        }

        private void UpdateCameraPosition()
        {
            if (_previewCamera == null) return;

            Vector3 targetPos = Vector3.zero;
            if (_previewModel != null) targetPos = _previewModel.transform.position + _centerOffset;

            if (_manualCameraPosition.HasValue)
            {
                _previewCamera.transform.localPosition = _manualCameraPosition.Value;
                _previewCamera.transform.LookAt(targetPos + Vector3.forward * 10f);
                return;
            }

            if (_autoRotate)
            {
                _yaw += _rotationSpeed * Time.deltaTime * OrbitAxis.y;
                _pitch += _rotationSpeed * Time.deltaTime * OrbitAxis.x;
            }

            if (_autoZoom)
            {
                _zoom += _zoomSpeed * Time.deltaTime;
                _zoom = Mathf.Max(0.5f, _zoom);
                _context.Zoom = _zoom;
            }

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);
            Vector3 finalCamPos = rotation * new Vector3(0, 0, -_zoom) + targetPos;

            _previewCamera.transform.position = finalCamPos;
            _previewCamera.transform.LookAt(targetPos);
        }

        private void SetLayerRecursive(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform) SetLayerRecursive(child.gameObject, layer);
        }

        public LiveModelPreviewBuilder WithCameraOffset(Vector3 offset) { _cameraOffset = offset; return this; }

        private void DestroyGhostObjects()
        {
            if (_sessionRoot != null)
            {
                _onPreviewDestroy?.Invoke(_previewModel, _processGroup);
                UnityEngine.Object.DestroyImmediate(_sessionRoot);
                _sessionRoot = null;
            }
        }

        public LiveModelPreviewBuilder WithAbsolutePosition(float? top = null, float? right = null, float? bottom = null, float? left = null)
        {
            return OnBuild(ve =>
            {
                ve.style.position = Position.Absolute;
                if (top.HasValue) ve.style.top = top.Value;
                if (right.HasValue) ve.style.right = right.Value;
                if (bottom.HasValue) ve.style.bottom = bottom.Value;
                if (left.HasValue) ve.style.left = left.Value;
            });
        }

        public void Dispose()
        {
            DestroyGhostObjects();

            if (_renderTexture != null)
            {
                _renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(_renderTexture);
                _renderTexture = null;
            }

            if (_viewGizmo != null)
            {
                _viewGizmo.Dispose();
                _viewGizmo = null;
            }

            if (!string.IsNullOrEmpty(_processGroup))
            {
                if (TheSingularityWorkshop.FSM_API.Scripts.FSM_UnityIntegrationAdvanced.Instance != null)
                    TheSingularityWorkshop.FSM_API.Scripts.FSM_UnityIntegrationAdvanced.Instance.RemoveProcessingGroup("Update", _processGroup);

                var fsms = FSM_API.Interaction.GetInstances("LiveModelPreviewFSM", _processGroup);
                foreach (var fsm in fsms) FSM_API.Interaction.DestroyInstance(fsm);

                FSM_API.Interaction.DestroyFiniteStateMachine("LiveModelPreviewFSM", _processGroup);
                FSM_API.Interaction.RemoveProcessingGroup(_processGroup);
            }
        }

        public LiveModelPreviewBuilder OnBuild(Action<VisualElement> action) { if (action != null) _onBuildActions.Add(action); return this; }
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string id) { }
        public void FromUIDocument(string doc) { }
        public LiveModelPreviewBuilder WithColor(Color color) { _backgroundColor = color; return this; }
        public LiveModelPreviewBuilder WithSize(float width, float height) { _size = new Vector2(width, height); return this; }
        public LiveModelPreviewBuilder WithAutoRotation(bool enabled, float speed = 10f) { _autoRotate = enabled; _rotationSpeed = speed; return this; }
        public LiveModelPreviewBuilder WithTourMode(bool enabled) { _tourMode = enabled; return this; }
    }
}