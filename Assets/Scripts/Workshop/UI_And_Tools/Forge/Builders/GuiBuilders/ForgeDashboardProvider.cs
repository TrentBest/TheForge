using System;
using System.Collections.Generic;
using System.Linq;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_API.Scripts;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes.Effects;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ForgeDashboardProvider : IGuiProvider
    {
        public string Title => "System Dashboard";
        private Dictionary<string, List<Type>> _registry;
        private long _uptimeTicks = 0;

        public ForgeDashboardProvider(Dictionary<string, List<Type>> registry)
        {
            _registry = registry;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var builder = new GraphicalUserInterfaceBuilder("ForgeDashboardRoot")
                .WithAutoGrow()
                .WithScrollable(true)
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f));

            builder.AddHeader("THE FORGE: COMPONENT TELEMETRY", GuiSkin.Active.PrimaryAccent);

            // --- 1. KPI ROW ---
            var kpiRow = new GraphicalUserInterfaceBuilder("KPIRow")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween)
                .WithMarginBottom(20);

            int totalComponents = _registry.Values.Sum(v => v.Count);

            kpiRow.AddChild(CreateKpiCard("REGISTERED COMPONENTS", totalComponents.ToString(), new Color(0.38f, 0.81f, 0.38f), animate: true));
            kpiRow.AddChild(CreateKpiCard("ACTIVE CATEGORIES", _registry.Keys.Count.ToString(), new Color(0.9f, 0.7f, 0.2f)));
            kpiRow.AddChild(CreateKpiCard("FSM ENGINE", Application.isPlaying ? "RUNTIME" : "EDITOR", Color.cyan));

            builder.AddChild(kpiRow);

            // --- 2. THE ENGINE ROUTING COLUMNS ---
            var routingContainer = new GraphicalUserInterfaceBuilder("RoutingContainer")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.FlexStart)
                .WithMarginBottom(20);

            // ---------------------------------------------------------
            // LEFT COLUMN: Unity Mono Lifecycle (Runtime)
            // ---------------------------------------------------------
            var runtimeGui = new GraphicalUserInterfaceBuilder("FSMs_Runtime")
                .WithFlexGrow(1).WithMarginRight(10)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithBorderTopWidth(3).WithBorderTopColor(new Color(0.38f, 0.81f, 0.38f))
                .WithPadding(15);

            runtimeGui.AddChild(new ForgeLabelBuilder("RUNTIME LIFECYCLE")
                .WithBold(true).WithColor(Color.white).WithMarginBottom(15));

            if (FSM_UnityIntegrationAdvanced.Instance != null)
            {
                var inst = FSM_UnityIntegrationAdvanced.Instance;

                runtimeGui.AddChild(CreateLifecycleNode("Awake", GetPrivateList(inst, "_awakeProcessingGroup"), new Color(0.3f, 0.7f, 0.9f)));
                runtimeGui.AddChild(CreateLifecycleNode("Start", GetPrivateList(inst, "_startProcessingGroup"), new Color(0.4f, 0.8f, 0.9f)));
                runtimeGui.AddChild(CreateLifecycleNode("FixedUpdate", GetPrivateList(inst, "_fixedUpdateProcessingGroup"), new Color(0.9f, 0.6f, 0.2f)));
                runtimeGui.AddChild(CreateLifecycleNode("Update", GetPrivateList(inst, "_updateProcessingGroup"), new Color(0.38f, 0.81f, 0.38f)));
                runtimeGui.AddChild(CreateLifecycleNode("LateUpdate", GetPrivateList(inst, "_lateUpdateProcessingGroup"), new Color(0.5f, 0.9f, 0.5f)));
                runtimeGui.AddChild(CreateLifecycleNode("OnGUI", GetPrivateList(inst, "_onGUI_ProcessingGroup"), new Color(0.8f, 0.8f, 0.8f)));
                runtimeGui.AddChild(CreateLifecycleNode("OnDrawGizmos", GetPrivateList(inst, "_onDrawGizmosProcessingGroup"), new Color(0.9f, 0.4f, 0.4f)));
            }
            else
            {
                runtimeGui.AddChild(new ForgeLabelBuilder("Runtime Integration Offline (Not in Play Mode)")
                    .WithColor(Color.gray).WithFontStyle(FontStyle.Italic));
            }
            routingContainer.AddChild(runtimeGui);

            // ---------------------------------------------------------
            // RIGHT COLUMN: Editor Hooks Lifecycle (Tooling)
            // ---------------------------------------------------------
            var editorGui = new GraphicalUserInterfaceBuilder("FSMs_EditorTime")
                .WithFlexGrow(1).WithMarginLeft(10)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithBorderTopWidth(3).WithBorderTopColor(new Color(0.8f, 0.4f, 0.8f))
                .WithPadding(15);

            editorGui.AddChild(new ForgeLabelBuilder("EDITOR LIFECYCLE")
                .WithBold(true).WithColor(Color.white).WithMarginBottom(15));

#if UNITY_EDITOR
            Type editorHookType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                editorHookType = assembly.GetType("TheSingularityWorkshop.FsmEditorHooks.FSM_EditorIntegrationAdvanced");
                if (editorHookType != null) break;
            }

            if (editorHookType != null)
            {
                var staticField = editorHookType.GetField("EditorUpdateGroups", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                List<string> staticGroups = staticField?.GetValue(null) as List<string>;
                if (staticGroups != null) editorGui.AddChild(CreateLifecycleNode("Core Editor Loops", staticGroups, new Color(0.8f, 0.4f, 0.8f)));

                var dynField = editorHookType.GetField("_registeredGroups", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (dynField != null)
                {
                    var dict = dynField.GetValue(null) as System.Collections.IDictionary;
                    List<string> dynamicGroups = new List<string>();
                    if (dict != null) foreach (var key in dict.Keys) dynamicGroups.Add(key.ToString());

                    if (dynamicGroups.Count > 0) editorGui.AddChild(CreateLifecycleNode("Dynamic Chronos Groups", dynamicGroups, Color.cyan));
                    else editorGui.AddChild(new ForgeLabelBuilder("No custom chronos profiles active.")
                        .WithColor(Color.gray).WithFontStyle(FontStyle.Italic).WithFontSize(11).WithMarginTop(10));
                }
            }
            else
            {
                editorGui.AddChild(new ForgeLabelBuilder("Editor Hooks not found in domain.")
                    .WithColor(Color.gray).WithFontStyle(FontStyle.Italic));
            }
#else
            editorGui.AddChild(new ForgeLabelBuilder("Editor Integration Offline (Standalone Build)")
                .WithColor(Color.gray).WithFontStyle(FontStyle.Italic));
#endif

            routingContainer.AddChild(editorGui);
            builder.AddChild(routingContainer);

            // --- 3. CATEGORY INVENTORY ---
            builder.AddHeader("INVENTORY BY CATEGORY", Color.gray);
            var categoryGrid = new GraphicalUserInterfaceBuilder("CategoryGrid")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.FlexStart)
                .WithFlexWrap(Wrap.Wrap)
                .WithMarginBottom(20);

            foreach (var kvp in _registry.OrderByDescending(x => x.Value.Count))
            {
                categoryGrid.AddChild(CreateCategoryCard(kvp.Key, kvp.Value.Count));
            }
            builder.AddChild(categoryGrid);

            // --- 4. TELEMETRY TICK ---
            Label resolvedTelemetryLabel = null;

            var telemetryBox = new GraphicalUserInterfaceBuilder("TelemetryBox")
                .WithMarginTop(20).WithPaddingTop(15)
                .WithBorderTopWidth(1).WithBorderTopColor(Color.gray)
                .AddChild(new ForgeLabelBuilder("Initializing FSM Telemetry...")
                    .WithColor(Color.cyan).WithBold(true)
                    // Capture the native label reference upon build so the loop can update it
                    .OnBuild(ve => resolvedTelemetryLabel = ve as Label));

            builder.AddChild(telemetryBox);

            VisualElement root = builder.Build();

            // Schedule the continuous telemetry update against the root
            string tickGroup = "DashboardTelemetry";
            root.schedule.Execute(() =>
            {
                FSM_API.Interaction.Update(tickGroup);
                _uptimeTicks++;
                float seconds = _uptimeTicks * 0.016f;

                if (resolvedTelemetryLabel != null)
                {
                    resolvedTelemetryLabel.text = $"[SYS.TICK] Group: {tickGroup} | UI Ticks: {_uptimeTicks} | Uptime: {seconds:F1}s";
                    float pulse = (Mathf.Sin(Time.realtimeSinceStartup * 4f) + 1f) / 2f;
                    resolvedTelemetryLabel.style.color = Color.Lerp(new Color(0.2f, 0.6f, 0.8f), Color.cyan, pulse);
                }
            }).Every(16);

            return root;
        }

        // ---------------------------------------------------------
        // ENGINE API SCRAPING 
        // ---------------------------------------------------------
        private List<string> GetPrivateList(object instance, string fieldName)
        {
            if (instance == null) return new List<string>();
            var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            return field?.GetValue(instance) as List<string> ?? new List<string>();
        }

        private List<(string name, int count)> ScrapeFsmDataForGroup(string groupName)
        {
            var list = new List<(string, int)>();
            try
            {
                Type interactionApi = typeof(FSM_API.Interaction);
                Type publicApi = typeof(FSM_API);

                var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
                IEnumerable<string> defNames = null;

                var getDefsMethod = interactionApi.GetMethods(flags).FirstOrDefault(m => m.Name == "GetAllDefinitionNames")
                                 ?? publicApi.GetMethods(flags).FirstOrDefault(m => m.Name == "GetFsmDefinitionNamesInGroup");

                if (getDefsMethod != null)
                {
                    var parameters = getDefsMethod.GetParameters().Length > 0 ? new object[] { groupName } : null;
                    defNames = getDefsMethod.Invoke(null, parameters) as IEnumerable<string>;
                }

                if (defNames != null)
                {
                    var getInstancesMethod = interactionApi.GetMethods(flags).FirstOrDefault(m => m.Name == "GetInstances");

                    foreach (var def in defNames)
                    {
                        int count = 0;
                        if (getInstancesMethod != null)
                        {
                            try
                            {
                                var instances = getInstancesMethod.Invoke(null, new object[] { def, groupName }) as System.Collections.ICollection;
                                if (instances != null) count = instances.Count;
                            }
                            catch { }
                        }
                        list.Add((def, count));
                    }
                }
                else
                {
                    list.Add(("[API REFLECTION ERROR]", 0));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Forge Dashboard] FSM Scraping Error: {ex.Message}");
            }
            return list;
        }

        // ---------------------------------------------------------
        // UI DATA CARDS
        // ---------------------------------------------------------
        private IGuiProvider CreateLifecycleNode(string phaseName, List<string> assignedGroups, Color phaseColor)
        {
            var node = new GraphicalUserInterfaceBuilder($"Node_{phaseName}")
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithBorderLeftWidth(4).WithBorderLeftColor(phaseColor)
                .WithPaddingLeft(10).WithPaddingRight(10).WithPaddingTop(8).WithPaddingBottom(8)
                .WithMarginBottom(5)
                .WithBorderRadius(4);

            node.AddChild(new ForgeLabelBuilder(phaseName)
                .WithColor(phaseColor).WithBold(true).WithFontSize(13).WithMarginBottom(4));

            var bucketsContainer = new GraphicalUserInterfaceBuilder("BucketsContainer")
                .WithFlexLayout(FlexDirection.Row).WithFlexWrap(Wrap.Wrap);

            bool foundAnyFsms = false;

            if (assignedGroups != null && assignedGroups.Count > 0)
            {
                foreach (var group in assignedGroups)
                {
                    var fsmBuckets = ScrapeFsmDataForGroup(group);

                    if (fsmBuckets.Count > 0)
                    {
                        foundAnyFsms = true;
                        foreach (var fsm in fsmBuckets)
                        {
                            var fsmCard = new GraphicalUserInterfaceBuilder($"FSM_{fsm.name}")
                                .WithBackgroundColor(new Color(0.18f, 0.18f, 0.22f))
                                .WithPaddingLeft(8).WithPaddingRight(8).WithPaddingTop(4).WithPaddingBottom(4)
                                .WithMarginRight(6).WithMarginBottom(6)
                                .WithBorderRadius(4)
                                .WithBorderTopWidth(2).WithBorderTopColor(phaseColor);

                            fsmCard.AddChild(new ForgeLabelBuilder(fsm.name)
                                .WithColor(Color.white).WithBold(true).WithFontSize(12));

                            fsmCard.AddChild(new ForgeLabelBuilder($"{fsm.count} Active Instances")
                                .WithColor(Color.gray).WithFontSize(10));

                            bucketsContainer.AddChild(fsmCard);
                        }
                    }
                }
            }

            if (!foundAnyFsms)
            {
                bucketsContainer.AddChild(new ForgeLabelBuilder("Waiting for FSM allocations...")
                    .WithColor(new Color(0.4f, 0.4f, 0.4f)).WithFontSize(11));
            }

            node.AddChild(bucketsContainer);
            return node;
        }

        private IGuiProvider CreateKpiCard(string title, string value, Color accent, bool animate = false)
        {
            var card = new GraphicalUserInterfaceBuilder($"KPI_{title}")
                .WithFlexGrow(1)
                .WithMarginLeft(5).WithMarginRight(5)
                .WithPaddingTop(15).WithPaddingBottom(15).WithPaddingLeft(20).WithPaddingRight(20)
                .WithBackgroundColor(new Color(0.18f, 0.18f, 0.22f))
                .WithBorderTopWidth(4).WithBorderTopColor(accent)
                .AddChild(new ForgeLabelBuilder(title)
                    .WithColor(Color.gray).WithFontSize(11).WithBold(true).WithMarginBottom(8))
                .AddChild(new ForgeLabelBuilder(value)
                    .WithColor(Color.white).WithFontSize(32).WithBold(true))
                .OnBuild(ve => {
                    // Inject the Breathing Effect dynamically on build if flagged
                    if (animate)
                    {
                        new BreathingEffect(ve, 0, 2.5f, "EditorUpdate", new List<Color> { accent, new Color(0.2f, 0.2f, 0.2f) });
                    }
                });

            return card;
        }

        private IGuiProvider CreateCategoryCard(string name, int count)
        {
            var card = new GraphicalUserInterfaceBuilder($"Category_{name}")
                .WithWidth(200)
                .WithMarginTop(5).WithMarginBottom(5).WithMarginRight(10)
                .WithPaddingTop(10).WithPaddingBottom(10).WithPaddingLeft(15).WithPaddingRight(15)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithBorderLeftWidth(3).WithBorderLeftColor(GuiSkin.Active.PrimaryAccent)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween);

            card.AddChild(new ForgeLabelBuilder(name.ToUpper())
                .WithColor(Color.white).WithBold(true));

            card.AddChild(new ForgeLabelBuilder(count.ToString())
                .WithColor(GuiSkin.Active.PrimaryAccent).WithBold(true));

            return card;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}