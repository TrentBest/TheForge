using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Diagnostics.Deployment
{
    public class Showcase_BenchmarkInstaller : IGuiProvider
    {
        public string Title => "SINGULARITY BENCHMARK: LOCAL INSTALLER";

        public VisualElement CreateGui(GuiContext ctx)
        {
            if (ctx == null) ctx = new GuiContext();

            // Using your ForgeSplitPanelBuilder for a Sectorized Workspace
            return new ForgeSplitPanelBuilder(sidebarWidth: 320, side: Side.Left)
                .WithSidebar(BuildInfoSidebar(ctx))
                .WithMain(BuildDeploymentMain(ctx))
                .CreateGui(ctx);
        }

        private IGuiProvider BuildInfoSidebar(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("InstallerSidebar")
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .AddHeader("EVALUATION PROTOCOL", Color.cyan)
                .AddChild(new ForgeLabelBuilder("BENCHMARK: Baseline Console").WithColor(Color.gray).Build());
        }

        private IGuiProvider BuildDeploymentMain(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("DeploymentMain")
                .WithPadding(40)
                .AddHeader("CLIENT DEVICE EVALUATION", Color.white)
                .AddChild(new ForgeButtonBuilder("🚀 DEPLOY LOCAL BENCHMARK")
                    .WithHeight(60)
                    .WithBackgroundColor(new Color(0.15f, 0.35f, 0.15f))
                    .OnClick(() => {
                        string path = Path.Combine(Application.streamingAssetsPath, "Benchmarks/Stress.exe");
                        if (File.Exists(path)) Process.Start(path);
                    }));
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}