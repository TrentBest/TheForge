using System.Collections.Generic;
using System;


namespace Assets.Scripts.Warlords
{
    [Serializable]
    public class WarlordsMapData
    {
        public string MapName;
        public int Width;
        public int Height;
        public TerrainType[] TerrainData;
        public List<PointOfInterest> PointsOfInterest = new List<PointOfInterest>();

        public WarlordsMapData(int width, int height)
        {
            Width = width;
            Height = height;
            TerrainData = new TerrainType[width * height];
        }
    }
}