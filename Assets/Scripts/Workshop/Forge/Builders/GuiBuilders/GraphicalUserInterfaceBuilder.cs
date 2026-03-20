using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Xml.Linq;


#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class GraphicalUserInterfaceBuilder : IGuiProvider
    {
        private bool isEditorMode = false;

        // Basic props
        private string title = string.Empty;
        private int width = 0;
        private int height = 0;

        // Padding
        private int paddingAll = 0, paddingTop = 0, paddingRight = 0, paddingBottom = 0, paddingLeft = 0;
        private int marginAll = 0, marginTop = 0, marginRight = 0, marginBottom = 0, marginLeft = 0;

        // Border widths/colors
        private int borderWidthAll = 0, borderTopWidth = 0, borderRightWidth = 0, borderBottomWidth = 0, borderLeftWidth = 0;
        private Color borderColorAll = default, borderTopColor = default, borderRightColor = default, borderBottomColor = default, borderLeftColor = default;

        // Background & corner radius
        private Color backgroundColor = default;
        private float borderRadiusAll = 0f, borderTopLeftRadius, borderBottomLeftRadius, borderTopRightRadius, borderBottomRightRadius;

        // Layout
        private FlexDirection flexDirection = FlexDirection.Column;
        private Justify justifyContent = Justify.FlexStart;
        private Align alignItems = Align.Stretch;
        private Wrap flexWrap = Wrap.NoWrap;

        // Identity
        private string name = "GuiPanel";
        private string className = "gui-panel";
        private string tooltip = string.Empty;

        // Features
        private bool scrollable = false;
        private ScrollViewMode scrollMode = ScrollViewMode.Vertical;
        private bool showHeader = false, showFooter = false;
        private string footerText = string.Empty;

        // Header style
        private StyleEnum<FontStyle> headerStyle = FontStyle.Bold;
        private StyleLength headerSize = 16;
        private int headerMarginTop = 0, headerMarginRight = 0, headerMarginBottom = 0, headerMarginLeft = 0;

        // Footer style
        private int footerMarginTop = 0;
        private float footerOpacity = 0.75f;

        // Assets
        private StyleSheet styleSheet;
        private VisualTreeAsset uxmlTemplate;

        // Composition
        private readonly List<Func<GuiContext, VisualElement>> children = new();
        private readonly List<Action<VisualElement>> onBuildActions = new();
        private readonly List<GraphicalUserInterfaceBuilder> panels = new();
        private readonly GraphicalUserInterfaceBuilder parent;

        // Dynamic Flex & Sizing
        private float percentWidth = -1f;
        private float percentHeight = -1f;
        private float flexGrow = -1f;
        private float flexShrink = -1f;
        private bool autoGrow = false;
        private bool noShrink = false;
        private StyleLength minHeight = new StyleLength(StyleKeyword.Null);
        private StyleLength minWidth = new StyleLength(StyleKeyword.Null);

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
        public GraphicalUserInterfaceBuilder()
        {
            name = "GuiContainer_" + Guid.NewGuid().ToString().Substring(0, 6);
        }
        // ---------- Fluent API ----------

        public string Title => title;

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
        public GraphicalUserInterfaceBuilder WithWidth(int width) { this.width = width; return this; }
        public GraphicalUserInterfaceBuilder WithHeight(int height) { this.height = height; return this; }

        public GraphicalUserInterfaceBuilder WithPercentSize(float widthPct = 100f, float heightPct = 100f)
        {
            this.width = 0; this.height = 0;
            this.percentWidth = Mathf.Clamp(widthPct, 0f, 100f);
            this.percentHeight = Mathf.Clamp(heightPct, 0f, 100f);
            return this;
        }

        // --- FLEX CONTROLS ---
        public GraphicalUserInterfaceBuilder WithAutoGrow(bool grow = true, bool noShrink = false)
        {
            this.autoGrow = grow;
            this.noShrink = noShrink;
            return this;
        }
        public GraphicalUserInterfaceBuilder WithFlexGrow(float grow) { this.flexGrow = grow; return this; }
        public GraphicalUserInterfaceBuilder WithFlexShrink(float shrink) { this.flexShrink = shrink; return this; }
        public GraphicalUserInterfaceBuilder WithMinHeight(float min) { this.minHeight = min; return this; }
        public GraphicalUserInterfaceBuilder WithMinWidth(float min) { this.minWidth = min; return this; }

        public GraphicalUserInterfaceBuilder WithMargins(int margin) { this.marginAll = margin; return this; }
        public GraphicalUserInterfaceBuilder WithMarginAll(int margin) { this.marginAll = margin; return this; }
        public GraphicalUserInterfaceBuilder WithMarginTop(int margin) { marginTop = margin; return this; }
        public GraphicalUserInterfaceBuilder WithMarginRight(int margin) { marginRight = margin; return this; }
        public GraphicalUserInterfaceBuilder WithMarginBottom(int margin) { marginBottom = margin; return this; }
        public GraphicalUserInterfaceBuilder WithMarginLeft(int margin) { marginLeft = margin; return this; }

        public GraphicalUserInterfaceBuilder WithPadding(int padding) { this.paddingAll = padding; return this; }
        public GraphicalUserInterfaceBuilder WithPaddingTop(int padding) { paddingTop = padding; return this; }
        public GraphicalUserInterfaceBuilder WithPaddingRight(int padding) { paddingRight = padding; return this; }
        public GraphicalUserInterfaceBuilder WithPaddingBottom(int padding) { paddingBottom = padding; return this; }
        public GraphicalUserInterfaceBuilder WithPaddingLeft(int padding) { paddingLeft = padding; return this; }

        public GraphicalUserInterfaceBuilder WithBorderWidth(int width) { borderWidthAll = width; return this; }
        public GraphicalUserInterfaceBuilder WithBorderTopWidth(int width) { borderTopWidth = width; return this; }
        public GraphicalUserInterfaceBuilder WithBorderRightWidth(int width) { borderRightWidth = width; return this; }
        public GraphicalUserInterfaceBuilder WithBorderBottomWidth(int width) { borderBottomWidth = width; return this; }
        public GraphicalUserInterfaceBuilder WithBorderLeftWidth(int width) { borderLeftWidth = width; return this; }

        public GraphicalUserInterfaceBuilder WithBorderColor(Color color) { borderColorAll = color; return this; }
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

        public GraphicalUserInterfaceBuilder WithFlexLayout(
     FlexDirection direction,
     Justify justify = Justify.FlexStart,
     Align align = Align.Auto)
        {
            this.flexDirection = direction;
            this.justifyContent = justify;
            this.alignItems = align;
            return this;
        }

        public GraphicalUserInterfaceBuilder WithFlexWrap(Wrap wrap) { flexWrap = wrap; return this; }
        public GraphicalUserInterfaceBuilder WithWrap(Wrap wrapMode) { flexWrap = wrapMode; return this; }

        public GraphicalUserInterfaceBuilder WithClass(string className) { this.className = className; return this; }
        public GraphicalUserInterfaceBuilder WithTooltip(string tooltip) { this.tooltip = tooltip; return this; }

        public GraphicalUserInterfaceBuilder WithScrollable(bool enable = true, ScrollViewMode mode = ScrollViewMode.Vertical)
        { scrollable = enable; scrollMode = mode; return this; }

        public GraphicalUserInterfaceBuilder WithFooter(string text)
        { footerText = text ?? string.Empty; showFooter = !string.IsNullOrWhiteSpace(footerText); return this; }

        public GraphicalUserInterfaceBuilder WithStyleSheet(StyleSheet ss) { styleSheet = ss; return this; }
        public GraphicalUserInterfaceBuilder WithUxmlTemplate(VisualTreeAsset uxml) { uxmlTemplate = uxml; return this; }

        // --- CHILDREN OVERLOADS ---
        public GraphicalUserInterfaceBuilder AddChild(VisualElement child) { children.Add(_ => child); return this; }
        public GraphicalUserInterfaceBuilder AddChild(Func<GuiContext, VisualElement> childFactory) { if (childFactory != null) children.Add(childFactory); return this; }
        public GraphicalUserInterfaceBuilder AddChild(IGuiProvider provider) { if (provider != null) children.Add(ctx => provider.CreateGui(ctx)); return this; }

        public GraphicalUserInterfaceBuilder OnBuild(Action<VisualElement> onBuildAction)
        { if (onBuildAction != null) onBuildActions.Add(onBuildAction); return this; }

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

        public GraphicalUserInterfaceBuilder ContinueWithParentPanel() { return this.parent; }

        public GraphicalUserInterfaceBuilder FindSuperPanel(string panelName)
        {
            if (this.name == panelName) return this;
            return parent?.FindSuperPanel(panelName);
        }

        public GraphicalUserInterfaceBuilder FindSubPanel(string panelName)
        {
            foreach (var item in panels) { if (item.name == panelName) return item; }
            foreach (var item in panels)
            {
                var found = item.FindSubPanel(panelName);
                if (found != null) return found;
            }
            return this;
        }

        public GraphicalUserInterfaceBuilder EndPanel() { return parent ?? this; }

        // ---------------------------------------------------------
        // DATA BINDING METHODS & CONTROLS
        // ---------------------------------------------------------

        public GraphicalUserInterfaceBuilder AddIntegerData(string label, int value, Action<int> onValueChange)
        {
            return AddChild(ctx =>
            {
                var field = new IntegerField(label) { value = value };
                field.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));
                field.style.marginBottom = 4;
                return field;
            });
        }

        public GraphicalUserInterfaceBuilder AddFloatData(string label, float value, Action<float> onValueChange)
        {
            return AddChild(ctx =>
            {
                var field = new FloatField(label) { value = value };
                field.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));
                field.style.marginBottom = 4;
                return field;
            });
        }

        public GraphicalUserInterfaceBuilder AddStringData(string label, string value, Action<string> onValueChange)
        {
            return AddChild(ctx =>
            {
                var field = new TextField(label) { value = value ?? string.Empty };
                field.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));
                field.style.marginBottom = 4;
                return field;
            });
        }

        public GraphicalUserInterfaceBuilder AddSliderData(string label, float min, float max, float current, Action<float> onValueChange)
        {
            return AddChild(ctx =>
            {
                var slider = new Slider(label, min, max) { value = current };
                slider.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));
                slider.style.marginBottom = 4;
                return slider;
            });
        }

        public GraphicalUserInterfaceBuilder AddIntSliderData(string label, int min, int max, int current, Action<int> onValueChange)
        {
            return AddChild(ctx =>
            {
                var slider = new SliderInt(label, min, max) { value = current };
                slider.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));
                slider.style.marginBottom = 4;
                return slider;
            });
        }

        public GraphicalUserInterfaceBuilder AddEnumData<T>(string label, T selectedValue, Action<T> onValueChange) where T : Enum
        {
            return AddChild(ctx =>
            {
#if UNITY_EDITOR
                if (this.isEditorMode)
                {
                    var field = new UnityEditor.UIElements.EnumFlagsField(label, selectedValue);
                    field.RegisterValueChangedCallback(evt => onValueChange?.Invoke((T)evt.newValue));
                    return field;
                }
#endif
                var names = Enum.GetNames(typeof(T)).ToList();
                int index = Array.IndexOf(Enum.GetValues(typeof(T)), selectedValue);

                var dropdown = new DropdownField(label, names, index);
                dropdown.RegisterValueChangedCallback(evt =>
                {
                    try
                    {
                        T result = (T)Enum.Parse(typeof(T), evt.newValue);
                        onValueChange?.Invoke(result);
                    }
                    catch { }
                });

                return dropdown;
            });
        }

        public GraphicalUserInterfaceBuilder AddButton(string text, Action onClick)
        {
            return AddChild(ctx =>
            {
                var btn = new Button(onClick) { text = text };
                btn.style.height = 30;
                btn.style.marginBottom = 4;
                return btn;
            });
        }

        public GraphicalUserInterfaceBuilder AddToggleData(string label, bool value, Action<bool> onValueChange)
        {
            return AddChild(ctx =>
            {
                var toggle = new Toggle(label) { value = value };
                toggle.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));
                toggle.style.marginBottom = 4;
                return toggle;
            });
        }

        public GraphicalUserInterfaceBuilder AddSeparator(Color color = default, int height = 1)
        {
            if (color == default) color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            return AddChild(ctx => new VisualElement
            {
                style = {
                    height = height,
                    backgroundColor = color,
                    marginTop = 5,
                    marginBottom = 5,
                    width = Length.Percent(100)
                }
            });
        }

        public GraphicalUserInterfaceBuilder AddImage(Texture2D texture, int width = 0, int height = 0)
        {
            return AddChild(ctx =>
            {
                var img = new Image { image = texture };
                if (width > 0) img.style.width = width;
                if (height > 0) img.style.height = height;
                img.scaleMode = ScaleMode.ScaleToFit;
                return img;
            });
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

                root.style.flexDirection = flexDirection;
                root.style.justifyContent = justifyContent;
                root.style.alignItems = alignItems;
                root.style.flexWrap = flexWrap;

                if (percentWidth > 0f) root.style.width = new Length(percentWidth, LengthUnit.Percent);
                else if (width > 0) root.style.width = width;

                if (percentHeight > 0f) root.style.height = new Length(percentHeight, LengthUnit.Percent);
                else if (height > 0) root.style.height = height;

                // Priority Flex Setup
                if (autoGrow) root.style.flexGrow = 1f;
                else if (flexGrow >= 0f) root.style.flexGrow = flexGrow;

                if (noShrink) root.style.flexShrink = 0f;
                else if (flexShrink >= 0f) root.style.flexShrink = flexShrink;
                else root.style.flexShrink = 1f; // Default allow shrink to prevent overlaps

                if (minHeight.keyword != StyleKeyword.Null || minHeight.value.value > 0) root.style.minHeight = minHeight;
                if (minWidth.keyword != StyleKeyword.Null || minWidth.value.value > 0) root.style.minWidth = minWidth;

                // Padding & Margins
                if (paddingAll > 0) { root.style.paddingTop = paddingAll; root.style.paddingRight = paddingAll; root.style.paddingBottom = paddingAll; root.style.paddingLeft = paddingAll; }
                if (paddingTop > 0) root.style.paddingTop = paddingTop;
                if (paddingRight > 0) root.style.paddingRight = paddingRight;
                if (paddingBottom > 0) root.style.paddingBottom = paddingBottom;
                if (paddingLeft > 0) root.style.paddingLeft = paddingLeft;

                if (marginAll > 0) { root.style.marginTop = marginAll; root.style.marginRight = marginAll; root.style.marginBottom = marginAll; root.style.marginLeft = marginAll; }
                if (marginTop > 0) root.style.marginTop = marginTop;
                if (marginBottom > 0) root.style.marginBottom = marginBottom;

                // Borders & Backgrounds
                if (borderWidthAll > 0) { root.style.borderTopWidth = borderWidthAll; root.style.borderRightWidth = borderWidthAll; root.style.borderBottomWidth = borderWidthAll; root.style.borderLeftWidth = borderWidthAll; }
                if (borderTopWidth > 0) root.style.borderTopWidth = borderTopWidth;
                if (borderRightWidth > 0) root.style.borderRightWidth = borderRightWidth;
                if (borderBottomWidth > 0) root.style.borderBottomWidth = borderBottomWidth;
                if (borderLeftWidth > 0) root.style.borderLeftWidth = borderLeftWidth;

                if (borderColorAll != default) { root.style.borderTopColor = borderColorAll; root.style.borderRightColor = borderColorAll; root.style.borderBottomColor = borderColorAll; root.style.borderLeftColor = borderColorAll; }
                if (borderTopColor != default) root.style.borderTopColor = borderTopColor;
                if (borderRightColor != default) root.style.borderRightColor = borderRightColor;
                if (borderBottomColor != default) root.style.borderBottomColor = borderBottomColor;
                if (borderLeftColor != default) root.style.borderLeftColor = borderLeftColor;

                if (backgroundColor != default) root.style.backgroundColor = backgroundColor;

                if (borderRadiusAll > 0f) { root.style.borderTopLeftRadius = borderRadiusAll; root.style.borderTopRightRadius = borderRadiusAll; root.style.borderBottomLeftRadius = borderRadiusAll; root.style.borderBottomRightRadius = borderRadiusAll; }
                if (borderTopLeftRadius > 0f) root.style.borderTopLeftRadius = borderTopLeftRadius;
                if (borderBottomLeftRadius > 0f) root.style.borderBottomLeftRadius = borderBottomLeftRadius;
                if (borderTopRightRadius > 0f) root.style.borderTopRightRadius = borderTopRightRadius;
                if (borderBottomRightRadius > 0f) root.style.borderBottomRightRadius = borderBottomRightRadius;
            }

            if (!string.IsNullOrWhiteSpace(className)) root.AddToClassList(className);

            // Stylesheets
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

            VisualElement content = root;
            if (scrollable)
            {
                var scroll = new ScrollView(scrollMode) { name = "gui-scrollview" };
                scroll.style.flexGrow = 1f;
                // CRITICAL FIX: Scrollviews must be allowed to shrink to boundary sizes
                scroll.style.flexShrink = 1f;
                root.Add(scroll);
                content = scroll;
            }

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

            foreach (var act in onBuildActions)
            {
                try { act?.Invoke(root); }
                catch (Exception ex) { context?.Log?.Invoke($"[GUI] OnBuild failed: {ex.Message}"); }
            }
            context?.OnBuilt?.Invoke(root);

            return root;
        }

        public Action<VisualElement> GetGuiBuilder()
        {
            return (root) =>
            {
                var content = Build();
                root.Add(content);
            };
        }

        // ---------------------------------------------------------
        // UXML SERIALIZATION METHODS
        // ---------------------------------------------------------

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath)
        {
            VisualElement root = Build();
            string uxmlContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                 "<ui:UXML xmlns:ui=\"UnityEngine.UIElements\" xmlns:uie=\"UnityEditor.UIElements\">\n";
            uxmlContent += SerializeElement(root, 1);
            uxmlContent += "</ui:UXML>";

            File.WriteAllText(assetPath, uxmlContent);
            AssetDatabase.Refresh();
            Debug.Log($"[Singularity Workshop] GUI Serialized to UIDocument at: {assetPath}");
        }

        private static string SerializeElement(VisualElement ve, int indent)
        {
            string spacing = new string(' ', indent * 4);
            string nameAttr = !string.IsNullOrEmpty(ve.name) ? $" name=\"{ve.name}\"" : "";
            string styleAttr = $" style=\"flex-grow: {ve.style.flexGrow.value}; flex-shrink: {ve.style.flexShrink.value};\"";

            string xml = $"{spacing}<ui:VisualElement{nameAttr}{styleAttr}>\n";

            foreach (var child in ve.Children())
            {
                xml += SerializeElement(child, indent + 1);
            }

            xml += $"{spacing}</ui:VisualElement>\n";
            return xml;
        }

        public static void ConvertToUIDocument(VisualElement root, string assetPath)
        {
            string uxml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                          "<ui:UXML xmlns:ui=\"UnityEngine.UIElements\">\n" +
                          SerializeElement(root, 1) +
                          "</ui:UXML>";

            File.WriteAllText(assetPath, uxml);
            AssetDatabase.Refresh();
        }
#endif

        public void FromUIDocument(string assetPath)
        {
            var visualTree = Resources.Load<VisualTreeAsset>(assetPath);
            if (visualTree != null)
            {
                this.WithUxmlTemplate(visualTree);
            }
            else
            {
                Debug.LogError($"[Singularity] Failed to hydrate GUI from {assetPath}");
            }
        }

        public static VisualElement ConvertFromUIDocument(string assetPath)
        {
            var template = Resources.Load<VisualTreeAsset>(assetPath);
            return template != null ? template.CloneTree() : new VisualElement();
        }

        public GraphicalUserInterfaceBuilder AddImage(string resourcesPath, float width, float height, Action onClick = null)
        {
            var imgBuilder = new ImageGuiBuilder(resourcesPath)
                .WithFixedSize(width, height)
                .OnClick(onClick);

            return AddChild(imgBuilder);
        }

        public GraphicalUserInterfaceBuilder AddImage(ImageGuiBuilder preConfiguredBuilder)
        {
            return AddChild(preConfiguredBuilder);
        }



        public GraphicalUserInterfaceBuilder AddDropdownData(string label, List<string> choices, int defaultIndex, Action<string> onValueChange)
        {
            return AddChild(ctx =>
            {
                // Safeguard against invalid list indexes
                int safeIndex = (choices != null && defaultIndex >= 0 && defaultIndex < choices.Count) ? defaultIndex : 0;

                // Create the UI Toolkit DropdownField
                var dropdown = new DropdownField(label, choices, safeIndex);

                // Bind the callback event
                dropdown.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));

                // Match the visual style of your other inputs
                dropdown.style.marginBottom = 4;

                return dropdown;
            });
        }

        public GraphicalUserInterfaceBuilder WithBorderAllColor(Color cyan)
        {
            WithBorderBottomColor(cyan);
            WithBorderTopColor(cyan);
            WithBorderRightColor(cyan);
            WithBorderLeftColor(cyan);
            return this;
        }

        public GraphicalUserInterfaceBuilder AddHeader(string text, Color color = default)
        {
            return AddChild(ctx =>
            {
                var header = new Label(text);
                header.style.unityFontStyleAndWeight = FontStyle.Bold;
                header.style.fontSize = 16; // Matches your default headerSize
                header.style.marginTop = 10;
                header.style.marginBottom = 10;
                header.style.unityTextAlign = TextAnchor.MiddleCenter;

                if (color != default)
                {
                    header.style.color = color;
                }

                return header;
            });
        }
    }
}