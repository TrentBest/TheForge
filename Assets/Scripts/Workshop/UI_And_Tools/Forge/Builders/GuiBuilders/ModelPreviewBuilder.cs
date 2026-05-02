using System;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ModelPreviewBuilder : IGuiProvider
    {
        public string Title => "3D Model Preview";

        private GameObject _targetObject;
        private LiveModelPreviewContext _context;
        private Image _previewImage;

        // 3D Orbital Configuration
        private float _zoom = 1.5f;
        private float _pitch = 25f;
        private float _yaw = 45f;
        private bool _autoRotate = true;
        private float _rotationSpeed = 15f;
        private Color _backgroundColor = new Color(0.05f, 0.05f, 0.05f);
        private float _lightIntensity = 1.2f;

        public ModelPreviewBuilder(GameObject targetObject)
        {
            _targetObject = targetObject;
        }

        public ModelPreviewBuilder WithZoom(float zoom) { _zoom = zoom; return this; }
        public ModelPreviewBuilder WithPitch(float pitch) { _pitch = pitch; return this; }
        public ModelPreviewBuilder WithYaw(float yaw) { _yaw = yaw; return this; }
        public ModelPreviewBuilder WithAutoRotate(bool autoRotate) { _autoRotate = autoRotate; return this; }
        public ModelPreviewBuilder WithRotationSpeed(float speed) { _rotationSpeed = speed; return this; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement { style = { flexGrow = 1, backgroundColor = _backgroundColor } };

            _previewImage = new Image { style = { flexGrow = 1 } };

            // Note: We could add PointerDown/Move/Wheel events here too, 
            // but they would map to _context.Yaw, _context.Pitch, and _context.Zoom instead of 2D Pan!
            // _previewImage.RegisterCallback<PointerMoveEvent>(OrbitCameraMath);

            root.Add(_previewImage);

            if (_targetObject != null)
            {
                _context = new LiveModelPreviewContext
                {
                    TargetModel = _targetObject,
                    Zoom = _zoom,
                    Pitch = _pitch,
                    Yaw = _yaw,
                    AutoRotate = _autoRotate,
                    RotationSpeed = _rotationSpeed,
                    BackgroundColor = _backgroundColor,
                    LightIntensity = _lightIntensity
                };

                if ( !FSM_API.Interaction.Exists("LiveModelPreviewFSM"))
                {
                    FSM_API.Create.CreateFiniteStateMachine("LiveModelPreviewFSM", -1, "LivePreviews")
                        .State("Idle", null, null, null)
                        .State("Rendering", null, LiveModelPreviewFSM.OnRenderTick, null) // Needs the 3D Render Tick
                        .Transition("Idle", "Rendering", c => true)
                        .BuildDefinition();
                }

                _context.Name = "ModelPreview_" + Guid.NewGuid().ToString();
                FSM_API.Create.CreateInstance("LiveModelPreviewFSM", _context, "LivePreviews");

                root.schedule.Execute(() => {
                    FSM_API.Interaction.Update("LivePreviews");
                    if (_context.RenderTexture != null)
                    {
                        _previewImage.image = _context.RenderTexture;
                        _previewImage.MarkDirtyRepaint();
                    }
                }).Every(16);

                root.RegisterCallback<DetachFromPanelEvent>(evt => Cleanup());
            }

            return root;
        }

        private void Cleanup()
        {
            if (_context != null)
            {
                if (_context.PreviewCamera != null) UnityEngine.Object.DestroyImmediate(_context.PreviewCamera.gameObject);
                if (_context.RenderTexture != null) _context.RenderTexture.Release();
                if (_context.TargetModel != null) UnityEngine.Object.DestroyImmediate(_context.TargetModel);
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}