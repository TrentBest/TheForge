using TheSingularityWorkshop.MicroPackages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TheSingularityWorkshop.Armada2525.Math
{
    public class GalaxyMath : IMicroPackage
    {
        // --- MICRO-PACKAGE CONTRACT ---

        // Math doesn't usually need Update loops, so we leave this empty.
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>();

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            // If this package needed to negotiate resources (e.g. "I need the GPU"), it would happen here.
            // For a Math library, we just pass.
        }

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log("[GalaxyMath] Mathematical constants loaded. Universe is deterministic.");
        }

        public GalaxyMath()
        {
        }

        // --- THE DETERMINISTIC CORE ---

        /// <summary>
        /// Generates a unique, repeatable ID for a specific coordinate in the galaxy.
        /// </summary>
        internal static uint GetStarSeed(int x, int y)
        {
            // We use large primes to scramble the bits of X and Y so patterns don't emerge.
            uint positionSeed = (uint)(x * 198491317) ^ (uint)(y * 6542989);
            return Squirrel3(positionSeed);
        }

        /// <summary>
        /// Determines if a star exists at this location based on the seed.
        /// </summary>
        internal static bool HasStar(uint seed)
        {
            // 15% chance of a star (Adjust density here)
            // We look at the last 2 digits of the seed (0-99)
            return (seed % 100) < 15;
        }

        /// <summary>
        /// Generates a float between min and max using the seed and a 'salt'.
        /// Salt is crucial: It lets you generate "Temperature" and "Size" from the same Star Seed
        /// without them being correlated (e.g. Big stars aren't always Hot).
        /// </summary>
        internal static float Range(uint seed, int salt, int min, int max)
        {
            // Re-hash the seed with the salt to get a new random-looking number
            uint newSeed = Squirrel3(seed + (uint)salt);

            // Normalize to 0.0 - 1.0
            float normalized = (newSeed % 10000) / 10000f;

            // Map to range
            return min + (normalized * (max - min));
        }


        internal static float Range(uint seed, int salt, float min, float max)
        {
            // Re-hash the seed with the salt to get a new random-looking number
            uint newSeed = Squirrel3(seed + (uint)salt);

            // Normalize to 0.0 - 1.0
            float normalized = (newSeed % 10000) / 10000f;

            // Map to range
            return min + (normalized * (max - min));
        }

        // Overload for float range
        internal static float Range(uint seed, float salt, float min, float max)
        {
            uint newSeed = Squirrel3(seed + (uint)salt);
            float normalized = (newSeed % 10000) / 10000f;
            return min + (normalized * (max - min));
        }

        internal static Color GetStarColor(uint seed)
        {
            // Use salt 55 for color generation
            float hue = Range(seed, 55, 0f, 1f);

            // Bias towards realistic star colors (Red/Orange/Yellow/Blue)
            if (hue < 0.3f) return new Color(1f, 0.4f, 0.4f); // Red Dwarf (Common)
            if (hue < 0.5f) return new Color(1f, 0.7f, 0.2f); // Orange K-Type
            if (hue < 0.7f) return new Color(1f, 0.9f, 0.4f); // Yellow G-Type (Sun)
            if (hue < 0.85f) return Color.white;              // White A-Type
            return new Color(0.6f, 0.8f, 1f);                 // Blue Giant (Rare)
        }

        internal static string GetProceduralName(uint seed)
        {
            // Greek-ish prefixes for that Sci-Fi feel
            string[] prefixes = {
                "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta",
                "Omicron", "Omega", "Sigma", "Tau", "Psi", "Chi", "Rho"
            };

            // Roman suffixes
            string[] suffixes = { "Prime", "Major", "Minor", "Ceti", "V", "VI", "X" };

            // Math to pick parts
            uint pIndex = seed % (uint)prefixes.Length;

            // 50% chance to have a cool suffix, 50% chance to have a number
            if ((seed % 2) == 0)
            {
                uint sIndex = (seed / 10) % (uint)suffixes.Length;
                return $"{prefixes[pIndex]} {suffixes[sIndex]}";
            }
            else
            {
                uint number = (seed % 900) + 100; // 3 digit number
                return $"{prefixes[pIndex]}-{number}";
            }
        }

        // --- SQUIRREL3 IMPLEMENTATION ---
        // Based on Math for Game Developers (GDC 2017) by Squirrel Eiserloh
        // This is fast, bit-wise math that creates excellent noise from sequential integers.
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