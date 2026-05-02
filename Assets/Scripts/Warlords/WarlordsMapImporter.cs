using System.IO;
using UnityEngine;


namespace Assets.Scripts.Warlords
{
    public class WarlordsMapImporter : MonoBehaviour
    {
        [Header("Source Map Image")]
        [Tooltip("Make sure the texture in Unity is set to 'Read/Write Enabled' and 'Point (no filter)'")]
        public Texture2D MapImage;

        [Header("Output Settings")]
        public string OutputFileName = "Illuria_BaseMap.json";

        [Header("Color to Terrain Mapping")]
        // Tweak these colors in the inspector to exactly match the hex/RGB values of the pixels in your image
        public Color ColorWater = new Color(0, 0, 1f);       // Blue
        public Color ColorPlains = new Color(0, 1f, 0);      // Green
        public Color ColorForest = new Color(0, 0.5f, 0);    // Dark Green
        public Color ColorHills = new Color(0.6f, 0.4f, 0);  // Brown
        public Color ColorMountains = new Color(0.5f, 0.5f, 0.5f); // Grey
        public Color ColorSwamp = new Color(0.5f, 0, 0.5f);  // Purple
        public Color ColorRoad = new Color(1f, 1f, 0);       // Yellow

        [ContextMenu("Generate JSON from Image")]
        public void GenerateMapData()
        {
            if (MapImage == null)
            {
                Debug.LogError("No map image assigned!");
                return;
            }

            int width = MapImage.width;
            int height = MapImage.height;

            WarlordsMapData newMap = new WarlordsMapData(width, height);
            newMap.MapName = "Illuria_Imported";

            // Unity reads pixels from bottom-left to top-right. 
            // If your map is upside down in-game, you may need to invert the Y axis loop here: (height - 1 - y)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = MapImage.GetPixel(x, y);
                    TerrainType terrain = GetTerrainFromColor(pixelColor);

                    int index = y * width + x;
                    newMap.TerrainData[index] = terrain;
                }
            }

            // Serialize and Save
            string json = JsonUtility.ToJson(newMap, true);
            string path = Path.Combine(Application.dataPath, OutputFileName);

            File.WriteAllText(path, json);
            Debug.Log($"Map data generated successfully! Saved to: {path}");
        }

        private TerrainType GetTerrainFromColor(Color pixelColor)
        {
            // We use a distance check because image compression often alters exact hex values slightly
            TerrainType closestTerrain = TerrainType.Plains;
            float closestDistance = float.MaxValue;

            CheckColorDistance(pixelColor, ColorWater, TerrainType.OpenWater, ref closestDistance, ref closestTerrain);
            CheckColorDistance(pixelColor, ColorPlains, TerrainType.Plains, ref closestDistance, ref closestTerrain);
            CheckColorDistance(pixelColor, ColorForest, TerrainType.Forest, ref closestDistance, ref closestTerrain);
            CheckColorDistance(pixelColor, ColorHills, TerrainType.Hills, ref closestDistance, ref closestTerrain);
            CheckColorDistance(pixelColor, ColorMountains, TerrainType.Mountains, ref closestDistance, ref closestTerrain);
            CheckColorDistance(pixelColor, ColorSwamp, TerrainType.Swamp, ref closestDistance, ref closestTerrain);
            CheckColorDistance(pixelColor, ColorRoad, TerrainType.Road, ref closestDistance, ref closestTerrain);

            return closestTerrain;
        }

        private void CheckColorDistance(Color target, Color checkColor, TerrainType type, ref float closestDist, ref TerrainType closestType)
        {
            // Calculate color distance
            float rDiff = target.r - checkColor.r;
            float gDiff = target.g - checkColor.g;
            float bDiff = target.b - checkColor.b;

            float distance = (rDiff * rDiff) + (gDiff * gDiff) + (bDiff * bDiff);

            if (distance < closestDist)
            {
                closestDist = distance;
                closestType = type;
            }
        }
    }
}