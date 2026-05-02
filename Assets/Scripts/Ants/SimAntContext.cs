using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Ants
{
    // --- 1. MACRO ENVIRONMENT CONTEXT ---
    public class SimAntContext
    {
        public int Width = 256;
        public int Height = 256;

        // Pheromone Channels
        public byte[,] PheromoneR; // Danger
        public byte[,] PheromoneG; // Food
        public byte[,] PheromoneB; // Home

        // Agent Positions
        public List<Vector2Int> Ants;

        public Texture2D DisplayTexture;
        public Color32[] PixelColors;

        public SimAntContext()
        {
            PheromoneR = new byte[Width, Height];
            PheromoneG = new byte[Width, Height];
            PheromoneB = new byte[Width, Height];
            PixelColors = new Color32[Width * Height];

            // Bilinear filtering makes the pheromones look like smooth clouds rather than sharp pixels
            DisplayTexture = new Texture2D(Width, Height) { filterMode = FilterMode.Bilinear };

            // Spawn 100 ants in the center
            Ants = new List<Vector2Int>();
            for (int i = 0; i < 100; i++)
            {
                Ants.Add(new Vector2Int(Width / 2, Height / 2));
            }
        }
    }
}