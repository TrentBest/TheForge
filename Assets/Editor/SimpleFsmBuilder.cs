using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Singularity.Editor
{
    /// <summary>
    /// Tactical configuration interface for Finite State Machines.
    /// Bridges the gap between editor-time parameters and runtime Processing Groups.
    /// Reforged to follow the Forge Protocol and the Experience Model.
    /// </summary>
    public class SimpleFsmBuilder : IGuiProvider
    {
        // --- DOMAIN DATA ---
        public string FsmName = "New_FSM_Unit";
        public bool IsActive = true;
        public float TickRate = 1.0f;

        public string Title => $"FSM ARCHITECT: {FsmName.ToUpper()}";

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. Root Container with Industrial Aesthetic
            var rootBuilder = new ForgeContainerBuilder("FsmBuilder_Root")
                .WithPadding(20f)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.07f))
                .WithBorderWidth(1f)
                .WithBorderColor(new Color(0.3f, 0.3f, 0.35f))
                .WithBorderRadius(8f);

            // 2. Header Section
            rootBuilder.AddChild(new ForgeLabelBuilder("FSM CONFIGURATION")
                .WithFontSize(20)
                .WithBold()
                .WithColor(Color.cyan)
                .WithMarginBottom(15f));

            // 3. Name Configuration
            rootBuilder.AddChild(new ForgeTextFieldBuilder("FSM IDENTITY", FsmName)
                .WithMarginBottom(15f)
                .OnValueChanged(evt => FsmName = evt.newValue));

            // 4. Runtime Parameters Row (Toggles & Sliders)
            var paramRow = new ForgeContainerBuilder("ParamRow")
                .WithDirection(FlexDirection.Row)
                .WithJustifyContent(Justify.SpaceBetween)
                .WithAlignItems(Align.Center)
                .WithPadding(10f)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithBorderRadius(4f)
                .WithMarginBottom(20f);

            // Integration of standard UI elements via DynamicGuiProvider for reactivity
            paramRow.AddChild(new DynamicGuiProvider(c => {
                var toggle = new Toggle("IS ACTIVE") { value = IsActive };
                toggle.style.color = Color.white;
                toggle.RegisterValueChangedCallback(evt => IsActive = evt.newValue);
                return toggle;
            }));

            paramRow.AddChild(new DynamicGuiProvider(c => {
                var slider = new Slider("TICK RATE", 0, 10) { value = TickRate, style = { flexGrow = 1, marginLeft = 20 } };
                slider.labelElement.style.color = Color.gray;
                slider.RegisterValueChangedCallback(evt => TickRate = evt.newValue);
                return slider;
            }));

            rootBuilder.AddChild(paramRow);

            // 5. Action Commands
            rootBuilder.AddChild(new ForgeButtonBuilder("⚡ COMPILE FSM DEFINITION")
                .WithHeight(40f)
                .WithBackgroundColor(new Color(0.1f, 0.3f, 0.5f))
                .WithBold()
                .OnClick(() => {
                    ForgeLogger.Log($"<color=cyan>[FSM_Forge]</color> Compiling {FsmName} | Rate: {TickRate:F2}Hz | Active: {IsActive}");
                    // Here you would trigger the FSM_API.Create.CreateFiniteStateMachine logic
                }));

            return rootBuilder.Build();
        }

        // --- INTERFACE IMPLEMENTATIONS ---

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var snapshot = CreateGui(new GuiContext());
            WorkshopUxmlBaker.Bake(snapshot, $"{FsmName}_Config_Export");
        }

        public void FromUIDocument(string assetPath)
        {
            ForgeLogger.Log("[FsmBuilder] Static hydration bypassed. Configuration is procedurally manifested.");
        }
    }
}