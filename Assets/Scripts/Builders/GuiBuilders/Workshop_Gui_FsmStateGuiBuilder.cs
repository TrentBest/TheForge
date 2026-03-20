using System;
using System.Collections.Generic;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Builders.GuiBuilders
{
    public class Workshop_Gui_FsmStateGuiBuilder : IGuiProvider
    {
        public string Title { get; set; } = "State Properties";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var builder = new GraphicalUserInterfaceBuilder("StateView")
                .WithPadding(15)
                .WithEditorMode(true);

            builder.AddChild(c => new TextField("State Identifier"));

            // --- ON ENTER ---
            builder.AddChild(c =>
            {
                var dropdown = new DropdownField("OnEnter:", GetRegisteredOnEnterMethods(), 0);
                dropdown.formatSelectedValueCallback = OnEnterFormatSelectedValueCallback;
                dropdown.formatListItemCallback = OnEnterFormatListItemCallback;
                return dropdown;
            });

            // --- ON UPDATE ---
            builder.AddChild(c =>
            {
                // Fixed Label: Was "OnEnter", changed to "OnUpdate"
                var dropdown = new DropdownField("OnUpdate:", GetRegisteredOnUpdateMethods(), 0);
                dropdown.formatSelectedValueCallback = OnUpdateFormatSelectedValueCallback;
                dropdown.formatListItemCallback = OnUpdateFormatListItemCallback;
                return dropdown;
            });

            // --- ON EXIT ---
            builder.AddChild(c =>
            {
                // Fixed Label: Was "OnEnter", changed to "OnExit"
                var dropdown = new DropdownField("OnExit:", GetRegisteredOnExitMethods(), 0);
                dropdown.formatSelectedValueCallback = OnExitFormatSelectedValueCallback;
                dropdown.formatListItemCallback = OnExitFormatListItemCallback;
                return dropdown;
            });

            return builder.Build();
        }

        // --------------------------------------------------------------------------------
        // CALLBACK IMPLEMENTATIONS
        // --------------------------------------------------------------------------------

        // -- OnEnter --
        private string OnEnterFormatListItemCallback(string arg)
        {
            // If the list item is null/empty, show "(None)" in the list
            return string.IsNullOrEmpty(arg) ? "(None)" : arg;
        }

        private string OnEnterFormatSelectedValueCallback(string arg)
        {
            // If nothing is selected, prompt the user
            return string.IsNullOrEmpty(arg) ? "Select Enter Logic.." : arg;
        }

        // -- OnUpdate --
        private string OnUpdateFormatListItemCallback(string arg)
        {
            return string.IsNullOrEmpty(arg) ? "(None)" : arg;
        }

        private string OnUpdateFormatSelectedValueCallback(string arg)
        {
            return string.IsNullOrEmpty(arg) ? "Select Update Logic.." : arg;
        }

        // -- OnExit --
        private string OnExitFormatListItemCallback(string arg)
        {
            return string.IsNullOrEmpty(arg) ? "(None)" : arg;
        }

        private string OnExitFormatSelectedValueCallback(string arg)
        {
            return string.IsNullOrEmpty(arg) ? "Select Exit Logic.." : arg;
        }

        // --------------------------------------------------------------------------------
        // DATA FETCHING
        // --------------------------------------------------------------------------------

        private List<string> GetRegisteredOnExitMethods()
        {
            List<string> methods = new List<string> { "" }; // Add empty string for "None" option
            // Assuming FSM_DefinitionsLibrary exists
            //methods.AddRange(FSM_DefinitionsLibrary.LoadRegisteredOnExitMethods());
            return methods;
        }

        private List<string> GetRegisteredOnUpdateMethods()
        {
            List<string> methods = new List<string> { "" };
           // methods.AddRange(FSM_DefinitionsLibrary.LoadRegisteredOnUpdateMethods());
            return methods;
        }

        List<string> GetRegisteredOnEnterMethods()
        {
            List<string> methods = new List<string> { "" };
            //methods.AddRange(FSM_DefinitionsLibrary.LoadRegisteredOnEnterMethods());
            return methods;
        }

        public Action<VisualElement> GetGuiBuilder()
        {
            throw new NotImplementedException();
        }

        public void ToUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }

        public void FromUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }
    }
}