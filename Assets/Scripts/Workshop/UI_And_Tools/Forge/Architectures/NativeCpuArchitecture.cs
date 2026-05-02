using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Architectures
{
    public class NativeCpuArchitecture : IComputeArchitecture
    {
        public string Id => "NativeCPU";
        public string DisplayName => "Native Threading (Single Core)";
        public string Description => "Ideal for: High-complexity, branching logic, UI flows, and single-actor AI.";
        public Color ThemeColor => Color.cyan;

        public VisualElement BuildEditorUI()
        {
            // PILLAR 1: Using ForgeContainerBuilder instead of raw VisualElement
            return new ForgeContainerBuilder("CpuPanel")
                .WithPadding(10)
                .AddChild(new ForgeButtonBuilder("EDIT STATES & TRANSITIONS")
                    .WithHeight(50)
                    .WithBackgroundColor(new Color(0.2f, 0.4f, 0.6f))
                    .OnClick(() => Debug.Log("Opening CPU State Editor..")))
                .Build();
        }
    }

    public class BurstJobsArchitecture : IComputeArchitecture
    {
        public string Id => "BurstJobs";
        public string DisplayName => "Burst Compiled Jobs (Multi-Core)";
        public string Description => "Ideal for: Tens of thousands of RTS Units/Boids.";
        public Color ThemeColor => new Color(1f, 0.6f, 0f);

        public VisualElement BuildEditorUI()
        {
            return new ForgeContainerBuilder("BurstPanel")
                .WithPadding(10)
                .AddChild(new ForgeButtonBuilder("DEFINE JOB STRUCT MEMORY")
                    .WithHeight(40).WithMarginBottom(10)
                    .WithBackgroundColor(new Color(0.6f, 0.4f, 0.1f)))
                .AddChild(new ForgeButtonBuilder("DEFINE BURST RULES")
                    .WithHeight(40)
                    .WithBackgroundColor(new Color(0.6f, 0.4f, 0.1f)))
                .Build();
        }
    }

    public class GpuComputeArchitecture : IComputeArchitecture
    {
        public string Id => "GPUCompute";
        public string DisplayName => "Massively Parallel (GPU Compute)";
        public string Description => "Ideal for: Millions of entities, operations directly on VRAM.";
        public Color ThemeColor => Color.red;

        public VisualElement BuildEditorUI()
        {
            // 1. Resolve Asset (Pillar 8: Context-First Initialization)
            ComputeShader simShader = null;
#if UNITY_EDITOR
            simShader = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Shaders/Compute/SwarmSimulation.compute");
#endif

            // 2. Build the UI using the updated Builder
            var shaderBuilder = new ComputeShaderGuiBuilder(simShader)
                .BindUniform("BufferState", "SYNC_ACTIVE")
                .BindUniform("VRAM_Usage", "0.45 GB");

            var shaderUI = shaderBuilder.CreateGui(new GuiContext());

            // 3. Wrap in a standard Forge Container
            return new ForgeContainerBuilder("GpuPanel")
                .WithBorderTopColor(Color.gray)
                .WithBorderTopWidth(1)
                .WithMarginTop(10)
                .AddChild(shaderUI)
                .Build();
        }
    }
}