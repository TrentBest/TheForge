using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    [RequireComponent(typeof(InWorldGuiBuilder))]
    [RequireComponent(typeof(Collider))]
    public class InWorldGuiInteractor : MonoBehaviour
    {
        private InWorldGuiBuilder _builder;
        private Collider _screenCollider;

        private Vector2 _lastUiPos;
        private bool _isHovering;

        private void Awake()
        {
            _builder = GetComponent<InWorldGuiBuilder>();
            _screenCollider = GetComponent<Collider>();
        }

        // =================================================================
        // THE BRIDGE: 
        // UI Toolkit locks its event properties. We use IMGUI Events to 
        // transport the synthetic data into the GetPooled() constructor.
        // =================================================================
        private Event CreateImguiEvent(EventType type, Vector2 position, int button = 0)
        {
            Event e = new Event();
            e.type = type;
            e.mousePosition = position;
            e.button = button;
            return e;
        }

        private void Update()
        {
            if (_builder.Document == null || _builder.Document.rootVisualElement == null || _builder.Document.panelSettings == null)
                return;

            var panel = _builder.Document.rootVisualElement.panel;
            if (panel == null) return;

            // 1. Raycast from Mouse (Can be replaced with VR Laser Pointer logic)
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool hitScreen = UnityEngine.Physics.Raycast(ray, out RaycastHit hit) && hit.collider == _screenCollider;

            if (hitScreen)
            {
                // 2. Convert UV hit to UI Toolkit Virtual Resolution
                Vector2 referenceRes = _builder.Document.panelSettings.referenceResolution;
                Vector2 uiPos = new Vector2(
                    hit.textureCoord.x * referenceRes.x,
                    (1.0f - hit.textureCoord.y) * referenceRes.y
                );

                if (!_isHovering) _isHovering = true;

                // 3. Inject Mouse Movement
                if (uiPos != _lastUiPos)
                {
                    using (var e = PointerMoveEvent.GetPooled(CreateImguiEvent(EventType.MouseMove, uiPos)))
                    {
                        panel.visualTree.SendEvent(e);
                    }
                    _lastUiPos = uiPos;
                }

                // 4. Inject Clicks
                if (Input.GetMouseButtonDown(0))
                {
                    using (var e = PointerDownEvent.GetPooled(CreateImguiEvent(EventType.MouseDown, uiPos, 0)))
                    {
                        panel.visualTree.SendEvent(e);
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    using (var e = PointerUpEvent.GetPooled(CreateImguiEvent(EventType.MouseUp, uiPos, 0)))
                    {
                        panel.visualTree.SendEvent(e);
                    }
                }
            }
            else if (_isHovering)
            {
                _isHovering = false;

                // 5. Clear Hover States
                // If we exit the screen, we fire a move event way off-screen.
                // The Panel Dispatcher will automatically generate the required 
                // PointerLeaveEvents for any elements that were being hovered.
                Vector2 offScreenPos = new Vector2(-1000, -1000);
                using (var e = PointerMoveEvent.GetPooled(CreateImguiEvent(EventType.MouseMove, offScreenPos)))
                {
                    panel.visualTree.SendEvent(e);
                }
                _lastUiPos = offScreenPos;
            }
        }
    }
}