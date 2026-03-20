using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Workshop.Forge.Builders.GuiBuilders.GuiTesting
{
    public class Workshop_Gui_Test_GraphicalUserInterfaceBuilder : IGuiProvider
    {
        public string Title => "Forge Layout Architect";
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

        public Workshop_Gui_Test_GraphicalUserInterfaceBuilder()
        {
            LoadAvailableGuiProviders();
        }

        private void LoadAvailableGuiProviders()
        {
            _guiProviderNames.Add("[ None - Colored Box ]");

            // Find all classes that implement IGuiProvider and have an empty constructor
            var type = typeof(IGuiProvider);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

            foreach (var t in types)
            {
                if (t.GetConstructor(Type.EmptyTypes) != null) // Ensure we can instantiate it
                {
                    _guiProviderTypes[t.Name] = t;
                }
            }

            // Sort alphabetically for the dropdown
            var sortedNames = _guiProviderTypes.Keys.ToList();
            sortedNames.Sort();
            _guiProviderNames.AddRange(sortedNames);
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            if (_childrenItems.Count == 0)
            {
                AddNewChild("Sidebar", 200f, 0f, 0f, 1f);
                AddNewChild("Main Content", 0f, 0f, 1f, 1f);
            }

            var root = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };

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
            controls.AddSeparator(Color.gray);
            controls.AddChild(new Label("Parent Flexbox Layout:") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
            controls.AddEnumData("Direction", _direction, (v) => { _direction = v; RefreshPreview(); });
            controls.AddEnumData("Justify", _justify, (v) => { _justify = v; RefreshPreview(); });
            controls.AddEnumData("Align", _align, (v) => { _align = v; RefreshPreview(); });
            controls.AddEnumData("Wrap", _wrap, (v) => { _wrap = v; RefreshPreview(); });

            // --- DYNAMIC CONTENT CRUD SECTION ---
            controls.AddSeparator(Color.gray);
            controls.AddChild(new Label("Child Elements (Live GUIs):") { style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.cyan } });
            controls.AddButton("+ Add New Child Area", () =>
            {
                AddNewChild($"Panel {_childrenItems.Count + 1}", 150f, 150f, 0f, 1f);
                RefreshChildrenList();
                RefreshPreview();
            });

            // Make the children list itself a fixed-height scrollview so it doesn't break the sidebar
            _childrenListContainer = new ScrollView
            {
                style = {
                    marginTop = 10, marginBottom = 10,
                    maxHeight = 350, // Prevents pushing the Color Forge off-screen
                    borderTopWidth = 1, borderBottomWidth = 1, borderTopColor = new Color(0.3f, 0.3f, 0.3f),borderBottomColor = new Color(0.3f, 0.3f, 0.3f),
                    borderLeftColor = new Color(0.3f, 0.3f, 0.3f),borderRightColor = new Color(0.3f, 0.3f, 0.3f),
                }
            };
            controls.AddChild(_childrenListContainer);
            RefreshChildrenList();

            // --- EMBEDDED COLOR FORGE ---
            controls.AddSeparator(Color.gray);
            controls.AddChild(new Label("Bulk Color Applicator:") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
            controls.AddSliderData("Corner Radius", 0, 50, _borderRadius, (v) => { _borderRadius = v; RefreshPreview(); });
            controls.AddIntSliderData("Border Width", 0, 10, _borderWidth, (v) => { _borderWidth = v; RefreshPreview(); });

            controls.AddEnumData("Target Property", _activeColorTarget, (v) => _activeColorTarget = v);

            var colorForge = new ColorForgePicker(Color.cyan, (pickedColor) =>
            {
                if (_activeColorTarget == ColorTarget.BackgroundColor || _activeColorTarget == ColorTarget.Both) _backgroundColor = pickedColor;
                if (_activeColorTarget == ColorTarget.BorderColor || _activeColorTarget == ColorTarget.Both) _borderColor = pickedColor;
                RefreshPreview();
            });

            controls.AddChild(builderCtx => {
                var colorContainer = new VisualElement { style = { height = 250, marginTop = 10, borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1, borderTopColor = Color.gray, borderBottomColor = Color.gray, borderLeftColor = Color.gray, borderRightColor = Color.gray, } };
                colorContainer.Add(colorForge.CreateGui(builderCtx));
                return colorContainer;
            });

            root.Add(controls.Build());

            // 2. THE PREVIEW AREA
            _previewContainer = new VisualElement();
            _previewContainer.style.flexGrow = 1;
            _previewContainer.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f);
            _previewContainer.style.justifyContent = Justify.Center;
            _previewContainer.style.alignItems = Align.Center;

            root.Add(_previewContainer);

            RefreshPreview();
            return root;
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
                        // Instantiate the selected provider dynamically!
                        child.LiveGuiInstance = (IGuiProvider)Activator.CreateInstance(t);
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

                itemBox.Add(foldout);
                _childrenListContainer.Add(itemBox);
            }
        }

        private void RefreshPreview()
        {
            if (_previewContainer == null) return;
            _previewContainer.Clear();

            var previewTitle = new Label("LIVE GUI PREVIEW") { style = { position = Position.Absolute, top = 10, left = 10, color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold } };
            _previewContainer.Add(previewTitle);

            // Build the Parent Layout Container
            var previewBuilder = new GraphicalUserInterfaceBuilder("LivePreview")
                .WithBackgroundColor(_backgroundColor)
                // Fallbacks assuming you don't have WithBorderColor(Color) yet
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
                    childWrapper.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f); // Dark background for nested GUIs
                    childWrapper.style.borderTopWidth = 2; // Keep the colored border so you can identify it!
                    childWrapper.style.borderTopColor = child.Color;

                    try
                    {
                        var liveGui = child.LiveGuiInstance.CreateGui(new GuiContext());
                        liveGui.style.flexGrow = 1; // Force the nested GUI to fill the Flex wrapper we created
                        childWrapper.Add(liveGui);
                    }
                    catch (Exception ex)
                    {
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

        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    }
}