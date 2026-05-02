using System;

namespace Workshop.Physics
{
    [Serializable]
    public class UniversalConstants
    {
        // Standard Earth Defaults
        public float SpeedOfLight = 299792458f;      // c: m/s
        public float PlanckConstant = 6.626f;        // h: x 10^-34 J*s 
        public float GravitationalConstant = 6.674f; // G: x 10^-11 N*m^2/kg^2
        public float ElementaryCharge = 1.602f;      // e: x 10^-19 C 

        /// <summary>
        /// Checks if the current constants perfectly match Earth's reality.
        /// </summary>
        public bool IsEarthStandard()
        {
            // Using a tiny epsilon for float comparison safety
            return Math.Abs(SpeedOfLight - 299792458f) < 0.001f &&
                   Math.Abs(PlanckConstant - 6.626f) < 0.001f &&
                   Math.Abs(GravitationalConstant - 6.674f) < 0.001f &&
                   Math.Abs(ElementaryCharge - 1.602f) < 0.001f;
        }

        /// <summary>
        /// Generates the deterministic Universe Seed based on the physical constants.
        /// </summary>
        public int GetUniverseSeed()
        {
            // The sacred override.
            if (IsEarthStandard()) return 42;

            // Forward-Hash the constants to create a unique, reproducible 32-bit seed.
            unchecked // Allow integer overflow, which is standard for hashing
            {
                int hash = 17;
                hash = hash * 31 + SpeedOfLight.GetHashCode();
                hash = hash * 31 + PlanckConstant.GetHashCode();
                hash = hash * 31 + GravitationalConstant.GetHashCode();
                hash = hash * 31 + ElementaryCharge.GetHashCode();
                return hash;
            }
        }

        public float GetAtomicScaleMultiplier()
        {
            float hRatio = PlanckConstant / 6.626f;
            float eRatio = ElementaryCharge / 1.602f;
            if (eRatio <= 0.0001f) return 1000f;
            return (hRatio * hRatio) / (eRatio * eRatio);
        }
    }
}