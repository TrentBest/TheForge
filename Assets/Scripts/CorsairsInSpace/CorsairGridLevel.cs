using UnityEngine;

namespace Assets.Scripts.CorsairsInSpace
{
    public class CorsairGridLevel
    {
        public int LevelDepth;
        public int Width = 64;
        public int Height = 64;

        // 0 = Solid Rock, 1 = Mined Corridor, 2 = Generator Room, 3 = Barracks
        public byte[,] VoxelGrid;

        public Texture2D BlueprintTexture;
        public Color32[] PixelColors;

        public CorsairGridLevel(int depth, int width = 64, int height = 64)
        {
            LevelDepth = depth;
            Width = width;
            Height = height;

            VoxelGrid = new byte[Width, Height];
            PixelColors = new Color32[Width * Height];

            BlueprintTexture = new Texture2D(Width, Height) { filterMode = FilterMode.Point };

            // Initialize all cells to Rock (0)
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    VoxelGrid[x, y] = 0;
                }
            }
        }
    }
}