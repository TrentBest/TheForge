using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    public enum BiomeType { Barren, Desert, Tundra, Ocean, Terran, Volcanic, Radiated }

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct StarSystemGpuData
    {
        public Color32 PackedFsmStates; // 4 FSMs per system
        public int Id;
        public int IsHomeworld; // Using int (0 or 1) as bools can cause alignment issues on GPUs
        public Vector3 GridCoordinate;
    }

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct StarGpuData
    {
        public Color32 PackedFsmStates; // 4 FSMs per star
        public float Radius;
        public float LightIntensity;
    }

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct PlanetGpuData
    {
        public Color32 PackedFsmStates; // 4 FSMs per planet
        public float OrbitDistance;
        public float Radius;
        public int BiomeTypeIndex; // Maps to BiomeType enum
        public int Richness;
    }

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct MoonGpuData
    {
        public Color32 PackedFsmStates; // 4 FSMs per moon
        public float Radius;
        public float OrbitDistance;
    }
}