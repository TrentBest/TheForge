using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.GuiTesting
{
    public class Workshop_Gui_BreathingGuiTest : IGuiProvider
    {
        public string Title => "Breathing Tests";

        public int SeparatorWidth { get; private set; } = 1;

        private VisualElement _previewContainer;
        private ScrollView _childrenListContainer;

        // --- IGuiProvider Reflection Cache ---
        private Dictionary<string, Type> _guiProviderTypes = new Dictionary<string, Type>();
        private List<string> _guiProviderNames = new List<string>();

        // --- Data Model for CRUD ---
        private class PreviewChildItem
        {
            public string Name = "New Item";
            public float Width = 50f;
            public float Height = 50f;
            public float FlexGrow = 0f;
            public float FlexShrink = 1f;
            public Color Color = Color.gray;

            // Live GUI Integration
            public string SelectedProviderName = "[ None - Colored Box ]";
            public IGuiProvider LiveGuiInstance = null;
            public bool EnableBreathing = false;
        }

        private List<PreviewChildItem> _childrenItems = new List<PreviewChildItem>();

        // --- Layout State ---
        private bool _usePercentage = false;
        private int _widthPx = 600;
        private int _heightPx = 400;
        private float _widthPct = 100f;
        private float _heightPct = 100f;
        private float _borderRadius = 8f;
        private int _borderWidth = 2;

        // --- Flex State ---
        private float _flexGrow = 0f;
        private float _flexShrink = 1f;
        private FlexDirection _direction = FlexDirection.Row;
        private Justify _justify = Justify.FlexStart;
        private Align _align = Align.Stretch;
        private Wrap _wrap = Wrap.Wrap;

        // --- Color Targeting State ---
        public enum ColorTarget { BackgroundColor, BorderColor, Both }
        private ColorTarget _activeColorTarget = ColorTarget.BackgroundColor;
        private Color _backgroundColor = new Color(0.12f, 0.12f, 0.14f);
        private Color _borderColor = Color.cyan;

        public Workshop_Gui_BreathingGuiTest()
        {
            Debug.Log("[LayoutArchitect] Constructor invoked. Initializing...");
            LoadAvailableGuiProviders();
        }

        private void LoadAvailableGuiProviders()
        {
            Debug.Log("[LayoutArchitect] Starting Assembly Scan for IGuiProviders...");
            _guiProviderNames.Add("[ None - Colored Box ]");

            try
            {
                var type = typeof(IGuiProvider);
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes()
                            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

                        foreach (var t in types)
                        {
                            if (t.GetConstructor(Type.EmptyTypes) != null)
                            {
                                _guiProviderTypes[t.Name] = t;
                            }
                        }
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // Highly common in Unity, safely ignore unreadable DLLs
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[LayoutArchitect] Minor error scanning assembly {assembly.FullName}: {ex.Message}");
                    }
                }

                var sortedNames = _guiProviderTypes.Keys.ToList();
                sortedNames.Sort();
                _guiProviderNames.AddRange(sortedNames);

                Debug.Log($"[LayoutArchitect] Scan Complete. Found {_guiProviderNames.Count - 1} valid IGuiProviders.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LayoutArchitect] FATAL in LoadAvailableGuiProviders: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            Debug.Log("[LayoutArchitect] CreateGui() started.");

            try
            {
                if (_childrenItems.Count == 0)
                {
                    AddNewChild("Sidebar", 200f, 0f, 0f, 1f);
                    AddNewChild("Main Content", 0f, 0f, 1f, 1f);
                }

                var root = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };

                Debug.Log("[LayoutArchitect] Building Architect Controls...");

                // 1. THE CONTROL PANEL
                var controls = new GraphicalUserInterfaceBuilder("Controls")
                    .WithWidth(380)
                    .WithPadding(10)
                    .WithBackgroundColor(new Color(0.10f, 0.10f, 0.12f))
                    .WithBorderRightWidth(1)
                    .WithBorderRightColor(new Color(0.3f, 0.3f, 0.3f))
                    .WithScrollable(true)
                    .WithTitle("Architect Controls");

                // --- SIZING SECTION ---
                controls.AddChild(new Label("Parent Container Sizing:") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 } });
                controls.AddToggleData("Use Percentage Units", _usePercentage, (val) => { _usePercentage = val; RefreshPreview(); });

                if (!_usePercentage)
                {
                    controls.AddIntSliderData("Width (px)", 50, 1920, _widthPx, (v) => { _widthPx = v; RefreshPreview(); });
                    controls.AddIntSliderData("Height (px)", 50, 1080, _heightPx, (v) => { _heightPx = v; RefreshPreview(); });
                }
                else
                {
                    controls.AddSliderData("Width (%)", 0, 100, _widthPct, (v) => { _widthPct = v; RefreshPreview(); });
                    controls.AddSliderData("Height (%)", 0, 100, _heightPct, (v) => { _heightPct = v; RefreshPreview(); });
                }

                // --- FLEXBOX SECTION ---
                controls.AddSeparator(Color.gray, SeparatorWidth);
                controls.AddChild(new Label("Parent Flexbox Layout:") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
                controls.AddEnumData("Direction", _direction, (v) => { _direction = v; RefreshPreview(); });
                controls.AddEnumData("Justify", _justify, (v) => { _justify = v; RefreshPreview(); });
                controls.AddEnumData("Align", _align, (v) => { _align = v; RefreshPreview(); });
                controls.AddEnumData("Wrap", _wrap, (v) => { _wrap = v; RefreshPreview(); });

                // --- DYNAMIC CONTENT CRUD SECTION ---
                controls.AddSeparator(Color.gray, SeparatorWidth);
                controls.AddChild(new Label("Child Elements (Live GUIs):") { style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.cyan } });
                controls.AddButton("+ Add New Child Area", () =>
                {
                    AddNewChild($"Panel {_childrenItems.Count + 1}", 150f, 150f, 0f, 1f);
                    RefreshChildrenList();
                    RefreshPreview();
                });

                _childrenListContainer = new ScrollView
                {
                    style = {
                        marginTop = 10, marginBottom = 10,
                        maxHeight = 350,
                        borderTopWidth = 1, borderBottomWidth = 1, borderTopColor = new Color(0.3f, 0.3f, 0.3f),borderBottomColor = new Color(0.3f, 0.3f, 0.3f),
                        borderLeftColor = new Color(0.3f, 0.3f, 0.3f),borderRightColor = new Color(0.3f, 0.3f, 0.3f),
                    }
                };
                controls.AddChild(_childrenListContainer);
                RefreshChildrenList();

                // --- EMBEDDED COLOR FORGE ---
                Debug.Log("[LayoutArchitect] Injecting ColorForgePicker...");
                controls.AddSeparator(Color.gray, SeparatorWidth);
                controls.AddChild(new Label("Bulk Color Applicator:") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
                controls.AddSliderData("Corner Radius", 0, 50, _borderRadius, (v) => { _borderRadius = v; RefreshPreview(); });
                controls.AddIntSliderData("Border Width", 0, 10, _borderWidth, (v) => { _borderWidth = v; RefreshPreview(); });
                controls.AddEnumData("Target Property", _activeColorTarget, (v) => _activeColorTarget = v);

                try
                {
                    var colorForge = new ColorForgePicker(Color.cyan, (pickedColor) =>
                    {
                        if (_activeColorTarget == ColorTarget.BackgroundColor || _activeColorTarget == ColorTarget.Both) _backgroundColor = pickedColor;
                        if (_activeColorTarget == ColorTarget.BorderColor || _activeColorTarget == ColorTarget.Both) _borderColor = pickedColor;
                        RefreshPreview();
                    });

                    controls.AddChild(builderCtx =>
                    {
                        var colorContainer = new VisualElement { style = { height = 250, marginTop = 10, borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1, borderTopColor = Color.gray, borderBottomColor = Color.gray, borderLeftColor = Color.gray, borderRightColor = Color.gray, } };
                        colorContainer.Add(colorForge.CreateGui(builderCtx));
                        return colorContainer;
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[LayoutArchitect] ColorForgePicker failed to load: {ex.Message}");
                    controls.AddChild(new Label("ColorForgePicker Unavailable") { style = { color = Color.red } });
                }

                Debug.Log("[LayoutArchitect] Compiling Controls Panel to VisualElement...");
                root.Add(controls.Build());

                // 2. THE PREVIEW AREA
                Debug.Log("[LayoutArchitect] Building Preview Container...");
                _previewContainer = new VisualElement();
                _previewContainer.style.flexGrow = 1;
                _previewContainer.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f);
                _previewContainer.style.justifyContent = Justify.Center;
                _previewContainer.style.alignItems = Align.Center;

                root.Add(_previewContainer);

                Debug.Log("[LayoutArchitect] Triggering First RefreshPreview()...");
                RefreshPreview();

                Debug.Log("[LayoutArchitect] CreateGui() completed successfully.");
                return root;
            }
            catch (Exception ex)
            {
                // CRITICAL FAIL-SAFE: Render the error to the GUI instead of failing silently.
                Debug.LogError($"[LayoutArchitect] CRITICAL FAILURE IN CreateGui: {ex.Message}\n{ex.StackTrace}");

                var errorRoot = new VisualElement { style = { flexGrow = 1, backgroundColor = new Color(0.2f, 0, 0), paddingTop = 20, paddingRight = 20, paddingLeft = 20, paddingBottom = 20 } };
                errorRoot.Add(new Label("Layout Architect Encountered a Fatal Error") { style = { color = Color.white, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
                errorRoot.Add(new TextField { value = $"{ex.Message}\n\n{ex.StackTrace}", multiline = true, isReadOnly = true, style = { flexGrow = 1 } });

                return errorRoot;
            }
        }

        private void AddNewChild(string name, float w, float h, float fGrow, float fShrink)
        {
            float hue = UnityEngine.Random.value;
            Color randomColor = Color.HSVToRGB(hue, 0.7f, 0.9f);

            _childrenItems.Add(new PreviewChildItem
            {
                Name = name,
                Width = w,
                Height = h,
                FlexGrow = fGrow,
                FlexShrink = fShrink,
                Color = randomColor
            });
        }

        private void RefreshChildrenList()
        {
            if (_childrenListContainer == null) return;
            _childrenListContainer.Clear();

            foreach (var child in _childrenItems)
            {
                var itemBox = new VisualElement
                {
                    style = {
                        backgroundColor = new Color(0.15f, 0.15f, 0.18f),
                        marginBottom = 6, marginRight = 8,
                        paddingTop = 5, paddingBottom = 5, paddingLeft = 5, paddingRight = 5,
                        borderLeftWidth = 4, borderLeftColor = child.Color,
                        borderTopRightRadius = 3, borderBottomRightRadius = 3
                    }
                };

                // Header Row
                var headerRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };
                var nameField = new TextField { value = child.Name, style = { flexGrow = 1, marginRight = 5 } };
                nameField.RegisterValueChangedCallback(e => { child.Name = e.newValue; RefreshPreview(); });
                headerRow.Add(nameField);

                var delBtn = new Button(() => { _childrenItems.Remove(child); RefreshChildrenList(); RefreshPreview(); }) { text = "X", style = { color = Color.red, unityFontStyleAndWeight = FontStyle.Bold } };
                headerRow.Add(delBtn);
                itemBox.Add(headerRow);

                var foldout = new Foldout { text = "Layout & Content", value = false };

                // --- THE MAGIC DROPDOWN ---
                var providerDropdown = new DropdownField("Live GUI", _guiProviderNames, child.SelectedProviderName);
                providerDropdown.RegisterValueChangedCallback(e =>
                {
                    child.SelectedProviderName = e.newValue;
                    if (_guiProviderTypes.TryGetValue(e.newValue, out Type t))
                    {
                        try
                        {
                            child.LiveGuiInstance = (IGuiProvider)Activator.CreateInstance(t);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[LayoutArchitect] Failed to instantiate {e.newValue}: {ex.Message}");
                            child.LiveGuiInstance = null;
                        }
                    }
                    else
                    {
                        child.LiveGuiInstance = null; // Revert to colored box
                    }
                    RefreshPreview();
                });
                foldout.Add(providerDropdown);

                // --- Sizing ---
                var widthField = new FloatField("Width (0=Auto)") { value = child.Width };
                widthField.RegisterValueChangedCallback(e => { child.Width = e.newValue; RefreshPreview(); });
                foldout.Add(widthField);

                var heightField = new FloatField("Height (0=Auto)") { value = child.Height };
                heightField.RegisterValueChangedCallback(e => { child.Height = e.newValue; RefreshPreview(); });
                foldout.Add(heightField);

                var growField = new FloatField("Flex Grow") { value = child.FlexGrow };
                growField.RegisterValueChangedCallback(e => { child.FlexGrow = e.newValue; RefreshPreview(); });
                foldout.Add(growField);

                var shrinkField = new FloatField("Flex Shrink") { value = child.FlexShrink };
                shrinkField.RegisterValueChangedCallback(e => { child.FlexShrink = e.newValue; RefreshPreview(); });
                foldout.Add(shrinkField);

                var breathingToggle = new Toggle("Enable Breathing") { value = child.EnableBreathing };
                breathingToggle.RegisterValueChangedCallback(e =>
                {
                    child.EnableBreathing = e.newValue;
                    RefreshPreview();
                });
                foldout.Add(breathingToggle);

                itemBox.Add(foldout);
                _childrenListContainer.Add(itemBox);
            }
        }

        private void RefreshPreview()
        {
            if (_previewContainer == null) return;

            try
            {
                _previewContainer.Clear();

                var previewTitle = new Label("LIVE GUI PREVIEW") { style = { position = Position.Absolute, top = 10, left = 10, color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold } };
                _previewContainer.Add(previewTitle);

                // Build the Parent Layout Container
                var previewBuilder = new GraphicalUserInterfaceBuilder("LivePreview")
                    .WithBackgroundColor(_backgroundColor)
                    .WithBorderTopColor(_borderColor).WithBorderBottomColor(_borderColor).WithBorderLeftColor(_borderColor).WithBorderRightColor(_borderColor)
                    .WithBorderWidth(_borderWidth)
                    .WithBorderRadius(_borderRadius)
                    .WithFlexLayout(_direction, _justify, _align)
                    .WithFlexWrap(_wrap)
                    .WithFlexGrow(_flexGrow)
                    .WithFlexShrink(_flexShrink)
                    .WithPadding(15);

                if (_usePercentage) previewBuilder.WithPercentSize(_widthPct, _heightPct);
                else previewBuilder.WithSize(_widthPx, _heightPx);

                // Build the Children
                foreach (var child in _childrenItems)
                {
                    var childWrapper = new VisualElement
                    {
                        style = {
                            width = child.Width > 0 ? child.Width : new StyleLength(StyleKeyword.Auto),
                            height = child.Height > 0 ? child.Height : new StyleLength(StyleKeyword.Auto),
                            flexGrow = child.FlexGrow,
                            flexShrink = child.FlexShrink,
                            marginBottom = 5, marginRight = 5,
                            overflow = Overflow.Hidden,
                            borderTopLeftRadius = 4, borderTopRightRadius = 4, borderBottomLeftRadius = 4, borderBottomRightRadius = 4
                        }
                    };

                    // Inject the actual GUI Provider OR fallback to the colored label
                    if (child.LiveGuiInstance != null)
                    {
                        childWrapper.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
                        if (!child.EnableBreathing)
                        {
                            childWrapper.style.borderTopWidth = 2;
                            childWrapper.style.borderTopColor = child.Color;
                        }
                        try
                        {
                            var liveGui = child.LiveGuiInstance.CreateGui(new GuiContext());
                            liveGui.style.flexGrow = 1;
                            childWrapper.Add(liveGui);

                            if (child.EnableBreathing)
                            {
                                var breathColors = new List<Color> { child.Color, new Color(0.1f, 0.1f, 0.15f) };

                                var breathEffect = new Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes.Effects.BreathingEffect(
                                    childWrapper,
                                    -1, // FSM Processing Rate (-1 usually means every tick)
                                    3f, // Breathing Rate (Speed)
                                    "GUI_Effects", // Processing Group
                                    breathColors
                                );

                                // CRITICAL: Prevent FSM Memory Leaks when RefreshPreview() calls Clear()
                                childWrapper.RegisterCallback<DetachFromPanelEvent>(evt =>
                                {
                                    breathEffect.IsActive = false;
                                    breathEffect.IsValid = false;

                                    // Assuming your FSM_API supports destroying an instance by its Handle ID.
                                    // If the exact method name differs, replace this with your API's teardown call.
                                    if (breathEffect.Status != null)
                                    {
                                        FSM_API.Interaction.DestroyInstance(breathEffect.Status);
                                    }
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[LayoutArchitect] Nested GUI {child.SelectedProviderName} failed to render: {ex.Message}");
                            childWrapper.Add(new Label($"Error loading {child.SelectedProviderName}:\n{ex.Message}") { style = { color = Color.red, whiteSpace = WhiteSpace.Normal } });
                        }
                    }
                    else
                    {
                        // Fallback to Colored Box
                        childWrapper.style.backgroundColor = child.Color;
                        childWrapper.Add(new Label(child.Name) { style = { color = Color.black, unityTextAlign = TextAnchor.MiddleCenter, unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1 } });
                    }

                    previewBuilder.AddChild(childWrapper);
                }

                _previewContainer.Add(previewBuilder.Build());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LayoutArchitect] RefreshPreview() Failed: {ex.Message}\n{ex.StackTrace}");
                _previewContainer.Add(new Label($"Preview Render Error:\n{ex.Message}") { style = { color = Color.red, backgroundColor = Color.black, paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10 } });
            }
        }

        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    }
}