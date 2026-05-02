using UnityEngine;

namespace Workshop.Core.Math
{
    public static class Cinemathematics
    {
        /// <summary>
        /// Standard normalized interpolation (0 to 1).
        /// </summary>
        public static float Lerp(float a, float b, float t) => a + (b - a) * Mathf.Clamp01(t);

        /// <summary>
        /// A non-linear dive curve where speed follows an accelerating -x^2 falloff.
        /// Perfect for gravitational plunging or orbital "diving" into a point.
        /// Returns a mapped value between 1.0 (start) and 0.0 (impact).
        /// </summary>
        public static float InverseQuadraticDive(float t)
        {
            t = Mathf.Clamp01(t);
            // If speed is -x^2, the distance remaining drops exponentially as t approaches 1.
            return 1.0f - (t * t);
        }

        /// <summary>
        /// Smoothly rotates towards a target quaternion, easing out as it aligns.
        /// </summary>
        public static Quaternion EaseOutRotation(Quaternion current, Quaternion target, float t)
        {
            float easedT = t * (2 - t); // Quadratic ease-out
            return Quaternion.Slerp(current, target, easedT);
        }
    }
}