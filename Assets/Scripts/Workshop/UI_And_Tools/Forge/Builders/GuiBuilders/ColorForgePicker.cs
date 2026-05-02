using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ColorForgePicker : IGuiProvider
    {
        public string Title => "Color Forge";

        private Color _currentColor;
        private Action<Color> _onColorChanged;
        private string _activeTab = "Sliders";
        private ForgeSplitPanelBuilder _splitBuilder;
        private VisualElement _root;
        private GuiContext _lastCtx;

        public ColorForgePicker() : this(Color.cyan, null) { }

        public ColorForgePicker(Color initialColor, Action<Color> callback)
        {
            _currentColor = initialColor;
            _onColorChanged = callback ?? (c => Debug.Log($"Color Picked: {c}"));
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            // THE FIX: Using the correct 2-argument constructor directly
            _splitBuilder = new ForgeSplitPanelBuilder(96, Side.Left);

            // 1. Define the Sidebar (The Vertical Tabs) using the fixed DynamicGuiProvider
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
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .Build();

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

                nav.Add(btn);
            }

            // Current Color Preview in Sidebar
            var separator = new VisualElement { style = { height = 2, backgroundColor = Color.black, marginTop = 5, marginBottom = 5 } };
            nav.Add(separator);

            var preview = new VisualElement { name = "ActiveColorPreview" };
            preview.style.height = 40;
            preview.style.backgroundColor = _currentColor;
            preview.style.marginTop = 10;
            preview.style.borderTopLeftRadius = 4;
            preview.style.borderTopRightRadius = 4;
            preview.style.borderBottomLeftRadius = 4;
            preview.style.borderBottomRightRadius = 4;

            nav.Add(preview);

            return nav;
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
            var rSlider = new Slider("R", 0, 1) { value = _currentColor.r };
            rSlider.RegisterValueChangedCallback(evt => { _currentColor.r = evt.newValue; Notify(); });

            var gSlider = new Slider("G", 0, 1) { value = _currentColor.g };
            gSlider.RegisterValueChangedCallback(evt => { _currentColor.g = evt.newValue; Notify(); });

            var bSlider = new Slider("B", 0, 1) { value = _currentColor.b };
            bSlider.RegisterValueChangedCallback(evt => { _currentColor.b = evt.newValue; Notify(); });

            var aSlider = new Slider("A", 0, 1) { value = _currentColor.a };
            aSlider.RegisterValueChangedCallback(evt => { _currentColor.a = evt.newValue; Notify(); });

            container.Add(rSlider);
            container.Add(gSlider);
            container.Add(bSlider);
            container.Add(aSlider);
        }

        private void BuildTextMode(VisualElement container)
        {
            var hexInput = new TextField("HEX CODE") { value = "#" + ColorUtility.ToHtmlStringRGBA(_currentColor) };
            hexInput.RegisterValueChangedCallback(evt => {
                if (ColorUtility.TryParseHtmlString(evt.newValue, out Color c))
                {
                    _currentColor = c;
                    Notify();
                }
            });
            container.Add(hexInput);
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
                _root = CreateGui(_lastCtx);
                parent.Insert(index, _root);
            }
        }

        // Bridge methods
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string path) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), path);
#endif
        public void FromUIDocument(string path) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(path));
    }
}