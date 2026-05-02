using UnityEngine;
using System.Runtime.InteropServices;

namespace Workshop.Swarmy
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DroneGPUData
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public uint State;       // FSM State (0: Patrol, 1: Tracking, 2: Decoy, etc.)
        public float Battery;    // Per-channel/Per-pixel state representation
        public uint TargetID;    // ID for the "Eyes" to track specific assets
    }
}