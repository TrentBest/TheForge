using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheSingularityWorkshop.Armada2525.DataModels
{
    [Serializable]
    public class UniverseData
    {
        public string Seed { get; set; } = "Cosmos_Omega";
        public float Radius { get; set; } = 5000f; // Macro lightyears
        public List<GalaxyData> Galaxies { get; set; } = new List<GalaxyData>();
    }

    [Serializable]
    public class GalaxyData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "New Galaxy";
        public Vector3 UniversePosition { get; set; } // Where it sits in the macro universe
        public float Radius { get; set; } = 150f;
        public Color ThemeColor { get; set; } = Color.cyan;

        public int NumberOfArms { get; set; } = 2; // For the spiral generator
        public float ChaosFactor { get; set; } = 15f;

        public List<StarSystemData> StarSystems { get; set; } = new List<StarSystemData>();
    }

    [Serializable]
    public class StarSystemData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "Uncharted System";
        public Vector3 LocalPosition { get; set; } // Relative to the Galaxy center
        public Color StarColor { get; set; } = Color.yellow;
    }
}