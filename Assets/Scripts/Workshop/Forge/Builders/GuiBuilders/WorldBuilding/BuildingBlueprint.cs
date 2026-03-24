using System;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.WorldBuilding
{
    [Serializable]
    public class BuildingBlueprint
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "Standard Structure";

        // --- Spawning Rules (The Query Filters) ---
        public byte RequiredZoneId { get; set; } = 1; // Links to UrbanZoneType.Id
        public int MinTechLevel { get; set; } = 0;
        public int MaxTechLevel { get; set; } = 12;

        // Density dictates if this spawns in the suburbs (0.1) or downtown (0.9)
        public float MinUrbanDensity { get; set; } = 0.0f;
        public float MaxUrbanDensity { get; set; } = 1.0f;

        // --- Physical Manifestation ---
        // How much physical space does this building take up?
        public Vector2 FootprintDimensions { get; set; } = new Vector2(20f, 20f);
        public float BaseHeight { get; set; } = 10f;
        public bool ScaleHeightWithDensity { get; set; } = true;

        // A reference to the actual 3D visual (Prefab path, Addressable ID, or procedural generation seed)
        public string VisualResourceKey { get; set; } = "Assets/Prefabs/Buildings/DefaultBlock.prefab";
    }
}