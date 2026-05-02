using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Warlords
{
    public class MapPresenter2D_Tilemap : MonoBehaviour
    {
        [Header("References")]
        public Tilemap TerrainTilemap;
        public TileManager TileRegistry;

        public void BuildMap(WarlordsMapData mapData)
        {
            TerrainTilemap.ClearAllTiles();

            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    int index = y * mapData.Width + x;
                    TerrainType type = mapData.TerrainData[index];

                    // Fetch the Tile asset from our Manager
                    Tile tileToPlace = TileRegistry.GetTile(type);

                    if (tileToPlace != null)
                    {
                        // Tilemap coordinates match your grid (x, y, z)
                        Vector3Int cellPosition = new Vector3Int(x, y, 0);
                        TerrainTilemap.SetTile(cellPosition, tileToPlace);
                    }
                }
            }

            // Refresh to ensure any "Rule Tiles" (like Roads/Coasts) calculate neighbors
            TerrainTilemap.RefreshAllTiles();
        }
    }
}