// File: Assets/Scripts/Workshop/Forge/Builders/GuiBuilders/ForgeDashboardProvider.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API.Scripts;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders.Themes;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
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
            kpiRow.AddChild(CreateKpiCard("REGISTERED COMPONENTS", totalComponents.ToString(), new Color(0.38f, 0.81f, 0.38f)));
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
            var runtimeGui = new GraphicalUserInterfaceBuilder("FSMs - Runtime")
                .WithFlexGrow(1).WithMarginRight(10)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithBorderTopWidth(3).WithBorderTopColor(new Color(0.38f, 0.81f, 0.38f))
                .WithPadding(15);

            runtimeGui.AddChild(new Label("RUNTIME LIFECYCLE") { style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.white, marginBottom = 15 } });

            if (FSM_UnityIntegrationAdvanced.Instance != null)
            {
                var inst = FSM_UnityIntegrationAdvanced.Instance;

                // Manually scrape the private fields using our helper to ensure they look like nice nodes
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
                runtimeGui.AddChild(new Label("Runtime Integration Offline (Not in Play Mode)") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Italic } });
            }
            routingContainer.AddChild(runtimeGui);

            // ---------------------------------------------------------
            // RIGHT COLUMN: Editor Hooks Lifecycle (Tooling)
            // ---------------------------------------------------------
            var editorGui = new GraphicalUserInterfaceBuilder("FSMs - EditorTime")
                .WithFlexGrow(1).WithMarginLeft(10)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithBorderTopWidth(3).WithBorderTopColor(new Color(0.8f, 0.4f, 0.8f))
                .WithPadding(15);

            editorGui.AddChild(new Label("EDITOR LIFECYCLE") { style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.white, marginBottom = 15 } });

#if UNITY_EDITOR
            Type editorHookType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                editorHookType = assembly.GetType("TheSingularityWorkshop.FsmEditorHooks.FSM_EditorIntegrationAdvanced");
                if (editorHookType != null) break;
            }

            if (editorHookType != null)
            {
                // Scrape static 'EditorUpdateGroups' List
                var staticField = editorHookType.GetField("EditorUpdateGroups", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                List<string> staticGroups = staticField?.GetValue(null) as List<string>;
                if (staticGroups != null) editorGui.AddChild(CreateLifecycleNode("Core Editor Loops", staticGroups, new Color(0.8f, 0.4f, 0.8f)));

                // Scrape private '_registeredGroups' Dictionary (Chronos API)
                var dynField = editorHookType.GetField("_registeredGroups", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (dynField != null)
                {
                    var dict = dynField.GetValue(null) as System.Collections.IDictionary;
                    List<string> dynamicGroups = new List<string>();
                    if (dict != null) foreach (var key in dict.Keys) dynamicGroups.Add(key.ToString());

                    if (dynamicGroups.Count > 0) editorGui.AddChild(CreateLifecycleNode("Dynamic Chronos Groups", dynamicGroups, Color.cyan));
                    else editorGui.AddChild(new Label("No custom chronos profiles active.") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Italic, fontSize = 11, marginTop = 10 } });
                }
            }
            else
            {
                editorGui.AddChild(new Label("Editor Hooks not found in domain.") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Italic } });
            }
#else
            editorGui.AddChild(new Label("Editor Integration Offline (Standalone Build)") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Italic } });
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

            VisualElement root = builder.Build();

            // --- 4. TELEMETRY TICK (Asteroids Hangar Pattern) ---
            var telemetryBox = new VisualElement { style = { marginTop = 20, paddingTop = 15, borderTopWidth = 1, borderTopColor = Color.gray } };
            Label telemetryLabel = new Label("Initializing FSM Telemetry...") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold } };
            telemetryBox.Add(telemetryLabel);
            root.Add(telemetryBox);

            string tickGroup = "DashboardTelemetry";
            root.schedule.Execute(() =>
            {
                // This ensures FSM logic for the UI runs, bridging Editor tick mapping
                FSM_API.FSM_API.Interaction.Update(tickGroup);
                _uptimeTicks++;
                float seconds = _uptimeTicks * 0.016f;
                telemetryLabel.text = $"[SYS.TICK] Group: {tickGroup} | UI Ticks: {_uptimeTicks} | Uptime: {seconds:F1}s";

                float pulse = (Mathf.Sin(Time.realtimeSinceStartup * 4f) + 1f) / 2f;
                telemetryLabel.style.color = Color.Lerp(new Color(0.2f, 0.6f, 0.8f), Color.cyan, pulse);
            }).Every(16);

            return root;
        }

        // --- Helper for the Reflection ---
        private List<string> GetPrivateList(object instance, string fieldName)
        {
            if (instance == null) return new List<string>();
            var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                return field.GetValue(instance) as List<string> ?? new List<string>();
            }
            return new List<string>();
        }

        // --- Helper for the Editor Node Styles ---
        private VisualElement CreateLifecycleNode(string phaseName, List<string> assignedGroups, Color phaseColor)
        {
            var node = new VisualElement { style = { backgroundColor = new Color(0.1f, 0.1f, 0.12f), borderLeftWidth = 4, borderLeftColor = phaseColor, paddingLeft = 10, paddingRight = 10, paddingTop = 8, paddingBottom = 8, marginBottom = 5, borderTopLeftRadius = 4, borderBottomLeftRadius = 4, borderBottomRightRadius = 4, borderTopRightRadius = 4 } };
            node.Add(new Label(phaseName) { style = { color = phaseColor, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 13, marginBottom = 4 } });

            var chipContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };

            if (assignedGroups != null && assignedGroups.Count > 0)
            {
                foreach (var group in assignedGroups)
                {
                    chipContainer.Add(new Label(group) { style = { backgroundColor = new Color(0.2f, 0.2f, 0.25f), color = Color.white, fontSize = 11, paddingLeft = 6, paddingRight = 6, paddingTop = 2, paddingBottom = 2, marginRight = 4, marginBottom = 4, borderTopLeftRadius = 3, borderTopRightRadius = 3, borderBottomLeftRadius = 3, borderBottomRightRadius = 3 } });
                }
            }
            else
            {
                chipContainer.Add(new Label("unassigned") { style = { color = new Color(0.4f, 0.4f, 0.4f), fontSize = 11, } });
            }

            node.Add(chipContainer);
            return node;
        }

        // --- Extracted Card Generators ---
        private VisualElement CreateKpiCard(string title, string value, Color accent)
        {
            var card = new VisualElement { style = { flexGrow = 1, marginLeft = 5, marginRight = 5, paddingTop = 15, paddingBottom = 15, paddingLeft = 20, paddingRight = 20, backgroundColor = new Color(0.18f, 0.18f, 0.22f), borderTopWidth = 4, borderTopColor = accent } };
            card.Add(new Label(title) { style = { color = Color.gray, fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 8 } });
            card.Add(new Label(value) { style = { color = Color.white, fontSize = 32, unityFontStyleAndWeight = FontStyle.Bold } });
            return card;
        }

        private VisualElement CreateCategoryCard(string name, int count)
        {
            var card = new VisualElement { style = { width = 200, marginTop = 5, marginBottom = 5, marginRight = 10, paddingTop = 10, paddingBottom = 10, paddingLeft = 15, paddingRight = 15, backgroundColor = new Color(0.15f, 0.15f, 0.18f), borderLeftWidth = 3, borderLeftColor = GuiSkin.Active.PrimaryAccent, flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };
            card.Add(new Label(name.ToUpper()) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });
            card.Add(new Label(count.ToString()) { style = { color = GuiSkin.Active.PrimaryAccent, unityFontStyleAndWeight = FontStyle.Bold } });
            return card;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}