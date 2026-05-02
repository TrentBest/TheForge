// File: Assets/Scripts/Workshop/Core/SovereignGuiModels.cs
using UnityEngine;

namespace Workshop.Core
{
    /// <summary>
    /// The unmanaged "Vessel" for a GUI surface. 
    /// Stationed in the Warehouse for the Render Pipeline to manifest.
    /// </summary>
    public struct SovereignGuiSurface
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector2 PhysicalSize;
        public int TextureId; // Registered in DataWarehouse
        public bool IsVisible;
        public int ColliderId;
    }

    /// <summary>
    /// A pure data collider for the custom interaction system.
    /// Stationed in a dedicated shelf for O(1) or O(N) spatial queries.
    /// </summary>
    public struct SovereignCollider
    {
        public Vector3 Center;
        public Vector3 HalfExtents;
        public Quaternion Rotation;
        public int OwnerEntityId; // Links back to the GUI surface
        public bool IsActive;

        // Helper to check if a ray hits this OBB (Oriented Bounding Box)
        public bool Intersects(Ray ray, out float distance)
        {
            // Custom Ray-OBB intersection logic goes here
            distance = 0;
            return false;
        }
    }
}