using System.Collections.Generic;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    [System.Serializable]
    public class ExplorationData
    {
        // The specific region this data pertains to
        public Rect Region;

        // When was this mapped? (In-game time)
        public float Timestamp;

        // The TL of the expedition that gathered this data
        public int RecordedTechnologyLevel;

        // List of 'Notable' features found (filtered by TL)
        public List<string> DiscoveredLandmarks = new List<string>();

        // Percentage of 'Certainty' (Fog of War factor)
        public float MappingAccuracy = 1.0f;
    }
}