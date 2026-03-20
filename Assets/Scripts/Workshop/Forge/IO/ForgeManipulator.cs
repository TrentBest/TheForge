using UnityEngine;

namespace TheSingularityWorkshop.Forge.Input
{
    public class ForgeManipulator : MonoBehaviour
    {
        [Header("Settings")]
        public float MaxInteractionDistance = 10f;
        public LayerMask InteractionLayer;

        [Header("Controls")]
        public KeyCode ManipulationKey = KeyCode.LeftControl;
        public KeyCode RotationModifier = KeyCode.LeftShift; // Hold Ctrl+Shift to rotate

        private Camera _cam;
        private ForgeDisplay _targetDisplay;

        // Manipulation State
        private enum Mode { None, Moving, Resizing, Rotating }
        private Mode _currentMode = Mode.None;

        // Drag Data
        private Vector3 _initialHitPoint;
        private Vector3 _initialObjPos;
        private Vector2 _initialSize;
        private Quaternion _initialRot;
        private Plane _dragPlane;

        void Start()
        {
            _cam = GetComponent<Camera>() ?? Camera.main;
        }

        void Update()
        {
            bool isCtrlHeld = UnityEngine.Input.GetKey(ManipulationKey);
            bool isClicking = UnityEngine.Input.GetMouseButton(0);

            // 1. Highlight / Reveal Handles
            HandleHover(isCtrlHeld);

            // 2. Start Drag
            if (isCtrlHeld && UnityEngine.Input.GetMouseButtonDown(0))
            {
                TryStartManipulation();
            }

            // 3. Update Drag
            if (_currentMode != Mode.None && isClicking)
            {
                UpdateManipulation();
            }

            // 4. End Drag
            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                _currentMode = Mode.None;
            }
        }

        private void HandleHover(bool showHandles)
        {
            Ray ray = _cam.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, MaxInteractionDistance, InteractionLayer))
            {
                var display = hit.collider.GetComponentInParent<ForgeDisplay>();

                // If we switched targets, hide old, show new
                if (_targetDisplay != display)
                {
                    if (_targetDisplay) _targetDisplay.SetManipulationMode(false);
                    _targetDisplay = display;
                }

                if (_targetDisplay) _targetDisplay.SetManipulationMode(showHandles);
            }
            else if (!showHandles && _currentMode == Mode.None)
            {
                // Clear handles if looking at nothing and not dragging
                if (_targetDisplay)
                {
                    _targetDisplay.SetManipulationMode(false);
                    _targetDisplay = null;
                }
            }
        }

        private void TryStartManipulation()
        {
            Ray ray = _cam.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, MaxInteractionDistance, InteractionLayer))
            {
                _targetDisplay = hit.collider.GetComponentInParent<ForgeDisplay>();
                if (!_targetDisplay) return;

                _initialHitPoint = hit.point;
                _initialObjPos = _targetDisplay.transform.position;
                _initialSize = _targetDisplay.PhysicalSize;
                _initialRot = _targetDisplay.transform.rotation;

                // Define plane for mouse dragging based on object facing
                _dragPlane = new Plane(_targetDisplay.transform.forward, _initialObjPos);

                // Determine Mode based on what we hit
                if (hit.collider.name == "Handle_Move")
                {
                    _currentMode = UnityEngine.Input.GetKey(RotationModifier) ? Mode.Rotating : Mode.Moving;
                }
                else if (hit.collider.name == "Handle_Resize")
                {
                    _currentMode = Mode.Resizing;
                }
                else
                {
                    // Clicking body moves it
                    _currentMode = Mode.Moving;
                }
            }
        }

        private void UpdateManipulation()
        {
            Ray ray = _cam.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (!_dragPlane.Raycast(ray, out float enter)) return;

            Vector3 currentHitPoint = ray.GetPoint(enter);
            Vector3 delta = currentHitPoint - _initialHitPoint;

            switch (_currentMode)
            {
                case Mode.Moving:
                    _targetDisplay.transform.position = _initialObjPos + delta;
                    break;

                case Mode.Rotating:
                    // Simple Y-Axis rotation based on horizontal mouse delta
                    float angle = (delta.x + delta.y) * 100f;
                    _targetDisplay.transform.rotation = _initialRot * Quaternion.Euler(0, -angle, 0);
                    break;

                case Mode.Resizing:
                    // Project delta onto local X/Y axis to find size change
                    float widthChange = Vector3.Dot(delta, _targetDisplay.transform.right);
                    float heightChange = Vector3.Dot(delta, -_targetDisplay.transform.up); // Dragging down increases height

                    _targetDisplay.Resize(_initialSize.x + widthChange, _initialSize.y + heightChange);
                    break;
            }
        }
    }
}