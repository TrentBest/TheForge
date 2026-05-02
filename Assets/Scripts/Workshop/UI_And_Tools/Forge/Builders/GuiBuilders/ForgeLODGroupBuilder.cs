using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ForgeLODGroupBuilder : IGuiProvider
    {
        public string Title => "LOD Group";

        private readonly List<VisualElement> _panels = new List<VisualElement>();
        private readonly FlexDirection _direction;
        private readonly float _lod1Size; // How small the peripheral panels get (e.g., 60px)

        public ForgeLODGroupBuilder(FlexDirection direction = FlexDirection.Row, float lod1Size = 60f)
        {
            _direction = direction;
            _lod1Size = lod1Size;
        }

        public ForgeLODGroupBuilder AddLODPanel(VisualElement panel, bool startsInFocus = false)
        {
            // Setup Native UI Toolkit Smooth Transitions
            panel.style.transitionDuration = new List<TimeValue> { new TimeValue(0.3f, TimeUnit.Second) };
            panel.style.transitionProperty = new List<StylePropertyName>
            {
                new StylePropertyName("flex-grow"),
                new StylePropertyName("width"),
                new StylePropertyName("height"),
                new StylePropertyName("opacity")
            };

            panel.style.overflow = Overflow.Hidden; // Prevents "drunk" bleeding when shrunk

            // Add click detection to steal focus
            panel.RegisterCallback<PointerDownEvent>(evt => SetFocus(panel));

            _panels.Add(panel);

            // Initialize State
            if (startsInFocus) ApplyLOD0(panel);
            else ApplyLOD1(panel);

            return this;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement { style = { flexGrow = 1, flexDirection = _direction } };

            foreach (var panel in _panels)
            {
                root.Add(panel);
            }

            ctx?.OnBuilt?.Invoke(root);
            return root;
        }

        private void SetFocus(VisualElement target)
        {
            foreach (var panel in _panels)
            {
                if (panel == target) ApplyLOD0(panel);
                else ApplyLOD1(panel);
            }
        }

        // ==========================================
        // LOD STATE DEFINITIONS
        // ==========================================

        private void ApplyLOD0(VisualElement ve)
        {
            // IN FOCUS: Allowed to expand and take up remaining space
            ve.style.flexGrow = 1;
            ve.style.opacity = 1f;

            if (_direction == FlexDirection.Row) ve.style.width = StyleKeyword.Auto;
            else ve.style.height = StyleKeyword.Auto;
        }

        private void ApplyLOD1(VisualElement ve)
        {
            // PERIPHERAL: Stripped of flex rights, forced to minimum size, dimmed
            ve.style.flexGrow = 0;
            ve.style.opacity = 0.6f;

            if (_direction == FlexDirection.Row) ve.style.width = _lod1Size;
            else ve.style.height = _lod1Size;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}