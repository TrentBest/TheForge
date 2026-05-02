// File: Assets/Scripts/Workshop/UI_And_Tools/Forge/Builders/GuiBuilders/GuiBuilderExtensions.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public static class GuiBuilderExtensions
    {
        public static GraphicalUserInterfaceBuilder AddLabel(
            this GraphicalUserInterfaceBuilder builder,
            string text,
            Color color,
            int fontSize = 14,
            FontStyle fontStyle = FontStyle.Normal,
            int marginBottom = 0,
            int width = 0,
            TextAnchor alignment = TextAnchor.UpperLeft,
            WhiteSpace whiteSpace = WhiteSpace.NoWrap,
            string name = null)
        {
            return builder.AddChild(ctx => {
                var label = new Label(text);
                if (!string.IsNullOrEmpty(name)) label.name = name;
                label.style.color = color;
                label.style.fontSize = fontSize;
                label.style.unityFontStyleAndWeight = fontStyle;
                label.style.marginBottom = marginBottom;
                label.style.unityTextAlign = alignment;
                label.style.whiteSpace = whiteSpace;
                if (width > 0) label.style.width = width;
                return label;
            });
        }

        public static GraphicalUserInterfaceBuilder AddCustomDropdown(
            this GraphicalUserInterfaceBuilder builder,
            string name,
            List<string> choices,
            string defaultValue,
            int width = 110,
            Color? bgColor = null)
        {
            return builder.AddChild(ctx => {
                var dropdown = new DropdownField(choices, defaultValue) { name = name };
                dropdown.style.width = width;
                dropdown.style.marginRight = 10;
                dropdown.style.backgroundColor = bgColor ?? new Color(0.1f, 0.1f, 0.1f);
                dropdown.style.color = Color.white;
                return dropdown;
            });
        }

        public static GraphicalUserInterfaceBuilder AddStyledButton(
            this GraphicalUserInterfaceBuilder builder,
            string text,
            Color bgColor,
            Color textColor,
            Action onClick = null,
            int height = 40,
            int width = 0,
            float flexGrow = 0,
            int marginLeft = 0)
        {
            return builder.AddChild(ctx => {
                var btn = new Button(onClick) { text = text };
                btn.style.backgroundColor = bgColor;
                btn.style.color = textColor;
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;
                btn.style.height = height;
                if (width > 0) btn.style.width = width;
                if (flexGrow > 0) btn.style.flexGrow = flexGrow;
                if (marginLeft > 0) btn.style.marginLeft = marginLeft;
                return btn;
            });
        }
        /// <summary>
        /// Syntactic sugar to resolve an IGuiProvider into a VisualElement without explicitly passing a new context.
        /// </summary>
        public static VisualElement Build(this IGuiProvider provider, GuiContext ctx = null)
        {
            return provider.CreateGui(ctx ?? new GuiContext());
        }
        public static GraphicalUserInterfaceBuilder OnMouseEnter(this GraphicalUserInterfaceBuilder builder, Action<VisualElement> action)
        {
            return builder.OnBuild(ve => ve.RegisterCallback<MouseEnterEvent>(e => action?.Invoke(ve)));
        }

        public static GraphicalUserInterfaceBuilder OnMouseLeave(this GraphicalUserInterfaceBuilder builder, Action<VisualElement> action)
        {
            return builder.OnBuild(ve => ve.RegisterCallback<MouseLeaveEvent>(e => action?.Invoke(ve)));
        }

        public static GraphicalUserInterfaceBuilder WithAutoMarginTop(this GraphicalUserInterfaceBuilder builder)
        {
            return builder.OnBuild(ve => ve.style.marginTop = Length.Auto());
        }

        public static GraphicalUserInterfaceBuilder WithEditorSelection(this GraphicalUserInterfaceBuilder builder, Action<VisualElement> onSelect)
        {
            return builder.OnBuild(ve => ve.AddManipulator(new ForgeResizeSelectionManipulator(onSelect)));
        }
    }
}