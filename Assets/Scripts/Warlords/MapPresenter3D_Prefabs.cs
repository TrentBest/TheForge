using System.Collections.Generic;
using UnityEngine;


namespace TheSingularityWorkshop.Warlords
{
    public class MapPresenter3D_Prefabs : MonoBehaviour, IMapPresenter
    {
        [Header("Tile Dimensions")]
        public float TileSize = 1.0f; // Distance between tile centers

        [Header("Prefab Dictionaries (Assigned in Inspector or via Registry)")]
        // In a real setup, these would be loaded via an Addressables registry or ScriptableObject
        public GameObject BasePlainsPrefab;
        public GameObject BaseWaterPrefab;
        public GameObject BaseForestPrefab;
        // .. etc ..

        // We need to keep track of instantiated prefabs so we can replace them when the editor changes a tile
        private GameObject[] _instantiatedTiles;
        private Transform _mapContainer;

        public void BuildMap(WarlordsMapData mapData)
        {
            ClearMap();

            _mapContainer = new GameObject("3D_Map_Container").transform;
            _mapContainer.SetParent(this.transform);

            _instantiatedTiles = new GameObject[mapData.Width * mapData.Height];

            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    UpdateTile(mapData, x, y);
                }
            }
        }

        public void UpdateTile(WarlordsMapData mapData, int x, int y)
        {
            int index = y * mapData.Width + x;
            TerrainType type = mapData.TerrainData[index];

            // 1. Destroy existing prefab at this location if it exists
            if (_instantiatedTiles[index] != null)
            {
                Destroy(_instantiatedTiles[index]);
            }

            // 2. Determine which specific prefab to use (Handling Combinations/Transitions)
            GameObject prefabToSpawn = DeterminePrefab(mapData, x, y, type);

            // 3. Instantiate and position
            if (prefabToSpawn != null)
            {
                Vector3 worldPos = new Vector3(x * TileSize, 0, y * TileSize);
                GameObject newTile = Instantiate(prefabToSpawn, worldPos, Quaternion.identity, _mapContainer);

                _instantiatedTiles[index] = newTile;
            }

            // Optional: If you update a tile, you usually need to tell its 4 neighbors 
            // to update their visuals too, so road connections or coastlines redraw correctly!
        }

        private GameObject DeterminePrefab(WarlordsMapData mapData, int x, int y, TerrainType type)
        {
            // Here is where your "combinations" logic lives.
            // For a basic implementation, just return the base prefab:
            switch (type)
            {
                case TerrainType.Plains: return BasePlainsPrefab;
                case TerrainType.OpenWater: return BaseWaterPrefab;
                case TerrainType.Forest: return BaseForestPrefab;
                // ..
                default: return BasePlainsPrefab;
            }

            // For advanced combinations (like Roads), you would check:
            // mapData.TerrainData[(y+1) * width + x] == TerrainType.Road (North neighbor)
            // and return a T-Junction prefab, Corner prefab, etc.
        }

        public void PlacePOI(PointOfInterest poi)
        {
            // Instantiate City/Ruin 3D models on top of the base terrain tiles
        }

        public void ClearMap()
        {
            if (_mapContainer != null)
            {
                Destroy(_mapContainer.gameObject);
            }
            _instantiatedTiles = null;
        }

        public Vector2Int GetGridPositionFromMouse(Vector2 mouseScreenPosition)
        {
            // For a 3D grid, we cast a ray from the camera to a flat mathematical plane
            Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);
            Plane mapPlane = new Plane(Vector3.up, Vector3.zero); // A flat plane at Y=0

            if (mapPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);

                // Convert world space back to grid coordinates
                int x = Mathf.RoundToInt(hitPoint.x / TileSize);
                int y = Mathf.RoundToInt(hitPoint.z / TileSize);

                return new Vector2Int(x, y);
            }

            return new Vector2Int(-1, -1); // Off-map
        }
    }
}