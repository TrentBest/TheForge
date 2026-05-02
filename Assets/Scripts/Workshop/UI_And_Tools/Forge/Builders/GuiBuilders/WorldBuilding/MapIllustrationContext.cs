using System.Collections.Generic;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    /// <summary>
    /// Data required to derive an in-world map from world state.
    /// </summary>
    public class MapIllustrationContext
    {
        // Source data for extraction
        public PlanetContext WorldData;

        // The "Lens" through which we see the world
        public int TechnologyLevel = 3; // Default to Medieval/Low-Fantasy

        // Coverage and frontiers
        public Rect EffectiveCoverage; // What part of the world is known?
        public List<ExplorationData> ExpeditionHistory;
    }
}