using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using UnityEngine;
// The Configuration Builder
public class MotorBuilder
    {
        public string Name { get; private set; } = "Thruster";
        public float MaxThrust { get; private set; } = 100f;

        public MotorBuilder(string name, float defaultThrust)
        {
            this.Name = name;
            this.MaxThrust = defaultThrust;
        }

        // Fluent Setters
        public MotorBuilder WithName(string name) { this.Name = name; return this; }
        public MotorBuilder WithMaxThrust(float force) { this.MaxThrust = force; return this; }

        // The "Build" method instantiates the actual GameObject and Component
        public MotorThrust Build(Transform parent)
        {
            var go = new GameObject(Name);
            if (parent != null) go.transform.SetParent(parent, false);

            var motor = go.AddComponent<MotorThrust>();
            motor.motorID = Name; // Map Builder Data to Component Data
            motor.maxThrustForce = MaxThrust;

            return motor;
        }

        // GUI Integration
        public GraphicalUserInterfaceBuilder GetGuiBuilder()
        {
            var motorGui = new MotorGuiBuilder(this);

            return new GraphicalUserInterfaceBuilder($"{Name}_Panel")
                .WithSize(0, 0) // Auto-size
                .WithBorderWidth(1)
                .WithBorderColor(new Color(0.3f, 0.3f, 0.3f))
                .WithPadding(5)
                .WithMargins(2)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                .AddChild(motorGui);
        }
    }

