using TheSingularityWorkshop.FSM_API;
using System.Collections.Generic;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    public class SubCellContext : IStateContext
    {
        public string Name { get; set; }
        public bool IsValid { get; set; } = true;

        public SubCellData Data { get; set; }
        public MeshRenderer VisualRenderer { get; set; }

        // EXPANSION
        public CosmicGridData SharedGridConfig { get; set; }
        public Vector3 ExactWorldPosition { get; set; }
        public bool IsInsideUniverse { get; set; } = false;

        // OBSERVER LOD THROTTLING
        public int DistanceToObserver { get; set; } = 999;
        public int LodThrottle { get; set; } = 60; // Frames to skip between updates
        public int FrameCounter { get; set; } = 0;

        public bool HasMatter { get; set; } = false;
    }

    public class UniverseBoundaryContext : IStateContext
    {
        public string Name { get; set; } = "BoundaryManager";
        public bool IsValid { get; set; } = true;
        public CosmicGridData GridConfig { get; set; } = new CosmicGridData();
        public Transform UniverseSphereVisual { get; set; }
    }

    // The Conductor now manages the Player/Observer!
    public class CosmicObserverContext : IStateContext
    {
        public string Name { get; set; } = "CosmicObserver";
        public bool IsValid { get; set; } = true;

        public Vector3Int ObserverGridPosition { get; set; } = new Vector3Int(0, 0, 0);
        public Transform ObserverVisual { get; set; }

        public List<SubCellContext> AllCells { get; set; } = new List<SubCellContext>();

        // Materials for the Twinkling processing effect
        public Material MatPlayer { get; set; }
        public Material MatAdjacent { get; set; }
        public Material MatDistant { get; set; }
        public Material MatInvisible { get; set; }
    }
}