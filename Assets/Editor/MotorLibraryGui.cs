using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Libraries;
using TheSingularityWorkshop.Builders; // For MotorBuilder
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace TheSingularityWorkshop.Editors
{
    public class MotorLibraryGui : IGuiProvider
    {
        private MotorBuilder currentSelection;
        private string selectedKey; // Tracks the dictionary key (to handle renaming)
        private VisualElement rightPane;
        private VisualElement listContainer;

        public string Title => "Motor Library";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement()
            {
                style = { flexDirection = FlexDirection.Row, flexGrow = 1, minHeight = 400 }
            };

            // --- LEFT PANE (Navigation / Create) ---
            var leftPane = new VisualElement()
            {
                style = { width = 200, borderRightWidth = 1, borderRightColor = new Color(0.3f, 0.3f, 0.3f), }
            };

            // 1. CREATE Button
            var createBtn = new Button(() => {
                var newMotor = new MotorBuilder("New_Motor_" + Random.Range(100, 999), 100f);
                BlueprintLibrary.Register(newMotor);
                SelectMotor(newMotor, ctx);
                RefreshList(ctx);
            })
            { text = "+ Create New Motor", style = { marginBottom = 10, height = 30 } };
            leftPane.Add(createBtn);

            // 2. READ List
            listContainer = new ScrollView();
            RefreshList(ctx);
            leftPane.Add(listContainer);

            // --- RIGHT PANE (Update / Delete) ---
            rightPane = new VisualElement()
            {
                style = { flexGrow = 1,  }
            };

            // Initial Empty State
            rightPane.Add(new Label("Select a Motor to Edit")
            {
                style = { unityFontStyleAndWeight = FontStyle.Italic, opacity = 0.5f }
            });

            root.Add(leftPane);
            root.Add(rightPane);

            return root;
        }

        // --- Helper: Select and Render Right Pane ---
        private void SelectMotor(MotorBuilder motor, GuiContext ctx)
        {
            currentSelection = motor;
            selectedKey = motor.Name; // Store original name to handle renames
            rightPane.Clear();

            if (motor == null) return;

            // Header Actions
            var headerRow = new VisualElement() { style = { flexDirection = FlexDirection.Row, marginBottom = 10, justifyContent = Justify.FlexEnd } };

            // 4. DELETE Button
            var deleteBtn = new Button(() => {
                BlueprintLibrary.DeleteMotor(selectedKey);
                currentSelection = null;
                rightPane.Clear();
                RefreshList(ctx);
            })
            { text = "Delete", style = { backgroundColor = new Color(0.6f, 0.2f, 0.2f) } };

            // 3. UPDATE (Save) Button
            var saveBtn = new Button(() => {
                // Handle Rename logic via Library
                BlueprintLibrary.RenameMotor(selectedKey, currentSelection);

                // Update our tracking key
                selectedKey = currentSelection.Name;

                RefreshList(ctx); // Refresh list in case name changed
                Debug.Log($"Saved {selectedKey}");
            })
            { text = "Save / Update", style = { width = 120 } };

            headerRow.Add(deleteBtn);
            headerRow.Add(saveBtn);
            rightPane.Add(headerRow);

            // Embed the User's MotorGuiBuilder
            var editorGui = new MotorGuiBuilder(currentSelection).CreateGui(ctx);

            // Style the container for visual separation
            var editorContainer = new VisualElement()
            {
                style = {
                    //borderWidth = 1,
                    //borderColor = new Color(0.3f,0.3f,0.3f),
                    //paddingAll = 10,
                    backgroundColor = new Color(0.15f, 0.15f, 0.15f),
                    //borderRadius = 4
                }
            };
            editorContainer.Add(editorGui);
            rightPane.Add(editorContainer);
        }

        // --- Helper: Refresh the List ---
        private void RefreshList(GuiContext ctx)
        {
            listContainer.Clear();
            foreach (var name in BlueprintLibrary.GetMotorNames())
            {
                var btn = new Button(() => {
                    var m = BlueprintLibrary.GetMotor(name);
                    SelectMotor(m, ctx);
                })
                { text = name };

                // Highlight active selection
                if (currentSelection != null && name == currentSelection.Name)
                {
                    btn.style.color = new Color(0.4f, 0.8f, 1f);
                    btn.style.unityFontStyleAndWeight = FontStyle.Bold;
                }

                listContainer.Add(btn);
            }
        }

        public System.Action<VisualElement> GetGuiBuilder()
        {
            throw new System.NotImplementedException();
        }

        public void ToUIDocument(string assetPath)
        {
            throw new System.NotImplementedException();
        }

        public void FromUIDocument(string assetPath)
        {
            throw new System.NotImplementedException();
        }
    }
}