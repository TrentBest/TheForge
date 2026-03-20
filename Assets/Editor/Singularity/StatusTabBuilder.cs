#if UNITY_EDITOR
using Assets.Editor.Singularity;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

public class StatusTabBuilder : IGuiProvider
{
    public string TabName => "Status";
    public string TabIcon => "📊";

    public string Title => TabName;

    public VisualElement CreateGui(GuiContext ctx)
    {
        // We use TwoPaneSplitView (native Unity control) which isn't directly wrapped 
        // by the generic Builder yet, so we inject it as a child.
        var root = new VisualElement { style = { flexGrow = 1 } };

        // Vertical Split: Top is Runtime Stats, Bottom is Editor/API State
        var splitView = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Vertical);

        // --- TOP PANE: Runtime Benchmark & Analytics ---
        var runtimePanel = new GraphicalUserInterfaceBuilder("RuntimeMetrics")
            .WithTitle("RUNTIME PERFORMANCE METRICS")
            .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
            .WithPadding(10)
            .WithScrollable(true)
            .AddChild(BuildRuntimeStats());

        // --- BOTTOM PANE: Editor State & Project Overview ---
        var editorPanel = new GraphicalUserInterfaceBuilder("EditorState")
            .WithTitle("EDITOR ENGINE STATE")
            .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
            .WithPadding(10)
            .WithScrollable(true)
            .AddChild(BuildEditorState());

        splitView.Add(runtimePanel.Build());
        splitView.Add(editorPanel.Build());

        root.Add(splitView);
        return root;
    }

    // This section is for data captured during Play Mode
    private IGuiProvider BuildRuntimeStats()
    {
        return new GraphicalUserInterfaceBuilder("StatsBody")
            .AddChild(ctx =>
            {
                if (!Application.isPlaying)
                {
                    return new Label("Waiting for Play Mode to capture runtime metrics..")
                    { style = { opacity = 0.5f, unityTextAlign = TextAnchor.MiddleCenter, paddingTop = 20 } };
                }

                // Mockup of what your "Injected Timer Funcs" will populate
                var grid = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };

                grid.Add(CreateMetricCard("Max Handle Count", "204", "Within limits (Max: 1000)"));
                grid.Add(CreateMetricCard("Avg Update (ms)", "0.04ms", "Excellent"));
                grid.Add(CreateMetricCard("GC Alloc/Frame", "0KB", "Zero-Alloc Active"));
                grid.Add(CreateMetricCard("Active Groups", "4", "Physics, AI, UI, Meta"));

                return grid;
            });
    }

    // This section shows what is currently registered in the API
    private IGuiProvider BuildEditorState()
    {
        return new GraphicalUserInterfaceBuilder("EditorBody")
            .AddChild(ctx =>
            {
                var container = new VisualElement();

                var groups = FSM_API.Internal.GetProcessingGroupNames();
                foreach (var g in groups)
                {
                    var groupBox = new VisualElement { style = { marginBottom = 10,  backgroundColor = new Color(0, 0, 0, 0.2f) } };
                    groupBox.Add(new Label($"Group: {g}") { style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.cyan } });

                    var buckets = FSM_API.Internal.GetBuckets();
                    if (buckets.TryGetValue(g, out var fsmBuckets))
                    {
                        var row = new ScrollView(ScrollViewMode.Horizontal) { style = { flexDirection = FlexDirection.Row, height = 80, marginTop = 5 } };
                        foreach (var bucket in fsmBuckets)
                        {
                            var card = new VisualElement
                            {
                                style = {
                                    width = 120, marginRight = 5, 
                                    backgroundColor = new Color(0.2f,0.2f,0.2f),
                                    borderLeftWidth = 2, borderLeftColor = Color.green
                                }
                            };
                            card.Add(new Label(bucket.Value.Definition.Name) { style = { fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold } });
                            card.Add(new Label($"Instances: {bucket.Value.Instances.Count}") { style = { fontSize = 10, opacity = 0.7f } });
                            row.Add(card);
                        }
                        groupBox.Add(row);
                    }
                    container.Add(groupBox);
                }
                return container;
            });
    }

    private VisualElement CreateMetricCard(string title, string value, string note)
    {
        var card = new VisualElement
        {
            style = {
                width = 150, height = 80, 
                backgroundColor = new Color(0.2f, 0.2f, 0.22f),
                borderLeftWidth = 3, borderLeftColor = new Color(1f, 0.6f, 0f)
            }
        };
        card.Add(new Label(title) { style = { fontSize = 10, color = Color.gray } });
        card.Add(new Label(value) { style = { fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, color = Color.white } });
        card.Add(new Label(note) { style = { fontSize = 9, color = Color.green, paddingTop = 5 } });
        return card;
    }

    public Action<VisualElement> GetGuiBuilder()
    {
        throw new NotImplementedException();
    }

    public void ToUIDocument(string assetPath)
    {
        throw new NotImplementedException();
    }

    public void FromUIDocument(string assetPath)
    {
        throw new NotImplementedException();
    }
}
#endif