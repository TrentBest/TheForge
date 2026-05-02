using System;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Systems.MicroPackages
{
    [Serializable]
    public class DiegeticLayoutManifest
    {
        public string TargetToolId;                 // Which GUI Provider this applies to

        [Header("Spatial Positioning")]
        public SpatialOrbit PreferredOrbit = SpatialOrbit.Aesthetics;
        public float ElevationOffset = 1.2f;        // 1.2m = Chest height, 0.8m = Desk height

        [Header("Physical Hardware")]
        public Vector2 PanelResolution = new Vector2(800, 600); // The virtual canvas size
        public float PhysicalScale = 0.002f;        // How large the UI is in the 3D world

        [Header("Behavioral Rules")]
        public bool IsSummonable = true;            // Can the user pull it to their hands?
        public bool BillboardsToUser = true;        // Does it constantly rotate to face the creator?
        public bool IsPinned = false;               // Does it lock in place once spawned?
    }

    public enum SpatialOrbit
    {
        Crucible = 0,    // 0m
        Ontology = 1,    // 3m
        Aesthetics = 2,  // 6m
        Staging = 3,     // 10m
        Output = 4       // 15m
    }
}