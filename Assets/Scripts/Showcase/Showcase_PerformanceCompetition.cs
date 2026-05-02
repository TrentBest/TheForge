using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Stress.UI;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools;

namespace Workshop.Diagnostics.Showcase
{
    public class Showcase_PerformanceCompetition : IGuiProvider
    {
        public string Title => "THE GREAT DIVIDE: CPU VS GPU PERFORMANCE DUEL";

        // Shared Context for Telemetry
        private class DuelContext
        {
            public int CpuAgents = 0;
            public int GpuAgents = 0;
            public float CpuFps = 0;
            public float GpuFps = 0;
            public long CpuOps = 0;
            public long GpuOps = 0;
        }

        private DuelContext _duel = new DuelContext();
        private Workshop_Gui_ApiBenchmarkProfiler _cpuPhase; // Existing CPU logic
        private ComputeShader _gpuStressShader; // Assigned via Forge Registry
        private ComputeBuffer _gpuBuffer;

        public VisualElement CreateGui(GuiContext ctx)
        {
            if (ctx == null) ctx = new GuiContext();

            // Pillar 1: Using SplitPanelBuilder for the Competition View
            return new ForgeSplitPanelBuilder(1, 2)
                 .WithSector(0, 0, new Workshop_Gui_ApiBenchmarkProfiler()) // CPU Phase
                 .WithSector(0, 1, new Swarmy_GpuStressProvider()) // GPU Phase
                 .CreateGui(ctx);
        }

        private IGuiProvider BuildCpuArena(GuiContext ctx)
        {
            // Reuses the Baseline Profiler logic
            _cpuPhase = new Workshop_Gui_ApiBenchmarkProfiler();

            return new GraphicalUserInterfaceBuilder("CpuArena")
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .AddHeader("PHASE 1: MANAGED CPU FSM", Color.yellow)
                .AddChild(new ForgeLabelBuilder("BASELINE: UNITY DOTS ~10K AGENTS").WithColor(Color.gray).Build())
                .AddChild(_cpuPhase.CreateGui(ctx)) // Injecting existing CPU bench
                .OnBuild(ve => {
                    // Custom telemetry hook to sync with our DuelContext
                    ve.schedule.Execute(() => {
                        // Capture logic from the existing profiler for the duel summary
                    }).Every(100);
                });
        }

        private IGuiProvider BuildGpuArena(GuiContext ctx)
        {
            // Utilizes the Forge GPU Pattern established in Swarmy
            return new GraphicalUserInterfaceBuilder("GpuArena")
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.05f))
                .AddHeader("PHASE 2: UNMANAGED FORGE GPU", Color.cyan)
                .AddChild(new ForgeLabelBuilder("SCALING TARGET: 1,000,000+ AGENTS").WithColor(Color.gray).Build())
                // Use the established GPU Architect to show the FSM-per-pixel logic
                .AddChild(new ComputeShaderGuiBuilder(_gpuStressShader)
                    .WithSlider("Parallel Width", 64f, 1024f)
                    .WithBuffer("_AgentBuffer", _gpuBuffer)
                    .CreateGui(ctx));
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}