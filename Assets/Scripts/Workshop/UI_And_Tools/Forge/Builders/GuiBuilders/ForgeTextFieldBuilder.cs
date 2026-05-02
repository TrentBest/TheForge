using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ForgeTextFieldBuilder : IGuiProvider
    {
        public string Title => "Forge TextField";

        private string _name = null;
        private string _label = null;
        private string _value = "";

        private Action<string> _onChanged;
        private Action<ChangeEvent<string>> _onValueChanged;
        private Action _onSubmit;

        private bool _isPasswordField = false;
        private bool _isMultiline = false;
        private bool _isReadOnly = false;

        private readonly List<Action<VisualElement>> _onBuildActions = new();

        // --- CONSTRUCTORS ---
        public ForgeTextFieldBuilder() { }

        public ForgeTextFieldBuilder(string labelOrName)
        {
            _label = labelOrName;
            _name = labelOrName;
        }

        public ForgeTextFieldBuilder(string label, string initialValue)
        {
            _label = label;
            _name = label;
            _value = initialValue;
        }

        // --- CORE FUNCTIONALITY ---
        public ForgeTextFieldBuilder WithName(string name) { _name = name; return this; }
        public ForgeTextFieldBuilder WithLabel(string label) { _label = label; return this; }
        public ForgeTextFieldBuilder WithValue(string value) { _value = value; return this; }

        // Backwards compatibility for tools that just want the string
        public ForgeTextFieldBuilder OnChanged(Action<string> cb) { _onChanged = cb; return this; }

        // Standard UI Toolkit event signature support (Fixes the ArchitectureAPI errors)
        public ForgeTextFieldBuilder OnValueChanged(Action<ChangeEvent<string>> cb) { _onValueChanged = cb; return this; }

        public ForgeTextFieldBuilder OnSubmit(Action cb) { _onSubmit = cb; return this; }

        public ForgeTextFieldBuilder AsPassword(bool isPassword = true) { _isPasswordField = isPassword; return this; }
        public ForgeTextFieldBuilder AsMultiline(bool isMultiline = true) { _isMultiline = isMultiline; return this; }
        public ForgeTextFieldBuilder AsReadOnly(bool isReadOnly = true) { _isReadOnly = isReadOnly; return this; }

        // --- THE ONBUILD ENGINE ---
        public ForgeTextFieldBuilder OnBuild(Action<VisualElement> onBuildAction)
        {
            if (onBuildAction != null) _onBuildActions.Add(onBuildAction);
            return this;
        }

        // --- INTERFACE IMPLEMENTATION ---
        public VisualElement CreateGui(GuiContext ctx)
        {
            TextField tf;

            // Instantiate native element
            if (!string.IsNullOrEmpty(_label))
                tf = new TextField(_label) { value = _value };
            else
                tf = new TextField { value = _value };

            if (!string.IsNullOrEmpty(_name)) tf.name = _name;

            // Apply configurations
            tf.isPasswordField = _isPasswordField;
            tf.multiline = _isMultiline;
            tf.isReadOnly = _isReadOnly;

            // Hook up reactive events
            if (_onChanged != null)
            {
                tf.RegisterValueChangedCallback(e => _onChanged(e.newValue));
            }

            if (_onValueChanged != null)
            {
                tf.RegisterValueChangedCallback(e => _onValueChanged(e));
            }

            if (_onSubmit != null)
            {
                tf.RegisterCallback<KeyDownEvent>(e =>
                {
                    if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                    {
                        _onSubmit();
                        e.StopPropagation(); // Prevent the Enter key from bubbling up
                    }
                });
            }

            // Execute all delayed styling/layout evaluation actions
            foreach (var act in _onBuildActions) act?.Invoke(tf);

            // Fire the OnBuilt hook so the calling Provider can capture the native reference if needed
            ctx?.OnBuilt?.Invoke(tf);

            return tf;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }

        // ==============================================================================
        // SIZING & SPACING
        // ==============================================================================
        public ForgeTextFieldBuilder WithHeight(StyleLength height) => OnBuild(ve => ve.style.height = height);
        public ForgeTextFieldBuilder WithWidth(StyleLength width) => OnBuild(ve => ve.style.width = width);
        public ForgeTextFieldBuilder WithMinHeight(StyleLength minHeight) => OnBuild(ve => ve.style.minHeight = minHeight);
        public ForgeTextFieldBuilder WithMaxHeight(StyleLength maxHeight) => OnBuild(ve => ve.style.maxHeight = maxHeight);
        public ForgeTextFieldBuilder WithMinWidth(StyleLength minWidth) => OnBuild(ve => ve.style.minWidth = minWidth);
        public ForgeTextFieldBuilder WithMaxWidth(StyleLength maxWidth) => OnBuild(ve => ve.style.maxWidth = maxWidth);

        public ForgeTextFieldBuilder WithMargin(StyleLength all) => WithMargin(all, all, all, all);
        public ForgeTextFieldBuilder WithMargin(StyleLength top, StyleLength right, StyleLength bottom, StyleLength left)
        {
            return OnBuild(ve => {
                ve.style.marginTop = top; ve.style.marginRight = right; ve.style.marginBottom = bottom; ve.style.marginLeft = left;
            });
        }
        public ForgeTextFieldBuilder WithMarginTop(StyleLength top) => OnBuild(ve => ve.style.marginTop = top);
        public ForgeTextFieldBuilder WithMarginBottom(StyleLength bottom) => OnBuild(ve => ve.style.marginBottom = bottom);
        public ForgeTextFieldBuilder WithMarginLeft(StyleLength left) => OnBuild(ve => ve.style.marginLeft = left);
        public ForgeTextFieldBuilder WithMarginRight(StyleLength right) => OnBuild(ve => ve.style.marginRight = right);

        public ForgeTextFieldBuilder WithPadding(StyleLength all) => WithPadding(all, all, all, all);
        public ForgeTextFieldBuilder WithPadding(StyleLength top, StyleLength right, StyleLength bottom, StyleLength left)
        {
            return OnBuild(ve => {
                ve.style.paddingTop = top; ve.style.paddingRight = right; ve.style.paddingBottom = bottom; ve.style.paddingLeft = left;
            });
        }
        public ForgeTextFieldBuilder WithPaddingTop(StyleLength top) => OnBuild(ve => ve.style.paddingTop = top);
        public ForgeTextFieldBuilder WithPaddingBottom(StyleLength bottom) => OnBuild(ve => ve.style.paddingBottom = bottom);
        public ForgeTextFieldBuilder WithPaddingLeft(StyleLength left) => OnBuild(ve => ve.style.paddingLeft = left);
        public ForgeTextFieldBuilder WithPaddingRight(StyleLength right) => OnBuild(ve => ve.style.paddingRight = right);

        // ==============================================================================
        // FLEXBOX LAYOUT
        // ==============================================================================
        public ForgeTextFieldBuilder WithFlexGrow(float grow) => OnBuild(ve => ve.style.flexGrow = grow);
        public ForgeTextFieldBuilder WithFlexShrink(float shrink) => OnBuild(ve => ve.style.flexShrink = shrink);
        public ForgeTextFieldBuilder WithFlexBasis(StyleLength basis) => OnBuild(ve => ve.style.flexBasis = basis);
        public ForgeTextFieldBuilder WithAlignSelf(Align align) => OnBuild(ve => ve.style.alignSelf = align);

        // ==============================================================================
        // COLORS & TYPOGRAPHY
        // ==============================================================================
        public ForgeTextFieldBuilder WithBackgroundColor(Color color) => OnBuild(ve => ve.style.backgroundColor = color);
        public ForgeTextFieldBuilder WithColor(Color color) => OnBuild(ve => ve.style.color = color);
        public ForgeTextFieldBuilder WithTextColor(Color color) => WithColor(color);
        public ForgeTextFieldBuilder WithFontSize(int size) => OnBuild(ve => ve.style.fontSize = size);
        public ForgeTextFieldBuilder WithFontStyle(FontStyle style) => OnBuild(ve => ve.style.unityFontStyleAndWeight = style);
        public ForgeTextFieldBuilder WithTextAlign(TextAnchor anchor) => OnBuild(ve => ve.style.unityTextAlign = anchor);
        public ForgeTextFieldBuilder WithWhiteSpace(WhiteSpace ws) => OnBuild(ve => ve.style.whiteSpace = ws);

        // ==============================================================================
        // BORDERS
        // ==============================================================================
        public ForgeTextFieldBuilder WithBorderColor(Color color)
        {
            return OnBuild(ve => {
                ve.style.borderTopColor = color; ve.style.borderRightColor = color; ve.style.borderBottomColor = color; ve.style.borderLeftColor = color;
            });
        }
        public ForgeTextFieldBuilder WithBorderWidth(float width)
        {
            return OnBuild(ve => {
                ve.style.borderTopWidth = width; ve.style.borderRightWidth = width; ve.style.borderBottomWidth = width; ve.style.borderLeftWidth = width;
            });
        }
        public ForgeTextFieldBuilder WithBorderRadius(float radius) => WithBorderRadius(radius, radius, radius, radius);
        public ForgeTextFieldBuilder WithBorderRadius(float tl, float tr, float br, float bl)
        {
            return OnBuild(ve => {
                ve.style.borderTopLeftRadius = tl; ve.style.borderTopRightRadius = tr; ve.style.borderBottomRightRadius = br; ve.style.borderBottomLeftRadius = bl;
            });
        }

        /// <summary>
        /// Enables or disables multiline support for the TextField.
        /// Essential for long-form data entry like descriptions or rule manifests.
        /// </summary>
        public ForgeTextFieldBuilder WithMultiline(bool isMultiline = true)
        {
            _isMultiline = isMultiline;
            return this; // Maintain fluent chaining
        }

        public ForgeTextFieldBuilder WithPlaceholder(string placeholderText)
        {
            return OnBuild(ve =>
            {
                var tf = ve as TextField;
                if (tf == null) return;

                // Create the ghost placeholder label
                var placeholder = new Label(placeholderText)
                {
                    pickingMode = PickingMode.Ignore // Ensure clicks pass through to the text field
                };

                // Style it to look like standard placeholder text
                placeholder.style.position = Position.Absolute;
                placeholder.style.left = 4;
                placeholder.style.top = 2; // Adjust these slightly based on your custom fonts
                placeholder.style.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                placeholder.style.unityFontStyleAndWeight = FontStyle.Italic;
                placeholder.style.overflow = Overflow.Hidden;

                // UI Toolkit TextFields have an internal element called "unity-text-input"
                // We inject the placeholder directly into that internal container.
                var textInput = tf.Q(TextField.textInputUssName);
                if (textInput != null)
                {
                    textInput.Add(placeholder);
                }
                else
                {
                    tf.Add(placeholder); // Fallback
                }

                // Logic to evaluate visibility
                void EvaluatePlaceholderVisibility()
                {
                    bool hasText = !string.IsNullOrEmpty(tf.value);
                    placeholder.style.display = hasText ? DisplayStyle.None : DisplayStyle.Flex;
                }

                // Hook into focus and value changes
                tf.RegisterValueChangedCallback(e => EvaluatePlaceholderVisibility());

                tf.RegisterCallback<FocusInEvent>(e => placeholder.style.display = DisplayStyle.None);
                tf.RegisterCallback<FocusOutEvent>(e => EvaluatePlaceholderVisibility());

                // Initialize state
                EvaluatePlaceholderVisibility();
            });
        }

       }
}