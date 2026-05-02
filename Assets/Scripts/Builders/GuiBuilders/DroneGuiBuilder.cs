using Assets.Scripts.Asteroids;
using Assets.Scripts.Builders;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Builders.GuiBuilders
{
    public class DroneGuiBuilder : IGuiProvider
    {
        private readonly DroneBuilder _builder;

        public DroneGuiBuilder(DroneBuilder builder) { _builder = builder; }

        public DroneGuiBuilder()
        {
            _builder = new DroneBuilder();
        }

        public string Title => "Drone Swarm Architect";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("DroneGui_Root")
                .WithFlexGrow(1f).WithPadding(20f).WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f));

            rootBuilder.AddChild(new ForgeContainerBuilder("Header").WithMarginBottom(15f)
                .AddChild(new ForgeLabelBuilder("DRONE UNIT MANIFEST").WithFontSize(22).WithBold().WithColor(new Color(0.4f, 0.8f, 1.0f)))
                .AddChild(new ForgeLabelBuilder("Configure propulsion array.").WithColor(Color.gray)));

            var propArray = new ForgeContainerBuilder("Propulsion").WithDirection(FlexDirection.Column);
            propArray.AddChild(RenderMotorSlot("FRONT PORT", _builder.FrontLeft, ctx));
            propArray.AddChild(RenderMotorSlot("FRONT STBD", _builder.FrontRight, ctx));

            rootBuilder.AddChild(propArray);
            return rootBuilder.Build();
        }

        private VisualElement RenderMotorSlot(string label, MotorBuilder motor, GuiContext ctx)
        {
            var container = new ForgeContainerBuilder($"Slot_{label}")
                .WithMarginBottom(10f).WithPadding(12f).WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f)).WithBorderRadius(5f);

            container.AddChild(new ForgeLabelBuilder(label).WithFontSize(10).WithColor(Color.gray).WithMarginBottom(8f));

            // FIXED: Decoupled Logic - UI Provider creates the sub-UI, not the data class
            if (motor != null)
            {
                var motorUi = new MotorGuiBuilder(motor);
                container.AddChild(motorUi.CreateGui(ctx));
            }

            return container.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}