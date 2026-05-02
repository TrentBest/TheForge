using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Xml.Linq;
using TheSingularityWorkshop.FSM_API;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes; // Required for GuiTheme

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class GraphicalUserInterfaceBuilder : IGuiProvider
    {
        #region --- Fields: Identity & State ---
        private bool isEditorMode = false;
        private string name = "GuiPanel";
        private string className = "gui-panel";

        private string tooltip = string.Empty;
        private string _fsmInstanceId;
        private GuiContext _activeCtx;
        #endregion

        #region --- Fields: Layout & Sizing ---
        private float width = 0f;
        private float height = 0f;
        private float percentWidth = -1f;
        private float percentHeight = -1f;
        private float flexGrow = -1f;
        private float flexShrink = -1f;
        private bool autoGrow = false;
        private bool noShrink = false;
        private StyleLength minHeight = new StyleLength(StyleKeyword.Null);
        private StyleLength minWidth = new StyleLength(StyleKeyword.Null);
        private StyleLength maxHeight = new StyleLength(StyleKeyword.Null);
        private StyleLength maxWidth = new StyleLength(StyleKeyword.Null);
        private FlexDirection flexDirection = FlexDirection.Column;
        private Justify justifyContent = Justify.FlexStart;
        private Align alignItems = Align.Stretch;
        private Wrap flexWrap = Wrap.NoWrap;
        #endregion

        #region --- Fields: Styling & Theming ---
        private GuiTheme _theme; // Explicit theme for this node

        private int paddingAll = 0, paddingTop = 0, paddingRight = 0, paddingBottom = 0, paddingLeft = 0;
        private int marginAll = 0, marginTop = 0, marginRight = 0, marginBottom = 0, marginLeft = 0;

        // Explicit override trackers
        private bool _hasExplicitBorderWidth = false;
        private bool _hasExplicitBorderColor = false;
        private bool _hasExplicitBg = false;
        private bool _hasExplicitRadius = false;

        private float borderWidthAll = 0, borderTopWidth = 0, borderRightWidth = 0, borderBottomWidth = 0, borderLeftWidth = 0;
        private Color borderColorAll = default, borderTopColor = default, borderRightColor = default, borderBottomColor = default, borderLeftColor = default;

        private Color backgroundColor = default;
        private float borderRadiusAll = 0f, borderTopLeftRadius, borderBottomLeftRadius, borderTopRightRadius, borderBottomRightRadius;
        #endregion

        #region --- Fields: UI Components & Assets ---
        private bool scrollable = false;
        private ScrollViewMode scrollMode = ScrollViewMode.Vertical;
        private bool showHeader = false, showFooter = false;
        private string footerText = string.Empty;

        private StyleEnum<FontStyle> headerStyle = FontStyle.Bold;
        private StyleLength headerSize = 16;
        private int headerMarginTop = 0, headerMarginRight = 0, headerMarginBottom = 0, headerMarginLeft = 0;

        private int footerMarginTop = 0;
        private float footerOpacity = 0.75f;

        private StyleSheet styleSheet;
        private VisualTreeAsset uxmlTemplate;
        private string processingGroup = "GUI";
        private Position position;
        #endregion

        #region --- Fields: Composition ---
        private readonly List<Func<GuiContext, VisualElement>> children = new();
        private readonly List<Action<VisualElement>> onBuildActions = new();
        private readonly List<GraphicalUserInterfaceBuilder> panels = new();
        private readonly GraphicalUserInterfaceBuilder parent;
        #endregion

        #region --- Constructors ---
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
            parent = this;
        }
        #endregion

        #region --- Fluent API: Header, Title & Theme ---
        public string Title { get; set; } = "Untitled Gui";
        public FSMHandle VisibilityStatus { get; private set; }

        public GraphicalUserInterfaceBuilder WithTitle(string title)
        {
            Title = title;
            this.showHeader = !string.IsNullOrWhiteSpace(title);
            return this;
        }

        public GraphicalUserInterfaceBuilder WithTheme(GuiTheme theme)
        {
            _theme = theme;
            return this;
        }

        public GraphicalUserInterfaceBuilder WithHeaderFontSize(int fontSize) { this.headerSize = fontSize; return this; }
        public GraphicalUserInterfaceBuilder WithHeaderMargins(int top = 0, int right = 0, int bottom = 4, int left = 0)
        { headerMarginTop = top; headerMarginRight = right; headerMarginBottom = bottom; headerMarginLeft = left; return this; }
        public GraphicalUserInterfaceBuilder WithHeaderStyle(StyleEnum<FontStyle> fontStyle) { this.headerStyle = fontStyle; return this; }
        #endregion

        #region --- Fluent API: Sizing & Flex ---
        public GraphicalUserInterfaceBuilder WithSize(float width, float height) { this.width = width; this.height = height; return this; }
        public GraphicalUserInterfaceBuilder WithWidth(float width) { this.width = width; return this; }
        public GraphicalUserInterfaceBuilder WithHeight(float height) { this.height = height; return this; }

        public GraphicalUserInterfaceBuilder WithPercentSize(float widthPct = 100f, float heightPct = 100f)
        {
            this.width = 0; this.height = 0;
            this.percentWidth = Mathf.Clamp(widthPct, 0f, 100f);
            this.percentHeight = Mathf.Clamp(heightPct, 0f, 100f);
            return this;
        }

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
        #endregion

        #region --- Fluent API: Margins & Padding ---
        public GraphicalUserInterfaceBuilder WithMargins(int margin) { this.marginAll = margin; return this; }
        public GraphicalUserInterfaceBuilder WithMargin(int margin) { this.marginAll = margin; return this; }
        public GraphicalUserInterfaceBuilder WithMarginTop(int margin) { marginTop = margin; return this; }
        public GraphicalUserInterfaceBuilder WithMarginRight(int margin) { marginRight = margin; return this; }
        public GraphicalUserInterfaceBuilder WithMarginBottom(int margin) { marginBottom = margin; return this; }
        public GraphicalUserInterfaceBuilder WithMarginLeft(int margin) { marginLeft = margin; return this; }

        public GraphicalUserInterfaceBuilder WithPadding(int padding) { this.paddingAll = padding; return this; }
        public GraphicalUserInterfaceBuilder WithPaddingTop(int padding) { paddingTop = padding; return this; }
        public GraphicalUserInterfaceBuilder WithPaddingRight(int padding) { paddingRight = padding; return this; }
        public GraphicalUserInterfaceBuilder WithPaddingBottom(int padding) { paddingBottom = padding; return this; }
        public GraphicalUserInterfaceBuilder WithPaddingLeft(int padding) { paddingLeft = padding; return this; }
        #endregion

        #region --- Fluent API: Borders & Background ---
        public GraphicalUserInterfaceBuilder WithBorderWidth(float width) { borderWidthAll = width; _hasExplicitBorderWidth = true; return this; }
        public GraphicalUserInterfaceBuilder WithBorderTopWidth(float width) { borderTopWidth = width; _hasExplicitBorderWidth = true; return this; }
        public GraphicalUserInterfaceBuilder WithBorderRightWidth(float width) { borderRightWidth = width; _hasExplicitBorderWidth = true; return this; }
        public GraphicalUserInterfaceBuilder WithBorderBottomWidth(float width) { borderBottomWidth = width; _hasExplicitBorderWidth = true; return this; }
        public GraphicalUserInterfaceBuilder WithBorderLeftWidth(float width) { borderLeftWidth = width; _hasExplicitBorderWidth = true; return this; }

        public GraphicalUserInterfaceBuilder WithBorderColor(Color color) { borderColorAll = color; _hasExplicitBorderColor = true; return this; }
        public GraphicalUserInterfaceBuilder WithBorderTopColor(Color color) { borderTopColor = color; _hasExplicitBorderColor = true; return this; }
        public GraphicalUserInterfaceBuilder WithBorderRightColor(Color color) { borderRightColor = color; _hasExplicitBorderColor = true; return this; }
        public GraphicalUserInterfaceBuilder WithBorderBottomColor(Color color) { borderBottomColor = color; _hasExplicitBorderColor = true; return this; }
        public GraphicalUserInterfaceBuilder WithBorderLeftColor(Color color) { borderLeftColor = color; _hasExplicitBorderColor = true; return this; }

        public GraphicalUserInterfaceBuilder WithBackgroundColor(Color color) { backgroundColor = color; _hasExplicitBg = true; return this; }

        public GraphicalUserInterfaceBuilder WithBorderRadius(float radius) { borderRadiusAll = Mathf.Max(0f, radius); _hasExplicitRadius = true; return this; }
        public GraphicalUserInterfaceBuilder WithBorderTopLeftRadius(float radius) { borderTopLeftRadius = Mathf.Max(0f, radius); _hasExplicitRadius = true; return this; }
        public GraphicalUserInterfaceBuilder WithBorderBottomLeftRadius(float radius) { borderBottomLeftRadius = Mathf.Max(0f, radius); _hasExplicitRadius = true; return this; }
        public GraphicalUserInterfaceBuilder WithBorderTopRightRadius(float radius) { borderTopRightRadius = Mathf.Max(0f, radius); _hasExplicitRadius = true; return this; }
        public GraphicalUserInterfaceBuilder WithBorderBottomRightRadius(float radius) { borderBottomRightRadius = Mathf.Max(0f, radius); _hasExplicitRadius = true; return this; }
        #endregion

        #region --- Fluent API: Layout Modes ---
        public GraphicalUserInterfaceBuilder WithFlexLayout(FlexDirection direction, Justify justify = Justify.FlexStart, Align align = Align.Auto)
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
        #endregion

        #region --- Fluent API: Children & Hierarchy ---
        public GraphicalUserInterfaceBuilder AddChild(VisualElement child) { children.Add(_ => child); return this; }
        public GraphicalUserInterfaceBuilder AddChild(Func<GuiContext, VisualElement> childFactory) { if (childFactory != null) children.Add(childFactory); return this; }
        public GraphicalUserInterfaceBuilder AddChild(IGuiProvider provider) { if (provider != null) children.Add(ctx => provider.CreateGui(ctx)); return this; }

        public GraphicalUserInterfaceBuilder OnBuild(Action<VisualElement> onBuildAction)
        { if (onBuildAction != null) onBuildActions.Add(onBuildAction); return this; }

        public GraphicalUserInterfaceBuilder WithPanel(string panelName, bool editorMode = false)
        {
            var panelBuilder = new GraphicalUserInterfaceBuilder(panelName, this);
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
        #endregion

        #region --- Data Binding & Controls ---
        public GraphicalUserInterfaceBuilder AddIntegerData(string label, int value, Action<int> onValueChange)
        {
            return AddChild(ctx => {
                var field = new IntegerField(label) { value = value };
                field.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));
                field.style.marginBottom = 4;
                return field;
            });
        }

        public GraphicalUserInterfaceBuilder AddFloatData(string label, float value, Action<float> onValueChange)
        {
            return AddChild(ctx => {
                var field = new FloatField(label) { value = value };
                field.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));
                field.style.marginBottom = 4;
                return field;
            });
        }

        public GraphicalUserInterfaceBuilder AddStringData(string label, string value, Action<string> onValueChange)
        {
            return AddChild(ctx => {
                var field = new TextField(label) { value = value ?? string.Empty };
                field.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));
                field.style.marginBottom = 4;
                return field;
            });
        }

        public GraphicalUserInterfaceBuilder AddSliderData(string label, float min, float max, float current, Action<float> onValueChange)
        {
            return AddChild(ctx => {
                var slider = new Slider(label, min, max) { value = current };
                slider.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));
                slider.style.marginBottom = 4;
                return slider;
            });
        }

        public GraphicalUserInterfaceBuilder AddIntSliderData(string label, int min, int max, int current, Action<int> onValueChange)
        {
            return AddChild(ctx => {
                var slider = new SliderInt(label, min, max) { value = current };
                slider.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));
                slider.style.marginBottom = 4;
                return slider;
            });
        }

        public GraphicalUserInterfaceBuilder AddEnumData<T>(string label, T selectedValue, Action<T> onValueChange) where T : Enum
        {
            return AddChild(ctx => {
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
                if (index < 0 || index >= names.Count) index = 0; // Fixes empty initialization crashes

                var dropdown = new DropdownField(label, names, index);
                dropdown.RegisterValueChangedCallback(evt => {
                    try { T result = (T)Enum.Parse(typeof(T), evt.newValue); onValueChange?.Invoke(result); }
                    catch { }
                });
                return dropdown;
            });
        }

        public GraphicalUserInterfaceBuilder AddButton(string text, Action onClick)
        {
            return AddChild(ctx => {
                var btn = new Button(onClick) { text = text };
                btn.style.height = 30;
                btn.style.marginBottom = 4;
                return btn;
            });
        }

        public GraphicalUserInterfaceBuilder AddToggleData(string label, bool value, Action<bool> onValueChange)
        {
            return AddChild(ctx => {
                var toggle = new Toggle(label) { value = value };
                toggle.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));
                toggle.style.marginBottom = 4;
                return toggle;
            });
        }

        public GraphicalUserInterfaceBuilder AddDropdownData(string label, List<string> choices, int defaultIndex, Action<string> onValueChange)
        {
            return AddChild(ctx => {
                if (choices == null || choices.Count == 0) choices = new List<string> { "None" };
                int safeIndex = (defaultIndex >= 0 && defaultIndex < choices.Count) ? defaultIndex : 0;

                // Uses safeIndex int instead of a string to avoid "Value not present" error
                var dropdown = new DropdownField(label, choices, safeIndex);
                dropdown.RegisterValueChangedCallback(evt => onValueChange?.Invoke(evt.newValue));
                dropdown.style.marginBottom = 4;
                return dropdown;
            });
        }
        #endregion

        #region --- Decoration & Images ---
        public GraphicalUserInterfaceBuilder AddSeparator(Color color = default, int height = 1)
        {
            if (color == default) color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            return AddChild(ctx => new VisualElement
            {
                style = { height = height, backgroundColor = color, marginTop = 5, marginBottom = 5, width = Length.Percent(100) }
            });
        }

        public GraphicalUserInterfaceBuilder AddImage(Texture2D texture, int width = 0, int height = 0)
        {
            return AddChild(ctx => {
                var img = new Image { image = texture };
                if (width > 0) img.style.width = width;
                if (height > 0) img.style.height = height;
                img.scaleMode = ScaleMode.ScaleToFit;
                return img;
            });
        }

        public GraphicalUserInterfaceBuilder AddImage(string resourcesPath, float width, float height, Action onClick = null)
        {
            var imgBuilder = new ImageGuiBuilder(resourcesPath).WithFixedSize(width, height).OnClick(onClick);
            return AddChild(imgBuilder);
        }

        public GraphicalUserInterfaceBuilder AddImage(ImageGuiBuilder preConfiguredBuilder) => AddChild(preConfiguredBuilder);

        public GraphicalUserInterfaceBuilder WithBorderAllColor(Color color)
        {
            WithBorderBottomColor(color); WithBorderTopColor(color); WithBorderRightColor(color); WithBorderLeftColor(color);
            return this;
        }

        public GraphicalUserInterfaceBuilder AddHeader(string text, Color color = default)
        {
            return AddChild(ctx => {
                var header = new Label(text);
                header.style.unityFontStyleAndWeight = FontStyle.Bold;
                header.style.fontSize = 16;
                header.style.marginTop = 10; header.style.marginBottom = 10;
                header.style.unityTextAlign = TextAnchor.MiddleCenter;

                // Inherit text color from effective theme if color isn't strictly set
                GuiTheme effTheme = GetEffectiveTheme();
                if (color != default) header.style.color = color;
                else if (effTheme != null) header.style.color = effTheme.GlobalDefault.TextColor;

                return header;
            });
        }

        public GraphicalUserInterfaceBuilder WithFooter(string text)
        { footerText = text ?? string.Empty; showFooter = !string.IsNullOrWhiteSpace(footerText); return this; }

        public GraphicalUserInterfaceBuilder WithStyleSheet(StyleSheet ss) { styleSheet = ss; return this; }
        public GraphicalUserInterfaceBuilder WithUxmlTemplate(VisualTreeAsset uxml) { uxmlTemplate = uxml; return this; }
        public GraphicalUserInterfaceBuilder WithEditorMode(bool isEditor = false) { isEditorMode = isEditor; return this; }
        #endregion

        #region --- Build Engine & Lifecycle ---

        // Cascading Theme Resolver
        public GuiTheme GetEffectiveTheme()
        {
            if (_theme != null) return _theme;
            if (parent != null && parent != this) return parent.GetEffectiveTheme();
            return GuiSkin.Active;
        }

        public VisualElement Build() => CreateGui(new GuiContext());

        public VisualElement CreateGui(GuiContext context)
        {
            _activeCtx = context;
            VisualElement root;

            Debug.Log($"[GUI_Builder] CreateGui triggered for '{name}'. Context: {context?.Name}");

            // --- FSM Lifecycle Initialization ---
            _fsmInstanceId = "Lifecycle_" + Guid.NewGuid().ToString().Substring(0, 8);
            SetupLifecycleFSM(context);

            if (uxmlTemplate != null)
            {
                root = uxmlTemplate.Instantiate();
                root.name = name;
                Debug.Log($"[GUI_Builder] '{name}' instantiated via UXML.");
            }
            else
            {
                root = new VisualElement { name = name, tooltip = tooltip };
                ApplyStyles(root);
                Debug.Log($"[GUI_Builder] '{name}' instantiated via C#. Initial FlexGrow: {root.style.flexGrow.value}, Position: {root.style.position.value}");
            }

            if (!string.IsNullOrWhiteSpace(className)) root.AddToClassList(className);

            // Assets & Stylesheets
            if (context?.StyleSheet != null) root.styleSheets.Add(context.StyleSheet);
            if (context?.AdditionalStyleSheets != null)
                foreach (var ss in context.AdditionalStyleSheets) if (ss != null) root.styleSheets.Add(ss);
            if (styleSheet != null) root.styleSheets.Add(styleSheet);

            // Header
            if (showHeader)
            {
                var header = new Label(Title) { name = "gui-header" };
                header.style.unityFontStyleAndWeight = headerStyle;
                header.style.fontSize = headerSize;
                header.style.marginTop = headerMarginTop; header.style.marginRight = headerMarginRight;
                header.style.marginBottom = headerMarginBottom; header.style.marginLeft = headerMarginLeft;

                var effTheme = GetEffectiveTheme();
                if (effTheme != null) header.style.color = effTheme.GlobalDefault.TextColor;

                root.Add(header);
            }

            VisualElement content = root;
            if (scrollable)
            {
                var scroll = new ScrollView(scrollMode) { name = "gui-scrollview" };
                scroll.style.flexGrow = 1f; scroll.style.flexShrink = 1f;
                root.Add(scroll);
                content = scroll;
                Debug.Log($"[GUI_Builder] '{name}' injected ScrollView. ScrollView FlexGrow: 1");
            }

            // Populate Children
            int childCount = 0;
            foreach (var factory in children)
            {
                try
                {
                    var child = factory?.Invoke(context);
                    if (child != null)
                    {
                        content.Add(child);
                        childCount++;
                    }
                }
                catch (Exception ex) { context?.Log?.Invoke($"[GUI] Child build failed: {ex.Message}"); }
            }
            Debug.Log($"[GUI_Builder] '{name}' built and attached {childCount} children.");

            // Footer
            if (showFooter)
            {
                var footer = new Label(footerText) { name = "gui-footer" };
                footer.style.marginTop = footerMarginTop; footer.style.opacity = footerOpacity;

                var effTheme = GetEffectiveTheme();
                if (effTheme != null) footer.style.color = effTheme.GlobalDefault.TextColor;

                root.Add(footer);
            }

            // Final Handshake
            foreach (var act in onBuildActions)
            {
                try { act?.Invoke(root); }
                catch (Exception ex) { context?.Log?.Invoke($"[GUI] OnBuild failed: {ex.Message}"); }
            }

            // --- Lifecycle Callbacks ---
            root.RegisterCallback<AttachToPanelEvent>(evt => {
                Debug.Log($"[GUI_Builder] '{name}' attached to panel: {root.panel?.GetType().Name ?? "UNKNOWN"}");
                VisibilityStatus?.TransitionTo("Active");
            });

            root.RegisterCallback<DetachFromPanelEvent>(evt => {
                VisibilityStatus?.TransitionTo("Teardown");
                VisibilityStatus?.EvaluateConditions();
            });

            // --- THE GEOMETRY TRACKER (THE LAYOUT TRUTH) ---
            // This fires when Unity's engine finally calculates the physical bounding box.
            root.RegisterCallback<GeometryChangedEvent>(evt => {
                var newRect = evt.newRect;
                var oldRect = evt.oldRect;

                // Only log meaningful changes to avoid console spam
                if (newRect != oldRect && (newRect.width > 0 || newRect.height > 0))
                {
                    Debug.Log($"[LAYOUT_RESOLVED] '{name}' | Pos: (X: {newRect.x}, Y: {newRect.y}) | Size: {newRect.width} x {newRect.height}");
                }
            });

            context?.OnBuilt?.Invoke(root);
            return root;
        }

        private void ApplyStyles(VisualElement root)
        {
            root.style.flexDirection = flexDirection;
            root.style.justifyContent = justifyContent;
            root.style.alignItems = alignItems;
            root.style.flexWrap = flexWrap;

            if (percentWidth > 0f) root.style.width = new Length(percentWidth, LengthUnit.Percent);
            else if (width > 0) root.style.width = width;

            if (percentHeight > 0f) root.style.height = new Length(percentHeight, LengthUnit.Percent);
            else if (height > 0) root.style.height = height;

            if (autoGrow) root.style.flexGrow = 1f;
            else if (flexGrow >= 0f) root.style.flexGrow = flexGrow;

            if (noShrink) root.style.flexShrink = 0f;
            else if (flexShrink >= 0f) root.style.flexShrink = flexShrink;
            else root.style.flexShrink = 1f;

            if (minHeight.keyword != StyleKeyword.Null || minHeight.value.value > 0) root.style.minHeight = minHeight;
            if (minWidth.keyword != StyleKeyword.Null || minWidth.value.value > 0) root.style.minWidth = minWidth;
            if (maxHeight.keyword != StyleKeyword.Null || maxHeight.value.value > 0) root.style.maxHeight = maxHeight;
            if (maxWidth.keyword != StyleKeyword.Null || maxWidth.value.value > 0) root.style.maxWidth = maxWidth;

            // Spacing
            if (paddingAll > 0) { root.style.paddingTop = paddingTop; root.style.paddingRight = paddingRight; root.style.paddingBottom = paddingBottom; root.style.paddingLeft = paddingLeft; }
            if (paddingTop > 0) root.style.paddingTop = paddingTop;
            if (paddingRight > 0) root.style.paddingRight = paddingRight;
            if (paddingBottom > 0) root.style.paddingBottom = paddingBottom;
            if (paddingLeft > 0) root.style.paddingLeft = paddingLeft;

            if (marginAll > 0) { root.style.marginTop = marginAll; root.style.marginRight = marginAll; root.style.marginBottom = marginAll; root.style.marginLeft = marginAll; }
            if (marginTop > 0) root.style.marginTop = marginTop;
            if (marginBottom > 0) root.style.marginBottom = marginBottom;

            // --- Theme Application & Overrides ---
            GuiTheme effectiveTheme = GetEffectiveTheme();

            if (_hasExplicitBorderWidth)
            {
                if (borderWidthAll > 0) { root.style.borderTopWidth = borderWidthAll; root.style.borderRightWidth = borderWidthAll; root.style.borderBottomWidth = borderWidthAll; root.style.borderLeftWidth = borderWidthAll; }
                if (borderTopWidth > 0) root.style.borderTopWidth = borderTopWidth;
                if (borderRightWidth > 0) root.style.borderRightWidth = borderRightWidth;
                if (borderBottomWidth > 0) root.style.borderBottomWidth = borderBottomWidth;
                if (borderLeftWidth > 0) root.style.borderLeftWidth = borderLeftWidth;
            }
            else if (effectiveTheme != null)
            {
                root.style.borderTopWidth = effectiveTheme.GlobalDefault.BorderTopWidth;
                root.style.borderRightWidth = effectiveTheme.GlobalDefault.BorderRightWidth;
                root.style.borderBottomWidth = effectiveTheme.GlobalDefault.BorderBottomWidth;
                root.style.borderLeftWidth = effectiveTheme.GlobalDefault.BorderLeftWidth;
            }

            if (_hasExplicitBorderColor)
            {
                if (borderColorAll != default) { root.style.borderTopColor = borderColorAll; root.style.borderRightColor = borderColorAll; root.style.borderBottomColor = borderColorAll; root.style.borderLeftColor = borderColorAll; }
                if (borderTopColor != default) root.style.borderTopColor = borderTopColor;
                if (borderRightColor != default) root.style.borderRightColor = borderRightColor;
                if (borderBottomColor != default) root.style.borderBottomColor = borderBottomColor;
                if (borderLeftColor != default) root.style.borderLeftColor = borderLeftColor;
            }
            else if (effectiveTheme != null)
            {
                root.style.borderTopColor = effectiveTheme.GlobalDefault.BorderColor;
                root.style.borderRightColor = effectiveTheme.GlobalDefault.BorderColor;
                root.style.borderBottomColor = effectiveTheme.GlobalDefault.BorderColor;
                root.style.borderLeftColor = effectiveTheme.GlobalDefault.BorderColor;
            }

            if (_hasExplicitBg && backgroundColor != default)
            {
                root.style.backgroundColor = backgroundColor;
            }
            else if (effectiveTheme != null)
            {
                root.style.backgroundColor = effectiveTheme.GlobalDefault.BackgroundColor;
            }

            if (_hasExplicitRadius)
            {
                if (borderRadiusAll > 0f) { root.style.borderTopLeftRadius = borderRadiusAll; root.style.borderTopRightRadius = borderRadiusAll; root.style.borderBottomLeftRadius = borderRadiusAll; root.style.borderBottomRightRadius = borderRadiusAll; }
                if (borderTopLeftRadius > 0f) root.style.borderTopLeftRadius = borderTopLeftRadius;
                if (borderBottomLeftRadius > 0f) root.style.borderBottomLeftRadius = borderBottomLeftRadius;
                if (borderTopRightRadius > 0f) root.style.borderTopRightRadius = borderTopRightRadius;
                if (borderBottomRightRadius > 0f) root.style.borderBottomRightRadius = borderBottomRightRadius;
            }
            else if (effectiveTheme != null)
            {
                root.style.borderTopLeftRadius = effectiveTheme.GlobalDefault.BorderTopLeftRadius;
                root.style.borderTopRightRadius = effectiveTheme.GlobalDefault.BorderTopRightRadius;
                root.style.borderBottomLeftRadius = effectiveTheme.GlobalDefault.BorderBottomLeftRadius;
                root.style.borderBottomRightRadius = effectiveTheme.GlobalDefault.BorderBottomRightRadius;
            }
        }

        private void SetupLifecycleFSM(GuiContext ctx)
        {
            string templateName = "GUI_Lifecycle_Template";

            try
            {
                if (!FSM_API.Interaction.Exists(templateName, processingGroup))
                {
                    FSM_API.Create.CreateFiniteStateMachine(templateName, -1, processingGroup)
                        .State("Active", null, null, null)
                        .State("Teardown", OnEnterTeardown, null, null)
                        .WithInitialState("Active")
                        .Transition("Active", "Teardown", context => false)
                        .BuildDefinition();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GUI Builder] Bypassed FSM template creation: {ex.Message}");
            }

            try
            {
                VisibilityStatus = FSM_API.Create.CreateInstance(templateName, ctx, processingGroup);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GUI Builder] FSM Instance creation failed: {ex.Message}");
            }
        }

        private static void OnEnterTeardown(object context)
        {
            if (context is GuiContext ctx)
            {
                ctx.Log?.Invoke("[Lifecycle FSM] GUI Detached. Cleaning Relational Stage...");
#if UNITY_EDITOR
                EditorStagingManager.CleanupCurrentStage();
#endif
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        #endregion

        #region --- Serialization & UIDocuments ---

        // Moved this OUT of the UNITY_EDITOR block so the runtime FSM can still access it.
        public GraphicalUserInterfaceBuilder WithProcessingGroup(string processingGroup)
        {
            this.processingGroup = processingGroup;
            return this;
        }

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath)
        {
            VisualElement root = Build();
            ConvertToUIDocument(root, assetPath);
        }

        private static string SerializeElement(VisualElement ve, int indent)
        {
            string spacing = new string(' ', indent * 4);
            string typeName = ve.GetType().Name;

            // Map element type to UXML tag (UI Toolkit uses ui: prefix)
            string tag = $"ui:{typeName}";

            string nameAttr = !string.IsNullOrEmpty(ve.name) ? $" name=\"{ve.name}\"" : "";

            // Capture Text values for Labels, Buttons, TextFields
            string textAttr = "";
            if (ve is TextElement textElement && !string.IsNullOrEmpty(textElement.text))
            {
                string safeText = textElement.text.Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;");
                textAttr = $" text=\"{safeText}\"";
            }

            // Capture Inline Styles into CSS format
            List<string> styles = new List<string>();

            // Layout & Flex
            if (ve.style.flexDirection.keyword != StyleKeyword.Null) styles.Add($"flex-direction: {ve.style.flexDirection.value.ToString().ToLower()};");
            if (ve.style.justifyContent.keyword != StyleKeyword.Null) styles.Add($"justify-content: {ve.style.justifyContent.value.ToString().ToLower()};");
            if (ve.style.alignItems.keyword != StyleKeyword.Null) styles.Add($"align-items: {ve.style.alignItems.value.ToString().ToLower()};");
            if (ve.style.flexGrow.keyword != StyleKeyword.Null) styles.Add($"flex-grow: {ve.style.flexGrow.value};");
            if (ve.style.flexShrink.keyword != StyleKeyword.Null) styles.Add($"flex-shrink: {ve.style.flexShrink.value};");

            // Size
            if (ve.style.width.keyword != StyleKeyword.Null) styles.Add($"width: {FormatLength(ve.style.width.value)};");
            if (ve.style.height.keyword != StyleKeyword.Null) styles.Add($"height: {FormatLength(ve.style.height.value)};");

            // Appearance (Colors converted to standard rgba)
            if (ve.style.backgroundColor.keyword != StyleKeyword.Null) styles.Add($"background-color: {FormatColor(ve.style.backgroundColor.value)};");
            if (ve.style.borderTopColor.keyword != StyleKeyword.Null) styles.Add($"border-color: {FormatColor(ve.style.borderTopColor.value)};");
            if (ve.style.borderTopWidth.keyword != StyleKeyword.Null) styles.Add($"border-width: {ve.style.borderTopWidth.value}px;");
            if (ve.style.borderTopLeftRadius.keyword != StyleKeyword.Null) styles.Add($"border-radius: {ve.style.borderTopLeftRadius.value}px;");
            if (ve.style.color.keyword != StyleKeyword.Null) styles.Add($"color: {FormatColor(ve.style.color.value)};");

            // Spacing
            if (ve.style.marginTop.keyword != StyleKeyword.Null) styles.Add($"margin-top: {ve.style.marginTop.value.value}px;");
            if (ve.style.marginBottom.keyword != StyleKeyword.Null) styles.Add($"margin-bottom: {ve.style.marginBottom.value.value}px;");
            if (ve.style.marginRight.keyword != StyleKeyword.Null) styles.Add($"margin-right: {ve.style.marginRight.value.value}px;");
            if (ve.style.marginLeft.keyword != StyleKeyword.Null) styles.Add($"margin-left: {ve.style.marginLeft.value.value}px;");
            if (ve.style.paddingTop.keyword != StyleKeyword.Null) styles.Add($"padding: {ve.style.paddingTop.value.value}px;");

            string styleAttr = styles.Count > 0 ? $" style=\"{string.Join(" ", styles)}\"" : "";

            string xml = $"{spacing}<{tag}{nameAttr}{textAttr}{styleAttr}";

            var children = ve.Children().ToList();
            if (children.Count == 0)
            {
                xml += " />\n"; // Self-closing tag
            }
            else
            {
                xml += ">\n";
                foreach (var child in children) xml += SerializeElement(child, indent + 1);
                xml += $"{spacing}</{tag}>\n";
            }

            return xml;
        }

        private static string FormatLength(Length length) => length.unit == LengthUnit.Percent ? $"{length.value}%" : $"{length.value}px";
        private static string FormatColor(Color c) => $"rgba({Mathf.RoundToInt(c.r * 255)}, {Mathf.RoundToInt(c.g * 255)}, {Mathf.RoundToInt(c.b * 255)}, {c.a:F2})";

        public static void ConvertToUIDocument(VisualElement root, string assetPath)
        {
            string uxml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                          "<ui:UXML xmlns:ui=\"UnityEngine.UIElements\" xmlns:uie=\"UnityEditor.UIElements\">\n" +
                          SerializeElement(root, 1) +
                          "</ui:UXML>";
            File.WriteAllText(assetPath, uxml);
            AssetDatabase.Refresh();
        }
#endif

        public void FromUIDocument(string assetPath)
        {
            var visualTree = Resources.Load<VisualTreeAsset>(assetPath);
            if (visualTree != null) this.WithUxmlTemplate(visualTree);
            else Debug.LogError($"[Singularity] Failed to hydrate GUI from {assetPath}");
        }

        public static VisualElement ConvertFromUIDocument(string assetPath)
        {
            var template = Resources.Load<VisualTreeAsset>(assetPath);
            return template != null ? template.CloneTree() : new VisualElement();
        }
        #endregion

        public GraphicalUserInterfaceBuilder WithJustifyContent(Justify center)
        {
            this.justifyContent = center;
            return this;
        }

        public GraphicalUserInterfaceBuilder WithAlignItems(Align alignment)
        {
            this.alignItems = alignment;
            return this;
        }

        public GraphicalUserInterfaceBuilder WithFlexDirection(FlexDirection column)
        {
            this.flexDirection = column;
            return this;
        }

        public GraphicalUserInterfaceBuilder WithPosition(Position position)
        {
            this.position = position;
            return this;
        }

        public GraphicalUserInterfaceBuilder WithMaxHeight(Length length)
        {
            this.maxHeight = length;
            return this;
        }

        public GraphicalUserInterfaceBuilder WithMaxHeight(float max)
        {
            this.maxHeight = max;
            return this;
        }

        public GraphicalUserInterfaceBuilder WithMaxWidth(Length length)
        {
            this.maxHeight = length;
            return this;
        }

        public GraphicalUserInterfaceBuilder WithMaxWidth(float max)
        {
            this.maxHeight = max;
            return this;
        }

        public GraphicalUserInterfaceBuilder WithWidth(StyleLength width)
        {
            return OnBuild(ve => ve.style.width = width);
        }

        public GraphicalUserInterfaceBuilder WithHeight(StyleLength height)
        {
            return OnBuild(ve => ve.style.height = height);
        }

        // While you are in there, you should grab the Min/Max bounds as well!
        public GraphicalUserInterfaceBuilder WithMinWidth(StyleLength minWidth)
        {
            return OnBuild(ve => ve.style.minWidth = minWidth);
        }

        public GraphicalUserInterfaceBuilder WithMaxWidth(StyleLength maxWidth)
        {
            return OnBuild(ve => ve.style.maxWidth = maxWidth);
        }

        public GraphicalUserInterfaceBuilder WithMinHeight(StyleLength minHeight)
        {
            return OnBuild(ve => ve.style.minHeight = minHeight);
        }

        public GraphicalUserInterfaceBuilder WithMaxHeight(StyleLength maxHeight)
        {
            return OnBuild(ve => ve.style.maxHeight = maxHeight);
        }

        public GraphicalUserInterfaceBuilder WithContextMenu(IForgeBuilder contextMenuBuilder)
        {
            return OnBuild(root => {
                // Safely cast the object built by the IForgeBuilder
                var manipulator = contextMenuBuilder.Build() as ContextualMenuManipulator;

                if (manipulator != null)
                {
                    root.AddManipulator(manipulator);
                }
            });
        }

        public GraphicalUserInterfaceBuilder AddSeparator(Color color, float width = 1f)
        {
            // In UIToolkit, a separator is simply a 1px high/wide box with a background color.
            return OnBuild(ve => {
                var sep = new VisualElement { name = "Separator" };
                sep.style.backgroundColor = color;

                // If the parent is a Column, it's a horizontal line. If a Row, it's a vertical line.
                if (ve.style.flexDirection == FlexDirection.Row)
                {
                    sep.style.width = width;
                    sep.style.height = Length.Percent(100);
                    sep.style.marginLeft = sep.style.marginRight = 5;
                }
                else
                {
                    sep.style.height = width;
                    sep.style.width = Length.Percent(100);
                    sep.style.marginTop = sep.style.marginBottom = 5;
                }

                ve.Add(sep);
            });
        }

        /// <summary>
        /// Sets the element to absolute positioning and defines its offsets.
        /// </summary>
        public GraphicalUserInterfaceBuilder WithAbsolutePosition(float? top = null, float? right = null, float? bottom = null, float? left = null)
        {
            return OnBuild(ve =>
            {
                ve.style.position = Position.Absolute;
                if (top.HasValue) ve.style.top = top.Value;
                if (right.HasValue) ve.style.right = right.Value;
                if (bottom.HasValue) ve.style.bottom = bottom.Value;
                if (left.HasValue) ve.style.left = left.Value;
            });
        }
    }
}