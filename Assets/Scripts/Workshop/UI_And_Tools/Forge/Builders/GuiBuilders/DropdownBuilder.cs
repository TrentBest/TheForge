using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Assuming this is the correct namespace based on your other tools
namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    // NO ATTRIBUTE HERE. This is a generic UI building block, not a Data Inspector.
    public class DropDownBuilder : IGuiProvider
    {
        private string _label;
        private List<string> _options;
        private string _selectedValue;
        private Action<string> _onChanged;
        private string _shortcutTabTarget;

        public string Title => "Dropdown Control";

        public DropDownBuilder(string label, List<string> options)
        {
            _label = label;
            _options = new List<string> { "None" };
            if (options != null) _options.AddRange(options);
        }

        public DropDownBuilder WithSelection(string value) { _selectedValue = value; return this; }
        public DropDownBuilder OnSelectionChanged(Action<string> callback) { _onChanged = callback; return this; }
        public DropDownBuilder WithShortcut(string tabName) { _shortcutTabTarget = tabName; return this; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var container = new VisualElement();

            // Use the index of the current value, or 0 ("None") if not found
            int index = string.IsNullOrEmpty(_selectedValue) ? 0 : _options.IndexOf(_selectedValue);
            if (index == -1) index = 0;

            var dropdown = new PopupField<string>(_label, _options, index);
            dropdown.RegisterValueChangedCallback(evt => _onChanged?.Invoke(evt.newValue));
            container.Add(dropdown);

            // Clue if list is empty (only "None")
            if (_options.Count == 1 && !string.IsNullOrEmpty(_shortcutTabTarget))
            {
                container.Add(new Button(() => { Debug.Log($"Navigate to {_shortcutTabTarget}"); })
                {
                    text = $"Go to {_shortcutTabTarget}..",
                    style = { fontSize = 9, color = Color.cyan, backgroundColor = Color.clear, borderTopWidth = 0, borderLeftWidth = 0, borderRightWidth = 0, borderBottomWidth = 0,
                        unityTextAlign = TextAnchor.MiddleLeft }
                });
            }

            return container;
        }

        // Standard Boilerplate
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}