using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ForgeNavButtonBuilder : IGuiProvider
    {
        public string Title => "Nav Button";

        private readonly string _text;
        private readonly Color _hoverAccent;
        private readonly Action _onClick;

        // Overload for default Singularity Cyan aesthetic
        public ForgeNavButtonBuilder(string text, Action onClick)
            : this(text, new Color(0f, 1f, 1f, 1f), onClick) { }

        public ForgeNavButtonBuilder(string text, Color hoverAccent, Action onClick)
        {
            _text = text;
            _hoverAccent = hoverAccent;
            _onClick = onClick;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var btn = new Button(_onClick) { text = _text };

            btn.style.backgroundColor = new Color(0.12f, 0.12f, 0.14f);
            btn.style.color = Color.silver;
            btn.style.borderLeftWidth = 4;
            btn.style.borderLeftColor = new Color(0.12f, 0.12f, 0.14f);
            btn.style.paddingTop = 10;
            btn.style.paddingBottom = 10;
            btn.style.marginBottom = 5;
            btn.style.unityTextAlign = TextAnchor.MiddleLeft;

            btn.style.borderTopWidth = 0;
            btn.style.borderBottomWidth = 0;
            btn.style.borderRightWidth = 0;

            btn.RegisterCallback<MouseEnterEvent>(e =>
            {
                btn.style.borderLeftColor = _hoverAccent;
                btn.style.color = Color.white;
            });
            btn.RegisterCallback<MouseLeaveEvent>(e =>
            {
                btn.style.borderLeftColor = new Color(0.12f, 0.12f, 0.14f);
                btn.style.color = Color.silver;
            });

            return btn;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}