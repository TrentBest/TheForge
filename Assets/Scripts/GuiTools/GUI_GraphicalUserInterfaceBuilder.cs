using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class GUI_GraphicalUserInterfaceBuilder : IGuiProvider
    {
        public string Title => "Forge Layout Architect (REST Enabled)";

        public int SeparatorWidth { get; private set; } = 1;

        // Captured dynamic containers for runtime mutability
        private VisualElement _previewContainer;
        private ScrollView _childrenListContainer;

        // --- IGuiProvider Reflection Cache ---
        private Dictionary<string, Type> _guiProviderTypes = new Dictionary<string, Type>();
        private List<string> _guiProviderNames = new List<string>();

        // --- REST Integration Cache ---
        private List<string> _availableRestEndpoints = new List<string>();

        // --- Data Model for CRUD ---
        private class PreviewChildItem
        {
            public string Name = "New Item";
            public float Width = 50f;
            public float Height = 50f;
            public float FlexGrow = 0f;
            public float FlexShrink = 1f;
            public Color Color = Color.gray;

            public string SelectedProviderName = "[ None - Colored Box ]";
            public IGuiProvider LiveGuiInstance = null;
            public string OnClickRestEndpoint = "None";
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

        private float _flexGrow = 0f;
        private float _flexShrink = 1f;
        private FlexDirection _direction = FlexDirection.Row;
        private Justify _justify = Justify.FlexStart;
        private Align _align = Align.Stretch;
        private Wrap _wrap = Wrap.Wrap;

        public enum ColorTarget { BackgroundColor, BorderColor, Both }
        private ColorTarget _activeColorTarget = ColorTarget.BackgroundColor;
        private Color _backgroundColor = new Color(0.12f, 0.12f, 0.14f);
        private Color _borderColor = Color.cyan;

        public GUI_GraphicalUserInterfaceBuilder()
        {
            LoadAvailableGuiProviders();
            LoadAvailableRestEndpoints();
        }

        private void LoadAvailableRestEndpoints()
        {
            _availableRestEndpoints = new List<string>
            {
                "None",
                "Autodesk: Get Hubs (GET)",
                "Autodesk: Create Project (POST)",
                "TopoSurvey: Get Nodes (GET)",
                "Forge: Trigger Global Build (POST)"
            };
        }

        private void LoadAvailableGuiProviders()
        {
            _guiProviderNames.Add("[ None - Colored Box ]");
            try
            {
                var type = typeof(IGuiProvider);
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes().Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
                        foreach (var t in types)
                        {
                            if (t.GetConstructor(Type.EmptyTypes) != null) _guiProviderTypes[t.Name] = t;
                        }
                    }
                    catch { }
                }
                var sortedNames = _guiProviderTypes.Keys.ToList();
                sortedNames.Sort();
                _guiProviderNames.AddRange(sortedNames);
            }
            catch (Exception ex)
            {
                ForgeLogger.LogError($"[LayoutArchitect] FATAL: {ex.Message}");
            }
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            try
            {
                if (_childrenItems.Count == 0)
                {
                    AddNewChild("Sidebar", 200f, 0f, 0f, 1f);
                    AddNewChild("Main Content", 0f, 0f, 1f, 1f);
                }

                // 1. THE MASTER ROOT
                var rootBuilder = new GraphicalUserInterfaceBuilder("ArchitectMasterRoot")
                    .WithFlexLayout(FlexDirection.Row)
                    .WithFlexGrow(1);

                // 2. THE CONTROL PANEL (Sidebar)
                var controls = new GraphicalUserInterfaceBuilder("Controls")
                    .WithWidth(400)
                    .WithPadding(10)
                    .WithBackgroundColor(new Color(0.10f, 0.10f, 0.12f))
                    .WithBorderRightWidth(1)
                    .WithBorderRightColor(new Color(0.3f, 0.3f, 0.3f))
                    .WithScrollable(true)
                    .WithTitle("Architect Controls");

                // --- SIZING ---
                controls.AddHeader("Parent Container Sizing:", Color.white);
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

                // --- FLEXBOX ---
                controls.AddSeparator(Color.gray, SeparatorWidth);
                controls.AddHeader("Parent Flexbox Layout:", Color.white);
                controls.AddEnumData("Direction", _direction, (v) => { _direction = v; RefreshPreview(); });
                controls.AddEnumData("Justify", _justify, (v) => { _justify = v; RefreshPreview(); });
                controls.AddEnumData("Align", _align, (v) => { _align = v; RefreshPreview(); });
                controls.AddEnumData("Wrap", _wrap, (v) => { _wrap = v; RefreshPreview(); });

                // --- CHILDREN CRUD ---
                controls.AddSeparator(Color.gray, SeparatorWidth);
                controls.AddHeader("Child Elements (Live GUIs & REST):", Color.cyan);
                controls.AddButton("+ Add New Child Area", () => {
                    AddNewChild($"Panel {_childrenItems.Count + 1}", 150f, 150f, 0f, 1f);
                    RefreshChildrenList();
                    RefreshPreview();
                });

                // Inject dynamic scroll container for children
                controls.AddChild(new GraphicalUserInterfaceBuilder("ChildrenListWrapper")
                    .WithMarginTop(10).WithMarginBottom(10)
                    .WithMaxHeight(350)
                    .WithBorderAllColor(new Color(0.3f, 0.3f, 0.3f))
                    .WithBorderWidth(1)
                    .WithScrollable(true)
                    .OnBuild(ve => {
                        // Extract the native ScrollView generated by WithScrollable(true)
                        _childrenListContainer = ve.Q<ScrollView>("gui-scrollview");
                        if (_childrenListContainer != null) RefreshChildrenList();
                    }));

                // --- COLOR FORGE ---
                controls.AddSeparator(Color.gray, SeparatorWidth);
                controls.AddHeader("Bulk Color Applicator:", Color.white);
                controls.AddSliderData("Corner Radius", 0, 50, _borderRadius, (v) => { _borderRadius = v; RefreshPreview(); });
                controls.AddIntSliderData("Border Width", 0, 10, _borderWidth, (v) => { _borderWidth = v; RefreshPreview(); });
                controls.AddEnumData("Target Property", _activeColorTarget, (v) => _activeColorTarget = v);

                try
                {
                    var colorForge = new ColorForgePicker(Color.cyan, (pickedColor) => {
                        if (_activeColorTarget == ColorTarget.BackgroundColor || _activeColorTarget == ColorTarget.Both) _backgroundColor = pickedColor;
                        if (_activeColorTarget == ColorTarget.BorderColor || _activeColorTarget == ColorTarget.Both) _borderColor = pickedColor;
                        RefreshPreview();
                    });

                    controls.AddChild(new GraphicalUserInterfaceBuilder("ColorForgeWrapper")
                        .WithHeight(250)
                        .WithMarginTop(10)
                        .WithBorderWidth(1)
                        .WithBorderAllColor(Color.gray)
                        .AddChild(colorForge));
                }
                catch { controls.AddHeader("ColorForgePicker Unavailable", Color.red); }

                // --- EXPORT TOOLS ---
                controls.AddSeparator(Color.gray, SeparatorWidth);
                controls.AddHeader("AI Code Generation:", new Color(0.8f, 0.4f, 1.0f));

                var aiPromptField = new TextField { multiline = true, isReadOnly = true };
                aiPromptField.style.minHeight = 200; aiPromptField.style.marginTop = 10;
                aiPromptField.style.whiteSpace = WhiteSpace.Normal; aiPromptField.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f);

                controls.AddButton("📝 Generate AI Prompt", () => aiPromptField.value = GenerateAIPrompt());
                controls.AddChild(ctx => aiPromptField); // Native fields are fine via lambda

                controls.AddSeparator(Color.gray, SeparatorWidth);
                controls.AddHeader("Native Tooling:", Color.green);
                controls.AddButton("💾 BAKE TO UXML", () => {
                    var target = _previewContainer.Children().FirstOrDefault(c => c.name == "LivePreview");
                    if (target != null) WorkshopUxmlBaker.Bake(target, "Architect_Export");
                    else ForgeLogger.LogError("[LayoutArchitect] Could not find 'LivePreview' container to bake.");
                });

                rootBuilder.AddChild(controls);

                // 3. THE PREVIEW AREA
                rootBuilder.AddChild(new GraphicalUserInterfaceBuilder("PreviewAreaWrapper")
                    .WithFlexGrow(1)
                    .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                    .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                    .OnBuild(ve => {
                        _previewContainer = ve;
                        RefreshPreview();
                    }));

                return rootBuilder.Build();
            }
            catch (Exception ex)
            {
                return new GraphicalUserInterfaceBuilder("ErrorRoot")
                    .WithFlexGrow(1).WithBackgroundColor(new Color(0.2f, 0, 0)).WithPadding(20)
                    .AddHeader("Layout Architect Encountered a Fatal Error", Color.white)
                    .AddChild(ctx => new TextField { value = $"{ex.Message}\n\n{ex.StackTrace}", multiline = true, isReadOnly = true, style = { flexGrow = 1 } })
                    .Build();
            }
        }

        private void RefreshChildrenList()
        {
            if (_childrenListContainer == null) return;
            _childrenListContainer.Clear();

            foreach (var child in _childrenItems)
            {
                var itemBox = new GraphicalUserInterfaceBuilder($"ItemBox_{child.Name}")
                    .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                    .WithMarginBottom(6).WithMarginRight(8)
                    .WithPadding(5)
                    .WithBorderLeftWidth(4).WithBorderLeftColor(child.Color)
                    .WithBorderTopRightRadius(3).WithBorderBottomRightRadius(3);

                // Header Row (Name + Delete Button)
                var headerRow = new GraphicalUserInterfaceBuilder("HeaderRow")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween);

                headerRow.AddChild(ctx => {
                    var nameField = new TextField { value = child.Name, style = { flexGrow = 1, marginRight = 5 } };
                    nameField.RegisterValueChangedCallback(e => { child.Name = e.newValue; RefreshPreview(); });
                    return nameField;
                });
                headerRow.AddChild(ctx => new Button(() => { _childrenItems.Remove(child); RefreshChildrenList(); RefreshPreview(); }) { text = "X", style = { color = Color.red, unityFontStyleAndWeight = FontStyle.Bold } });

                itemBox.AddChild(headerRow);

                // Foldout Controls
                itemBox.AddChild(ctx => {
                    var foldout = new Foldout { text = "Layout & Action Binding", value = false };

                    var providerDropdown = new DropdownField("Live GUI", _guiProviderNames, child.SelectedProviderName);
                    providerDropdown.RegisterValueChangedCallback(e => {
                        child.SelectedProviderName = e.newValue;
                        if (_guiProviderTypes.TryGetValue(e.newValue, out Type t))
                        {
                            try { child.LiveGuiInstance = (IGuiProvider)Activator.CreateInstance(t); } catch { child.LiveGuiInstance = null; }
                        }
                        else child.LiveGuiInstance = null;
                        RefreshPreview();
                    });
                    foldout.Add(providerDropdown);

                    var restDropdown = new DropdownField("OnClick Action", _availableRestEndpoints, child.OnClickRestEndpoint);
                    restDropdown.RegisterValueChangedCallback(e => { child.OnClickRestEndpoint = e.newValue; RefreshPreview(); });
                    foldout.Add(restDropdown);

                    var wField = new FloatField("Width (0=Auto)") { value = child.Width }; wField.RegisterValueChangedCallback(e => { child.Width = e.newValue; RefreshPreview(); }); foldout.Add(wField);
                    var hField = new FloatField("Height (0=Auto)") { value = child.Height }; hField.RegisterValueChangedCallback(e => { child.Height = e.newValue; RefreshPreview(); }); foldout.Add(hField);
                    var fgField = new FloatField("Flex Grow") { value = child.FlexGrow }; fgField.RegisterValueChangedCallback(e => { child.FlexGrow = e.newValue; RefreshPreview(); }); foldout.Add(fgField);
                    var fsField = new FloatField("Flex Shrink") { value = child.FlexShrink }; fsField.RegisterValueChangedCallback(e => { child.FlexShrink = e.newValue; RefreshPreview(); }); foldout.Add(fsField);

                    return foldout;
                });

                _childrenListContainer.Add(itemBox.Build());
            }
        }

        private void RefreshPreview()
        {
            if (_previewContainer == null) return;
            try
            {
                _previewContainer.Clear();

                _previewContainer.Add(new GraphicalUserInterfaceBuilder("PreviewTitle")
                    .WithAbsolutePosition(10, null, null, 10)
                    .AddHeader("LIVE GUI PREVIEW", Color.gray).Build());

                var previewBuilder = new GraphicalUserInterfaceBuilder("LivePreview")
                    .WithBackgroundColor(_backgroundColor)
                    .WithBorderAllColor(_borderColor)
                    .WithBorderWidth(_borderWidth).WithBorderRadius(_borderRadius)
                    .WithFlexLayout(_direction, _justify, _align).WithFlexWrap(_wrap)
                    .WithFlexGrow(_flexGrow).WithFlexShrink(_flexShrink).WithPadding(15);

                if (_usePercentage) previewBuilder.WithPercentSize(_widthPct, _heightPct);
                else previewBuilder.WithSize(_widthPx, _heightPx);

                foreach (var child in _childrenItems)
                {
                    var childWrapper = new GraphicalUserInterfaceBuilder(child.Name)
                        .WithFlexGrow(child.FlexGrow)
                        .WithFlexShrink(child.FlexShrink)
                        .WithMarginBottom(5).WithMarginRight(5)
                        .WithBorderRadius(4)
                        .OnBuild(ve => {
                            ve.style.overflow = Overflow.Hidden;
                            ve.style.width = child.Width > 0 ? child.Width : new StyleLength(StyleKeyword.Auto);
                            ve.style.height = child.Height > 0 ? child.Height : new StyleLength(StyleKeyword.Auto);

                            // Apply REST styling
                            if (child.OnClickRestEndpoint != "None")
                            {
                                ve.style.borderBottomWidth = 3;
                                ve.style.borderBottomColor = new Color(0.2f, 0.8f, 0.2f);
                                string endpointToFire = child.OnClickRestEndpoint;
                                ve.RegisterCallback<ClickEvent>(e => ForgeLogger.Log($"[REST Triggered] Firing: {endpointToFire}"));
                            }
                        });

                    if (child.LiveGuiInstance != null)
                    {
                        childWrapper.WithBackgroundColor(new Color(0.12f, 0.12f, 0.12f));
                        childWrapper.WithBorderTopWidth(2).WithBorderTopColor(child.Color);
                        childWrapper.AddChild(ctx => {
                            try
                            {
                                var liveGui = child.LiveGuiInstance.CreateGui(new GuiContext());
                                liveGui.style.flexGrow = 1;
                                if (child.OnClickRestEndpoint != "None") liveGui.pickingMode = PickingMode.Ignore;
                                return liveGui;
                            }
                            catch (Exception ex)
                            {
                                return new Label($"Error loading {child.SelectedProviderName}:\n{ex.Message}") { style = { color = Color.red, whiteSpace = WhiteSpace.Normal } };
                            }
                        });
                    }
                    else
                    {
                        childWrapper.WithBackgroundColor(child.Color);
                        childWrapper.AddChild(ctx => {
                            var lbl = new Label(child.Name) { style = { color = Color.black, unityTextAlign = TextAnchor.MiddleCenter, unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1 } };
                            if (child.OnClickRestEndpoint != "None")
                            {
                                lbl.text += "\n[ ⚡ API Action Bound ]";
                                lbl.style.fontSize = 11;
                            }
                            return lbl;
                        });
                    }

                    previewBuilder.AddChild(childWrapper);
                }
                _previewContainer.Add(previewBuilder.Build());
            }
            catch (Exception ex)
            {
                _previewContainer.Add(new Label($"Preview Render Error:\n{ex.Message}") { style = { color = Color.red, backgroundColor = Color.black, paddingBottom = 10, paddingTop = 10 } });
            }
        }

        private string GenerateAIPrompt()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Hey AI, please write a C# class implementing `IGuiProvider` using my `GraphicalUserInterfaceBuilder` fluent API.");
            sb.AppendLine("Here is the exact layout specification I just designed in the visual architect:");
            sb.AppendLine();

            sb.AppendLine("### Parent Layout Builder");
            sb.AppendLine($"- **Flex Layout**: Direction: {_direction}, Justify: {_justify}, Align: {_align}, Wrap: {_wrap}");
            sb.AppendLine($"- **Size**: " + (_usePercentage ? $"{_widthPct}% x {_heightPct}%" : $"{_widthPx}px x {_heightPx}px"));
            sb.AppendLine($"- **Style**: BackgroundColor(r:{_backgroundColor.r:F2}, g:{_backgroundColor.g:F2}, b:{_backgroundColor.b:F2})");
            sb.AppendLine($"- **Borders**: Width {_borderWidth}, Radius {_borderRadius}, Color(r:{_borderColor.r:F2}, g:{_borderColor.g:F2}, b:{_borderColor.b:F2})");
            sb.AppendLine();

            sb.AppendLine("### Children Elements");
            for (int i = 0; i < _childrenItems.Count; i++)
            {
                var child = _childrenItems[i];
                sb.AppendLine($"{i + 1}. **{child.Name}**");

                string wText = child.Width > 0 ? $"{child.Width}px" : "Auto/Stretch";
                string hText = child.Height > 0 ? $"{child.Height}px" : "Auto/Stretch";
                sb.AppendLine($"   - **Size Target**: Width: {wText}, Height: {hText}");
                sb.AppendLine($"   - **Flex Behavior**: Grow: {child.FlexGrow}, Shrink: {child.FlexShrink}");

                if (child.SelectedProviderName != "[ None - Colored Box ]")
                    sb.AppendLine($"   - **Content Injection**: Inject an instance of `{child.SelectedProviderName}` here.");
                else
                    sb.AppendLine($"   - **Content Placeholder**: Just put a placeholder box or empty builder here for now.");

                if (child.OnClickRestEndpoint != "None")
                    sb.AppendLine($"   - **REST Action**: Bind a ClickEvent to trigger the API endpoint: `{child.OnClickRestEndpoint}`");
            }
            sb.AppendLine();
            sb.AppendLine("Please generate the full C# code for this provider.");
            return sb.ToString();
        }

        private void AddNewChild(string name, float w, float h, float fGrow, float fShrink)
        {
            _childrenItems.Add(new PreviewChildItem
            {
                Name = name,
                Width = w,
                Height = h,
                FlexGrow = fGrow,
                FlexShrink = fShrink,
                Color = Color.HSVToRGB(UnityEngine.Random.value, 0.7f, 0.9f)
            });
        }

        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    }
}