using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ForgeLabelBuilder : IGuiProvider
    {
        private string _text;
        private Color _color = Color.white;
        private float _fontSize = 12;
        private FontStyle _fontStyle = FontStyle.Normal;

        // Layout & Margin
        private float _marginTop = 0;
        private float _marginBottom = 0;
        private float _marginLeft = 0;
        private float _marginRight = 0;
        private float? _margin = null;

        // Padding
        private float? _paddingTop = null;
        private float? _paddingBottom = null;
        private float? _paddingLeft = null;
        private float? _paddingRight = null;

        // Background & Borders
        private Color? _backgroundColor = null;
        private float? _borderRadius = null;

        private float? _borderWidthTop = null;
        private float? _borderWidthBottom = null;
        private float? _borderWidthLeft = null;
        private float? _borderWidthRight = null;

        private Color? _borderColorTop = null;
        private Color? _borderColorBottom = null;
        private Color? _borderColorLeft = null;
        private Color? _borderColorRight = null;

        // Sizing & Alignment
        private StyleLength _width = StyleKeyword.Null;
        private float? _flexGrow = null;
        private TextAnchor _textAlign = TextAnchor.UpperLeft;
        private WhiteSpace _whiteSpace = WhiteSpace.Normal;
        private float _opacity = 1f;

        // Callbacks & Identification
        private Action<VisualElement> _onBuildCallback;
        private string _name = null;

        private bool _isSelectable = false;

        public string Title => "Forge Label";

        public ForgeLabelBuilder(string text)
        {
            _text = text;
        }

        // --- NEW: Header Preset for Singularity Forge ---
        public ForgeLabelBuilder WithHeaderStyle()
        {
            _fontSize = 18;
            _fontStyle = FontStyle.Bold;
            _color = new Color(0f, 1f, 1f, 1f); // Singularity Cyan
            return this;
        }

        public ForgeLabelBuilder WithColor(Color color) { _color = color; return this; }
        public ForgeLabelBuilder WithFontSize(float size) { _fontSize = size; return this; }
        public ForgeLabelBuilder WithFontStyle(FontStyle style) { _fontStyle = style; return this; }
        public ForgeLabelBuilder WithBold(bool isBold = true) { if (isBold) _fontStyle = FontStyle.Bold; return this; }

        public ForgeLabelBuilder WithMarginTop(float margin) { _marginTop = margin; return this; }
        public ForgeLabelBuilder WithMarginBottom(float margin) { _marginBottom = margin; return this; }
        public ForgeLabelBuilder WithMarginLeft(float margin) { _marginLeft = margin; return this; }
        public ForgeLabelBuilder WithMarginRight(float margin) { _marginRight = margin; return this; }
        public ForgeLabelBuilder WithMargin(float margin) { _margin = margin; return this; }
        public ForgeLabelBuilder WithMargin(float vertical, float horizontal) { _marginTop = vertical; _marginBottom = vertical; _marginLeft = horizontal; _marginRight = horizontal; return this; }
        public ForgeLabelBuilder WithMargin(float top, float right, float bottom, float left) { _marginTop = top; _marginRight = right; _marginBottom = bottom; _marginLeft = left; return this; }

        public ForgeLabelBuilder WithPadding(float padding)
        {
            _paddingTop = padding;
            _paddingBottom = padding;
            _paddingLeft = padding;
            _paddingRight = padding;
            return this;
        }
        public ForgeLabelBuilder WithPaddingTop(float padding) { _paddingTop = padding; return this; }
        public ForgeLabelBuilder WithPaddingBottom(float padding) { _paddingBottom = padding; return this; }
        public ForgeLabelBuilder WithPaddingLeft(float padding) { _paddingLeft = padding; return this; }
        public ForgeLabelBuilder WithPaddingRight(float padding) { _paddingRight = padding; return this; }

        public ForgeLabelBuilder WithBackgroundColor(Color color) { _backgroundColor = color; return this; }
        public ForgeLabelBuilder WithBorderRadius(float radius) { _borderRadius = radius; return this; }

        public ForgeLabelBuilder WithBorderWidth(float width)
        {
            _borderWidthTop = width;
            _borderWidthBottom = width;
            _borderWidthLeft = width;
            _borderWidthRight = width;
            return this;
        }

        public ForgeLabelBuilder WithBorderWidthTop(float width) { _borderWidthTop = width; return this; }
        public ForgeLabelBuilder WithBorderWidthBottom(float width) { _borderWidthBottom = width; return this; }
        public ForgeLabelBuilder WithBorderWidthLeft(float width) { _borderWidthLeft = width; return this; }
        public ForgeLabelBuilder WithBorderWidthRight(float width) { _borderWidthRight = width; return this; }

        public ForgeLabelBuilder WithBorderTopWidth(float width) => WithBorderWidthTop(width);
        public ForgeLabelBuilder WithBorderBottomWidth(float width) => WithBorderWidthBottom(width);
        public ForgeLabelBuilder WithBorderLeftWidth(float width) => WithBorderWidthLeft(width);
        public ForgeLabelBuilder WithBorderRightWidth(float width) => WithBorderWidthRight(width);

        public ForgeLabelBuilder WithBorderColor(Color color)
        {
            _borderColorTop = color;
            _borderColorBottom = color;
            _borderColorLeft = color;
            _borderColorRight = color;
            return this;
        }
        public ForgeLabelBuilder WithBorderColorTop(Color color) { _borderColorTop = color; return this; }
        public ForgeLabelBuilder WithBorderColorBottom(Color color) { _borderColorBottom = color; return this; }
        public ForgeLabelBuilder WithBorderColorLeft(Color color) { _borderColorLeft = color; return this; }
        public ForgeLabelBuilder WithBorderColorRight(Color color) { _borderColorRight = color; return this; }

        public ForgeLabelBuilder WithBorderTopColor(Color color) => WithBorderColorTop(color);
        public ForgeLabelBuilder WithBorderBottomColor(Color color) => WithBorderColorBottom(color);
        public ForgeLabelBuilder WithBorderLeftColor(Color color) => WithBorderColorLeft(color);
        public ForgeLabelBuilder WithBorderRightColor(Color color) => WithBorderColorRight(color);

        public ForgeLabelBuilder WithWidth(float width) { _width = new StyleLength(width); return this; }
        public ForgeLabelBuilder WithWidth(StyleLength width) { _width = width; return this; }
        public ForgeLabelBuilder WithFlexGrow(float flexGrow) { _flexGrow = flexGrow; return this; }
        public ForgeLabelBuilder WithTextAlign(TextAnchor alignment) { _textAlign = alignment; return this; }
        public ForgeLabelBuilder WithAlignment(TextAnchor alignment) { _textAlign = alignment; return this; }
        public ForgeLabelBuilder WithWhiteSpace(WhiteSpace space) { _whiteSpace = space; return this; }
        public ForgeLabelBuilder WithWordWrap(bool wrap = true) { _whiteSpace = wrap ? WhiteSpace.Normal : WhiteSpace.NoWrap; return this; }
        public ForgeLabelBuilder WithOpacity(float opacity) { _opacity = opacity; return this; }
        public ForgeLabelBuilder WithName(string name) { _name = name; return this; }

        public ForgeLabelBuilder OnBuild(Action<VisualElement> callback) { _onBuildCallback += callback; return this; }
        public ForgeLabelBuilder WithStyle(Action<IStyle> styleAction) { _onBuildCallback += ve => styleAction?.Invoke(ve.style); return this; }
        public ForgeLabelBuilder WithCopySelect(bool selectable = true) { _isSelectable = selectable; return this; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            VisualElement targetElement;

            if (_isSelectable)
            {
                var disguisedField = new TextField { value = _text, isReadOnly = true, multiline = true };
                disguisedField.style.backgroundColor = Color.clear;
                disguisedField.style.borderTopWidth = 0;
                disguisedField.style.borderBottomWidth = 0;
                disguisedField.style.borderLeftWidth = 0;
                disguisedField.style.borderRightWidth = 0;
                disguisedField.style.paddingLeft = 0;
                disguisedField.style.paddingRight = 0;
                disguisedField.style.paddingTop = 0;
                disguisedField.style.paddingBottom = 0;
                targetElement = disguisedField;
            }
            else
            {
                targetElement = new Label(_text);
            }

            if (!string.IsNullOrEmpty(_name)) targetElement.name = _name;

            targetElement.style.color = _color;
            targetElement.style.fontSize = _fontSize;
            targetElement.style.unityFontStyleAndWeight = _fontStyle;
            targetElement.style.whiteSpace = _whiteSpace;
            targetElement.style.opacity = _opacity;

            if (_margin.HasValue)
            {
                targetElement.style.marginTop = _margin.Value;
                targetElement.style.marginBottom = _margin.Value;
                targetElement.style.marginLeft = _margin.Value;
                targetElement.style.marginRight = _margin.Value;
            }
            else
            {
                targetElement.style.marginTop = _marginTop;
                targetElement.style.marginBottom = _marginBottom;
                targetElement.style.marginLeft = _marginLeft;
                targetElement.style.marginRight = _marginRight;
            }

            if (_paddingTop.HasValue) targetElement.style.paddingTop = _paddingTop.Value;
            if (_paddingBottom.HasValue) targetElement.style.paddingBottom = _paddingBottom.Value;
            if (_paddingLeft.HasValue) targetElement.style.paddingLeft = _paddingLeft.Value;
            if (_paddingRight.HasValue) targetElement.style.paddingRight = _paddingRight.Value;

            if (_backgroundColor.HasValue) targetElement.style.backgroundColor = _backgroundColor.Value;
            if (_borderRadius.HasValue)
            {
                targetElement.style.borderTopLeftRadius = _borderRadius.Value;
                targetElement.style.borderTopRightRadius = _borderRadius.Value;
                targetElement.style.borderBottomLeftRadius = _borderRadius.Value;
                targetElement.style.borderBottomRightRadius = _borderRadius.Value;
            }

            if (_borderWidthTop.HasValue) targetElement.style.borderTopWidth = _borderWidthTop.Value;
            if (_borderWidthBottom.HasValue) targetElement.style.borderBottomWidth = _borderWidthBottom.Value;
            if (_borderWidthLeft.HasValue) targetElement.style.borderLeftWidth = _borderWidthLeft.Value;
            if (_borderWidthRight.HasValue) targetElement.style.borderRightWidth = _borderWidthRight.Value;

            if (_borderColorTop.HasValue) targetElement.style.borderTopColor = _borderColorTop.Value;
            if (_borderColorBottom.HasValue) targetElement.style.borderBottomColor = _borderColorBottom.Value;
            if (_borderColorLeft.HasValue) targetElement.style.borderLeftColor = _borderColorLeft.Value;
            if (_borderColorRight.HasValue) targetElement.style.borderRightColor = _borderColorRight.Value;

            if (_width.keyword != StyleKeyword.Null) targetElement.style.width = _width;
            if (_flexGrow.HasValue) targetElement.style.flexGrow = _flexGrow.Value;

            targetElement.style.unityTextAlign = _textAlign;

            _onBuildCallback?.Invoke(targetElement);
            return targetElement;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}