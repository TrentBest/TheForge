using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace TheSingularityWorkshop.MicroPackages.Providers
{
    /// <summary>
    /// The Functional Execution Provider.
    /// Injects specific logic (Actions) into the OnUpdate cycle of an FSM State.
    /// Reforged to kill stubs and follow the "God Mode" Experience Model.
    /// </summary>
    public class FsmStateOnUpdateActionProvider : IProvider, IGuiProvider
    {
        // --- IPROVIDER IMPLEMENTATION ---

        // 1022 follows the Logic sequence (1020: Context, 1021: Blueprint)
        public int Id => 1022;
        public ProviderType ProviderType => ProviderType.StateOnUpdate;

        // --- CORE ACTION DATA ---
        public string TargetFsm { get; private set; }
        public string TargetState { get; private set; }
        public Action<object> OnUpdateAction { get; private set; }

        private long _executionCount = 0;

        public FsmStateOnUpdateActionProvider(string fsmName, string stateName, Action<object> onUpdateAction)
        {
            TargetFsm = fsmName;
            TargetState = stateName;

            // We wrap the action to track telemetry for the Forge Dashboard
            OnUpdateAction = (ctx) =>
            {
                onUpdateAction?.Invoke(ctx);
                _executionCount++;
            };
        }

        // --- IGUIPROVIDER IMPLEMENTATION (Logic Telemetry Dashboard) ---

        public string Title => "FSM ACTION MONITOR";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("ActionProvider_Root")
                .WithFlexGrow(1f)
                .WithPadding(20f)
                .WithBackgroundColor(new Color(0.05f, 0.08f, 0.05f)); // Logic Green Tint

            // Header Section: Visualizing the Logic Injection
            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithBorderColor(new Color(0.2f, 0.8f, 0.2f)) // Action Green
                .WithBorderWidth(0, 0, 3f, 0)
                .WithMarginBottom(15f)
                .AddChild(new ForgeLabelBuilder("STATE LOGIC INJECTOR")
                    .WithFontSize(20).WithBold().WithColor(new Color(0.5f, 1.0f, 0.5f)))
                .AddChild(new ForgeLabelBuilder($"Target: {TargetFsm} ➔ {TargetState}")
                    .WithFontSize(10).WithColor(Color.gray))
            );

            // Telemetry Panel
            rootBuilder.AddChild(new ForgeContainerBuilder("Telemetry")
                .WithPadding(15f)
                .WithBackgroundColor(new Color(0.1f, 0.12f, 0.1f))
                .WithBorderRadius(5f)
                // Use DynamicGuiProvider to handle real-time execution count updates
                .AddChild(new DynamicGuiProvider(c => {
                    var box = new VisualElement();
                    box.Add(new ForgeLabelBuilder("RUNTIME EXECUTION STATS").WithBold().WithMarginBottom(10f).Build());

                    var statsRow = new ForgeContainerBuilder("StatsRow")
                        .WithDirection(FlexDirection.Row)
                        .WithJustifyContent(Justify.SpaceBetween)
                        .AddChild(new ForgeLabelBuilder("Tick Cycles Recorded:").WithFontSize(12))
                        .AddChild(new ForgeLabelBuilder(_executionCount.ToString("N0"))
                            .WithColor(Color.green).WithBold().WithFontSize(14));

                    box.Add(statsRow.Build());
                    return box;
                }))
            );

            // Logic Descriptor
            rootBuilder.AddChild(new ForgeContainerBuilder("Description")
                .WithMarginTop(20f)
                .WithPadding(10f)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                .AddChild(new ForgeLabelBuilder("ACTION PAYLOAD").WithFontSize(10).WithColor(Color.gray))
                .AddChild(new ForgeLabelBuilder("This provider holds a compiled delegate. Logic is processed on the 'Rules' or 'Physics' Processing Group depending on the FSM Definition.")
                    .WithFontSize(11).WithWhiteSpace(WhiteSpace.Normal))
            );

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(new GuiContext());
            WorkshopUxmlBaker.Bake(root, $"Action_{TargetFsm}_{TargetState}");
        }

        public void FromUIDocument(string assetPath)
        {
            Debug.LogWarning("[FsmActionProvider] Logic delegates cannot be recovered from static UXML. Re-injection required at runtime.");
        }
    }
}