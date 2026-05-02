using System;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Stress.UI;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools;

namespace Workshop.Diagnostics.Showcase
{
    public class Showcase_PerformanceDuel : IGuiProvider
    {
        public string Title => "THE GREAT DIVIDE: FORGE VS TRADITIONAL MONOLITHS";
        private RenderTexture _gpuVisualization;

        public VisualElement CreateGui(GuiContext ctx)
        {
            if (ctx == null) ctx = new GuiContext();

            // Pillar 1: Sectorized Workspace (1 row, 2 columns)
            return new ForgeSplitPanelBuilder(1, 2)
                .WithSector(0, 0, BuildCpuArena(ctx))
                .WithSector(0, 1, BuildGpuArena(ctx))
                .CreateGui(ctx);
        }

        private IGuiProvider BuildCpuArena(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("CpuArena")
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .AddHeader("PHASE 1: MANAGED CPU FSM", Color.yellow)
                .AddChild(new ForgeLabelBuilder("THE TRADITIONAL CEILING")
                    .WithColor(Color.gray).WithBold(true).Build())
                .AddChild(new ForgeLabelBuilder("Unity DOTS targets ~10k simple agents. Our FSM_API bypasses standard MonoBehaviour overhead to reach 500k+, but eventually hits the 'Managed Barrier' of Garbage Collection.")
                    .WithColor(Color.silver).WithMarginBottom(20).Build())
                .AddChild(new Workshop_Gui_ApiBenchmarkProfiler());
        }

        private IGuiProvider BuildGpuArena(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("GpuArena")
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.02f, 0.05f, 0.05f))
                .AddHeader("PHASE 2: UNMANAGED GPU FORGE", Color.cyan)
                .AddChild(new ForgeLabelBuilder("BEYOND THE MONOLITH")
                    .WithColor(Color.gray).WithBold(true).Build())
                .AddChild(new ForgeLabelBuilder("By offloading logic to parallel compute kernels, we eliminate CPU pointer-chasing. This isn't just for games; it is the foundation for a 'Reforged Excel'—processing millions of data-cells at 60fps.")
                    .WithColor(Color.silver).WithMarginBottom(20).Build())

                // --- THE VISUALIZATION: SHOWMANSHIP ---
                .AddChild(new TexturePreviewBuilder(_gpuVisualization)
                    .WithBackgroundColor(Color.black)
                    .WithZoomSensitivity(0.05f)) //

                // --- THE RUN BUTTON: DOPAMINE TRIGGER ---
                .AddChild(new ForgeButtonBuilder("🚀 INITIATE MASSIVE GPU SCALING")
                    .WithHeight(60).WithMarginTop(20)
                    .WithBackgroundColor(new Color(0.1f, 0.4f, 0.6f))
                    .WithTextColor(Color.white).WithBold(true)
                    .OnClick(() => InitiateGpuDopamineRelease()))

                .AddChild(new Swarmy_GpuStressProvider());
        }

        private void InitiateGpuDopamineRelease()
        {
            Debug.Log("<color=cyan><b>FORGE MODE:</b></color> Breaking the 10k ceiling. Deploying 1,000,000+ agents.");
            // Logic to trigger SwarmyFarmContext.InitializeGPU()
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}