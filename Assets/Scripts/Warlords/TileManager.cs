using System;
using System.Collections.Generic;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Warlords
{
    public class TileManager : MonoBehaviour, IStateContext
    {
        public bool IsValid { get; set; } = false;
        public string Name { get; set; } = "Tile Manager";

        [Header("Mapping Assets")]
        public List<TerrainVisual> TerrainAssets = new List<TerrainVisual>();
        public List<POIVisual> POIAssets = new List<POIVisual>();

        [Serializable]
        public class TerrainVisual
        {
            public TerrainType Type;
            public Sprite Sprite;
        }

        [Serializable]
        public class POIVisual
        {
            public POIType Type;
            public Sprite Sprite;
        }

        void Awake()
        {
            IsValid = true;
        }

        // Helper methods for the Editor to call
        public Sprite GetSprite(TerrainType type) => TerrainAssets.Find(x => x.Type == type)?.Sprite;
        public Sprite GetSprite(POIType type) => POIAssets.Find(x => x.Type == type)?.Sprite;
    }
}