using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq; // For dictionary keys to list
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Builders.GuiBuilders
{
    public class SwarmGroupGuiBuilder : IGuiProvider
    {
        private readonly SwarmGroupBuilder builder;

        // In a real scenario, inject a reference to your "Drone Catalog"
        private List<DroneBuilder> availableDrones;

        public SwarmGroupGuiBuilder(SwarmGroupBuilder builder)
        {
            this.builder = builder;
            // Mocking available drones for the dropdown (This should come from your persistence layer)
            this.availableDrones = new List<DroneBuilder>()
            {
                new DroneBuilder().WithName("Scout_MK1").WithBehavior("Scout"),
                new DroneBuilder().WithName("Heavy_Lifter").WithBehavior("Cargo"),
                new DroneBuilder().WithName("Interceptor").WithBehavior("Aggressive")
            };
        }

        public string Title { get; set; } = "Swarm Group Configuration";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement();

            // 1. Group Identity
            var nameField = new TextField("Swarm Name") { value = builder.GroupName };
            nameField.RegisterValueChangedCallback(evt => builder.WithName(evt.newValue));
            root.Add(nameField);

            root.Add(new Label("Composition")
            {
                style = { marginTop = 15, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 14 }
            });

            // 2. Render Existing Composition Rows
            var compositionContainer = new VisualElement();
            RefreshCompositionList(compositionContainer, ctx);
            root.Add(compositionContainer);

            // 3. "Add Drone" Section
            var addRow = new VisualElement()
            {
                style = { flexDirection = FlexDirection.Row, marginTop = 10, borderTopWidth = 1, borderTopColor = new Color(0.3f, 0.3f, 0.3f), paddingTop = 5 }
            };

            // Dropdown to select a drone prototype
            var droneNames = availableDrones.Select(d => d.DroneName).ToList();
            var droneSelector = new DropdownField("Add Type", droneNames, 0);
            droneSelector.style.flexGrow = 1;

            var addBtn = new Button(() => {
                var selectedName = droneSelector.value;
                var proto = availableDrones.Find(d => d.DroneName == selectedName);
                if (proto != null)
                {
                    builder.AddDroneType(proto, 1); // Default to 1
                    RefreshCompositionList(compositionContainer, ctx);
                }
            })
            { text = "+" };

            addRow.Add(droneSelector);
            addRow.Add(addBtn);
            root.Add(addRow);

            // 4. Build Button
            var buildBtn = new Button(() => {
                builder.Build(Vector3.zero); // Or a specific spawn point
                Debug.Log($"[SwarmBuilder] Spawning {builder.GroupName}..");
            })
            { text = "Spawn Swarm", style = { height = 30, marginTop = 20, backgroundColor = new Color(0.2f, 0.4f, 0.2f) } };

            root.Add(buildBtn);

            return root;
        }

        public void FromUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }

        public Action<VisualElement> GetGuiBuilder()
        {
            throw new NotImplementedException();
        }

        public void ToUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }

        private void RefreshCompositionList(VisualElement container, GuiContext ctx)
        {
            container.Clear();
            var comp = builder.GetComposition();

            if (comp.Count == 0)
            {
                container.Add(new Label("No drones assigned.") { style = { opacity = 0.5f, marginLeft = 5 } });
                return;
            }

            foreach (var entry in comp)
            {
                var drone = entry.Key;
                var count = entry.Value;

                var row = new VisualElement()
                {
                    style = { flexDirection = FlexDirection.Row, marginBottom = 2, alignItems = Align.Center, backgroundColor = new Color(0.18f, 0.18f, 0.18f),  }
                };

                // Name
                var label = new Label(drone.DroneName) { style = { flexGrow = 1, unityFontStyleAndWeight = FontStyle.Bold } };

                // Count Input
                var countField = new IntegerField() { value = count };
                countField.style.width = 50;
                countField.RegisterValueChangedCallback(evt => {
                    // Update builder directly
                    builder.AddDroneType(drone, Mathf.Max(1, evt.newValue));
                });

                // Remove Button
                var removeBtn = new Button(() => {
                    builder.RemoveDroneType(drone);
                    RefreshCompositionList(container, ctx);
                })
                { text = "X", style = { color = Color.red } };

                row.Add(label);
                row.Add(new Label("Count:") { style = { marginRight = 5, fontSize = 10 } });
                row.Add(countField);
                row.Add(removeBtn);

                container.Add(row);
            }
        }
    }
}