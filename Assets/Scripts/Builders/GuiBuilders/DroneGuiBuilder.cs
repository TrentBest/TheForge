using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Builders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;

namespace TheSingularityWorkshop.Builders.GuiBuilders
{
    public class DroneGuiBuilder : IGuiProvider
    {
        private readonly DroneBuilder builder;

        public DroneGuiBuilder(DroneBuilder builder)
        {
            this.builder = builder;
        }

        public string Title => "Drone Swarm";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement();

            // 1. Drone Settings
            var nameField = new TextField("Drone Name") { value = builder.DroneName };
            nameField.RegisterValueChangedCallback(evt => builder.WithName(evt.newValue));
            root.Add(nameField);

            var behaviorField = new TextField("Behavior ID") { value = builder.BehaviorID };
            behaviorField.RegisterValueChangedCallback(evt => builder.WithBehavior(evt.newValue));
            root.Add(behaviorField);

            root.Add(new Label("Motor Configuration")
            {
                style = { marginTop = 15, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 14 }
            });

            // 2. Embed Motor GUIs
            var frontRow = new VisualElement() { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };
            frontRow.Add(RenderMotor(builder.FrontLeft, ctx));
            frontRow.Add(RenderMotor(builder.FrontRight, ctx));
            root.Add(frontRow);

            var rearRow = new VisualElement() { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginTop = 5 } };
            rearRow.Add(RenderMotor(builder.RearLeft, ctx));
            rearRow.Add(RenderMotor(builder.RearRight, ctx));
            root.Add(rearRow);

            // 3. Build Button
            var buildBtn = new Button(() => {
                builder.Build(Vector3.zero);
                Debug.Log($"[DroneBuilder] Constructed {builder.DroneName}");
            })
            { text = "Construct Drone", style = { height = 30, marginTop = 20 } };
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

        private VisualElement RenderMotor(MotorBuilder motorBuilder, GuiContext ctx)
        {
            // Use the parameterless Build() to match your existing AtomBuilder pattern
            var element = motorBuilder.GetGuiBuilder().Build();

            element.style.flexGrow = 1;
            element.style.width = Length.Percent(48);
            return element;
        }
    }
}