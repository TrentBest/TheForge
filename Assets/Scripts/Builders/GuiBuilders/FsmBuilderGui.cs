using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API;
using Assets.Scripts.Builders.GuiBuilders;

namespace Assets.Scripts
{
    /// <summary>
    /// A GUI tool for visually designing and building FSM Definitions.
    /// Uses the GraphicalUserInterfaceBuilder for layout and styling.
    /// </summary>
    public class FsmBuilderGui : IGuiProvider
    {
        // --- Data Model for the Editor ---
        private string fsmName = "NewFSM";
        private string processGroup = "Update";
        private int processRate = -1;
        private string initialState = "";

        // Simple list to track state names being added
        private List<string> stateNames = new List<string>();
        private string newStateNameInput = "Idle"; // Buffer for the "Add State" text field

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. Initialize the main builder
            var gui = new GraphicalUserInterfaceBuilder("FsmEditorPanel")
                .WithTitle("FSM Blueprint Editor")
                .WithHeaderFontSize(18)
                .WithPadding(10)
                .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f, 0.9f)) // Dark background
                .WithBorderRadius(8f)
                .WithAutoGrow(); // Fill available space

            // 2. Core Settings Section
            gui.WithPanel("CoreSettings")
                .WithTitle("Core Configuration")
                .WithHeaderFontSize(14)
                .WithHeaderStyle(FontStyle.Bold)
                .WithPadding(5)
                .WithBorderWidth(1)
                .WithBorderColor(new Color(0.1f, 0.1f, 0.1f))
                .WithBackgroundColor(new Color(0.25f, 0.25f, 0.25f))
                .WithMarginBottom(10)
                // Add Fields
                .AddStringData("FSM Name", fsmName, val => fsmName = val)
                .AddStringData("Processing Group", processGroup, val => processGroup = val)
                .AddIntegerData("Process Rate", processRate, val => processRate = val)
                .AddStringData("Initial State", initialState, val => initialState = val);

            // 3. States Section
            var statesPanel = gui.WithPanel("StatesPanel")
                .WithTitle($"Defined States ({stateNames.Count})")
                .WithHeaderFontSize(14)
                .WithPadding(5)
                .WithBorderWidth(1)
                .WithBorderColor(new Color(0.1f, 0.1f, 0.1f))
                .WithBackgroundColor(new Color(0.25f, 0.25f, 0.25f))
                .WithMarginBottom(10);

            // List existing states
            if (stateNames.Count > 0)
            {
                foreach (var state in stateNames)
                {
                    statesPanel.AddChild(context =>
                    {
                        var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 2 } };
                        row.Add(new Label($"• {state}") { style = { flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft } });

                        // Delete button for each state
                        var delBtn = new Button(() =>
                        {
                            stateNames.Remove(state);
                            // Trigger rebuild of the UI to reflect changes
                            // Note: In a real scenario, you might want a cleaner refresh mechanism
                            context.OnBuilt?.Invoke(CreateGui(context));
                        })
                        { text = "X" };
                        delBtn.style.width = 25;
                        row.Add(delBtn);

                        return row;
                    });
                }
            }
            else
            {
                statesPanel.AddChild(new Label("No states defined.") { style = { opacity = 0.5f, unityFontStyleAndWeight = FontStyle.Italic } });
            }

            // "Add State" Mini-Form
            statesPanel.AddChild(context =>
            {
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 8, paddingTop = 8, borderTopWidth = 1, borderTopColor = new Color(0.4f, 0.4f, 0.4f) } };

                var input = new TextField { value = newStateNameInput, style = { flexGrow = 1, marginRight = 5 } };
                input.RegisterValueChangedCallback(e => newStateNameInput = e.newValue);

                var addBtn = new Button(() =>
                {
                    if (!string.IsNullOrWhiteSpace(newStateNameInput) && !stateNames.Contains(newStateNameInput))
                    {
                        stateNames.Add(newStateNameInput);
                        if (string.IsNullOrEmpty(initialState)) initialState = newStateNameInput; // Auto-set initial if empty
                        // Refresh GUI logic here would ideally trigger a re-render
                    }
                })
                { text = "Add State" };

                row.Add(input);
                row.Add(addBtn);
                return row;
            });

            // 4. Action Buttons (Footer)
            gui.AddChild(context =>
            {
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 10, justifyContent = Justify.FlexEnd } };

                var buildBtn = new Button(() => BuildFsmDefinition(context))
                {
                    text = "Build & Register FSM",
                    style = { height = 30, backgroundColor = new Color(0.2f, 0.6f, 0.2f) }
                };

                row.Add(buildBtn);
                return row;
            });

            return gui.Build();
        }

        private void BuildFsmDefinition(GuiContext ctx)
        {
            try
            {
                // Start the Fluent FSM Builder
                var builder = FSM_API.Create.CreateFiniteStateMachine(fsmName, processRate, processGroup);

                // Add States
                foreach (var sName in stateNames)
                {
                    // For this editor, we create "Stub" states. 
                    // To add actual logic, you would likely need a more complex Node Graph editor, 
                    // or you could use Reflection to find methods matching "OnEnter_StateName".
                    builder.State(
                        sName,
                        c => Debug.Log($"[{fsmName}] Enter {sName}"),
                        c => { /* Update logic */ },
                        c => Debug.Log($"[{fsmName}] Exit {sName}")
                    );
                }

                // Set Initial State
                if (!string.IsNullOrEmpty(initialState))
                {
                    builder.WithInitialState(initialState);
                }

                // Finalize
                builder.BuildDefinition();

                ctx.Log?.Invoke($"Successfully built and registered FSM: {fsmName}");
                Debug.Log($"<color=green>FSM '{fsmName}' Registered!</color>");
            }
            catch (Exception ex)
            {
                ctx.Log?.Invoke($"Error building FSM: {ex.Message}");
                Debug.LogError($"FSM Build Error: {ex.Message}");
            }
        }
    }
}