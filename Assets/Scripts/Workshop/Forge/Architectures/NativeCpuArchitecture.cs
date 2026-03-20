using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.Sandbox.Builders;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Architectures
{
    public class NativeCpuArchitecture : IComputeArchitecture
    {
        public string Id => "NativeCPU";
        public string DisplayName => "Native Threading (Single Core)";
        public string Description => "Ideal for: High-complexity, branching logic, UI flows, network state, and single-actor AI.";
        public Color ThemeColor => Color.cyan;

        public VisualElement BuildEditorUI()
        {
            var panel = new VisualElement();
            panel.Add(new Button(() => Debug.Log("Opening CPU State Editor..")) { text = "EDIT STATES & TRANSITIONS", style = { height = 50, backgroundColor = new Color(0.2f, 0.4f, 0.6f) } });
            return panel;
        }
    }

    public class BurstJobsArchitecture : IComputeArchitecture
    {
        public string Id => "BurstJobs";
        public string DisplayName => "Burst Compiled Jobs (Multi-Core)";
        public string Description => "Ideal for: Tens of thousands of complex entities (RTS Units, Boids). Uses C# Structs and Unity's Job System to max out every CPU core.";
        public Color ThemeColor => new Color(1f, 0.6f, 0f);

        public VisualElement BuildEditorUI()
        {
            var panel = new VisualElement();
            panel.Add(new Button(() => Debug.Log("Opening Job Struct Compiler..")) { text = "DEFINE JOB STRUCT MEMORY", style = { height = 40, backgroundColor = new Color(0.6f, 0.4f, 0.1f), marginBottom = 10 } });
            panel.Add(new Button(() => Debug.Log("Opening Burst Rule Editor..")) { text = "DEFINE BURST-COMPATIBLE RULES", style = { height = 40, backgroundColor = new Color(0.6f, 0.4f, 0.1f) } });
            return panel;
        }
    }

    public class GpuComputeArchitecture : IComputeArchitecture
    {
        public string Id => "GPUCompute";
        public string DisplayName => "Massively Parallel (GPU Compute)";
        public string Description => "Ideal for: Millions of entities, cellular automata, particle systems, and massive-scale environmental rules. Operates directly on VRAM.";
        public Color ThemeColor => Color.red;

        public VisualElement BuildEditorUI()
        {
            var panel = new VisualElement();

            // 1. Instantiate the Compute Shader Builder
            var shaderBuilder = new ComputeShaderGuiBuilder();

            // 2. Ask it for its UI and inject it directly into this plugin's panel!
            // We pass a new GuiContext, but you can pass down a master context if needed.
            var shaderUI = shaderBuilder.CreateGui(new GuiContext());

            // Add a little styling buffer
            shaderUI.style.marginTop = 10;
            shaderUI.style.borderTopWidth = 1;
            shaderUI.style.borderTopColor = Color.gray;

            panel.Add(shaderUI);

            return panel;
        }
    }
}