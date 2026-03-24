using System;
using System.Text;
using UnityEngine;

namespace TheSingularityWorkshop.Forge
{
    /// <summary>
    /// A custom 128-bit-style number composed of 4 uints, chunked at 1 Billion (10^9).
    /// Max Value: 999,999,999 Astral, 999,999,999 Cosmic, 999,999,999 Mega, 999,999,999 Base.
    /// </summary>
    [Serializable]
    public struct AstroInt
    {
        public const uint CHUNK_LIMIT = 1_000_000_000; // 1 Billion

        public uint Astral; // 10^27 (Octillions to Decillions)
        public uint Cosmic; // 10^18 (Quintillions to Octillions)
        public uint Mega;   // 10^9  (Billions to Quintillions)
        public uint Base;   // 10^0  (Units to Billions)

        public AstroInt(uint astral, uint cosmic, uint mega, uint baseVal)
        {
            Astral = astral;
            Cosmic = cosmic;
            Mega = mega;
            Base = baseVal;
            Normalize();
        }

        /// <summary>
        /// Ensures no chunk exceeds the 1 Billion limit, cascading excess up the tiers.
        /// </summary>
        public void Normalize()
        {
            if (Base >= CHUNK_LIMIT)
            {
                Mega += Base / CHUNK_LIMIT;
                Base %= CHUNK_LIMIT;
            }
            if (Mega >= CHUNK_LIMIT)
            {
                Cosmic += Mega / CHUNK_LIMIT;
                Mega %= CHUNK_LIMIT;
            }
            if (Cosmic >= CHUNK_LIMIT)
            {
                Astral += Cosmic / CHUNK_LIMIT;
                Cosmic %= CHUNK_LIMIT;
            }
            // If Astral exceeds CHUNK_LIMIT, you have truly broken the universe.
        }

        /// <summary>
        /// Returns the massive 36-digit exact string representation (e.g., 5,000,200,000...)
        /// </summary>
        public string ToExactString()
        {
            StringBuilder sb = new StringBuilder();
            bool started = false;

            if (Astral > 0) { sb.Append($"{Astral:N0},"); started = true; }
            if (Cosmic > 0 || started) { sb.Append(started ? $"{Cosmic:D9}," : $"{Cosmic:N0},"); started = true; }
            if (Mega > 0 || started) { sb.Append(started ? $"{Mega:D9}," : $"{Mega:N0},"); started = true; }
            sb.Append(started ? $"{Base:D9}" : $"{Base:N0}");

            return sb.ToString();
        }

        /// <summary>
        /// Returns a formatted string using the custom named tiers (e.g., 5 Astral | 200 Cosmic)
        /// </summary>
        public string ToNamedMagnitudeString()
        {
            if (Astral == 0 && Cosmic == 0 && Mega == 0 && Base == 0) return "0";

            string res = "";
            if (Astral > 0) res += $"{Astral} Astral  ";
            if (Cosmic > 0) res += $"{Cosmic} Cosmic  ";
            if (Mega > 0) res += $"{Mega} Mega  ";
            if (Base > 0) res += $"{Base}";
            return res.Trim();
        }
    }
}