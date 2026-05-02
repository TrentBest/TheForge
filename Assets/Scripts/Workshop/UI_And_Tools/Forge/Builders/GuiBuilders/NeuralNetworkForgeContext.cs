using Hermit.Core;
using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Core;
using Workshop.UI_And_Tools.Forge.Hermit.Core;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    // --- 1. EXPANDED DATA ARCHITECTURE ---

    public class NeuralNetworkForgeContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "New Neural Network";

        public HermitBrain LiveBrain { get; set; }


        // Simulation State
        public bool IsPlaying { get; set; } = false;
        public float SimulationSpeed { get; set; } = 1.0f;
        public int CurrentPropagationLayer { get; set; } = 0;
        public bool StepRequested { get; set; } = false;
        // Architecture (The "Regions")
        public List<NeuralRegion> Regions { get; set; } = new List<NeuralRegion>();

        // Dynamic Inputs
        public List<NeuralInputDef> Inputs { get; set; } = new List<NeuralInputDef>();

        // Selection State for Inspector
        public object SelectedObject { get; set; } // Can be a Region, a Node, or a Synapse

        // Visualization
        public HashSet<int> FiringSynapses { get; set; } = new HashSet<int>();
        public List<Tuple<int, int>> Synapses { get; set; } = new List<Tuple<int, int>>();
        public List<Vector3> NodePositions { get; set; } = new List<Vector3>();
    }
}