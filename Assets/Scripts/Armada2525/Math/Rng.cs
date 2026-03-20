using System;

namespace TheSingularityWorkshop.Armada2525.MathUtils
{
    public static class Rng
    {
        // The magic noise constant for Squirrel3
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

        public static float GetFloat(int position, uint seed)
        {
            // Returns a float between 0.0 and 1.0
            return (float)Squirrel3(position, seed) / uint.MaxValue;
        }
    }
}