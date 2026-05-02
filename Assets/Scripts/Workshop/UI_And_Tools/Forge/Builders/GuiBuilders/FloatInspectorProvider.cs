using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Pong
{
    public class FloatInspectorProvider : IGuiProvider
    {
        public string Title => "Float Inspector";

        private string _displayName;
        private Func<float> _getter;
        private Action<float> _setter;
        private bool _isReadOnly;

        // Constructor designed to be instantiated by the ReflectiveGuiBuilder
        public FloatInspectorProvider(string displayName, Func<object> getter, Action<object> setter)
        {
            _displayName = displayName;
            _getter = () => (float)getter();

            if (setter == null)
            {
                _isReadOnly = true;
                _setter = null;
            }
            else
            {
                _isReadOnly = false;
                _setter = val => setter(val);
            }
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder($"FloatInspector_{_displayName}")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithPadding(5)
                .WithBorderBottomColor(new Color(0.2f, 0.2f, 0.2f)).WithBorderBottomWidth(1)
                .Build();

            // The Display Name
            var nameLabel = new Label(_displayName.ToUpper() + ":") { style = { color = Color.gray, width = 120 } };
            root.Add(nameLabel);

            if (_isReadOnly)
            {
                // REPORTING GUI (Private Data)
                var valLabel = new Label();
                valLabel.style.color = Color.cyan;
                valLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

                // Live Polling
                root.schedule.Execute(() => {
                    valLabel.text = _getter().ToString("F2");
                }).Every(50);

                root.Add(valLabel);

                // A visual lock icon to indicate it's private
                root.Add(new Label("🔒") { style = { color = Color.gray, fontSize = 10, marginLeft = 5 } });
            }
            else
            {
                // EDITABLE GUI (Public Data)
                var slider = new Slider(0f, 1000f); // Arbitrary range for example
                slider.style.flexGrow = 1;
                slider.value = _getter();
                slider.RegisterValueChangedCallback(e => _setter(e.newValue));

                // Live poll the slider position in case data changes externally
                root.schedule.Execute(() => {
                    if (slider.focusController?.focusedElement != slider) // Don't override while user is dragging
                    {
                        slider.value = _getter();
                    }
                }).Every(50);

                root.Add(slider);
            }

            return root;
        }

        public Action<VisualElement> GetGuiBuilder() => null;
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}