using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Builders;
using Assets.Scripts.Builders.GuiBuilders;

namespace Assets.Scripts.Builders
{
    public class SwarmGroupBuilder
    {
        public string GroupName { get; private set; } = "Alpha Squad";
        public string StrategyID { get; private set; } = "CircleFormation";

        // The composition: A drone blueprint (Builder) mapped to a count
        private Dictionary<DroneBuilder, int> composition = new Dictionary<DroneBuilder, int>();

        public SwarmGroupBuilder WithName(string name) { GroupName = name; return this; }

        public SwarmGroupBuilder AddDroneType(DroneBuilder dronePrototype, int count)
        {
            if (dronePrototype == null) return this;

            if (composition.ContainsKey(dronePrototype))
                composition[dronePrototype] = count;
            else
                composition.Add(dronePrototype, count);

            return this;
        }

        public SwarmGroupBuilder RemoveDroneType(DroneBuilder dronePrototype)
        {
            if (composition.ContainsKey(dronePrototype)) composition.Remove(dronePrototype);
            return this;
        }

        public Dictionary<DroneBuilder, int> GetComposition() => composition;

        // Instantiates the swarm in the scene
        public GameObject Build(Vector3 origin)
        {
            var swarmRoot = new GameObject(GroupName);
            swarmRoot.transform.position = origin;

            // Simple formation logic (grid for now)
            int totalIndex = 0;
            int rowWidth = 5;
            float spacing = 2.0f;

            foreach (var entry in composition)
            {
                var proto = entry.Key;
                var count = entry.Value;

                for (int i = 0; i < count; i++)
                {
                    // Calculate offset
                    float x = (totalIndex % rowWidth) * spacing;
                    float z = (totalIndex / rowWidth) * spacing;
                    Vector3 pos = origin + new Vector3(x, 0, z);

                    // Build the drone using the prototype
                    // Note: We might need to modify DroneBuilder.Build to accept a parent
                    var droneController = proto.Build(pos);
                    droneController.transform.SetParent(swarmRoot.transform);

                    // Naming convention: "DroneName_01", "DroneName_02"
                    droneController.name = $"{proto.DroneName}_{i + 1}";

                    totalIndex++;
                }
            }

            return swarmRoot;
        }

        public GraphicalUserInterfaceBuilder GetGuiBuilder()
        {
            // Inject the library of available drones here (Mocking 'DroneLibrary.All' for now)
            var swarmGui = new SwarmGroupGuiBuilder(this);

            return new GraphicalUserInterfaceBuilder($"{GroupName}_SwarmEditor")
                .WithTitle("Swarm Composition")
                .WithSize(450, 500)
                .WithPadding(10)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.12f))
                .WithScrollable(true, UnityEngine.UIElements.ScrollViewMode.Vertical)
                .AddChild(swarmGui);
        }
    }
}