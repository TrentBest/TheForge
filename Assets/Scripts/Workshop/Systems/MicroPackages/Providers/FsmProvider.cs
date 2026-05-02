using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.MicroPackages.Providers
{
    /// <summary>
    /// The Central Registry for FSM Blueprints.
    /// Defines the state logic and transitions that drive simulation atoms.
    /// Reforged to follow the Singularity Forge Protocol.
    /// </summary>
    public class FsmProvider : IProvider, IGuiProvider
    {
        // --- IPROVIDER IMPLEMENTATION ---

        // 1021 sits logically next to the Context Provider (1020)
        public int Id => 1021;

        // Categorized as an FSM Blueprint Provider
        public ProviderType ProviderType => ProviderType.FiniteStateMachine;

        // --- CORE DATA ---
        public string FsmName { get; set; } = "New_Definition";
        public string InitialState { get; set; } = "Idle";

        // Mocking/Internal tracking of defined states for this blueprint
        private readonly List<string> _definedStates = new List<string> { "Idle", "Processing", "Fault" };

        // --- IGUIPROVIDER IMPLEMENTATION (Blueprint Dashboard) ---

        public string Title => "FSM DEFINITION ARCHITECT";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("FsmProvider_Root")
                .WithFlexGrow(1f)
                .WithPadding(20f)
                .WithBackgroundColor(new Color(0.06f, 0.05f, 0.08f)); // Deep Void Purple

            // Header Section: Visualizing the Logic Origin
            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithBorderColor(new Color(0.6f, 0.2f, 0.8f)) // FSM Purple
                .WithBorderWidth(0, 0, 3f, 0)
                .WithMarginBottom(15f)
                .AddChild(new ForgeLabelBuilder("FSM LOGIC BLUEPRINT")
                    .WithFontSize(20).WithBold().WithColor(new Color(0.8f, 0.5f, 1.0f)))
                .AddChild(new ForgeLabelBuilder($"Managing logic for: {FsmName}")
                    .WithFontSize(10).WithColor(Color.gray))
            );

            // Body: Configuration & State List
            var body = new ForgeContainerBuilder("BlueprintBody").WithFlexGrow(1f);

            // 1. Config Block
            body.AddChild(new ForgeContainerBuilder("ConfigBlock")
                .WithPadding(10f).WithMarginBottom(15f).WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .AddChild(new ForgeTextFieldBuilder("FSM Name", FsmName)
                    .OnValueChanged(evt => FsmName = evt.newValue))
                .AddChild(new ForgeTextFieldBuilder("Initial State", InitialState)
                    .OnValueChanged(evt => InitialState = evt.newValue))
            );

            // 2. Defined States List (Dynamic rendering)
            body.AddChild(new ForgeLabelBuilder("DEFINED STATES").WithBold().WithFontSize(12).WithMarginBottom(8f));

            body.AddChild(new ForgeContainerBuilder("StateListContainer")
                .WithFlexGrow(1f).WithPadding(10f).WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .AddChild(new DynamicGuiProvider(c => {
                    var list = new VisualElement { name = "StateList" };
                    foreach (var state in _definedStates)
                    {
                        var row = new ForgeContainerBuilder($"State_{state}")
                            .WithDirection(FlexDirection.Row)
                            .WithJustifyContent(Justify.SpaceBetween)
                            .WithPadding(5f).WithMarginBottom(2f)
                            .WithBackgroundColor(new Color(0.15f, 0.12f, 0.18f))
                            .AddChild(new ForgeLabelBuilder($"➔ {state}").WithFontSize(11))
                            .AddChild(new ForgeLabelBuilder(state == InitialState ? "[START]" : "")
                                .WithColor(Color.cyan).WithFontSize(9));

                        list.Add(row.Build());
                    }
                    return list;
                }))
            );

            rootBuilder.AddChild(body);

            // Action Ribbon: Rebuilding the definition in the engine
            rootBuilder.AddChild(new ForgeContainerBuilder("ActionRibbon")
                .WithMarginTop(15f).WithDirection(FlexDirection.Row)
                .AddChild(new ForgeButtonBuilder("REBUILD DEFINITION")
                    .WithBackgroundColor(new Color(0.3f, 0.2f, 0.5f))
                    .OnClick(() => Debug.Log($"[FSM] Blueprint '{FsmName}' compiled and updated in Registry."))));

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(new GuiContext());
            WorkshopUxmlBaker.Bake(root, $"FSM_Blueprint_{FsmName}");
        }

        public void FromUIDocument(string assetPath)
        {
            Debug.LogWarning("[FsmProvider] Blueprints are generated via Fluent API in Forge; static UXML import not supported.");
        }
    }
}