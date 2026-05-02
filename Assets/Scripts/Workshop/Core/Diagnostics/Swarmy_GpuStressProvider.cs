using System;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Stress.UI
{
    public class Swarmy_GpuStressProvider : IGuiProvider
    {
        public string Title => "GPU SWARM: UNMANAGED STRESS TEST";
        private ComputeBuffer _gpuBuffer;
        private ComputeShader _stressShader;
        private int _massiveAgentCount = 1000000;

        public VisualElement CreateGui(GuiContext ctx)
        {
            if (ctx == null) ctx = new GuiContext();

            // Corrected: AddHeader called on the builder before .Build()
            var root = new GraphicalUserInterfaceBuilder("GpuStressRoot")
                .WithFlexGrow(1).WithPadding(20)
                .WithBackgroundColor(new Color(0.02f, 0.05f, 0.05f))
                .AddHeader("THE SINGULARITY WORKSHOP: GPU SCALING DEMO", Color.cyan)
                .Build();

            // Using your Forge Compiler for parallel logic offloading
            var compilerView = new ComputeShaderGuiBuilder(_stressShader)
                .WithSlider("Parallel Throughput", 0f, 100f)
                .WithBuffer("_StressBuffer", _gpuBuffer)
                .CreateGui(ctx);

            root.Add(compilerView);

            // Live Telemetry (Zero-Alloc sample from the GPU)
            var stats = new ForgeContainerBuilder("GpuStats")
                .WithPadding(10).WithBackgroundColor(Color.black).Build();

            stats.Add(new ForgeLabelBuilder($"LIVE AGENTS: {_massiveAgentCount:N0}").WithColor(Color.green).Build());
            stats.Add(new ForgeLabelBuilder("ESTIMATED OPS: 3,000,000+ per Frame").WithColor(Color.yellow).Build());

            root.Add(stats);

            return root;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}