using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public static class ZoningMath
    {
        /// <summary>
        /// Converts a byte ID (0-255) into a normalized float (0.0 to 1.0) for the texture channel.
        /// </summary>
        public static float EncodeZone(byte zoneId) => (float)zoneId / 255f;

        /// <summary>
        /// Reads the normalized float back into the discrete byte ID.
        /// </summary>
        public static byte DecodeZone(float bChannel) => (byte)Mathf.RoundToInt(bChannel * 255f);
    }
}