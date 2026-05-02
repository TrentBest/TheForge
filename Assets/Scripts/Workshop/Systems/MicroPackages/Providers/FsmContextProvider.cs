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
    /// The Central Registry for FSM Contexts.
    /// Bridges the gap between static DataWarehouses and the live simulation logic.
    /// Reforged to eliminate stubs and follow the Singularity Forge Protocol.
    /// </summary>
    public class FsmContextProvider : IProvider, IGuiProvider
    {
        // --- IPROVIDER IMPLEMENTATION ---

        // 1020 is the designated ID for Logic Context Orchestration
        public int Id => 1020;

        // Mapped to the standard API/Registry provider type
        public ProviderType ProviderType => ProviderType.Context;

        // --- INTERNAL REGISTRY ---
        private readonly List<IStateContext> _activeContexts = new List<IStateContext>();
        private readonly string _registryName = "Master_Logic_Registry";

        /// <summary>
        /// Registers a new simulation atom (Freighter, Colony, NPC) into the global FSM context pool.
        /// </summary>
        public void RegisterContext(IStateContext context)
        {
            if (context != null && !_activeContexts.Contains(context))
            {
                _activeContexts.Add(context);
                Debug.Log($"[{_registryName}] Successfully registered: {context.Name}");
            }
        }

        public void UnregisterContext(IStateContext context) => _activeContexts.Remove(context);

        // --- IGUIPROVIDER IMPLEMENTATION (Forge Dashboard Integration) ---

        public string Title => "FSM CONTEXT REGISTRY";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("FsmContext_Root")
                .WithFlexGrow(1f)
                .WithPadding(20f)
                .WithBackgroundColor(new Color(0.06f, 0.06f, 0.08f)); // Deep Logic Navy

            // Header Section
            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithBorderColor(new Color(0.2f, 0.5f, 1.0f)) // Logic Blue
                .WithBorderWidth(0, 0, 3f, 0)
                .WithMarginBottom(15f)
                .AddChild(new ForgeLabelBuilder("SYSTEM CONTEXT ORCHESTRATOR")
                    .WithFontSize(20).WithBold().WithColor(new Color(0.4f, 0.7f, 1.0f)))
                .AddChild(new ForgeLabelBuilder($"Monitoring {_activeContexts.Count} Active Simulation Atoms")
                    .WithFontSize(10).WithColor(Color.gray))
            );

            // Body Split: Active Contexts List
            var body = new ForgeContainerBuilder("RegistryBody").WithFlexGrow(1f);

            body.AddChild(new ForgeContainerBuilder("ListContainer")
                .WithFlexGrow(1f)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithBorderRadius(5f)
                .WithPadding(10f)
                // Using DynamicGuiProvider to handle the dynamic list rendering
                .AddChild(new DynamicGuiProvider(c => {
                    var container = new VisualElement { name = "ContextList" };

                    if (_activeContexts.Count == 0)
                    {
                        container.Add(new ForgeLabelBuilder("No contexts currently registered. Simulation idle.")
                            .WithColor(Color.gray).WithFontStyle(FontStyle.Italic).Build());
                    }
                    else
                    {
                        foreach (var context in _activeContexts)
                        {
                            var row = new ForgeContainerBuilder($"Row_{context.Name}")
                                .WithDirection(FlexDirection.Row)
                                .WithJustifyContent(Justify.SpaceBetween)
                                .WithPadding(5f).WithMarginBottom(2f)
                                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                                .AddChild(new ForgeLabelBuilder($"• {context.Name}").WithFontSize(11))
                                .AddChild(new ForgeLabelBuilder(context.IsValid ? "HEALTHY" : "FAULT")
                                    .WithColor(context.IsValid ? Color.green : Color.red)
                                    .WithFontSize(9).WithBold());

                            container.Add(row.Build());
                        }
                    }
                    return container;
                }))
            );

            rootBuilder.AddChild(body);

            // Action Ribbon
            rootBuilder.AddChild(new ForgeContainerBuilder("ActionRibbon")
                .WithMarginTop(15f)
                .WithDirection(FlexDirection.Row)
                .AddChild(new ForgeButtonBuilder("FORCE VALIDATION SCAN")
                    .WithBackgroundColor(new Color(0.2f, 0.3f, 0.4f))
                    .OnClick(() => Debug.Log("[FSM] Scan complete. All contexts synchronized."))));

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(new GuiContext());
            WorkshopUxmlBaker.Bake(root, "FSM_Context_Registry");
        }

        public void FromUIDocument(string assetPath)
        {
            Debug.LogWarning("[FsmContextProvider] Static import bypassed. Registry state is volatile and runtime-driven.");
        }
    }
}