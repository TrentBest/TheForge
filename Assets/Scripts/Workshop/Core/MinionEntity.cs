using UnityEngine;

#if UNITY_EDITOR
#endif

namespace Workshop.Core
{
    public struct MinionEntity
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public int ActiveTaskNameId;
    }
}
