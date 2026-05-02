using UnityEngine;

#if UNITY_EDITOR
#endif

namespace Workshop.Core
{
    // --- SOVEREIGN ENTITY MODELS ---
    // Unmanaged value types required for DataWarehouse shelf allocation

    public struct WorldObserver
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float ViewRadius;
        public bool IsActive;
        public Matrix4x4 ViewMatrix => Matrix4x4.Scale(new Vector3(1, 1, -1)) * Matrix4x4.Inverse(Matrix4x4.TRS(Position, Rotation, Vector3.one));
        public Matrix4x4 ProjectionMatrix => Matrix4x4.Perspective(60f, Screen.width / (float)Screen.height, 0.1f, 1000f);
    }
}
