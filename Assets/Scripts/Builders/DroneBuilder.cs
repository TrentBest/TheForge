using UnityEngine;
using UnityEngine.UIElements; // <--- Added this to fix 'ScrollViewMode' error
using Assets.Scripts.Builders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Assets.Scripts.Swarmy;
using Assets.Scripts.Builders.GuiBuilders;

namespace Assets.Scripts.Asteroids
{
    public class DroneBuilder
    {
        public string DroneName { get; private set; } = "New Drone";
        public string BehaviorID { get; private set; } = "StandardDrone";

        // Sub-Builders
        public MotorBuilder FrontLeft { get; private set; }
        public MotorBuilder FrontRight { get; private set; }
        public MotorBuilder RearLeft { get; private set; }
        public MotorBuilder RearRight { get; private set; }

        public DroneBuilder()
        {
            FrontLeft = new MotorBuilder("FWD_LT", 100f);
            FrontRight = new MotorBuilder("FWD_RT", 100f);
            RearLeft = new MotorBuilder("AFT_LT", 100f);
            RearRight = new MotorBuilder("AFT_RT", 100f);
        }

        public DroneBuilder WithName(string name) { DroneName = name; return this; }
        public DroneBuilder WithBehavior(string id) { BehaviorID = id; return this; }

        public DroneController Build(Vector3 position)
        {
            var go = new GameObject(DroneName);
            go.transform.position = position;

            var controller = go.AddComponent<DroneController>();
            // controller.droneID = DroneName; 
            // controller.behaviorID = BehaviorID;

            // Build Motors
            var fl = FrontLeft.Build();
            fl.LocalOffset = new Vector3(-1, 0, 1);

            var fr = FrontRight.Build();
            fr.LocalOffset = new Vector3(1, 0, 1);

            var rl = RearLeft.Build();
            rl.LocalOffset = new Vector3(-1, 0, -1);

            var rr = RearRight.Build();
            rr.LocalOffset = new Vector3(1, 0, -1);

            return controller;
        }

        public GraphicalUserInterfaceBuilder GetGuiBuilder()
        {
            var droneGui = new DroneGuiBuilder(this);

            return new GraphicalUserInterfaceBuilder($"{DroneName}_Config")
                .WithTitle("Drone Configuration")
                .WithSize(500, 600)
                .WithPadding(10)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.12f))
                .WithScrollable(true, ScrollViewMode.Vertical) // This now works
                .AddChild(droneGui);
        }
    }
}