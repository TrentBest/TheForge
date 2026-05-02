using UnityEngine;
using UnityEngine.InputSystem;
using Workshop.Core.Diagnostics; // Injected for ForgeLogger
using Workshop.Core.Memory;

namespace Workshop.UI_And_Tools.Forge.IO
{
    /// <summary>
    /// THE LOBOTOMIZED MANIPULATOR.
    /// 100% Static. Zero Mono-tether. 
    /// Manages the spatial orientation of Forge Displays via Raycast arbitration.
    /// </summary>
    public static class ForgeManipulatorSystem
    {
        private enum ManipulationMode { None, Moving, Resizing, Rotating }

        // Persistent System State
        private static ManipulationMode _currentMode = ManipulationMode.None;
        private static ForgeDisplay _targetDisplay;
        private static Camera _cam;

        // Interaction Constants
        private const float MAX_DISTANCE = 15f;
        private const KeyCode KEY_MANIPULATE = KeyCode.LeftControl;
        private const KeyCode KEY_ROTATE = KeyCode.LeftShift;

        // Drag Buffer Data
        private static Vector3 _initialHitPoint;
        private static Vector3 _initialObjPos;
        private static Vector2 _initialSize;
        private static Quaternion _initialRot;
        private static Plane _dragPlane;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Ignite()
        {
            ForgeLogger.Log("Spatial Manipulation System Ignited. (Pure Static)")
                .WithHeader("Manipulator")
                .WithColor("#00FFCC") // Neon Cyan
                .SendToUnity();

            // Inject the system tick into the engine's render loop
            Application.onBeforeRender -= SystemTick;
            Application.onBeforeRender += SystemTick;
        }

        private static void SystemTick()
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return;

            // THE FIX: Raw polling of the New Input System Devices
            bool isCtrlHeld = Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed;
            bool isShiftHeld = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
            bool isClicking = Mouse.current != null && Mouse.current.leftButton.isPressed;

            // Safe Mouse Position Read
            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

            HandleHoverAffordance(isCtrlHeld, mousePos);

            // Start Drag
            if (isCtrlHeld && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryCaptureInteraction(mousePos, isShiftHeld);
            }

            // Update Drag
            if (_currentMode != ManipulationMode.None && isClicking)
            {
                ApplySpatialTransformation(mousePos);
            }

            // End Drag
            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame && _currentMode != ManipulationMode.None)
            {
                ForgeLogger.Log($"Interaction Terminated. Target: {_targetDisplay?.name}").WithHeader("Manipulator").WithColor(Color.red).SendToUnity();
                _currentMode = ManipulationMode.None;
            }
        }

        private static void HandleHoverAffordance(bool showHandles, Vector2 mousePos)
        {
            Ray ray = _cam.ScreenPointToRay(mousePos);
            // We use Layer 5 (UI) or a specific Forge layer if established
            if (UnityEngine.Physics.Raycast(ray, out var hit, MAX_DISTANCE))
            {
                var display = hit.collider.GetComponentInParent<ForgeDisplay>();

                if (_targetDisplay != display)
                {
                    if (_targetDisplay) _targetDisplay.SetManipulationMode(false);
                    _targetDisplay = display;
                }

                if (_targetDisplay) _targetDisplay.SetManipulationMode(showHandles);
            }
            else if (!showHandles && _currentMode == ManipulationMode.None)
            {
                if (_targetDisplay)
                {
                    _targetDisplay.SetManipulationMode(false);
                    _targetDisplay = null;
                }
            }
        }

        private static void TryCaptureInteraction(Vector2 mousePos, bool isShiftHeld)
        {
            Ray ray = _cam.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (UnityEngine.Physics.Raycast(ray, out var hit, MAX_DISTANCE))
            {
                _targetDisplay = hit.collider.GetComponentInParent<ForgeDisplay>();
                if (!_targetDisplay) return;

                _initialHitPoint = hit.point;
                _initialObjPos = _targetDisplay.transform.position;
                _initialSize = _targetDisplay.PhysicalSize;
                _initialRot = _targetDisplay.transform.rotation;

                _dragPlane = new Plane(_targetDisplay.transform.forward, _initialObjPos);

                // Determine Mode based on collision target
                if (hit.collider.name == "Handle_Move")
                {
                    _currentMode = isShiftHeld ? ManipulationMode.Rotating : ManipulationMode.Moving;
                }
                else if (hit.collider.name == "Handle_Resize")
                {
                    _currentMode = ManipulationMode.Resizing;
                }
                else
                {
                    _currentMode = ManipulationMode.Moving;
                }

                ForgeLogger.Log($"Captured Interaction: {_currentMode} on {_targetDisplay.name}")
                    .WithHeader("Manipulator")
                    .WithColor(Color.yellow)
                    .SendToUnity();
            }
        }

        private static void ApplySpatialTransformation(Vector2 mousePos)
        {
            Ray ray = _cam.ScreenPointToRay(mousePos);
            if (!_dragPlane.Raycast(ray, out float enter)) return;

            Vector3 currentHitPoint = ray.GetPoint(enter);
            Vector3 delta = currentHitPoint - _initialHitPoint;

            switch (_currentMode)
            {
                case ManipulationMode.Moving:
                    _targetDisplay.transform.position = _initialObjPos + delta;
                    break;

                case ManipulationMode.Rotating:
                    float angle = (delta.x + delta.y) * 100f;
                    _targetDisplay.transform.rotation = _initialRot * Quaternion.Euler(0, -angle, 0);
                    break;

                case ManipulationMode.Resizing:
                    float widthChange = Vector3.Dot(delta, _targetDisplay.transform.right);
                    float heightChange = Vector3.Dot(delta, -_targetDisplay.transform.up);

                    _targetDisplay.Resize(_initialSize.x + widthChange, _initialSize.y + heightChange);

                    // Throttle resizing logs to avoid flood, but keep context
                    if (Time.frameCount % 30 == 0)
                    {
                        ForgeLogger.Log($"Resizing: {_targetDisplay.PhysicalSize.x:F2} x {_targetDisplay.PhysicalSize.y:F2}")
                            .WithHeader("Spatial")
                            .WithColor("#FF00FF") // Magenta for manifestation
                            .SendToUnity();
                    }
                    break;
            }
        }
    }
}