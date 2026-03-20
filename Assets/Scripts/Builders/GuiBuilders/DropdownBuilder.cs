using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Builders.GuiBuilders
{
    public class DropDownBuilder : IGuiProvider
    {
        private string _label;
        private List<string> _options;
        private string _selectedValue;
        private System.Action<string> _onChanged;
        private string _shortcutTabTarget;

        public string Title  { get; }
        public DropDownBuilder(string label, List<string> options)
        {
            _label = label;
            _options = new List<string> { "None" };
            _options.AddRange(options);
        }

        public DropDownBuilder WithSelection(string value) { _selectedValue = value; return this; }
        public DropDownBuilder OnSelectionChanged(System.Action<string> callback) { _onChanged = callback; return this; }
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
                container.Add(new Button(() => { /* Hub Tab Switch Logic */ })
                {
                    text = $"Go to {_shortcutTabTarget}..",
                    style = { fontSize = 9, color = Color.cyan, backgroundColor = Color.clear }
                });
            }

            return container;
        }

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
