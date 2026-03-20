using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders.WorldBuilding
{
    [Serializable]
    public class WorldSimulationData
    {
        public string Seed { get; set; } = "Genesis_01";
        public long CurrentYear { get; set; } = 0;

        public float SurfaceHeat { get; set; } = 5000f;
        public float AtmosphericPressure { get; set; } = 0f;
        public float GreenhouseGases { get; set; } = 0f;
        public float CrustalTension { get; set; } = 0f;

        public List<ContinentData> Continents { get; set; } = new List<ContinentData>();

        // Non-Serialized transient data
        [NonSerialized] public Mesh PlanetMesh;
    }

    [Serializable]
    public class StarSystemData
    {
        public string SystemSeed { get; set; } = "Sol_01";
        public StarData LocalStar { get; set; } = new StarData();
        public List<PlanetData> Planets { get; set; } = new List<PlanetData>();
    }

    [Serializable]
    public class StarData
    {
        public string Name { get; set; } = "Sol Prime";
        public string StarClass { get; set; } = "Yellow Dwarf";
        public float Radius { get; set; } = 15f;
        public float LightIntensity { get; set; } = 1.5f;
    }

    [Serializable]
    public class PlanetData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "Terra";
        public float OrbitDistance { get; set; } = 60f; // Distance from the Star
        public float Radius { get; set; } = 1f;

        // The planet owns its topology and its satellites
        public List<ContinentData> Continents { get; set; } = new List<ContinentData>();
        public List<MoonData> Moons { get; set; } = new List<MoonData>();

        [NonSerialized] public Mesh PlanetMesh;
    }

    [Serializable]
    public class MoonData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "Luna";
        public float Radius { get; set; } = 0.2f;
        public float OrbitDistance { get; set; } = 3f; // Distance from the Planet
    }
}