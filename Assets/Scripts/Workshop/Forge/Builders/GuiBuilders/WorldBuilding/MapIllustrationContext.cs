using System.Collections.Generic;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders.WorldBuilding;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// Data required to derive an in-world map from world state.
    /// </summary>
    public class MapIllustrationContext
    {
        // Source data for extraction
        public WorldSimulationData WorldData;

        // The "Lens" through which we see the world
        public int TechnologyLevel = 3; // Default to Medieval/Low-Fantasy

        // Coverage and frontiers
        public Rect EffectiveCoverage; // What part of the world is known?
        public List<ExplorationData> ExpeditionHistory;
    }
}