using UnityEngine;

#if UNITY_EDITOR
#endif

namespace Workshop.Core
{
    public struct SplashLetterEntity
    {
        public int LetterIndex;
        // --- Added for FSM Anchoring ---
        public Vector3 OriginalPosition;
        public Quaternion OriginalRotation;
        public Vector3 OriginalScale;
        // -------------------------------
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public int MeshId;
        public int MaterialId;
        public Color FactionColor;
    }
}
