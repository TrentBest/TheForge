using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ForgeButtonBuilder : IGuiProvider
    {
        public string Title => "Forge Button";

        private readonly string _text;
        private string _name = null;
        private Action _onClick;

        // Core state variables
        private float _flexGrow = 0;
        private float _paddingTop, _paddingBottom, _paddingLeft, _paddingRight;
        private Color _backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        private Color _textColor = Color.white;
        private float _height = float.NaN;
        private float _width = float.NaN;
        private float _marginTop = 0;
        private float _marginBottom = 0;
        private float _marginLeft = 0;
        private float _marginRight = 0;
        private FontStyle _fontStyle = FontStyle.Normal;
        private int _fontSize = 12;

        private Color? _borderTopColor, _borderBottomColor, _borderLeftColor, _borderRightColor;
        private float? _borderTopWidth, _borderBottomWidth, _borderLeftWidth, _borderRightWidth;

        private readonly List<Action<VisualElement>> _onBuildActions = new();

        // --- FIXED CONSTRUCTORS ---
        // By removing the (string name) constructor, C# will correctly route all single-string
        // instantiations to this constructor, properly assigning the button's text!
        public ForgeButtonBuilder(string text = "", Action onClick = null)
        {
            _text = text;
            _onClick = onClick;
        }

        // Added fluent method to handle UI Toolkit queries Q<Button>("name")
        public ForgeButtonBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var btn = new Button(_onClick);

            if (!string.IsNullOrEmpty(_name)) btn.name = _name;
            if (!string.IsNullOrEmpty(_text)) btn.text = _text;

            // Apply explicitly stored fields
            btn.style.backgroundColor = _backgroundColor;
            btn.style.color = _textColor;
            btn.style.unityFontStyleAndWeight = _fontStyle;
            btn.style.fontSize = _fontSize;
            btn.style.marginTop = _marginTop;
            btn.style.marginBottom = _marginBottom;
            btn.style.marginLeft = _marginLeft;
            btn.style.marginRight = _marginRight;
            btn.style.flexGrow = _flexGrow;
            btn.style.paddingTop = _paddingTop;
            btn.style.paddingBottom = _paddingBottom;
            btn.style.paddingLeft = _paddingLeft;
            btn.style.paddingRight = _paddingRight;

            if (!float.IsNaN(_height)) btn.style.height = _height;
            if (!float.IsNaN(_width)) btn.style.width = _width;

            if (_borderTopColor.HasValue) btn.style.borderTopColor = _borderTopColor.Value;
            if (_borderRightColor.HasValue) btn.style.borderRightColor = _borderRightColor.Value;
            if (_borderBottomColor.HasValue) btn.style.borderBottomColor = _borderBottomColor.Value;
            if (_borderLeftColor.HasValue) btn.style.borderLeftColor = _borderLeftColor.Value;

            if (_borderTopWidth.HasValue) btn.style.borderTopWidth = _borderTopWidth.Value;
            if (_borderRightWidth.HasValue) btn.style.borderRightWidth = _borderRightWidth.Value;
            if (_borderBottomWidth.HasValue) btn.style.borderBottomWidth = _borderBottomWidth.Value;
            if (_borderLeftWidth.HasValue) btn.style.borderLeftWidth = _borderLeftWidth.Value;

            // Execute all delayed evaluation actions (the fluent wrapper engine)
            foreach (var act in _onBuildActions) act?.Invoke(btn);

            ctx?.OnBuilt?.Invoke(btn);
            return btn;
        }

        public ForgeButtonBuilder OnBuild(Action<VisualElement> onBuildAction)
        {
            if (onBuildAction != null) _onBuildActions.Add(onBuildAction);
            return this;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }

        // ==============================================================================
        // CORE & INTERACTION
        // ==============================================================================
        public ForgeButtonBuilder WithOnClick(Action onClick) { _onClick = onClick; return this; }
        public ForgeButtonBuilder OnClick(Action onClick) => WithOnClick(onClick);
        public ForgeButtonBuilder WithTooltip(string tooltip) => OnBuild(ve => ve.tooltip = tooltip);
        public ForgeButtonBuilder WithFocusable(bool focusable) => OnBuild(ve => ve.focusable = focusable);
        public ForgeButtonBuilder SetEnabled(bool enabled) => OnBuild(ve => ve.SetEnabled(enabled));

        // ==============================================================================
        // SIZING & SPACING
        // ==============================================================================
        public ForgeButtonBuilder WithHeight(float height) { _height = height; return this; }
        public ForgeButtonBuilder WithWidth(float width) { _width = width; return this; }
        public ForgeButtonBuilder WithMinHeight(StyleLength minHeight) => OnBuild(ve => ve.style.minHeight = minHeight);
        public ForgeButtonBuilder WithMaxHeight(StyleLength maxHeight) => OnBuild(ve => ve.style.maxHeight = maxHeight);
        public ForgeButtonBuilder WithMinWidth(StyleLength minWidth) => OnBuild(ve => ve.style.minWidth = minWidth);
        public ForgeButtonBuilder WithMaxWidth(StyleLength maxWidth) => OnBuild(ve => ve.style.maxWidth = maxWidth);

        public ForgeButtonBuilder WithMargin(float top, float right, float bottom, float left)
        {
            _marginTop = top; _marginRight = right; _marginBottom = bottom; _marginLeft = left;
            return this;
        }
        public ForgeButtonBuilder WithMargin(float all) => WithMargin(all, all, all, all);
        public ForgeButtonBuilder WithMarginTop(float top) { _marginTop = top; return this; }
        public ForgeButtonBuilder WithMarginBottom(float bottom) { _marginBottom = bottom; return this; }
        public ForgeButtonBuilder WithMarginLeft(float left) { _marginLeft = left; return this; }
        public ForgeButtonBuilder WithMarginRight(float right) { _marginRight = right; return this; }

        public ForgeButtonBuilder WithPadding(float top, float right, float bottom, float left)
        {
            _paddingTop = top; _paddingRight = right; _paddingBottom = bottom; _paddingLeft = left;
            return this;
        }
        public ForgeButtonBuilder WithPadding(float all) => WithPadding(all, all, all, all);
        public ForgeButtonBuilder WithPaddingTop(float top) { _paddingTop = top; return this; }
        public ForgeButtonBuilder WithPaddingBottom(float bottom) { _paddingBottom = bottom; return this; }
        public ForgeButtonBuilder WithPaddingLeft(float left) { _paddingLeft = left; return this; }
        public ForgeButtonBuilder WithPaddingRight(float right) { _paddingRight = right; return this; }

        // ==============================================================================
        // FLEXBOX LAYOUT
        // ==============================================================================
        public ForgeButtonBuilder WithFlexGrow(float grow) { _flexGrow = grow; return this; }
        public ForgeButtonBuilder WithFlexShrink(float shrink) => OnBuild(ve => ve.style.flexShrink = shrink);
        public ForgeButtonBuilder WithFlexBasis(StyleLength basis) => OnBuild(ve => ve.style.flexBasis = basis);
        public ForgeButtonBuilder WithFlexDirection(FlexDirection dir) => OnBuild(ve => ve.style.flexDirection = dir);
        public ForgeButtonBuilder WithFlexWrap(Wrap wrap) => OnBuild(ve => ve.style.flexWrap = wrap);
        public ForgeButtonBuilder WithJustifyContent(Justify justify) => OnBuild(ve => ve.style.justifyContent = justify);
        public ForgeButtonBuilder WithAlignItems(Align align) => OnBuild(ve => ve.style.alignItems = align);
        public ForgeButtonBuilder WithAlignSelf(Align align) => OnBuild(ve => ve.style.alignSelf = align);

        // ==============================================================================
        // POSITIONING & DISPLAY
        // ==============================================================================
        public ForgeButtonBuilder WithPosition(Position position) => OnBuild(ve => ve.style.position = position);
        public ForgeButtonBuilder WithTop(StyleLength top) => OnBuild(ve => ve.style.top = top);
        public ForgeButtonBuilder WithBottom(StyleLength bottom) => OnBuild(ve => ve.style.bottom = bottom);
        public ForgeButtonBuilder WithLeft(StyleLength left) => OnBuild(ve => ve.style.left = left);
        public ForgeButtonBuilder WithRight(StyleLength right) => OnBuild(ve => ve.style.right = right);
        public ForgeButtonBuilder WithDisplay(DisplayStyle display) => OnBuild(ve => ve.style.display = display);
        public ForgeButtonBuilder WithVisibility(Visibility visibility) => OnBuild(ve => ve.style.visibility = visibility);
        public ForgeButtonBuilder WithOpacity(float opacity) => OnBuild(ve => ve.style.opacity = opacity);
        public ForgeButtonBuilder WithOverflow(Overflow overflow) => OnBuild(ve => ve.style.overflow = overflow);

        // ==============================================================================
        // COLORS & BACKGROUNDS
        // ==============================================================================
        public ForgeButtonBuilder WithBackgroundColor(Color color) { _backgroundColor = color; return this; }
        public ForgeButtonBuilder WithBackgroundImage(Texture2D texture) => OnBuild(ve => ve.style.backgroundImage = new StyleBackground(texture));
        public ForgeButtonBuilder WithBackgroundImage(Sprite sprite) => OnBuild(ve => ve.style.backgroundImage = new StyleBackground(sprite));
        public ForgeButtonBuilder WithBackgroundTintColor(Color color) => OnBuild(ve => ve.style.unityBackgroundImageTintColor = color);
        public ForgeButtonBuilder WithBackgroundSize(BackgroundSize size) => OnBuild(ve => ve.style.backgroundSize = size);

        // ==============================================================================
        // TYPOGRAPHY
        // ==============================================================================
        public ForgeButtonBuilder WithTextColor(Color color) { _textColor = color; return this; }
        public ForgeButtonBuilder WithColor(Color color) => WithTextColor(color);
        public ForgeButtonBuilder WithFontStyle(FontStyle style) { _fontStyle = style; return this; }
        public ForgeButtonBuilder WithFontSize(int size) { _fontSize = size; return this; }
        public ForgeButtonBuilder WithTextAlign(TextAnchor anchor) => OnBuild(ve => ve.style.unityTextAlign = anchor);
        public ForgeButtonBuilder WithTextOverflow(TextOverflow overflow) => OnBuild(ve => ve.style.textOverflow = overflow);
        public ForgeButtonBuilder WithWhiteSpace(WhiteSpace ws) => OnBuild(ve => ve.style.whiteSpace = ws);
        public ForgeButtonBuilder WithLetterSpacing(StyleLength spacing) => OnBuild(ve => ve.style.letterSpacing = spacing);
        public ForgeButtonBuilder WithWordSpacing(StyleLength spacing) => OnBuild(ve => ve.style.wordSpacing = spacing);

        public ForgeButtonBuilder WithBold(bool v = true)
        {
            _fontStyle = v ? FontStyle.Bold : FontStyle.Normal;
            return this;
        }

        // ==============================================================================
        // BORDERS
        // ==============================================================================
        public ForgeButtonBuilder WithBorderColor(Color color)
        {
            _borderTopColor = color; _borderBottomColor = color; _borderLeftColor = color; _borderRightColor = color;
            return this;
        }
        public ForgeButtonBuilder WithBorderTopColor(Color color) { _borderTopColor = color; return this; }
        public ForgeButtonBuilder WithBorderBottomColor(Color color) { _borderBottomColor = color; return this; }
        public ForgeButtonBuilder WithBorderLeftColor(Color color) { _borderLeftColor = color; return this; }
        public ForgeButtonBuilder WithBorderRightColor(Color color) { _borderRightColor = color; return this; }

        public ForgeButtonBuilder WithBorderWidth(float width)
        {
            _borderTopWidth = width; _borderBottomWidth = width; _borderLeftWidth = width; _borderRightWidth = width;
            return this;
        }
        public ForgeButtonBuilder WithBorderTopWidth(float width) { _borderTopWidth = width; return this; }
        public ForgeButtonBuilder WithBorderBottomWidth(float width) { _borderBottomWidth = width; return this; }
        public ForgeButtonBuilder WithBorderLeftWidth(float width) { _borderLeftWidth = width; return this; }
        public ForgeButtonBuilder WithBorderRightWidth(float width) { _borderRightWidth = width; return this; }

        public ForgeButtonBuilder WithBorderRadius(float radius) => WithBorderRadius(radius, radius, radius, radius);
        public ForgeButtonBuilder WithBorderRadius(float tl, float tr, float br, float bl)
        {
            return OnBuild(ve => {
                ve.style.borderTopLeftRadius = tl;
                ve.style.borderTopRightRadius = tr;
                ve.style.borderBottomRightRadius = br;
                ve.style.borderBottomLeftRadius = bl;
            });
        }

        // ==============================================================================
        // TRANSFORMS
        // ==============================================================================
        public ForgeButtonBuilder WithTranslate(Translate translate) => OnBuild(ve => ve.style.translate = translate);
        public ForgeButtonBuilder WithTranslate(float x, float y) => OnBuild(ve => ve.style.translate = new Translate(new Length(x), new Length(y)));
        public ForgeButtonBuilder WithRotate(Rotate rotate) => OnBuild(ve => ve.style.rotate = rotate);
        public ForgeButtonBuilder WithRotate(float angleDeg) => OnBuild(ve => ve.style.rotate = new Rotate(new Angle(angleDeg, AngleUnit.Degree)));
        public ForgeButtonBuilder WithScale(Scale scale) => OnBuild(ve => ve.style.scale = scale);
        public ForgeButtonBuilder WithScale(float scale) => OnBuild(ve => ve.style.scale = new Scale(new Vector2(scale, scale)));
        public ForgeButtonBuilder WithTransformOrigin(TransformOrigin origin) => OnBuild(ve => ve.style.transformOrigin = origin);
    }
}