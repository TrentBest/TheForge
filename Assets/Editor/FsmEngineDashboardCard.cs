using Assets.Editor;
using System;
using System.Linq;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FsmEditorHooks;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Extensions;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;

namespace Singularity
{
    public class Workshop_Gui_FsmEngineDashboard : IGuiProvider
    {
        public string Title => "FSM Engine Dashboard";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var theme = GuiSkin.Active;

            var dashboard = new GraphicalUserInterfaceBuilder("Dashboard")
                .WithPadding(12)
                .WithAutoGrow()
                // 1. Live Telemetry
                .AddChild(new Label("ENGINE TELEMETRY").Bold().Color(theme.PrimaryAccent).FontSize(14))
                .AddChild(new ActionGuiProvider(c => {
                    var row = new VisualElement().Row().JustifyEnd();
                    row.Add(CreateLiveStat("DEFS", () => FSM_API.Internal.TotalFsmDefinitionCount.ToString()));
                    row.Add(CreateLiveStat("INST", () => FSM_API.Internal.TotalFsmHandleCount.ToString()));
                    return row;
                }))
                // 2. Processing Buckets
                .AddChild(new Label("PROCESSING BUCKETS").Margin(0, 0, 10, 2).FontSize(10).Color(Color.gray))
                .AddChild(new ActionGuiProvider(c => {
                    var scroll = new ScrollView { style = { height = 400 } };
                    scroll.schedule.Execute(() => RefreshBuckets(scroll)).Every(1000);
                    RefreshBuckets(scroll);
                    return scroll;
                }))
                // 3. Global Actions
                .AddChild(new ActionGuiProvider(c => {
                    return new Button(() => FSM_EditorIntegrationAdvanced.RegisterEditorGroup("Forge_Cluster_" + UnityEngine.Random.Range(10, 99), 30))
                    { text = "+ PROVISION BUCKET" }.ApplyProfile(GuiElementType.ButtonSecondary);
                }));

            return dashboard.Build().ApplyProfile(GuiElementType.Panel);
        }

        private void RefreshBuckets(VisualElement container)
        {
            container.Clear();
            foreach (var bucket in FSM_API.Internal.GetBuckets())
            {
                var row = new GraphicalUserInterfaceBuilder($"Bucket_{bucket.Key}")
                    .WithPadding(5)
                    .AddChild(new ActionGuiProvider(ctx => {
                        var header = new VisualElement().Row().JustifyEnd();
                        header.Add(new Label(bucket.Key).Bold().Color(Color.white).FlexGrow(1));
                        header.Add(new Label($"{bucket.Value.Count} FSMs").Color(GuiSkin.Active.PrimaryAccent));
                        return header;
                    }))
                    .AddChild(new Label($"{bucket.Value.Sum(s => s.Value.Instances.Count)} INSTANCES").FontSize(9).Color(Color.gray))
                    .Build();

                row.Border(1, new Color(1, 1, 1, 0.1f)).Margin(0, 0, 2, 2);
                container.Add(row);
            }
        }

        private VisualElement CreateLiveStat(string label, Func<string> val)
        {
            var l = new Label(val()).Bold().Color(Color.white);
            l.schedule.Execute(() => l.text = val()).Every(500);
            var r = new VisualElement().Row().Margin(8, 0, 0, 0);
            r.Add(new Label(label + ": ").FontSize(10).Color(Color.gray));
            r.Add(l);
            return r;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string p) { }
        public void FromUIDocument(string p) { }
    }
}