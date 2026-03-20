using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Singularity.Editor
{
    public static class SingularityGui
    {
        // --- 1. HIERARCHY MANAGEMENT ---

        /// <summary>
        /// Adds a child and returns the CHILD. Used for drilling down.
        /// </summary>
        public static T Embed<T>(this VisualElement parent, T child) where T : VisualElement
        {
            parent.Add(child);
            return child;
        }

        /// <summary>
        /// Adds a child and returns the PARENT. Used for adding siblings.
        /// </summary>
        public static VisualElement Append<T>(this VisualElement parent, T child) where T : VisualElement
        {
            parent.Add(child);
            return parent;
        }

        // --- 2. LAYOUT & STYLING (Flexbox Abstraction) ---

        public static T Row<T>(this T e) where T : VisualElement
        {
            e.style.flexDirection = FlexDirection.Row;
            return e;
        }

        public static T Column<T>(this T e) where T : VisualElement
        {
            e.style.flexDirection = FlexDirection.Column;
            return e;
        }

        public static T Grow<T>(this T e, float value = 1f) where T : VisualElement
        {
            e.style.flexGrow = value;
            return e;
        }

        public static T Width<T>(this T e, float value) where T : VisualElement
        {
            e.style.width = value;
            return e;
        }

        public static T Height<T>(this T e, float value) where T : VisualElement
        {
            e.style.height = value;
            return e;
        }

        public static T Background<T>(this T e, Color color) where T : VisualElement
        {
            e.style.backgroundColor = color;
            return e;
        }

        public static T Padding<T>(this T e, float padding) where T : VisualElement
        {
            e.style.paddingTop = padding;
            e.style.paddingBottom = padding;
            e.style.paddingLeft = padding;
            e.style.paddingRight = padding;
            return e;
        }

        // --- 3. EVENT HANDLING ---

        public static Button OnClick(this Button b, Action action)
        {
            b.clicked += action;
            return b;
        }

        /// <summary>
        /// Binds a value change to a logic update.
        /// </summary>
        public static T OnChange<T, V>(this T field, Action<V> action) where T : BaseField<V>
        {
            field.RegisterValueChangedCallback(evt => action(evt.newValue));
            return field;
        }
    }
}