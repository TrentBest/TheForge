using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Builders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;

namespace TheSingularityWorkshop.Builders.GuiBuilders
{
    public class MotorGuiBuilder : IGuiProvider
    {
        private MotorBuilder builder;

        public MotorGuiBuilder(MotorBuilder builder)
        {
            this.builder = builder;
        }

        public string Title { get; set; } = "Motor Properties";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement();

            var title = new Label(builder.Name)
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 2 }
            };
            root.Add(title);

            var nameField = new TextField("ID") { value = builder.Name };
            nameField.RegisterValueChangedCallback(evt => builder.WithName(evt.newValue));
            root.Add(nameField);

            var thrustField = new FloatField("Max Force") { value = builder.MaxThrust };
            thrustField.RegisterValueChangedCallback(evt => builder.WithMaxThrust(evt.newValue));
            root.Add(thrustField);

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
    }
}