using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class CameraMatrixBuilder : IGuiProvider
    {
        public string Title { get; set; } = "SECURITY FEED MATRIX";
        private List<RenderTexture> _feeds = new List<RenderTexture>();
        private int _columns;

        /// <summary>
        /// Creates a matrix of live camera feeds.
        /// </summary>
        /// <param name="columns">How many monitors wide the grid should be.</param>
        public CameraMatrixBuilder(int columns = 2)
        {
            _columns = Mathf.Max(1, columns);
        }

        /// <summary>
        /// Hacks into a physical Unity Camera and pipes its output to the UI.
        /// </summary>
        public CameraMatrixBuilder WithCameraFeed(Camera cam, int texWidth = 512, int texHeight = 512)
        {
            if (cam.targetTexture == null)
            {
                cam.targetTexture = new RenderTexture(texWidth, texHeight, 24)
                {
                    name = $"MatrixFeed_{cam.name}",
                    filterMode = FilterMode.Bilinear
                };
            }
            _feeds.Add(cam.targetTexture);
            return this;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // The Root Matrix Container
            var rootBuilder = new GraphicalUserInterfaceBuilder("CameraMatrixRoot")
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                .WithPadding(10)
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.FlexStart)
                .WithWrap(Wrap.Wrap) // Crucial: Allows the grid to wrap to the next row
                .WithPercentSize(100, 100);

            // Calculate width based on columns (minus a tiny bit for margins)
            float percentWidth = (100f / _columns) - 2f;

            for (int i = 0; i < _feeds.Count; i++)
            {
                var feed = _feeds[i];
                string camName = feed.name.Replace("MatrixFeed_", "").ToUpper();

                // Suit up individual monitor
                var monitorBuilder = new GraphicalUserInterfaceBuilder($"Monitor_{i}")
                    .WithMarginAll(1) // 1% margin
                    .WithBorderWidth(2)
                    .WithBorderColor(new Color(0.2f, 0.2f, 0.2f))
                    .WithBackgroundColor(Color.black)
                    .WithFlexLayout(FlexDirection.Column, Justify.SpaceBetween, Align.Stretch)
                    .OnBuild(ve =>
                    {
                        ve.style.width = new Length(percentWidth, LengthUnit.Percent);

                        // To keep a 16:9 aspect ratio for the height
                        ve.schedule.Execute(() => {
                            if (ve.resolvedStyle.width > 0)
                                ve.style.height = ve.resolvedStyle.width * (9f / 16f);
                        });
                    });

                // The actual video feed element
                var screenBuilder = new GraphicalUserInterfaceBuilder("VideoFeed")
                    .WithAutoGrow()
                    .OnBuild(ve =>
                    {
                        // Map the RenderTexture to the UI Toolkit Background
                        ve.style.backgroundImage = Background.FromRenderTexture(feed);
                        ve.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                    });

                // The OSD (On Screen Display) Overlay
                var osdOverlay = new GraphicalUserInterfaceBuilder("OSD")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.FlexEnd)
                    .WithPadding(5)
                    .WithPercentSize(100, 100)
                    .OnBuild(ve => ve.style.position = Position.Absolute) // Overlay on top of video
                    .AddChild(new Label($"CAM {i + 1}: {camName}") { style = { color = Color.red, unityFontStyleAndWeight = FontStyle.Bold, backgroundColor = new Color(0, 0, 0, 0.5f) } })
                    .AddChild(new Label("• REC") { style = { color = Color.red, unityFontStyleAndWeight = FontStyle.Bold } });

                monitorBuilder.AddChild(screenBuilder);
                monitorBuilder.AddChild(osdOverlay);
                rootBuilder.AddChild(monitorBuilder);
            }

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
    }
}