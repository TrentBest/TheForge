using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Workshop.Forge.Builders.GuiBuilders
{
    public class ColorForgePicker : IGuiProvider
    {
        public string Title => "Color Forge";

        private Color _currentColor;
        private Action<Color> _onColorChanged;
        private string _activeTab = "Sliders";
        private SplitPanelBuilder _splitBuilder;
        private VisualElement _root;

        public ColorForgePicker() : this(Color.cyan, null) { }

        public ColorForgePicker(Color initialColor, Action<Color> callback)
        {
            _currentColor = initialColor;
            _onColorChanged = callback ?? (c => Debug.Log($"Color Picked: {c}"));
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // We use the SplitPanelBuilder to define the layout
            _splitBuilder = new SplitPanelBuilder(sidebarWidth: 96, Side.Left);

            // 1. Define the Sidebar (The Vertical Tabs)
            _splitBuilder.WithSidebar(new DynamicGuiProvider(c => CreateVerticalTabNav()));

            // 2. Define the Main Content (The Active Mode)
            _splitBuilder.WithMain(new DynamicGuiProvider(c => CreateMainContent()));

            _root = _splitBuilder.CreateGui(ctx);
            return _root;
        }

        private VisualElement CreateVerticalTabNav()
        {
            var nav = new GraphicalUserInterfaceBuilder("ColorNav")
                .WithPadding(5)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

            string[] tabs = { "Sliders", "Input", "Palette" };
            foreach (var tab in tabs)
            {
                bool isActive = _activeTab == tab;
                var btn = new Button(() => { _activeTab = tab; Refresh(); }) { text = tab };

                // Style the vertical tab
                btn.style.height = 40;
                btn.style.marginBottom = 5;
                btn.style.backgroundColor = isActive ? new Color(0.3f, 0.3f, 0.6f) : new Color(0.2f, 0.2f, 0.2f);
                btn.style.color = Color.white;

                nav.AddChild(btn);
            }

            // Current Color Preview in Sidebar
            nav.AddSeparator(Color.black, 2);
            nav.AddChild(ctx => {
                var preview = new VisualElement { name = "ActiveColorPreview" };
                preview.style.height = 40;
                preview.style.backgroundColor = _currentColor;
                preview.style.marginTop = 10;
                preview.style.borderTopLeftRadius = 4;
                preview.style.borderTopLeftRadius = 4;
                preview.style.borderTopLeftRadius = 4;
                preview.style.borderTopLeftRadius = 4;
                return preview;
            });

            return nav.Build();
        }

        private VisualElement CreateMainContent()
        {
            var container = new VisualElement();
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;
            container.style.paddingTop = 10;
            container.style.flexGrow = 1;

            switch (_activeTab)
            {
                case "Sliders": BuildSliderMode(container); break;
                case "Input": BuildTextMode(container); break;
                case "Palette": BuildPaletteMode(container); break;
            }

            return container;
        }

        private void BuildSliderMode(VisualElement container)
        {
            var b = new GraphicalUserInterfaceBuilder("SliderView");
            b.AddSliderData("R", 0, 1, _currentColor.r, (v) => { _currentColor.r = v; Notify(); });
            b.AddSliderData("G", 0, 1, _currentColor.g, (v) => { _currentColor.g = v; Notify(); });
            b.AddSliderData("B", 0, 1, _currentColor.b, (v) => { _currentColor.b = v; Notify(); });
            b.AddSliderData("A", 0, 1, _currentColor.a, (v) => { _currentColor.a = v; Notify(); });
            container.Add(b.Build());
        }

        private void BuildTextMode(VisualElement container)
        {
            var b = new GraphicalUserInterfaceBuilder("TextView");
            b.AddStringData("HEX CODE", "#" + ColorUtility.ToHtmlStringRGBA(_currentColor), (val) => {
                if (ColorUtility.TryParseHtmlString(val, out Color c)) { _currentColor = c; Notify(); }
            });
            container.Add(b.Build());
        }

        private void BuildPaletteMode(VisualElement container)
        {
            var palette = new VisualElement();
            palette.style.flexDirection = FlexDirection.Row;
            palette.style.flexWrap = Wrap.Wrap;

            Color[] colors = { Color.red, Color.green, Color.blue, Color.cyan, Color.magenta, Color.yellow, Color.white, Color.black, new Color(0.5f, 0, 0.5f) };

            foreach (var c in colors)
            {
                var swatch = new Button(() => { _currentColor = c; Notify(); }) { text = "" };
                swatch.style.width = 35;
                swatch.style.height = 35;
                swatch.style.marginRight = 5;
                swatch.style.marginBottom = 5;
                swatch.style.backgroundColor = c;
                palette.Add(swatch);
            }
            container.Add(palette);
        }

        private void Notify()
        {
            _onColorChanged?.Invoke(_currentColor);
            // Targeted update of the preview in the sidebar
            var preview = _root?.Q<VisualElement>("ActiveColorPreview");
            if (preview != null) preview.style.backgroundColor = _currentColor;
        }

        private void Refresh()
        {
            // Trigger the UI to re-render using the parent Context's refresh logic 
            // Or manually rebuild this component's hierarchy
            if (_root != null && _root.parent != null)
            {
                var parent = _root.parent;
                int index = parent.IndexOf(_root);
                parent.Remove(_root);
                parent.Insert(index, CreateGui(new GuiContext()));
            }
        }

        // Bridge methods
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => _splitBuilder?.ToUIDocument(path);
        public void FromUIDocument(string path) => _splitBuilder?.FromUIDocument(path);
    }

    /// <summary>
    /// A helper to wrap a simple lambda into an IGuiProvider for SplitPanel sectors
    /// </summary>
    public class DynamicGuiProvider : IGuiProvider
    {
        private Func<GuiContext, VisualElement> _factory;
        public string Title => "Dynamic";
        public DynamicGuiProvider(Func<GuiContext, VisualElement> factory) => _factory = factory;
        public VisualElement CreateGui(GuiContext ctx) => _factory?.Invoke(ctx);

        public Action<VisualElement> GetGuiBuilder()
        {
            throw new NotImplementedException();
        }

        public void ToUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }

        public void FromUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }
    }
}