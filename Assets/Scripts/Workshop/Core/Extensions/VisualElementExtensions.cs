using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;

namespace Workshop.Core.Extensions
{
    public static class VisualElementExtensions
    {
        // --- MISSION: Apply Magenta Forge Styling via Themes ---
        public static T ApplyProfile<T>(this T el, GuiElementType type) where T : VisualElement
        {
            var profile = GuiSkin.Active.GetProfile(type);
            el.style.backgroundColor = profile.BackgroundColor;
            el.style.color = profile.TextColor;

            // Explicit border and radius assignments
            el.style.borderLeftWidth = profile.BorderWidth;
            el.style.borderRightWidth = profile.BorderWidth;
            el.style.borderTopWidth = profile.BorderWidth;
            el.style.borderBottomWidth = profile.BorderWidth;

            el.style.borderLeftColor = profile.BorderColor;
            el.style.borderRightColor = profile.BorderColor;
            el.style.borderTopColor = profile.BorderColor;
            el.style.borderBottomColor = profile.BorderColor;

            el.style.borderTopLeftRadius = profile.CornerRadius;
            el.style.borderTopRightRadius = profile.CornerRadius;
            el.style.borderBottomLeftRadius = profile.CornerRadius;
            el.style.borderBottomRightRadius = profile.CornerRadius;

            return el;
        }

        // --- FIX: Background Sizing (Replaces obsolete unityBackgroundScaleMode) ---
        // Note: We've removed the non-existent BackgroundScaleMode enum dependency
        public static T ScaleMode<T>(this T el, ScaleMode mode) where T : VisualElement
        {
            switch (mode)
            {
                case UnityEngine.ScaleMode.ScaleAndCrop:
                    el.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
                    break;
                case UnityEngine.ScaleMode.ScaleToFit:
                    el.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                    break;
                case UnityEngine.ScaleMode.StretchToFill:
                    // "Stretch" is the default behavior when no specific size is set, 
                    // but we can explicitly set it to 100% 100%
                    el.style.backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100));
                    break;
            }
            return el;
        }

        // --- PADDING (All Overloads Preserved) ---
        public static T Padding<T>(this T el, float val) where T : VisualElement
        {
            el.style.paddingLeft = val;
            el.style.paddingRight = val;
            el.style.paddingTop = val;
            el.style.paddingBottom = val;
            return el;
        }

        public static T Padding<T>(this T el, float horizontal, float vertical) where T : VisualElement
        {
            el.style.paddingLeft = horizontal;
            el.style.paddingRight = horizontal;
            el.style.paddingTop = vertical;
            el.style.paddingBottom = vertical;
            return el;
        }

        public static T Padding<T>(this T el, float left, float right, float top, float bottom) where T : VisualElement
        {
            el.style.paddingLeft = left;
            el.style.paddingRight = right;
            el.style.paddingTop = top;
            el.style.paddingBottom = bottom;
            return el;
        }

        // --- MARGIN (All Overloads Preserved) ---
        public static T Margin<T>(this T el, float val) where T : VisualElement
        {
            el.style.marginLeft = val;
            el.style.marginRight = val;
            el.style.marginTop = val;
            el.style.marginBottom = val;
            return el;
        }

        public static T Margin<T>(this T el, float horizontal, float vertical) where T : VisualElement
        {
            el.style.marginLeft = horizontal;
            el.style.marginRight = horizontal;
            el.style.marginTop = vertical;
            el.style.marginBottom = vertical;
            return el;
        }

        public static T Margin<T>(this T el, float left, float right, float top, float bottom) where T : VisualElement
        {
            el.style.marginLeft = left;
            el.style.marginRight = right;
            el.style.marginTop = top;
            el.style.marginBottom = bottom;
            return el;
        }

        // --- BORDER ---
        public static T Border<T>(this T el, float width, Color color) where T : VisualElement
        {
            el.style.borderLeftWidth = width;
            el.style.borderRightWidth = width;
            el.style.borderTopWidth = width;
            el.style.borderBottomWidth = width;
            el.style.borderLeftColor = color;
            el.style.borderRightColor = color;
            el.style.borderTopColor = color;
            el.style.borderBottomColor = color;
            return el;
        }

        public static T BorderWidth<T>(this T el, float width) where T : VisualElement
        {
            el.style.borderLeftWidth = width;
            el.style.borderRightWidth = width;
            el.style.borderTopWidth = width;
            el.style.borderBottomWidth = width;
            return el;
        }

        public static T BorderColor<T>(this T el, Color color) where T : VisualElement
        {
            el.style.borderLeftColor = color;
            el.style.borderRightColor = color;
            el.style.borderTopColor = color;
            el.style.borderBottomColor = color;
            return el;
        }

        // --- RADIUS ---
        public static T Radius<T>(this T el, float val) where T : VisualElement
        {
            el.style.borderTopLeftRadius = val;
            el.style.borderTopRightRadius = val;
            el.style.borderBottomLeftRadius = val;
            el.style.borderBottomRightRadius = val;
            return el;
        }

        // --- TEXT & TYPOGRAPHY ---
        public static T Color<T>(this T el, Color color) where T : VisualElement
        {
            el.style.color = color;
            return el;
        }

        public static T Bold<T>(this T el) where T : VisualElement
        {
            el.style.unityFontStyleAndWeight = FontStyle.Bold;
            return el;
        }

        public static T FontSize<T>(this T el, float size) where T : VisualElement
        {
            el.style.fontSize = size;
            return el;
        }

        public static T AlignText<T>(this T el, TextAnchor anchor) where T : VisualElement
        {
            el.style.unityTextAlign = anchor;
            return el;
        }

        // --- LAYOUT ---
        public static T FlexGrow<T>(this T el, float val) where T : VisualElement
        {
            el.style.flexGrow = val;
            return el;
        }

        public static T Row<T>(this T el) where T : VisualElement
        {
            el.style.flexDirection = FlexDirection.Row;
            return el;
        }

        public static T AlignCenter<T>(this T el) where T : VisualElement
        {
            el.style.alignItems = Align.Center;
            return el;
        }

        public static T JustifyEnd<T>(this T el) where T : VisualElement
        {
            el.style.justifyContent = Justify.FlexEnd;
            return el;
        }

        public static T Background<T>(this T el, Color color) where T : VisualElement
        {
            el.style.backgroundColor = color;
            return el;
        }

        public static T Height<T>(this T el, float val) where T : VisualElement
        {
            el.style.height = val;
            return el;
        }

        public static T Width<T>(this T el, float val) where T : VisualElement
        {
            el.style.width = val;
            return el;
        }
    }
}