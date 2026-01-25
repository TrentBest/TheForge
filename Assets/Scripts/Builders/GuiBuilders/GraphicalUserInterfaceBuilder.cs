using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Builders.GuiBuilders
{
    public class GraphicalUserInterfaceBuilder : IGuiProvider
    {
        private bool isEditorMode = false;
        // Basic props
        private string title = string.Empty;
        private int width = 0;
        private int height = 0;

        // Padding
        private int paddingAll = 0;
        private int paddingTop = 0;
        private int paddingRight = 0;
        private int paddingBottom = 0;
        private int paddingLeft = 0;

        // Border widths/colors
        private int borderWidthAll = 0;
        private int borderTopWidth = 0;
        private int borderRightWidth = 0;
        private int borderBottomWidth = 0;
        private int borderLeftWidth = 0;

        private int marginAll = 0;
        private int marginTop = 0;
        private int marginRight = 0;
        private int marginBottom = 0;
        private int marginLeft = 0;

        private Color borderColorAll = default;
        private Color borderTopColor = default;
        private Color borderRightColor = default;
        private Color borderBottomColor = default;
        private Color borderLeftColor = default;

        // Background & corner radius
        private Color backgroundColor = default;

        // Layout
        private FlexDirection flexDirection = FlexDirection.Column;
        private Justify justifyContent = Justify.FlexStart;
        private Align alignItems = Align.Stretch;
        //private float rowGap = 6f;
        //private float columnGap = 6f;

        // Identity
        private string name = "GuiPanel";
        private string className = "gui-panel";
        private string tooltip = string.Empty;

        // Features
        private bool scrollable = false;
        private ScrollViewMode scrollMode = ScrollViewMode.Vertical;
        private bool showHeader = false;
        private bool showFooter = false;
        private string footerText = string.Empty;

        // Header style
        private StyleEnum<FontStyle> headerStyle = FontStyle.Bold;
        private StyleLength headerSize = 16;
        private int headerMarginTop = 0;
        private int headerMarginRight = 0;
        private int headerMarginBottom = 0;
        private int headerMarginLeft = 0;

        // Footer style
        private int footerMarginAll = 0;
        private int footerMarginTop = 0;
        private int footerMarginRight = 0;
        private int footerMarginBottom = 0;
        private int footerMarginLeft = 0;

        private float footerOpacity = 0.75f;

        // Assets
        private StyleSheet styleSheet;
        private VisualTreeAsset uxmlTemplate;

        private float borderRadiusAll = 0f;
        private float borderTopLeftRadius;
        private float borderBottomLeftRadius;
        private float borderTopRightRadius;
        private float borderBottomRightRadius;

        // Composition
        private readonly List<Func<GuiContext, VisualElement>> children = new();
        private readonly List<Action<VisualElement>> onBuildActions = new();
        private readonly List<GraphicalUserInterfaceBuilder> panels = new();
        private readonly GraphicalUserInterfaceBuilder parent;

        //Dynamic sizing
        private float percentWidth = -1f;
        private float percentHeight = -1f;
        private bool autoGrow = false;
        private bool noShrink = false;

        public GraphicalUserInterfaceBuilder(string panelName)
        {
            name = panelName;
            parent = this;
        }

        public GraphicalUserInterfaceBuilder(string panelName, GraphicalUserInterfaceBuilder parentBuilder)
        {
            name = panelName;
            parent = parentBuilder;
        }

        // ---------- Fluent API ----------

        public GraphicalUserInterfaceBuilder WithTitle(string title)
        {
            this.title = title;
            this.showHeader = !string.IsNullOrWhiteSpace(title);
            return this;
        }

        public GraphicalUserInterfaceBuilder WithHeaderFontSize(int fontSize) { this.headerSize = fontSize; return this; }
        public GraphicalUserInterfaceBuilder WithHeaderMargins(int top = 0, int right = 0, int bottom = 4, int left = 0)
        { headerMarginTop = top; headerMarginRight = right; headerMarginBottom = bottom; headerMarginLeft = left; return this; }
        public GraphicalUserInterfaceBuilder WithHeaderStyle(StyleEnum<FontStyle> fontStyle) { this.headerStyle = fontStyle; return this; }

        public GraphicalUserInterfaceBuilder WithSize(int width, int height) { this.width = width; this.height = height; return this; }

        public GraphicalUserInterfaceBuilder WithMargins(int margin)
        {
            this.marginAll = margin;
            return this;
        }
        public GraphicalUserInterfaceBuilder WithMarginTop(int margin) { marginTop = margin; return this; }
        public GraphicalUserInterfaceBuilder WithMarginRight(int margin) { marginRight = margin; return this; }
        public GraphicalUserInterfaceBuilder WithMarginBottom(int margin) { marginBottom = margin; return this; }
        public GraphicalUserInterfaceBuilder WithMarginLeft(int margin) { marginLeft = margin; return this; }

        public GraphicalUserInterfaceBuilder WithPadding(int padding)
        {
            this.paddingAll = padding;
            return this;
        }

        public GraphicalUserInterfaceBuilder WithPaddingTop(int padding) { paddingTop = padding; return this; }
        public GraphicalUserInterfaceBuilder WithPaddingRight(int padding) { paddingRight = padding; return this; }
        public GraphicalUserInterfaceBuilder WithPaddingBottom(int padding) { paddingBottom = padding; return this; }
        public GraphicalUserInterfaceBuilder WithPaddingLeft(int padding) { paddingLeft = padding; return this; }



        public GraphicalUserInterfaceBuilder WithPercentSize(float widthPct = 100f, float heightPct = 100f)
        {
            this.width = 0;   // disable fixed width
            this.height = 0;  // disable fixed height
            this.percentWidth = Mathf.Clamp(widthPct, 0f, 100f);
            this.percentHeight = Mathf.Clamp(heightPct, 0f, 100f);
            return this;
        }

        public GraphicalUserInterfaceBuilder WithAutoGrow(bool grow = true, bool noShrink = true)
        {
            this.autoGrow = grow;
            this.noShrink = noShrink;
            return this;
        }



        public GraphicalUserInterfaceBuilder WithBorderWidth(int width)
        {
            borderWidthAll = width;
            return this;
        }
        public GraphicalUserInterfaceBuilder WithBorderTopWidth(int width) { borderTopWidth = width; return this; }
        public GraphicalUserInterfaceBuilder WithBorderRightWidth(int width) { borderRightWidth = width; return this; }
        public GraphicalUserInterfaceBuilder WithBorderBottomWidth(int width) { borderBottomWidth = width; return this; }
        public GraphicalUserInterfaceBuilder WithBorderLeftWidth(int width) { borderLeftWidth = width; return this; }


        public GraphicalUserInterfaceBuilder WithBorderColor(Color color)
        { borderColorAll = color; return this; }
        public GraphicalUserInterfaceBuilder WithBorderTopColor(Color color) { borderTopColor = color; return this; }
        public GraphicalUserInterfaceBuilder WithBorderRightColor(Color color) { borderRightColor = color; return this; }
        public GraphicalUserInterfaceBuilder WithBorderBottomColor(Color color) { borderBottomColor = color; return this; }
        public GraphicalUserInterfaceBuilder WithBorderLeftColor(Color color) { borderLeftColor = color; return this; }
        public GraphicalUserInterfaceBuilder WithBackgroundColor(Color color) { backgroundColor = color; return this; }
        public GraphicalUserInterfaceBuilder WithBorderRadius(float radius) { borderRadiusAll = Mathf.Max(0f, radius); return this; }
        public GraphicalUserInterfaceBuilder WithBorderTopLeftRadius(float radius) { borderTopLeftRadius = Mathf.Max(0f, radius); return this; }
        public GraphicalUserInterfaceBuilder WithBorderBottomLeftRadius(float radius) { borderBottomLeftRadius = Mathf.Max(0f, radius); return this; }
        public GraphicalUserInterfaceBuilder WithBorderTopRightRadius(float radius) { borderTopRightRadius = Mathf.Max(0f, radius); return this; }
        public GraphicalUserInterfaceBuilder WithBorderBottomRightRadius(float radius) { borderBottomRightRadius = Mathf.Max(0f, radius); return this; }
        public GraphicalUserInterfaceBuilder WithFlexLayout(FlexDirection direction, Justify justify, Align align)
        { flexDirection = direction; justifyContent = justify; alignItems = align; return this; }

        //public GraphicalUserInterfaceBuilder WithGaps(float rowGap, float columnGap)
        //{ this.rowGap = Mathf.Max(0f, rowGap); this.columnGap = Mathf.Max(0f, columnGap); return this; }

        public GraphicalUserInterfaceBuilder WithClass(string className) { this.className = className; return this; }
        public GraphicalUserInterfaceBuilder WithTooltip(string tooltip) { this.tooltip = tooltip; return this; }

        public GraphicalUserInterfaceBuilder WithScrollable(bool enable = true, ScrollViewMode mode = ScrollViewMode.Vertical)
        { scrollable = enable; scrollMode = mode; return this; }

        public GraphicalUserInterfaceBuilder WithFooter(string text)
        { footerText = text ?? string.Empty; showFooter = !string.IsNullOrWhiteSpace(footerText); return this; }

        public GraphicalUserInterfaceBuilder WithStyleSheet(StyleSheet ss) { styleSheet = ss; return this; }
        public GraphicalUserInterfaceBuilder WithUxmlTemplate(VisualTreeAsset uxml) { uxmlTemplate = uxml; return this; }

        // Children
        public GraphicalUserInterfaceBuilder AddChild(VisualElement child) { children.Add(_ => child); return this; }
        public GraphicalUserInterfaceBuilder AddChild(Func<GuiContext, VisualElement> childFactory) { if (childFactory != null) children.Add(childFactory); return this; }
        public GraphicalUserInterfaceBuilder AddChild(IGuiProvider provider) { if (provider != null) children.Add(ctx => provider.CreateGui(ctx)); return this; }

        public GraphicalUserInterfaceBuilder OnBuild(Action<VisualElement> onBuildAction)
        { if (onBuildAction != null) onBuildActions.Add(onBuildAction); return this; }


        private Wrap flexWrap = Wrap.NoWrap;
        public GraphicalUserInterfaceBuilder WithFlexWrap(Wrap wrap)
        {
            flexWrap = wrap;
            return this;
        }

        public GraphicalUserInterfaceBuilder AddIntegerData(string label, int value, Action<int> onValueChange)
        {
            return AddChild(ctx =>
            {
                // Use this builder's specific editor mode
                if (this.isEditorMode)
                {
                    var field = new IntegerField(label) { value = value };
                    field.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));
                    return field;
                }
                // Display mode: Simple label pair
                var container = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                container.Add(new Label($"{label}: ") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
                container.Add(new Label(value.ToString()));
                return container;
            });
        }

        public GraphicalUserInterfaceBuilder AddFloatData(string label, float value, Action<float> onValueChange)
        {
            return AddChild(ctx =>
            {
                // Use this builder's specific editor mode
                if (this.isEditorMode)
                {
                    var field = new FloatField(label) { value = value };
                    field.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));
                    return field;
                }
                // Display mode: Simple label pair
                var container = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                container.Add(new Label($"{label}: ") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
                container.Add(new Label(value.ToString("F2")));
                return container;
            });
        }

        public GraphicalUserInterfaceBuilder AddStringData(string label, string value, Action<string> onValueChange)
        {
            return AddChild(ctx =>
            {
                // Use this builder's specific editor mode
                if (this.isEditorMode)
                {
                    var field = new TextField(label) { value = value ?? string.Empty };
                    field.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));
                    return field;
                }
                // Display mode: Simple label pair
                var container = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                container.Add(new Label($"{label}: ") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
                container.Add(new Label(value ?? string.Empty));
                return container;
            });
        }



        public GraphicalUserInterfaceBuilder WithEditorMode(bool isEditor = false)
        {
            isEditorMode = isEditor;
            return this;
        }

        public GraphicalUserInterfaceBuilder WithPanel(string panelName, bool editorMode = false)
        {
            var panelBuilder = new GraphicalUserInterfaceBuilder(panelName);

            panels.Add(panelBuilder);


            return panelBuilder;
        }

        public GraphicalUserInterfaceBuilder ContinueWithParentPanel()
        {
            return this.parent;
        }

        public GraphicalUserInterfaceBuilder FindSuperPanel(string panelName)
        {
            //double check it's not the current panel
            if (this.name == panelName)
            {
                return this;
            }
            //It's not us, so call up the chain if we have a parent
            return parent?.FindSuperPanel(panelName);
        }

        public GraphicalUserInterfaceBuilder FindSubPanel(string panelName)
        {
            foreach (var item in panels)
            {
                if (item.name == panelName)
                {
                    return item;
                }
            }
            foreach (var item in panels)
            {
                var found = item.FindSubPanel(panelName);
                if (found != null)
                {
                    return found;
                }
            }
            return this;
        }

        public GraphicalUserInterfaceBuilder EndPanel()
        {
            return parent ?? this;
        }

        // ---------- Build ----------
        public VisualElement Build() => CreateGui(new GuiContext());

        public VisualElement CreateGui(GuiContext context)
        {
            VisualElement root;
            if (uxmlTemplate != null)
            {
                root = uxmlTemplate.Instantiate();
                root.name = name;
            }
            else
            {
                root = new VisualElement { name = name, tooltip = tooltip };

                // Layout & size
                root.style.flexDirection = flexDirection;
                root.style.justifyContent = justifyContent;
                root.style.alignItems = alignItems;
                root.style.flexWrap = flexWrap;



                if (percentWidth > 0f) root.style.width = new Length(percentWidth, LengthUnit.Percent);
                else if (width > 0) root.style.width = width;
                else root.style.width = StyleKeyword.Auto;

                if (percentHeight > 0f) root.style.height = new Length(percentHeight, LengthUnit.Percent);
                else if (height > 0) root.style.height = height;
                else root.style.height = StyleKeyword.Auto;

                if (autoGrow) root.style.flexGrow = 1f;
                if (noShrink) root.style.flexShrink = 0f;

                // Padding
                if (paddingAll > 0)
                {
                    root.style.paddingTop = paddingAll;
                    root.style.paddingRight = paddingAll;
                    root.style.paddingBottom = paddingAll;
                    root.style.paddingLeft = paddingAll;
                }
                if (paddingTop > 0) root.style.paddingTop = paddingTop;
                if (paddingRight > 0) root.style.paddingRight = paddingRight;
                if (paddingBottom > 0) root.style.paddingBottom = paddingBottom;
                if (paddingLeft > 0) root.style.paddingLeft = paddingLeft;

                // Border widths
                if (borderWidthAll > 0)
                {
                    root.style.borderTopWidth = borderWidthAll;
                    root.style.borderRightWidth = borderWidthAll;
                    root.style.borderBottomWidth = borderWidthAll;
                    root.style.borderLeftWidth = borderWidthAll;
                }
                if (borderTopWidth > 0) root.style.borderTopWidth = borderTopWidth;
                if (borderRightWidth > 0) root.style.borderRightWidth = borderRightWidth;
                if (borderBottomWidth > 0) root.style.borderBottomWidth = borderBottomWidth;
                if (borderLeftWidth > 0) root.style.borderLeftWidth = borderLeftWidth;

                // Border colors
                if (borderColorAll != default)
                {
                    root.style.borderTopColor = borderColorAll;
                    root.style.borderRightColor = borderColorAll;
                    root.style.borderBottomColor = borderColorAll;
                    root.style.borderLeftColor = borderColorAll;

                }
                if (borderTopColor != default) root.style.borderTopColor = borderTopColor;
                if (borderRightColor != default) root.style.borderRightColor = borderRightColor;
                if (borderBottomColor != default) root.style.borderBottomColor = borderBottomColor;
                if (borderLeftColor != default) root.style.borderLeftColor = borderLeftColor;

                // Background
                if (backgroundColor != default) root.style.backgroundColor = backgroundColor;

                // Corner radius (apply uniformly to all corners)
                if (borderRadiusAll > 0f)
                {
                    root.style.borderTopLeftRadius = borderRadiusAll;
                    root.style.borderTopRightRadius = borderRadiusAll;
                    root.style.borderBottomLeftRadius = borderRadiusAll;
                    root.style.borderBottomRightRadius = borderRadiusAll;
                }
                if (borderTopLeftRadius > 0f) root.style.borderTopLeftRadius = borderTopLeftRadius;
                if (borderBottomLeftRadius > 0f) root.style.borderBottomLeftRadius = borderBottomLeftRadius;
                if (borderTopRightRadius > 0f) root.style.borderTopRightRadius = borderTopRightRadius;
                if (borderBottomRightRadius > 0f) root.style.borderBottomRightRadius = borderBottomRightRadius;
            }

            if (!string.IsNullOrWhiteSpace(className))
                root.AddToClassList(className);

            // Stylesheets: context first, then builder-specific
            if (context?.StyleSheet != null) root.styleSheets.Add(context.StyleSheet);
            if (context?.AdditionalStyleSheets != null)
                foreach (var ss in context.AdditionalStyleSheets) if (ss != null) root.styleSheets.Add(ss);
            if (styleSheet != null) root.styleSheets.Add(styleSheet);

            // Header
            if (showHeader)
            {
                var header = new Label(title) { name = "gui-header" };
                header.style.unityFontStyleAndWeight = headerStyle;
                header.style.fontSize = headerSize;
                header.style.marginTop = headerMarginTop;
                header.style.marginRight = headerMarginRight;
                header.style.marginBottom = headerMarginBottom;
                header.style.marginLeft = headerMarginLeft;
                root.Add(header);
            }

            // Content container (optional scroll view)
            VisualElement content = root;
            if (scrollable)
            {
                var scroll = new ScrollView(scrollMode) { name = "gui-scrollview" };
                scroll.style.flexGrow = 1f;
                root.Add(scroll);
                content = scroll;
            }

            // Children
            foreach (var factory in children)
            {
                try
                {
                    var child = factory?.Invoke(context);
                    if (child != null) content.Add(child);
                }
                catch (Exception ex)
                {
                    context?.Log?.Invoke($"[GUI] Child build failed: {ex.Message}");
                }
            }

            // Footer
            if (showFooter)
            {
                var footer = new Label(footerText) { name = "gui-footer" };
                footer.style.marginTop = footerMarginTop;
                footer.style.opacity = footerOpacity;
                root.Add(footer);
            }

            // Callbacks
            foreach (var act in onBuildActions)
            {
                try { act?.Invoke(root); }
                catch (Exception ex) { context?.Log?.Invoke($"[GUI] OnBuild failed: {ex.Message}"); }
            }
            context?.OnBuilt?.Invoke(root);

            return root;
        }
    }
}
