using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class EditorControlFactory : IControlFactory
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
#if UNITY_EDITOR
            var cf = new ColorField(label) { value = value };
            cf.RegisterValueChangedCallback(e => onChange?.Invoke(e.newValue));
            return cf;
#else
            return new Label("Editor Controls Not Available");
#endif
        }

        public VisualElement CreateObjectField(string label, Type type, UnityEngine.Object value, Action<UnityEngine.Object> onChange)
        {
#if UNITY_EDITOR
            var of = new ObjectField(label) { objectType = type, value = value };
            of.RegisterValueChangedCallback(e => onChange?.Invoke(e.newValue));
            return of;
#else
            return new Label("Editor Controls Not Available");
#endif
        }

        public VisualElement CreateEnumField<T>(string label, T value, Action<T> onChange) where T : Enum
        {
#if UNITY_EDITOR
            var field = new UnityEngine.UIElements.EnumField(label, value);
            field.RegisterValueChangedCallback(e => onChange?.Invoke((T)e.newValue));
            return field;
#else
            // Fallback for strict builds
            return new Label("Editor Controls Not Available");
#endif
        }
    }
}