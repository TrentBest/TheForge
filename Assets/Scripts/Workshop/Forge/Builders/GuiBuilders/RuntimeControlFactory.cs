using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace TheSingularityWorkshop.Builders.GuiBuilders
{
    public class RuntimeControlFactory : IControlFactory
    {
        public VisualElement CreateLabel(string text) => new Label(text);

        public VisualElement CreateTextField(string label, string value, Action<string> onChange)
        {
            var field = new TextField(label) { value = value ?? string.Empty };
            field.RegisterValueChangedCallback(e => onChange?.Invoke(e.newValue));
            return field;
        }

        public VisualElement CreateToggle(string label, bool value, Action<bool> onChange)
        {
            var toggle = new Toggle(label) { value = value };
            toggle.RegisterValueChangedCallback(e => onChange?.Invoke(e.newValue));
            return toggle;
        }

        public VisualElement CreateSlider(string label, float min, float max, float value, Action<float> onChange)
        {
            var slider = new Slider(label, min, max) { value = value };
            slider.RegisterValueChangedCallback(e => onChange?.Invoke(e.newValue));
            return slider;
        }

        public VisualElement CreateColorField(string label, Color value, Action<Color> onChange)
        {
            var root = new VisualElement();
            root.Add(new Label(label));

            var swatch = new VisualElement { style = { height = 20, backgroundColor = value, marginBottom = 5 } };
            root.Add(swatch);

            Action Update = () => {
                swatch.style.backgroundColor = value;
                onChange?.Invoke(value);
            };

            root.Add(CreateSlider("R", 0, 1, value.r, v => { value.r = v; Update(); }));
            root.Add(CreateSlider("G", 0, 1, value.g, v => { value.g = v; Update(); }));
            root.Add(CreateSlider("B", 0, 1, value.b, v => { value.b = v; Update(); }));

            return root;
        }

        public VisualElement CreateObjectField(string label, Type type, UnityEngine.Object value, Action<UnityEngine.Object> onChange)
        {
            return new Label($"{label}: [Object Selection Not Supported In-World]");
        }

        public VisualElement CreateEnumField<T>(string label, T value, Action<T> onChange) where T : Enum
        {
            var names = Enum.GetNames(typeof(T)).ToList();
            int index = Array.IndexOf(Enum.GetValues(typeof(T)), value);

            var dropdown = new DropdownField(label, names, index);
            dropdown.RegisterValueChangedCallback(evt =>
            {
                try
                {
                    T result = (T)Enum.Parse(typeof(T), evt.newValue);
                    onChange?.Invoke(result);
                }
                catch { }
            });

            return dropdown;
        }
    }
}