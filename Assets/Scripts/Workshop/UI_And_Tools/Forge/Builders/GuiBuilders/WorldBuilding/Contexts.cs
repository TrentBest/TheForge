using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    public class StarSystemContext : IStateContext
    {
        // --- IStateContext ---
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "Uncharted System";

        // --- Core Identity ---
        public string SystemSeed { get; set; } = "Sol_01";

        // --- HEAVY MANAGED DATA (The Lore & 4X Layer) ---
        public string ControllingEmpire { get; set; } = "Unclaimed Space";
        public string StrategicValue { get; set; } = "Unknown";
        public string LoreDescription { get; set; } = "A quiet system on the edge of known space.";
        public List<string> DiscoveredAnomalies { get; set; } = new List<string>();
        public List<string> TradeRoutes { get; set; } = new List<string>();
        public List<string> SystemTags { get; set; } = new List<string>();

        // --- Hierarchical References ---
        public StarContext LocalStar { get; set; }
        public List<PlanetContext> Planets { get; set; } = new List<PlanetContext>();

        // --- GPU Bridge ---
        private readonly int _index;
        private readonly StarSystemGpuData[] _gpuArray;

        public StarSystemContext(int index, StarSystemGpuData[] gpuArray)
        {
            _index = index;
            _gpuArray = gpuArray;
        }

        // --- Hot Data Translators ---
        public int Id { get => _gpuArray[_index].Id; set => _gpuArray[_index].Id = value; }
        public bool IsHomeworld { get => _gpuArray[_index].IsHomeworld == 1; set => _gpuArray[_index].IsHomeworld = value ? 1 : 0; }
        public Vector3 GridCoordinate { get => _gpuArray[_index].GridCoordinate; set => _gpuArray[_index].GridCoordinate = value; }

        // System-Level FSMs
        public byte TradeFsm { get => _gpuArray[_index].PackedFsmStates.r; set { var tmp = _gpuArray[_index]; tmp.PackedFsmStates.r = value; _gpuArray[_index] = tmp; } }
        public byte HazardFsm { get => _gpuArray[_index].PackedFsmStates.g; set { var tmp = _gpuArray[_index]; tmp.PackedFsmStates.g = value; _gpuArray[_index] = tmp; } }
    }

    public class StarContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "Sol Prime";

        // --- HEAVY MANAGED DATA (The Lore & 4X Layer) ---
        public string SpectralClass { get; set; } = "G2V";
        public string EvolutionaryStage { get; set; } = "Main Sequence";
        public string LoreDescription { get; set; } = "A warm, stable yellow dwarf.";
        public List<string> AnomalousReadings { get; set; } = new List<string>();

        private readonly int _index;
        private readonly StarGpuData[] _gpuArray;

        public StarContext(int index, StarGpuData[] gpuArray)
        {
            _index = index;
            _gpuArray = gpuArray;
        }

        // FSMs for the Star (Solar Flares, Age/Expansion, etc)
        public byte StellarActivityFsm { get => _gpuArray[_index].PackedFsmStates.r; set { var tmp = _gpuArray[_index]; tmp.PackedFsmStates.r = value; _gpuArray[_index] = tmp; } }

        public float Radius { get => _gpuArray[_index].Radius; set => _gpuArray[_index].Radius = value; }
        public float LightIntensity { get => _gpuArray[_index].LightIntensity; set => _gpuArray[_index].LightIntensity = value; }
    }

    public class PlanetContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "Terra";
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Seed { get; set; } = "Genesis_01";

        // --- HEAVY MANAGED DATA (The Lore & 4X Layer) ---
        public string AtmosphereType { get; set; } = "Standard Nitrogen-Oxygen";
        public string CapitalCityName { get; set; } = "Unestablished";
        public string GoverningFaction { get; set; } = "Independent";
        public string LoreDescription { get; set; } = "A newly surveyed world with untapped potential.";
        public bool HasRings { get; set; } = false;
        public List<string> PlanetaryTraits { get; set; } = new List<string>();
        public List<string> ExportGoods { get; set; } = new List<string>();
        public List<string> NativeSpecies { get; set; } = new List<string>();

        // --- Unity/Visual Managed Data ---
        public Mesh PlanetMesh { get; set; }
        public List<ContinentData> Continents { get; set; } = new List<ContinentData>();
        public List<MoonContext> Moons { get; set; } = new List<MoonContext>();

        private readonly int _index;
        private readonly PlanetGpuData[] _gpuArray;
        public PlanetContext()
        {

        }
        public PlanetContext(int index, PlanetGpuData[] gpuArray)
        {
            _index = index;
            _gpuArray = gpuArray;
        }

        // --- The 4:1 FSM Sailboat Engines ---
        public byte GeologyFsm { get => _gpuArray[_index].PackedFsmStates.r; set { var tmp = _gpuArray[_index]; tmp.PackedFsmStates.r = value; _gpuArray[_index] = tmp; } }
        public byte ClimateFsm { get => _gpuArray[_index].PackedFsmStates.g; set { var tmp = _gpuArray[_index]; tmp.PackedFsmStates.g = value; _gpuArray[_index] = tmp; } }
        public byte EconomyFsm { get => _gpuArray[_index].PackedFsmStates.b; set { var tmp = _gpuArray[_index]; tmp.PackedFsmStates.b = value; _gpuArray[_index] = tmp; } }
        public byte MilitaryFsm { get => _gpuArray[_index].PackedFsmStates.a; set { var tmp = _gpuArray[_index]; tmp.PackedFsmStates.a = value; _gpuArray[_index] = tmp; } }

        // --- Standard Hot Data ---
        public float OrbitDistance { get => _gpuArray[_index].OrbitDistance; set => _gpuArray[_index].OrbitDistance = value; }
        public float Radius { get => _gpuArray[_index].Radius; set => _gpuArray[_index].Radius = value; }
        public BiomeType Biome { get => (BiomeType)_gpuArray[_index].BiomeTypeIndex; set => _gpuArray[_index].BiomeTypeIndex = (int)value; }
        public int Richness { get => _gpuArray[_index].Richness; set => _gpuArray[_index].Richness = value; }
    }

    public class MoonContext : IStateContext
    {
        // --- IStateContext (Fixed) ---
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "Luna";
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // --- HEAVY MANAGED DATA (The Lore & 4X Layer) ---
        public bool IsTidallyLocked { get; set; } = true;
        public string MoonClassification { get; set; } = "Barren Rock";
        public string LoreDescription { get; set; } = "A silent, heavily cratered satellite.";
        public List<string> PointsOfInterest { get; set; } = new List<string>();
        public Mesh MoonMesh { get; set; }

        // --- GPU Bridge ---
        private readonly int _index;
        private readonly MoonGpuData[] _gpuArray;
        public MoonContext()
        {

        }
        public MoonContext(int index, MoonGpuData[] gpuArray)
        {
            _index = index;
            _gpuArray = gpuArray;
        }

        // --- The 4:1 FSM Sailboat Engines ---
        public byte OrbitalMechanicsFsm { get => _gpuArray[_index].PackedFsmStates.r; set { var tmp = _gpuArray[_index]; tmp.PackedFsmStates.r = value; _gpuArray[_index] = tmp; } }
        public byte SubsurfaceActivityFsm { get => _gpuArray[_index].PackedFsmStates.g; set { var tmp = _gpuArray[_index]; tmp.PackedFsmStates.g = value; _gpuArray[_index] = tmp; } }

        // --- Standard Hot Data ---
        public float Radius { get => _gpuArray[_index].Radius; set => _gpuArray[_index].Radius = value; }
        public float OrbitDistance { get => _gpuArray[_index].OrbitDistance; set => _gpuArray[_index].OrbitDistance = value; }
    }
}