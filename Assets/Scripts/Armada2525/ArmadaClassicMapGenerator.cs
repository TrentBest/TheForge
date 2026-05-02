using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Workshop.Core.Math; // Pure Deterministic Math
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding;

namespace Workshop.Armada2525
{
    public static class ArmadaClassicMapGenerator
    {
        public const int MapWidth = 12;
        public const int MapHeight = 12;

        private static readonly Vector2Int[] PlayerSpawns = new Vector2Int[]
        {
            new Vector2Int(1, 1),
            new Vector2Int(MapWidth - 2, 1),
            new Vector2Int(1, MapHeight - 2),
            new Vector2Int(MapWidth - 2, MapHeight - 2),
            new Vector2Int(MapWidth / 2, 1),
            new Vector2Int(MapWidth / 2, MapHeight - 2)
        };

        private static List<Vector2Int> GetFixedStarPositions()
        {
            var positions = new List<Vector2Int>();
            positions.AddRange(PlayerSpawns);

            for (int x = 1; x < MapWidth - 1; x += 2)
            {
                for (int y = 1; y < MapHeight - 1; y += 2)
                {
                    var pos = new Vector2Int(x, y);
                    if (!positions.Contains(pos))
                    {
                        positions.Add(pos);
                    }
                }
            }
            return positions;
        }

        public static async Task GenerateClassicGalaxyAsync(ArmadaGalaxyContext context, uint seed)
        {
            var fixedPositions = GetFixedStarPositions();

            if (context.StarSystems == null) context.StarSystems = new Dictionary<int, StarSystemContext>();
            context.StarSystems.Clear();

            await Task.Run(() =>
            {
                int systemCount = fixedPositions.Count;
                var systemGpuData = new StarSystemGpuData[systemCount];
                var planetGpuData = new PlanetGpuData[systemCount * 5];

                int globalPlanetIndex = 0;
                int systemId = 1;

                // The Deterministic Tape Tracker
                int rngPos = 0;

                foreach (var pos in fixedPositions)
                {
                    int systemArrayIndex = systemId - 1;

                    var system = new StarSystemContext(systemArrayIndex, systemGpuData)
                    {
                        Id = systemId,
                        Name = GenerateRandomStarName(seed, ref rngPos),
                        GridCoordinate = new Vector3(pos.x * 10f, pos.y * 10f, 0)
                    };

                    // Planet Count: 1 to 5
                    int planetCount = 1 + (int)(Rng.GetFloat(rngPos++, seed) * 5);

                    for (int p = 0; p < planetCount; p++)
                    {
                        var planet = new PlanetContext(globalPlanetIndex, planetGpuData)
                        {
                            Name = $"{system.Name} {p + 1}",

                            // Biome: 0 to 4
                            Biome = (BiomeType)(int)(Rng.GetFloat(rngPos++, seed) * 5),

                            // Richness: 10 to 99
                            Richness = 10 + (int)(Rng.GetFloat(rngPos++, seed) * 90)
                        };

                        system.Planets.Add(planet);
                        globalPlanetIndex++;
                    }

                    if (Array.IndexOf(PlayerSpawns, pos) != -1)
                    {
                        system.IsHomeworld = true;
                        system.Planets[0].Biome = BiomeType.Terran;
                        system.Planets[0].Richness = 100;
                    }

                    lock (context.StarSystems)
                    {
                        context.StarSystems.Add(systemId, system);
                    }
                    systemId++;
                }
            });
        }

        private static string GenerateRandomStarName(uint seed, ref int rngPos)
        {
            string[] prefixes = { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta" };
            string[] suffixes = { "Centauri", "Orionis", "Draconis", "Lyrae", "Cygni", "Eridani" };

            int pIdx = (int)(Rng.GetFloat(rngPos++, seed) * prefixes.Length);
            int sIdx = (int)(Rng.GetFloat(rngPos++, seed) * suffixes.Length);

            return $"{prefixes[pIdx]} {suffixes[sIdx]}";
        }
    }
}