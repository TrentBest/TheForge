using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    public class IcosphereForgeContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "IcosphereForge";

        public int SubdivisionLevel { get; set; } = 2;
        public float LineThickness { get; set; } = 0.01f;

        // Smart Triggers
        public bool NeedsGeometryRebuild { get; set; } = true;
        public bool NeedsThicknessUpdate { get; set; } = false;

        public GameObject GeneratedPlanetPrefab { get; set; }

        // Cache the lines so we can edit them instantly without destroying them
        public List<LineRenderer> ActiveLines { get; set; } = new List<LineRenderer>();
    }
}