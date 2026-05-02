using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ForgeDropdownBuilder : IGuiProvider
    {
        public string Title => "Forge Dropdown";

        private string _label;
        private List<string> _choices = new List<string>();
        private string _value;
        private Action<ChangeEvent<string>> _onValueChanged;
        private Action<string> _onStringChanged;

        private readonly List<Action<VisualElement>> _onBuildActions = new();

        // --- CONSTRUCTORS ---
        public ForgeDropdownBuilder() { }

        public ForgeDropdownBuilder(string label, List<string> choices)
        {
            _label = label;
            _choices = choices ?? new List<string>();
            if (_choices.Count > 0) _value = _choices[0];
        }

        public ForgeDropdownBuilder(string label, List<string> choices, string value)
        {
            _label = label;
            _choices = choices ?? new List<string>();
            _value = value;
        }

        public ForgeDropdownBuilder(string label, List<string> choices, string value, Action<string> onValueChanged)
        {
            _label = label;
            _choices = choices ?? new List<string>();
            _value = value;
            _onStringChanged = onValueChanged;
        }

        // --- CORE FUNCTIONALITY ---
        public ForgeDropdownBuilder WithLabel(string label) { _label = label; return this; }
        public ForgeDropdownBuilder WithChoices(List<string> choices) { _choices = choices; return this; }
        public ForgeDropdownBuilder WithValue(string value) { _value = value; return this; }

        /// <summary>
        /// Fluent alias for WithValue to support FSM and Equipment Forge patterns.
        /// </summary>
        public ForgeDropdownBuilder WithSelection(string selection)
        {
            _value = selection;
            return this;
        }

        public ForgeDropdownBuilder OnValueChanged(Action<string> cb) { _onStringChanged = cb; return this; }
        public ForgeDropdownBuilder OnChanged(Action<string> cb) => OnValueChanged(cb); // Standard Forge Alias
        public ForgeDropdownBuilder OnValueChanged(Action<ChangeEvent<string>> cb) { _onValueChanged = cb; return this; }

        // --- THE ONBUILD ENGINE ---
        public ForgeDropdownBuilder OnBuild(Action<VisualElement> onBuildAction)
        {
            if (onBuildAction != null) _onBuildActions.Add(onBuildAction);
            return this;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var dropdown = new DropdownField(_label, _choices, _value);

            if (_onStringChanged != null)
            {
                dropdown.RegisterValueChangedCallback(e => _onStringChanged(e.newValue));
            }
            if (_onValueChanged != null)
            {
                dropdown.RegisterValueChangedCallback(e => _onValueChanged(e));
            }

            foreach (var act in _onBuildActions) act?.Invoke(dropdown);

            ctx?.OnBuilt?.Invoke(dropdown);

            return dropdown;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        /// <summary>
        /// Serializes the Dropdown to a UXML asset via the Workshop Baking engine.
        /// </summary>
        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? "ForgeDropdown_Export" : System.IO.Path.GetFileNameWithoutExtension(assetPath);
            WorkshopUxmlBaker.Bake(root, fileName);
        }

        public void FromUIDocument(string assetPath)
        {
            UnityEngine.Debug.LogWarning("[ForgeDropdown] FromUIDocument bypassed. Dropdowns are procedurally generated.");
        }

        // ==============================================================================
        // SIZING & SPACING (Forge Standard)
        // ==============================================================================
        public ForgeDropdownBuilder WithHeight(StyleLength height) => OnBuild(ve => ve.style.height = height);
        public ForgeDropdownBuilder WithWidth(StyleLength width) => OnBuild(ve => ve.style.width = width);

        public ForgeDropdownBuilder WithMargin(StyleLength all) => OnBuild(ve => ve.style.marginTop = ve.style.marginBottom = ve.style.marginLeft = ve.style.marginRight = all);
        public ForgeDropdownBuilder WithMarginTop(StyleLength top) => OnBuild(ve => ve.style.marginTop = top);
        public ForgeDropdownBuilder WithMarginBottom(StyleLength bottom) => OnBuild(ve => ve.style.marginBottom = bottom);
        public ForgeDropdownBuilder WithMarginLeft(StyleLength left) => OnBuild(ve => ve.style.marginLeft = left);
        public ForgeDropdownBuilder WithMarginRight(StyleLength right) => OnBuild(ve => ve.style.marginRight = right);

        public ForgeDropdownBuilder WithFlexGrow(float grow) => OnBuild(ve => ve.style.flexGrow = grow);
        public ForgeDropdownBuilder WithFlexShrink(float shrink) => OnBuild(ve => ve.style.flexShrink = shrink);
    }
}