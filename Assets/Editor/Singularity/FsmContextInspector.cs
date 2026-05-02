using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;
using System.Linq;

namespace Assets.Editor.Singularity
{
    /// <summary>
    /// A high-fidelity diagnostic window that inspects all FSMHandles tied to a specific context.
    /// Replaced legacy StatusTab with a generic, FSM-centric manifestation.
    /// </summary>
    public class FsmContextInspector : IGuiProvider
    {
        public string Title => "FSM CONTEXT INSPECTOR";

        private readonly object _targetContext;
        private readonly string _contextName;

        public FsmContextInspector(object targetContext, string contextName = "Unknown Entity")
        {
            _targetContext = targetContext;
            _contextName = contextName;
        }

        /* * [TODO: API ENHANCEMENT - HANDLE RETRIEVAL]
         * 1. Currently, FSM_API.Interaction.GetInstance requires (Name, Context, Group).
         * 2. Deficit: Need a public FSM_API.Interaction.GetAllInstances(object context) to avoid 
         * manual interrogation of FSM_API.Internal._groupHandles or _buckets.
         * 3. Goal: Make it easy for a user to "Get and Work with" their contexts without knowing 
         * the specific FSM group topology beforehand.
         */

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("FsmInspector_Root")
                .WithFlexGrow(1f)
                .WithPadding(15f)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.07f));

            // Header Section
            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithMarginBottom(15f).WithPadding(10f)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithBorderWidth(0, 0, 1f, 0).WithBorderColor(Color.cyan)
                .AddChild(new ForgeLabelBuilder(_contextName.ToUpper()).WithFontSize(18).WithBold().WithColor(Color.cyan))
                .AddChild(new ForgeLabelBuilder("SYSTEMIC LOGIC MANIFEST").WithFontSize(10).WithColor(Color.gray)));

            // Dynamic Blade Section
            rootBuilder.AddChild(new DynamicGuiProvider(c =>
            {
                var scroll = new ScrollView(ScrollViewMode.Vertical);

                /* * [TODO: CONTEXT INTERACTION GAPS]
                 * Once Interaction.GetAllInstances(context) is live, replace this manual query.
                 * We want to enable users to not just "see" the blades, but "Work with" them:
                 * - Bulk State Transitions (e.g., Force all combat FSMs on this context to "Retreat").
                 * - Hot-swapping FSM Definitions on a per-context basis via the GUI.
                 */
                var handles = GetInstancesByContext(ctx);

                if (handles == null || handles.Count == 0)
                {
                    scroll.Add(new ForgeLabelBuilder("NO ACTIVE FSM HANDLES DETECTED")
                        .WithMarginTop(40f).OnBuild(l => l.style.unityTextAlign = TextAnchor.MiddleCenter).Build());
                    return scroll;
                }

                foreach (var handle in handles)
                {
                    scroll.Add(CreateLogicBlade(handle));
                }

                return scroll;
            }));

            return rootBuilder.Build();
        }

        private List<FSMHandle> GetInstancesByContext(IStateContext targetContext)
        {

           return FSM_API.Internal.GetAllFsmHandles().Where(s=>s.Context != null && s.Context == targetContext).ToList();
                        
        }

        private VisualElement CreateLogicBlade(FSMHandle handle)
        {
            var blade = new ForgeContainerBuilder($"Blade_{handle.Id}")
                .WithMarginBottom(8f).WithPadding(12f)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                .WithBorderWidth(1f, 1f, 1f, 3f)
                .WithBorderColor(new Color(0.3f, 0.3f, 0.35f))
                .OnBuild(ve => ve.style.borderLeftColor = handle.IsValid ? new Color(0.1f, 0.8f, 0.4f) : Color.red);

            blade.AddChild(new ForgeContainerBuilder("BladeHeader")
                .WithDirection(FlexDirection.Row).WithJustifyContent(Justify.SpaceBetween)
                .AddChild(new ForgeLabelBuilder(handle.Definition.Name).WithBold())
                .AddChild(new ForgeLabelBuilder($"GROUP: {handle.Definition.ProcessingGroup}").WithFontSize(9).WithColor(Color.gray)));

            blade.AddChild(new ForgeContainerBuilder("Stats")
                .WithDirection(FlexDirection.Row).WithMarginTop(8f)
                .AddChild(CreateMiniStat("STATE", handle.CurrentState, Color.cyan))
                .AddChild(CreateMiniStat("EXECUTION", handle.IsValid ? "LIVE" : "HALTED", handle.IsValid ? Color.green : Color.red)));

            return blade.Build();
        }

        private IGuiProvider CreateMiniStat(string label, string val, Color valColor)
        {
            return new ForgeContainerBuilder("MiniStat")
                .WithMarginRight(20f)
                .AddChild(new ForgeLabelBuilder(label).WithFontSize(8).WithColor(Color.gray))
                .AddChild(new ForgeLabelBuilder(val).WithFontSize(12).WithBold().WithColor(valColor));
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), $"FsmInspector_{_contextName}");
        public void FromUIDocument(string path) { }
    }
}