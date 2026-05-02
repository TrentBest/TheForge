using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public interface IControlFactory
    {
        VisualElement CreateLabel(string text);
        VisualElement CreateTextField(string label, string value, Action<string> onChange);
        VisualElement CreateToggle(string label, bool value, Action<bool> onChange);
        VisualElement CreateSlider(string label, float min, float max, float value, Action<float> onChange);
        VisualElement CreateColorField(string label, Color value, Action<Color> onChange);
        VisualElement CreateObjectField(string label, Type type, UnityEngine.Object value, Action<UnityEngine.Object> onChange);
        VisualElement CreateEnumField<T>(string label, T value, Action<T> onChange) where T : Enum;
    }
}
