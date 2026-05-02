using System;
using System.Collections.Generic;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    [Serializable]
    public class CosmicGridData
    {
        public string Seed { get; set; } = "DeepSpace_01";
        public int GridExtents { get; set; } = 2;

        // THE BIG BANG EXPANSION DATA
        public long UniverseAgeYears { get; set; } = 0;
        public float CurrentRadius { get; set; } = 0.5f; // Starts as a tiny singularity
        public float ExpansionSpeed { get; set; } = 0.1f;
    }

    [Serializable]
    public class CosmicCellData
    {
        public Vector3Int GridCoordinates { get; set; }
        public float Entropy { get; set; } = 0f;
        public int ShipsTransitioningOut { get; set; } = 0;
        public List<SubCellData> SubCells { get; set; } = new List<SubCellData>();
    }

    [Serializable]
    public class SubCellData
    {
        public Vector3Int LocalCoordinates { get; set; }
        public float SubEntropy { get; set; } = 0f;
    }
}