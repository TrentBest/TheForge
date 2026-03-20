using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class TexturePreviewBuilder : IGuiProvider
    {
        public string Title => "Texture Canvas";

        private TexturePreviewContext _context;
        private ImageGuiBuilder _internalImageBuilder;

        // Interaction state
        private bool _isDragging = false;
        private Vector2 _lastMousePosition;
        private float _zoomSensitivity = 0.1f;
        private float _panSensitivity = 0.01f;

        public TexturePreviewBuilder(RenderTexture targetTexture)
        {
            _context = new TexturePreviewContext { TargetTexture = targetTexture };

            // Compose the ImageGuiBuilder to handle the "Look"
            _internalImageBuilder = new ImageGuiBuilder(targetTexture)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                .WithScaleMode(ScaleMode.ScaleAndCrop); // Better for interactive viewports
        }

        // Pass through styling to the internal builder
        public TexturePreviewBuilder WithBackgroundColor(Color color) { _internalImageBuilder.WithBackgroundColor(color); return this; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. Let the Image Builder create the visual hierarchy
            var root = _internalImageBuilder.CreateGui(ctx);

            // 2. Find the Image element within that hierarchy to attach listeners
            // This is cleaner than inheriting; we just "augment" the visual output
            var img = root.Q<Image>();
            if (img == null) return root;

            // 3. Register the interactivity (Pan/Zoom logic)
            img.RegisterCallback<PointerDownEvent>(OnPointerDown);
            img.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            img.RegisterCallback<PointerUpEvent>(OnPointerUp);
            img.RegisterCallback<PointerOutEvent>(OnPointerUp);
            img.RegisterCallback<WheelEvent>(OnWheel);

            // 4. Unified Repaint Loop
            root.schedule.Execute(() => {
                if (_context.TargetTexture != null)
                {
                    img.MarkDirtyRepaint();
                }
            }).Every(16);

            return root;
        }

        #region Interaction Logic
        private void OnPointerDown(PointerDownEvent evt)
        {
            _isDragging = true;
            _lastMousePosition = evt.position;
            ((VisualElement)evt.target).CapturePointer(evt.pointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isDragging) return;
            Vector2 delta = (Vector2)evt.position - _lastMousePosition;
            _context.PanOffset -= new Vector2(delta.x, -delta.y) * (_panSensitivity / _context.ZoomLevel);
            _context.IsDirty = true;
            _lastMousePosition = evt.position;
        }

        private void OnPointerUp(EventBase evt)
        {
            _isDragging = false;
            if (evt is PointerUpEvent pu) ((VisualElement)evt.target).ReleasePointer(pu.pointerId);
        }

        private void OnWheel(WheelEvent evt)
        {
            float zoomDelta = evt.delta.y * _zoomSensitivity;
            _context.ZoomLevel = Mathf.Clamp(_context.ZoomLevel - zoomDelta, 0.1f, 100000f);
            _context.IsDirty = true;
            evt.StopPropagation();
        }
        #endregion

        public TexturePreviewContext GetContext() => _context;
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }

        public TexturePreviewBuilder WithZoomSensitivity(float sensitivity)
        {
            _zoomSensitivity = sensitivity;
            return this;
        }

        public TexturePreviewBuilder WithPanSensitivity(float sensitivity)
        {
            _panSensitivity = sensitivity;
            return this;
        }
    }

    public class TexturePreviewContext
    {
        public bool IsDirty { get; internal set; }
        public RenderTexture TargetTexture { get;  set; }
        public Vector2 PanOffset { get;  set; }
        public float ZoomLevel { get;  set; }
    }
}