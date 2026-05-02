using UnityEngine;

namespace Assets.Scripts.MastersOfOrionII
{
    /// <summary>
    /// The central body. Determines the "Goldilocks Zone" for planets.
    /// </summary>
    public struct Star
    {
        public int Id;

        // Physical Properties
        public StarSpectralClass Class;
        public float Mass;          // Relative to Sol (1.0 = Sun)
        public float Radius;        // Relative to Sol
        public float Luminosity;    // Energy output (critical for planet temp)
        public float TemperatureK;  // Surface temp in Kelvin

        // Visuals
        public Color ChromaticAberration; // For the renderer
    }
}
