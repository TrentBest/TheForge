using UnityEditor;
using UnityEngine;
using TheSingularityWorkshop.Sandbox.Ants;

namespace TheSingularityWorkshop.Sandbox.Ants.Editor
{
    public static class AntFarmGenerator
    {
        // Colors for visualization in the UI
        public static readonly Color ColorAir = new Color(0.1f, 0.1f, 0.12f); // Dark background
        public static readonly Color ColorSand = new Color(194 / 255f, 178 / 255f, 128 / 255f); // Sand color
        public static readonly Color ColorAnt = Color.red;

        [MenuItem("The Forge/Generators/Ant Farm Environment")]
        public static void GenerateDefaultFarm()
        {
            int width = 512;
            int height = 512;
            string path = "Assets/Editor/Sandbox/ProceduralAntFarm.png";

            GenerateFarm(width, height, path);
        }

        public static void GenerateFarm(int width, int height, string assetPath)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // We generate an initial 'container' view:
                    // Sand at the bottom 70%, Air at the top 30%

                    byte state = AntStates.EmptyAir;
                    Color visualColor = ColorAir;

                    // Create the sand layers (Bottom 70%)
                    if (y < height * 0.7f)
                    {
                        state = AntStates.SandDirt;
                        visualColor = ColorSand;
                    }

                    // Sprinkle some initial ants in the air just above the sand
                    if (y == (int)(height * 0.7f) && Random.value > 0.95f)
                    {
                        state = AntStates.AntWandering;
                        visualColor = ColorAnt;
                    }

                    // PACK THE DATA: Put the FSM state byte into the Red channel!
                    texture.SetPixel(x, y, new Color(state / 255f, visualColor.g, visualColor.b, visualColor.a));
                }
            }

            // Optional: Draw a "glass pane" highlight for the container look
            // (You can visualize this in the visualizer script later)

            texture.Apply();

            // Save the asset
            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(assetPath, bytes);
            AssetDatabase.Refresh();

            Debug.Log($"Procedural Ant Farm texture generated at: {assetPath}. Packed FSM states are in the RED channel.");
        }
    }
}