using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.Systems.MicroPackages.Literature;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Literature
{
    public class Workshop_Gui_ScryingPool : IGuiProvider
    {
        public string Title => "THE SCRYING POOL";

        private StoryboardFrame _targetFrame;
        private Camera _scryerCamera; // The virtual camera rendering the viewport

        public Workshop_Gui_ScryingPool(StoryboardFrame frameToScry, Camera virtualCamera)
        {
            _targetFrame = frameToScry;
            _scryerCamera = virtualCamera;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new ForgeContainerBuilder("ScryingPoolRoot")
                .WithFlexGrow(1).WithBackgroundColor(Color.black);

            // 1. The 3D Viewport (Assumes a Render Texture is bound to the background)
            // In your engine, this is where you inject LiveModelPreviewBuilder or TexturePreviewBuilder
            var viewport = new ForgeContainerBuilder("Viewport")
                .WithFlexGrow(1)
                .WithBorderColor(new Color(0.64f, 0.17f, 0.77f)).WithBorderWidth(2)
                .Build();

            // 2. The Holographic HUD (Overlays the Viewport)
            var hud = new ForgeContainerBuilder("ScryerHUD")
                .WithPosition(Position.Absolute)
                .WithBackgroundColor(new Color(0.0f, 0.0f, 0.0f, 0.8f))
                .WithPadding(15).WithBorderRadius(8)
                .WithDirection(FlexDirection.Row)
                .Build();

            var lensControls = new ForgeContainerBuilder("LensControls")
                .WithFlexGrow(1).WithDirection(FlexDirection.Column)
                .AddChild(new ForgeLabelBuilder($"LENS OF TRUTH: ANCHORED TO FRAME {_targetFrame.SequenceNumber}")
                    .WithColor(Color.cyan).WithBold().WithMarginBottom(10))
                .OnBuild(ve => {
                    // Field of View Slider
                    var fovLabel = new Label($"Aperture (FOV): {_targetFrame.FieldOfView}") { style = { color = Color.white } };
                    var fovSlider = new Slider(15f, 120f) { value = _targetFrame.FieldOfView > 0 ? _targetFrame.FieldOfView : 60f };
                    fovSlider.RegisterValueChangedCallback(evt => {
                        _targetFrame.FieldOfView = evt.newValue;
                        fovLabel.text = $"Aperture (FOV): {evt.newValue:F1}";
                        if (_scryerCamera != null) _scryerCamera.fieldOfView = evt.newValue; // LIVE UPDATE THE LENS
                    });
                    ve.Add(fovLabel); ve.Add(fovSlider);
                });

            var captureBtn = new ForgeButtonBuilder("SEAL ARTIFACT (CAPTURE FRAME)")
                .WithBackgroundColor(new Color(0.64f, 0.17f, 0.77f)).WithTextColor(Color.white)
                .WithHeight(60).WithWidth(300).WithBold().WithFontSize(16)
                .WithOnClick(() => ExecuteCapture());

            hud.Add(lensControls.Build());
            hud.Add(captureBtn.CreateGui(ctx));
            viewport.Add(hud);

            root.OnBuild(ve => ve.Add(viewport));
            return root.Build();
        }

        private void ExecuteCapture()
        {
            if (_scryerCamera != null)
            {
                // 1. Save the mathematical state
                _targetFrame.CameraPosition = _scryerCamera.transform.position;
                _targetFrame.CameraRotation = _scryerCamera.transform.rotation;

                // 2. Capture the pixels (RenderTexture to Texture2D)
                Debug.Log($"[Scrying Pool] Artifact Sealed. Position: {_targetFrame.CameraPosition} | Rotation: {_targetFrame.CameraRotation.eulerAngles}");

                // 3. Tell the Storyboard to refresh its image
            }
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}