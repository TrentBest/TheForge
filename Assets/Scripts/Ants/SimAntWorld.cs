using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Ants
{
    public class SimAntWorld
    {
        public int Width = 512;
        public int Height = 512;

        public SimSettings Settings;

        public byte[,] PheromoneR;
        public byte[,] PheromoneG; // Food Trails
        public byte[,] PheromoneB; // Home Trails
        public byte[,] ResourceGrid; // 0=Empty, 1=Food, 2=Water, 3=Wall

        public List<MacroAntContext> Swarm;

        public Texture2D DisplayTexture;
        public Color32[] PixelColors;

        public int TotalFoodStored = 0;
        public int TotalWaterStored = 0;

        public SimAntWorld()
        {
            Settings = new SimSettings();
            PheromoneR = new byte[Width, Height];
            PheromoneG = new byte[Width, Height];
            PheromoneB = new byte[Width, Height];
            ResourceGrid = new byte[Width, Height];
            PixelColors = new Color32[Width * Height];

            DisplayTexture = new Texture2D(Width, Height) { filterMode = FilterMode.Bilinear };
            Swarm = new List<MacroAntContext>();
        }
    }
}