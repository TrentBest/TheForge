using System;
using System.Collections.Generic;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Warlords
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
            public Tile TileAsset; // The actual Unity Tile used by Tilemaps
        }

        [Serializable]
        public class POIVisual
        {
            public POIType Type;
            public Sprite Sprite;
        }

        void Awake() => IsValid = true;

        // HELPER: Returns the actual Tile asset (used for 2D/3D Tilemap rendering)
        public Tile GetTile(TerrainType type) => TerrainAssets.Find(x => x.Type == type)?.TileAsset;

        // HELPER: Returns the Sprite inside a Tile (used for UI Elements / Map Editor)
        public Sprite GetSprite(TerrainType type)
        {
            var visual = TerrainAssets.Find(x => x.Type == type);
            return visual?.TileAsset != null ? visual.TileAsset.sprite : null;
        }

        // HELPER: Returns the POI sprite (Cities, Ruins, etc.)
        public Sprite GetSprite(POIType type) => POIAssets.Find(x => x.Type == type)?.Sprite;
    }
}