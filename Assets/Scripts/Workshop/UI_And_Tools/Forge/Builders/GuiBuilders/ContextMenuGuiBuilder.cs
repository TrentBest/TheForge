using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders; // Required for IForgeBuilder

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ContextMenuGuiBuilder : IForgeBuilder
    {
        // --- Internal State ---
        private readonly List<Action<ContextualMenuPopulateEvent>> _menuItems = new List<Action<ContextualMenuPopulateEvent>>();
        private string _toolName = "ContextMenuBuilder";

        // --- IForgeBuilder Implementation ---
        public string ToolName => _toolName;

        public Type GetProductType() => typeof(ContextualMenuManipulator);

        public object Build()
        {
            // Returns the compiled manipulator to be attached to a VisualElement
            return new ContextualMenuManipulator(evt =>
            {
                foreach (var item in _menuItems)
                {
                    item(evt);
                }
            });
        }

        public IGuiProvider GetGuiProvider()
        {
            // A context menu is a manipulator attached to existing geometry/UI, 
            // not a standalone panel, so it doesn't provide its own distinct GUI.
            return null;
        }

        // --- Fluent Builder Methods ---

        /// <summary>
        /// Optional: Overrides the default tool name for reflection/tracking purposes.
        /// </summary>
        public ContextMenuGuiBuilder WithName(string name)
        {
            _toolName = name;
            return this;
        }

        /// <summary>
        /// Adds a clickable action to the context menu.
        /// </summary>
        /// <param name="actionName">The text displayed in the menu.</param>
        /// <param name="action">The logic executed when clicked.</param>
        /// <param name="statusCheck">Optional condition to disable or hide the item.</param>
        public ContextMenuGuiBuilder AddAction(string actionName, Action<DropdownMenuAction> action, Func<DropdownMenuAction, DropdownMenuAction.Status> statusCheck = null)
        {
            _menuItems.Add(evt =>
            {
                evt.menu.AppendAction(actionName, action, statusCheck ?? (a => DropdownMenuAction.Status.Normal));
            });
            return this;
        }

        /// <summary>
        /// Adds a visual separator line to the context menu.
        /// </summary>
        /// <param name="path">Optional sub-path if the separator is inside a nested menu folder.</param>
        public ContextMenuGuiBuilder AddSeparator(string path = "")
        {
            _menuItems.Add(evt => evt.menu.AppendSeparator(path));
            return this;
        }
    }
}