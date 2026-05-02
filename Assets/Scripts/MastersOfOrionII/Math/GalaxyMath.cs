using System;
using System.Collections.Generic;
using UnityEngine;
using Workshop.Systems.MicroPackages;

namespace Assets.Scripts.MastersOfOrionII.Math
{
    /// <summary>
    /// The GalaxyMath Micro-Package: The Deterministic Blueprint of the Universe.
    /// Provides Squirrel3 bit-noise hashing for O(1) procedural generation.
    /// </summary>
    public class GalaxyMath : IMicroPackage, IArchivalMetadata
    {
        // --- Core Identity ---
        public string PackageId { get; set; } = "Workshop.Math.Galaxy";
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>();

        // --- Architectural Intent ---
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // Math is delivered as a pure, lightweight digital data stream
        public string DeliveryProfileId => "Delivery_DigitalDataStream";

        // Foundational math has no dependencies
        public string[] GetDependencies() => new string[0];
        public string[] GetExclusions() => new string[0];

        #region --- IArchivalMetadata (The Data Card) ---

        public string Version => "1.0.0";
        public string Author => "The Singularity Workshop";
        public ForgeSpatialZone SpatialZone => ForgeSpatialZone.LogicAndCompute; // The Cortex

        public string ShortDescription => "Deterministic bit-noise and Squirrel3 hashing.";
        public string LongDescription => "Provides high-performance, deterministic bit-scrambling logic. Essential for O(1) galactic generation, allowing stars and planets to be 'found' rather than 'stored'. Includes star seeding, salt-based range mapping, and procedural naming.";
        public string ThumbnailUrl => "Textures/Thumbnails/Archive_Math_Galaxy";

        public int ReleaseYear => 1996; // Masters of Orion II era
        public string OriginalAuthor => "Squirrel Eiserloh (Algorithm), Trent Best (Implementation)";
        public string OriginalPublisher => "The Singularity Workshop";
        public string OriginalPlatform => "The Forge";
        public string Era => "The 4X Golden Age";
        public string HistoricalSignificance => "Preserves the deterministic spirit of 90s strategy games, where entire universes could be resolved from a single integer.";
        public Color AccentColor => new Color(0.3f, 0.5f, 1.0f); // Galactic Blue

        #endregion

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[The Cortex]</b> {PackageId} online. Universe is deterministic. Coordinate-to-Seed mapping synchronized.");
        }

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            // Pure math libraries generally don't need to negotiate.
            arbitrator.None();
        }

        public GalaxyMath() { }

        // --- THE DETERMINISTIC CORE ---

        /// <summary>
        /// Generates a unique, repeatable ID for a specific coordinate in the galaxy.
        /// </summary>
        public static uint GetStarSeed(int x, int y)
        {
            uint positionSeed = (uint)(x * 198491317) ^ (uint)(y * 6542989);
            return Squirrel3(positionSeed);
        }

        /// <summary>
        /// Determines if a star exists at this location based on the seed.
        /// </summary>
        public static bool HasStar(uint seed)
        {
            return (seed % 100) < 15; // 15% density
        }

        /// <summary>
        /// Generates a float between min and max using a seed and a 'salt'.
        /// Salt ensures that different properties (Size vs Temp) remain uncorrelated.
        /// </summary>
        public static float Range(uint seed, int salt, float min, float max)
        {
            uint newSeed = Squirrel3(seed + (uint)salt);
            float normalized = (newSeed % 10000) / 10000f;
            return min + (normalized * (max - min));
        }

        public static float Range(uint seed, int salt, int min, int max) => Range(seed, salt, (float)min, (float)max);
        public static float Range(uint seed, float salt, float min, float max) => Range(seed, (int)salt, min, max);

        public static Color GetStarColor(uint seed)
        {
            float hue = Range(seed, 55, 0f, 1f);
            if (hue < 0.3f) return new Color(1f, 0.4f, 0.4f); // Red Dwarf
            if (hue < 0.5f) return new Color(1f, 0.7f, 0.2f); // Orange K-Type
            if (hue < 0.7f) return new Color(1f, 0.9f, 0.4f); // Yellow G-Type
            if (hue < 0.85f) return Color.white;              // White A-Type
            return new Color(0.6f, 0.8f, 1f);                 // Blue Giant
        }

        public static string GetProceduralName(uint seed)
        {
            string[] prefixes = { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta", "Omicron", "Omega", "Sigma", "Tau", "Psi", "Chi", "Rho" };
            string[] suffixes = { "Prime", "Major", "Minor", "Ceti", "V", "VI", "X" };

            uint pIndex = seed % (uint)prefixes.Length;
            if ((seed % 2) == 0)
            {
                uint sIndex = (seed / 10) % (uint)suffixes.Length;
                return $"{prefixes[pIndex]} {suffixes[sIndex]}";
            }

            uint number = (seed % 900) + 100;
            return $"{prefixes[pIndex]}-{number}";
        }

        // --- SQUIRREL3 IMPLEMENTATION ---
        private static uint Squirrel3(uint position, uint seed = 0)
        {
            const uint BIT_NOISE1 = 0xB5297A4D;
            const uint BIT_NOISE2 = 0x68E31DA4;
            const uint BIT_NOISE3 = 0x1B56C4E9;

            uint mangled = position;
            mangled *= BIT_NOISE1;
            mangled += seed;
            mangled ^= (mangled >> 8);
            mangled += BIT_NOISE2;
            mangled ^= (mangled << 8);
            mangled *= BIT_NOISE3;
            mangled ^= (mangled >> 8);
            return mangled;
        }
    }
}