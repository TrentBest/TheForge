using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    /// <summary>
    /// A native UI Toolkit manipulator tailored for the Forge.
    /// Attach this to any element to enable drag-resizing and click-selection.
    /// </summary>
    public class ForgeResizeSelectionManipulator : MouseManipulator
    {
        private Vector2 _startMousePos;
        private Vector2 _startSize;
        private bool _isResizing = false;
        private readonly int _edgeThreshold = 8;
        private readonly Action<VisualElement> _onSelected;

        public ForgeResizeSelectionManipulator(Action<VisualElement> onSelected = null)
        {
            _onSelected = onSelected;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseDown(MouseDownEvent e)
        {
            if (IsOnRightEdge(e.localMousePosition) || IsOnBottomEdge(e.localMousePosition))
            {
                _isResizing = true;
                _startMousePos = e.mousePosition;
                _startSize = new Vector2(target.resolvedStyle.width, target.resolvedStyle.height);
                target.CaptureMouse();
                e.StopPropagation();
            }
            else
            {
                // If they clicked the body, trigger the selection callback for the Inspector
                _onSelected?.Invoke(target);
            }
        }

        private void OnMouseMove(MouseMoveEvent e)
        {
            if (!_isResizing) return;

            Vector2 delta = e.mousePosition - _startMousePos;
            target.style.width = Mathf.Max(50, _startSize.x + delta.x);
            target.style.height = Mathf.Max(50, _startSize.y + delta.y);
            e.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            if (_isResizing)
            {
                _isResizing = false;
                target.ReleaseMouse();
                e.StopPropagation();
            }
        }

        private bool IsOnRightEdge(Vector2 pos) => pos.x >= target.layout.width - _edgeThreshold;
        private bool IsOnBottomEdge(Vector2 pos) => pos.y >= target.layout.height - _edgeThreshold;
    }
}