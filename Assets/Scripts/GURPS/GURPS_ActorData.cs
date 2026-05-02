using UnityEngine;

namespace Assets.Scripts.GURPS
{
    [System.Serializable]
    public struct GURPS_ActorData
    {
        public Vector3 Position;   // 12 bytes
        public float Facing;      // 4 bytes (16 total)
        public int StateId;       // 4 bytes
        public float CurrentHealth; // 4 bytes
        public int ST;            // 4 bytes
        public int DX;            // 4 bytes (32 total)
        public int IQ;            // 4 bytes
        public int HT;            // 4 bytes
        public int StatusFlags;   // 4 bytes
        public float Padding;     // 4 bytes to keep 16-byte alignment (48 bytes total)
    }
}