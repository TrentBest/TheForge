using UnityEngine;
using UnityEngine.UIElements;
using System;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;
using Workshop.Core.Diagnostics;

namespace Assets.Scripts.Builders.GuiBuilders
{
    /// <summary>
    /// Specialized GUI Provider for Swarmy Motor properties.
    /// Manages thrust vectors and identification for physics-driven entities.
    /// Reforged to follow the Forge Protocol and kill raw instantiations.
    /// </summary>
    public class MotorGuiBuilder : IGuiProvider
    {
        private MotorBuilder _builder;

        public MotorGuiBuilder(MotorBuilder builder)
        {
            _builder = builder;
        }

        public string Title { get; set; } = "MOTOR PROPERTIES";

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. Root Container with "Swarmy" Industrial Aesthetics
            var rootBuilder = new ForgeContainerBuilder("MotorProp_Root")
                .WithPadding(15f)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithBorderWidth(1f)
                .WithBorderColor(new Color(0.3f, 0.3f, 0.35f))
                .WithBorderRadius(4f);

            // 2. Header Section
            rootBuilder.AddChild(new ForgeLabelBuilder(_builder.Name.ToUpper())
                .WithBold()
                .WithFontSize(14)
                .WithColor(Color.cyan)
                .WithMarginBottom(10f));

            // 3. Data Entry Matrix
            rootBuilder.AddChild(new ForgeTextFieldBuilder("MOTOR ID", _builder.Name)
                .WithMarginBottom(5f)
                .OnValueChanged(evt => _builder.WithName(evt.newValue)));

            // Utilizing ForgeTextField for numeric thrust to ensure builder consistency
            rootBuilder.AddChild(new ForgeTextFieldBuilder("MAX FORCE (N)", _builder.MaxThrust.ToString("F2"))
                .WithMarginBottom(10f)
                .OnValueChanged(evt => {
                    if (float.TryParse(evt.newValue, out float val))
                    {
                        _builder.WithMaxThrust(val);
                    }
                }));

            // 4. Telemetry Visualization Placeholder
            rootBuilder.AddChild(new ForgeContainerBuilder("ThrustTelemetry")
                .WithHeight(4f)
                .WithBackgroundColor(new Color(0.05f, 0.4f, 0.1f)) // "Pulse" Green
                .WithBorderRadius(2f)
                .WithMarginTop(5f)
                .OnBuild(ve => {
                    // In a live Swarmy context, this would scale with actual current thrust
                    ve.style.width = Length.Percent(75f);
                }));

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(new GuiContext());
            string name = string.IsNullOrEmpty(assetPath) ? $"Motor_{_builder.Name}" : assetPath;
            WorkshopUxmlBaker.Bake(root, name);
        }

        public void FromUIDocument(string assetPath)
        {
            ForgeLogger.LogWarning("[MotorGui] Dynamic builder properties cannot be hydrated from static UXML. Re-binding required.");
        }
    }
}