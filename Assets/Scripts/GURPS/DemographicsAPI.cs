using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    public class DemographicsAPI : IGurpsApiProvider
    {
        private Guid _id = Guid.Parse("00000000-0000-0000-0000-000000001007");
        public Guid Id { get => _id; set => _id = value; }
        public string ModuleName => "Demographics Engine";
        public string Title => "POPULATION RESOLVER";

        private ComputeBuffer _populationBuffer;
        private ComputeShader _simShader;
        private DataWarehouse _warehouse;
        private int _populationSize = 100000; // 100k Citizens

        public DemographicsAPI() { _warehouse = new DataWarehouse(); Bind(_warehouse); }
        public DemographicsAPI(DataWarehouse warehouse) { Bind(warehouse); }

        public void Bind(DataWarehouse warehouse) => _warehouse = warehouse;

        public void Initialize()
        {
            // Allocate VRAM for the population
            int stride = sizeof(int) * 5; // 5 integers in the struct
            _populationBuffer = new ComputeBuffer(_populationSize, stride);

#if UNITY_EDITOR
            _simShader = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Shaders/GURPS_PopulationSim.compute");
#endif

            GeneratePopulation();
        }

        public void GeneratePopulation()
        {
            if (_simShader == null) return;
            int kernel = _simShader.FindKernel("CS_GeneratePopulation");
            _simShader.SetBuffer(kernel, "_PopulationBuffer", _populationBuffer);
            _simShader.SetInt("_Seed", UnityEngine.Random.Range(0, 100000));
            _simShader.Dispatch(kernel, Mathf.CeilToInt(_populationSize / 64f), 1, 1);
            ForgeLogger.Log($"[DURPS] GPU: {_populationSize:N0} citizens birthed via 3d6 resolution.");
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var gui = new GraphicalUserInterfaceBuilder(ModuleName)
                .WithPadding(20)
                .AddChild(new ForgeLabelBuilder("GALACTIC DEMOGRAPHICS").WithBold().WithColor(Color.cyan))
                .AddChild(new ForgeLabelBuilder($"ACTIVE CITIZENS: {_populationSize:N0}").WithColor(Color.gray))

                // Pillar 1: Reusing existing GURPS ProbabilityGraph
                .AddChild(new ProbabilityGraphBuilder("Attribute Distribution (ST/DX/IQ/HT)")
                    .WithMean(10.5f)
                    .WithStandardDeviation(2.95f)
                    .CreateGui(ctx))

                .AddSeparator(Color.cyan, 1)
                .AddButton("RE-GENERATE WORLD BIOLOGY", GeneratePopulation);

            return gui.Build();
        }

        public void SaveToCache() { /* GPU Buffers are transient; state keys stored in Warehouse */ }
        public void LoadFromCache() { /* Hydrate population settings */ }
        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}