using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.Swarmy
{
    public class SwarmyFarmContext : IGuiProvider
    {
        private ComputeBuffer _droneBuffer;
        private ComputeShader _simShader;
        private int _droneCount = 1000000; // Massively scaled swarm

        public string Title => "SWARM INTELLIGENCE FARM";

        public void InitializeGPU()
        {
            // Pillar 3: Voxel/Agent Memory Safety
            // Struct Layout: float3 pos(3), float3 vel(3), uint state(1), float battery(1), uint targetID(1), padding(1)
            // Total: 10 slots (32-bit units)
            int stride = sizeof(float) * 8 + sizeof(uint) * 2;
            _droneBuffer = new ComputeBuffer(_droneCount, stride);

            // Pillar 4: Ecosystem Arbitration
            // Register this buffer to the DataWarehouse so the Command Deck can observe it.
            // DataWarehouse.Instance.GetOrCreateShelf<SwarmDrone>("ActiveSwarm").SetGpuBuffer(_droneBuffer);
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Rule 8: Context-First Initialization
            if (ctx == null) ctx = new GuiContext();

            // Pillar 1: Fluent Builder Integration
            // Now successfully utilizes .WithBuffer and .WithSlider
            var builder = new ComputeShaderGuiBuilder(_simShader)
                .WithBuffer("_SwarmBuffer", _droneBuffer)
                .WithSlider("Attractor Strength", 0f, 10f)
                .WithSlider("Fractional Balance", 0f, 1f);

            var root = builder.CreateGui(ctx);

            // Rule 3: Use Reflection for Tooling
            var profiler = new ReflectiveGuiBuilder<SwarmyFarmContext>(this).CreateGui(ctx);
            root.Add(profiler);

            return root;
        }

        public void Cleanup()
        {
            // Rule 4: Graceful Teardowns
            _droneBuffer?.Release();
            _droneBuffer = null;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}