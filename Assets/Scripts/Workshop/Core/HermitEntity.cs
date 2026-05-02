using UnityEngine;

#if UNITY_EDITOR
#endif

namespace Workshop.Core
{
    public struct HermitEntity
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Color CoreColor;
        public float PulseIntensity;
    }
}
