using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.Asteroids
{
    /// <summary>
    /// Dynamically adjusts a Camera's Viewport Rect based on the size of a UI Toolkit Element.
    /// Prevents 3D scene content from bleeding under opaque UI panels.
    /// </summary>
    public class CameraViewportAdapter : IDisposable
    {
        private VisualElement _panel;
        private Camera _targetCamera;
        private Side _anchorSide;

        public enum Side { Left, Right, Top, Bottom }

        public CameraViewportAdapter(VisualElement panel, Camera camera, Side anchorSide = Side.Left)
        {
            _panel = panel;
            _targetCamera = camera;
            _anchorSide = anchorSide;

            if (_panel != null)
            {
                // Re-calculate the camera offset any time the UI flexbox resizes the panel
                _panel.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (_targetCamera == null || _panel == null) return;

            // Get the physical screen width vs the UI layout width
            float panelWidth = evt.newRect.width;
            float screenWidth = Screen.width;

            // Avoid division by zero on initialization frames
            if (screenWidth <= 0) return;

            float ratio = panelWidth / screenWidth;

            // Squish the camera viewport so it never renders where the panel is
            if (_anchorSide == Side.Left)
            {
                _targetCamera.rect = new Rect(ratio, 0, 1f - ratio, 1f);
            }
            else if (_anchorSide == Side.Right)
            {
                _targetCamera.rect = new Rect(0, 0, 1f - ratio, 1f);
            }
        }

        public void Dispose()
        {
            if (_panel != null)
            {
                _panel.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            }
            if (_targetCamera != null)
            {
                // Restore the camera to full screen when the UI closes
                _targetCamera.rect = new Rect(0, 0, 1f, 1f);
            }
        }
    }
}