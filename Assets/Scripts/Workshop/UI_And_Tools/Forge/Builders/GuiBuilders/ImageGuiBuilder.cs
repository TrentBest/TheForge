using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{ 
    public class ImageGuiBuilder : IGuiProvider
    {
        public string Title { get; set; } = "Image Container";

        private Texture _texture;
        private ScaleMode _scaleMode = ScaleMode.ScaleToFit;
        private Action _onClick;
        private float? _width;
        private float? _height;
        private string _caption;
        private Color _backgroundColor = Color.clear;
        private Color _borderColor = Color.clear;
        private float _borderWidth = 0;

        // Constructors for either direct textures or Resources paths
        public ImageGuiBuilder(Texture texture) { _texture = texture; }
        public ImageGuiBuilder(string resourcesPath)
        {
            _texture = Resources.Load<Texture>(resourcesPath);
            if (_texture == null) Debug.LogWarning($"[ImageGuiBuilder] Could not find image at Resources/{resourcesPath}");
        }
        // Add this alongside your Texture2D and String constructors
        public ImageGuiBuilder()
        {
            _texture = null;
        }
        // Fluent Modifiers
        public ImageGuiBuilder WithScaleMode(ScaleMode mode) { _scaleMode = mode; return this; }
        public ImageGuiBuilder WithFixedSize(float width, float height) { _width = width; _height = height; return this; }
        public ImageGuiBuilder WithCaption(string caption) { _caption = caption; return this; }
        public ImageGuiBuilder WithBackgroundColor(Color color) { _backgroundColor = color; return this; }
        public ImageGuiBuilder WithBorder(Color color, float width) { _borderColor = color; _borderWidth = width; return this; }

        // Interactive binding
        public ImageGuiBuilder OnClick(Action action) { _onClick = action; return this; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var container = new VisualElement
            {
                style = {
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                    backgroundColor = _backgroundColor,
                    borderTopColor = _borderColor, borderBottomColor = _borderColor,
                    borderLeftColor = _borderColor, borderRightColor = _borderColor,
                    borderTopWidth = _borderWidth, borderBottomWidth = _borderWidth,
                    borderLeftWidth = _borderWidth, borderRightWidth = _borderWidth,
                    overflow = Overflow.Hidden // Keeps the image inside borders
                }
            };

            // Setup the Image Element
            var img = new Image { image = _texture, scaleMode = _scaleMode };

            if (_width.HasValue) img.style.width = _width.Value;
            if (_height.HasValue) img.style.height = _height.Value;
            else { img.style.flexGrow = 1; img.style.width = Length.Percent(100); } // Default to fill space

            // Make it interactive if an action is provided
            if (_onClick != null)
            {
                img.RegisterCallback<ClickEvent>(evt =>
                {
                    evt.StopPropagation(); // Prevent clicking through to menus behind it
                    _onClick?.Invoke();
                });

                // Add a subtle hover effect to indicate it's clickable
                img.RegisterCallback<MouseEnterEvent>(evt => img.style.unityBackgroundImageTintColor = new Color(0.8f, 0.8f, 0.8f));
                img.RegisterCallback<MouseLeaveEvent>(evt => img.style.unityBackgroundImageTintColor = Color.white);
            }

            container.Add(img);

            // Setup the Caption
            if (!string.IsNullOrEmpty(_caption))
            {
                var captionLabel = new Label(_caption)
                {
                    style = {
                        marginTop = 5, marginBottom = 5,
                        color = Color.white,
                        unityFontStyleAndWeight = FontStyle.Bold,
                        unityTextAlign = TextAnchor.MiddleCenter,
                        whiteSpace = WhiteSpace.Normal // Allow caption to wrap
                    }
                };
                container.Add(captionLabel);
            }

            return container;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}