using TheSingularityWorkshop.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// Context shared with GUI builders: styling, lifecycle, logging, and services.
    /// </summary>
    public class GuiContext : IStateContext
    {
        // IStateContext
        public bool EditMode { get; set; } = false;
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "GuiContext";

        /// <summary>Primary stylesheet (optional). You can also use AdditionalStyleSheets.</summary>
        public StyleSheet StyleSheet { get; set; }

        /// <summary>Extra stylesheets appended to the panel in order.</summary>
        public List<StyleSheet> AdditionalStyleSheets { get; } = new();

        /// <summary>Invoked after a panel is fully built.</summary>
        public Action<VisualElement> OnBuilt { get; set; }

        /// <summary>Invoked on GUI builder logs (info/warnings/errors).</summary>
        public Action<string> Log { get; set; } = _ => { };

        /// <summary>Optional per-context flags (e.g., RTL, Compact, Debug).</summary>
        public Dictionary<string, bool> Flags { get; } = new();

        /// <summary>General-purpose service bag (e.g., localization, validation, bus).</summary>
        public Dictionary<Type, object> Services { get; } = new();
        public RuntimeControlFactory Controls { get; internal set; }

        public void AddStyle(StyleSheet ss)
        {
            if (ss != null) AdditionalStyleSheets.Add(ss);
        }

        public void AddService<T>(T instance) where T : class
        {
            if (instance != null) Services[typeof(T)] = instance;
        }

        public bool TryGetService<T>(out T instance) where T : class
        {
            if (Services.TryGetValue(typeof(T), out var obj))
            {
                instance = obj as T;
                return instance != null;
            }
            instance = null;
            return false;
        }

        public void Clear()
        {

        }
    }
}
