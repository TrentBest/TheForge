using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Math;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class AstroIntProvider : IGuiProvider
    {
        public string Title { get; private set; }

        private AstroInt _value;
        public AstroInt Value
        {
            get => _value;
            set
            {
                _value = value;
                _value.Normalize();
                UpdateUIFromValue();
                OnValueChanged?.Invoke(_value);
            }
        }

        public Action<AstroInt> OnValueChanged;

        private IntegerField _astralField;
        private IntegerField _cosmicField;
        private IntegerField _megaField;
        private IntegerField _baseField;

        private Label _exactReadout;
        private bool _isUpdatingUI = false;

        public AstroIntProvider(string title, AstroInt initialValue, Action<AstroInt> onValueChanged = null)
        {
            Title = title;
            _value = initialValue;
            _value.Normalize();
            OnValueChanged = onValueChanged;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("AstroInt_" + Title)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithMarginBottom(15).WithPaddingBottom(15)
                .WithBorderBottomWidth(1).WithBorderBottomColor(new Color(0.3f, 0.3f, 0.3f))
                .Build();

            // HEADER
            root.Add(new Label(Title) { style = { color = new Color(0.7f, 0.8f, 1f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });

            // 4-TIER INPUT ROW
            var inputContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };

            _astralField = CreateTierInput("ASTRAL", _value.Astral, v => UpdateTier(ref _value.Astral, v));
            _cosmicField = CreateTierInput("COSMIC", _value.Cosmic, v => UpdateTier(ref _value.Cosmic, v));
            _megaField = CreateTierInput("MEGA", _value.Mega, v => UpdateTier(ref _value.Mega, v));
            _baseField = CreateTierInput("BASE", _value.Base, v => UpdateTier(ref _value.Base, v));

            inputContainer.Add(_astralField.parent);
            inputContainer.Add(_cosmicField.parent);
            inputContainer.Add(_megaField.parent);
            inputContainer.Add(_baseField.parent);
            root.Add(inputContainer);

            // EXACT READOUT (Dynamic 36-digit comma string)
            var readoutRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 10, paddingBottom = 5, paddingLeft = 5, paddingRight = 5, paddingTop = 5, backgroundColor = new Color(0.1f, 0.1f, 0.15f), borderTopLeftRadius = 3, borderTopRightRadius = 3, borderBottomRightRadius = 3, borderBottomLeftRadius = 3 } };
            readoutRow.Add(new Label("EXACT:") { style = { color = Color.gray, width = 50, unityTextAlign = TextAnchor.MiddleLeft } });
            _exactReadout = new Label(_value.ToExactString()) { style = { color = new Color(0.9f, 0.9f, 0.9f), flexGrow = 1, unityTextAlign = TextAnchor.MiddleRight, unityFontStyleAndWeight = FontStyle.Italic } };
            readoutRow.Add(_exactReadout);
            root.Add(readoutRow);

            return root;
        }

        private IntegerField CreateTierInput(string label, uint initialVal, Action<uint> onChanged)
        {
            var container = new VisualElement { style = { flexGrow = 1, marginLeft = 2, marginRight = 2 } };

            container.Add(new Label(label) { style = { fontSize = 9, color = Color.gray, unityTextAlign = TextAnchor.MiddleCenter } });

            var field = new IntegerField() { value = (int)initialVal, style = { flexGrow = 1 } };

            field.RegisterValueChangedCallback(evt =>
            {
                if (_isUpdatingUI) return;

                // Prevent negatives
                uint clamped = evt.newValue < 0 ? 0 : (uint)evt.newValue;
                onChanged(clamped);
            });

            container.Add(field);
            return field;
        }

        private void UpdateTier(ref uint tierTarget, uint newValue)
        {
            tierTarget = newValue;
            Value = _value; // Trigger the setter which Normalizes and Updates UI
        }

        private void UpdateUIFromValue()
        {
            if (_astralField == null) return;

            _isUpdatingUI = true;
            _astralField.SetValueWithoutNotify((int)_value.Astral);
            _cosmicField.SetValueWithoutNotify((int)_value.Cosmic);
            _megaField.SetValueWithoutNotify((int)_value.Mega);
            _baseField.SetValueWithoutNotify((int)_value.Base);
            _exactReadout.text = _value.ToExactString();
            _isUpdatingUI = false;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}