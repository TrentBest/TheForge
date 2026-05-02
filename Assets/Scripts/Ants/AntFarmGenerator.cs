using System.IO;
using UnityEngine;
using Workshop.Core.Diagnostics;
using Workshop.Core.Math; // Using our deterministic Rng
// using UnityEditor; // RIPPED OUT. We run at runtime now.

namespace Assets.Scripts.Ants
{
    //public static class AntStates
    //{
    //    public const byte EmptyAir = 0;
    //    public const byte SandDirt = 1;
    //    public const byte Stone = 2;
    //    public const byte AntWandering = 4;
    //    public const byte AntCarrying = 6;
    //}

    public static class AntFarmGenerator
    {
        // Colors for visualization in the UI (Mapped later by the renderer)
        public static readonly Color32 ColorAir = new Color32(25, 25, 30, 255);
        public static readonly Color32 ColorSand = new Color32(194, 178, 128, 255);
        public static readonly Color32 ColorStone = new Color32(100, 100, 100, 255);
        public static readonly Color32 ColorAnt = new Color32(255, 0, 0, 255);

        // Optional Editor hook if you still want a button, but now it's abstracted!
#if UNITY_EDITOR
        [UnityEditor.MenuItem("The Forge/Generators/Ant Farm Environment (Binary)")]
        public static void GenerateDefaultFarmEditor()
        {
            GenerateFarmManifest(512, 512, "Assets/Editor/Sandbox/ProceduralAntFarm.bin", 42);
        }
#endif

        /// <summary>
        /// Generates the raw unmanaged state memory. Zero Unity overhead. Runs instantly.
        /// </summary>
        public static byte[] GenerateRawFarmData(int width, int height, uint seed)
        {
            byte[] gridData = new byte[width * height];

            for (int x = 0; x < width; x++)
            {
                // Organic Terrain Generation: Sine waves driven by the Seed instead of a flat 70% line
                float terrainHeightBase = height * 0.6f;
                float organicHills = Mathf.Sin(x * 0.05f + seed) * 20f + Mathf.Cos(x * 0.01f - seed) * 40f;
                int surfaceY = (int)(terrainHeightBase + organicHills);

                for (int y = 0; y < height; y++)
                {
                    int idx = x + (y * width);

                    if (y < surfaceY)
                    {
                        // 2% chance of Stone to create digging obstacles
                        gridData[idx] = Rng.GetFloat(idx, seed + 1) > 0.98f ? AntStates.Stone : AntStates.SandDirt;
                    }
                    else
                    {
                        gridData[idx] = AntStates.EmptyAir;
                    }

                    // Sprinkle ants EXACTLY on the surface line
                    if (y == surfaceY && Rng.GetFloat(idx, seed + 2) > 0.95f)
                    {
                        gridData[idx] = AntStates.AntWandering;
                    }
                }
            }

            return gridData;
        }

        /// <summary>
        /// Bakes the raw byte array directly to disk as a Binary Manifest.
        /// This skips Texture encoding and loads 1000x faster into the DataWarehouse.
        /// </summary>
        public static void GenerateFarmManifest(int width, int height, string absolutePath, uint seed)
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            byte[] rawGrid = GenerateRawFarmData(width, height, seed);

            // Blast the raw bytes to the hard drive
            File.WriteAllBytes(absolutePath, rawGrid);

            sw.Stop();
            ForgeLogger.Log($"[Manifest] Procedural Ant Farm generated at: {absolutePath} in {sw.Elapsed.TotalMilliseconds:F2}ms. Size: {rawGrid.Length / 1024f} KB.");

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// Converts the raw DataWarehouse bytes into a visual Texture2D ONLY when a screen needs to see it.
        /// Uses SetPixelData which is O(1) memory mapping, completely bypassing SetPixel loops.
        /// </summary>
        public static Texture2D CreateVisualizerTexture(byte[] rawGrid, int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            // Allocate visual color array
            Color32[] colors = new Color32[rawGrid.Length];

            for (int i = 0; i < rawGrid.Length; i++)
            {
                byte state = rawGrid[i];
                if (state == AntStates.EmptyAir) colors[i] = ColorAir;
                else if (state == AntStates.SandDirt) colors[i] = ColorSand;
                else if (state == AntStates.Stone) colors[i] = ColorStone;
                else if (state == AntStates.AntWandering || state == AntStates.AntCarrying) colors[i] = ColorAnt;
            }

            // O(1) Memory blast into the GPU texture
            tex.SetPixelData(colors, 0);
            tex.Apply(false, false);

            return tex;
        }
    }
}