using UnityEngine;

namespace TheSingularityWorkshop.Sandbox.Ants
{
    // These define the raw byte values used in the GPU texture
    public static class AntStates
    {
        public const byte EmptyAir = 0;
        public const byte SandDirt = 1;
        public const byte AntWandering = 2; // Looking for sand or food
        public const byte AntDigging = 3;    // Actively turning sand to air
        public const byte AntFalling = 4;    // Subject to gravity
        public const byte AntPheromone = 5;  // Optional: communication trail
    }

    // A helper struct to convert a single packed uint into 4 separate byte states
    public struct Packed4AntStates
    {
        public byte Ant1, Ant2, Ant3, Ant4;

        public Packed4AntStates(uint packedValue)
        {
            Ant1 = (byte)(packedValue & 0xFF);
            Ant2 = (byte)((packedValue >> 8) & 0xFF);
            Ant3 = (byte)((packedValue >> 16) & 0xFF);
            Ant4 = (byte)((packedValue >> 24) & 0xFF);
        }
    }
}