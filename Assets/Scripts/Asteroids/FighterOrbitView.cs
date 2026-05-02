#if UNITY_EDITOR
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.Asteroids
{
    public class FighterOrbitView : VisualElement
    {
        private static int _previewInstanceCount = 0;

        private GameObject _previewScene;
        private Camera _previewCamera;
        private GameObject _modelInstance;
        private RenderTexture _renderTexture;
        private FighterPreviewFSM _fsmDriver;

        private bool _isDragging;
        private Vector3 _lastMousePos;
        private float _currentCameraDistance;
        public bool IsInteractive { get; set; } = true;
        public GameObject ModelInstance => _modelInstance;
        public FighterOrbitView(GameObject prefab, HangarTheme theme, bool interactive = true, Vector3 initialRotation = default, float cameraDistance = 10f)
        {
            IsInteractive = interactive;
            _currentCameraDistance = cameraDistance;

            style.backgroundColor = theme.ViewerBackground;
            style.overflow = Overflow.Hidden;
            style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

            _previewScene = new GameObject($"UI_Preview_Scene_{_previewInstanceCount}");
            _previewScene.transform.position = new Vector3(_previewInstanceCount * 500f, 10000f, 0);
            _previewInstanceCount++;

            var lightGo = new GameObject("Preview_Light");
            lightGo.transform.SetParent(_previewScene.transform);
            lightGo.transform.localRotation = Quaternion.Euler(50, -30, 0);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;

            var camGo = new GameObject("Preview_Camera");
            camGo.transform.SetParent(_previewScene.transform);
            camGo.transform.localPosition = new Vector3(0, 2, -_currentCameraDistance);
            _previewCamera = camGo.AddComponent<Camera>();
            _previewCamera.cameraType = CameraType.Preview;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = theme.ViewerBackground;
            _previewCamera.nearClipPlane = 0.1f;
            _previewCamera.farClipPlane = 100f;

            if (prefab != null)
            {
                _modelInstance = GameObject.Instantiate(prefab, _previewScene.transform);
                _modelInstance.transform.localPosition = Vector3.zero;
                _modelInstance.transform.localEulerAngles = initialRotation;

                // FIX: Attach the FSM only if it IS the main interactive viewport!
                if (IsInteractive)
                {
                    _fsmDriver = _modelInstance.AddComponent<FighterPreviewFSM>();
                    _fsmDriver.Initialize();
                }
            }

            _renderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            _previewCamera.targetTexture = _renderTexture;
            style.backgroundImage = Background.FromRenderTexture(_renderTexture);

            pickingMode = PickingMode.Position;

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            RegisterCallback<WheelEvent>(OnScroll);

            // NEW: Self-contained Update Loop!
            // When attached to the screen, it runs the FSM and forces repaints.
            RegisterCallback<AttachToPanelEvent>(e => {
                this.schedule.Execute(() => {
                    if (IsInteractive && _fsmDriver != null)
                    {
                        FSM_API.Interaction.Update("HangarPreview");
                    }
                    ForceRender();
                }).Every(16); // ~60fps
            });

            RegisterCallback<DetachFromPanelEvent>(e => Cleanup());
        }

        public void ForceRender()
        {
            if (_previewCamera != null)
            {
                _previewCamera.Render();
                MarkDirtyRepaint(); // FIX: Required to tell UI Toolkit the RenderTexture has changed
            }
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            // FIX: Return if NOT interactive
            if (!IsInteractive || _modelInstance == null) return;

            _isDragging = true;
            if (_fsmDriver != null) _fsmDriver.IsInteracting = true; // Pause Auto-Tour
            _lastMousePos = evt.position;
            this.CapturePointer(evt.pointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isDragging || _modelInstance == null) return;
            Vector3 delta = evt.position - _lastMousePos;
            _lastMousePos = evt.position;

            // Manual Drag Rotation
            _modelInstance.transform.Rotate(Vector3.up, -delta.x * 0.5f, Space.World);
            _modelInstance.transform.Rotate(Vector3.right, delta.y * 0.5f, Space.World);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (_isDragging)
            {
                _isDragging = false;
                if (_fsmDriver != null) _fsmDriver.IsInteracting = false; // Resume Auto-Tour eventually
                this.ReleasePointer(evt.pointerId);
            }
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            if (_isDragging)
            {
                _isDragging = false;
                if (_fsmDriver != null) _fsmDriver.IsInteracting = false;
                this.ReleasePointer(evt.pointerId);
            }
        }

        private void OnScroll(WheelEvent evt)
        {
            if (!IsInteractive || _modelInstance == null) return;
            Zoom(evt.delta.y * 0.05f);
            evt.StopPropagation();
        }

        // --- PUBLIC CONTROLS FOR BUTTONS ---

        public void RotateModel(Vector3 axis, float degrees)
        {
            if (_modelInstance != null)
            {
                _modelInstance.transform.Rotate(axis, degrees, Space.World);
            }
        }

        public void Zoom(float delta)
        {
            if (_previewCamera != null)
            {
                _currentCameraDistance = Mathf.Clamp(_currentCameraDistance + delta, 2f, 40f);
                _previewCamera.transform.localPosition = new Vector3(0, 2, -_currentCameraDistance);
            }
        }

        public void TestFire() { if (_modelInstance != null) _modelInstance.transform.Rotate(Vector3.right, -15f, Space.World); }

        private void Cleanup()
        {
            if (_previewCamera != null) _previewCamera.targetTexture = null;
            if (_renderTexture != null) { _renderTexture.Release(); UnityEngine.Object.DestroyImmediate(_renderTexture); }
            if (_previewScene != null) UnityEngine.Object.DestroyImmediate(_previewScene);
        }

        
        public void SwapPrefab(GameObject newPrefab, Vector3 initialRotation = default)
        {
            if (_modelInstance != null)
            {
                UnityEngine.Object.DestroyImmediate(_modelInstance);
                _fsmDriver = null;
            }

            if (newPrefab != null)
            {
                _modelInstance = GameObject.Instantiate(newPrefab, _previewScene.transform);
                _modelInstance.transform.localPosition = Vector3.zero;
                _modelInstance.transform.localEulerAngles = initialRotation;

                if (IsInteractive)
                {
                    _fsmDriver = _modelInstance.AddComponent<FighterPreviewFSM>();
                    _fsmDriver.Initialize();
                }
            }
            ForceRender();
        }
    }
}
#endif