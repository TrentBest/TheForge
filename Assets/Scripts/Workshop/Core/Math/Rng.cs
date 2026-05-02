using System;

namespace Workshop.Core.Math
{
    /// <summary>
    /// Stateless, deterministic pseudo-random number generator.
    /// Based on the Squirrel3 noise function. Ideal for Job Systems and Compute Shaders.
    /// </summary>
    public static class Rng
    {
        // The magic noise constants for Squirrel3
        private const uint NOISE1 = 0xb5297a4d;
        private const uint NOISE2 = 0x68e31da4;
        private const uint NOISE3 = 0x1b56c4e9;

        /// <summary>
        /// Returns a deterministic pseudo-random integer based on position and seed.
        /// </summary>
        public static uint Squirrel3(int position, uint seed)
        {
            uint n = (uint)position;
            n *= NOISE1;
            n += seed;
            n ^= n >> 8;
            n += NOISE2;
            n ^= n << 8;
            n *= NOISE3;
            n ^= n >> 8;
            return n;
        }

        /// <summary>
        /// Returns a deterministic float between 0.0 and 1.0
        /// </summary>
        public static float GetFloat(int position, uint seed)
        {
            return (float)Squirrel3(position, seed) / uint.MaxValue;
        }

        /// <summary>
        /// Returns a deterministic float between -1.0 and 1.0
        /// </summary>
        public static float GetFloatRange(int position, uint seed)
        {
            return (GetFloat(position, seed) * 2f) - 1f;
        }
    }
}