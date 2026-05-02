using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine.UIElements;
using Workshop.Core.Memory;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class GuiContext : IStateContext
    {
        public bool EditMode { get; set; } = false;
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "GuiContext";

        public DataWarehouse DataWarehouse { get; set; }
        public StyleSheet StyleSheet { get; set; }
        public List<StyleSheet> AdditionalStyleSheets { get; } = new();
        public Action<VisualElement> OnBuilt { get; set; }
        public Action<string> Log { get; set; } = _ => { };
        public Dictionary<string, bool> Flags { get; } = new();

        // Fix: Properly exposed Service Bag for Pillar 4 Arbitration
        public Dictionary<Type, object> Services { get; } = new();
        public RuntimeControlFactory Controls { get; set; }

        public void AddStyle(StyleSheet ss) => AdditionalStyleSheets.Add(ss);
        public void AddService<T>(T instance) where T : class => Services[typeof(T)] = instance;
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
        public void Clear() { }
    }
}