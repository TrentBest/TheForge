using UnityEngine;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class NeuralRegion
    {
        public string RegionName { get; set; }
        public int NodeCount { get; set; }
        public Color RegionColor { get; set; } = Color.white;
        // In your FSM architecture, this could map to a specific Processing Group or Sub-State Machine
    }
}