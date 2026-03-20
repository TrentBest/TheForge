using System;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders.WorldBuilding
{
    [Serializable]
    public class ContinentData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "Tectonic Plate";
        public Color PlateColor { get; set; } = Color.white;

        // --- MASS LINKING SYSTEM ---
        public float MassPercentage { get; set; } = 10f; // 0 to 100%
        public bool IsLocked { get; set; } = false;      // Prevents being drained by other sliders

        // --- ZERO-DRIFT COORDINATE SYSTEM ---
        public int AnchorFaceIndex { get; set; } = 0;

        // Barycentric coordinates relative to the 3 vertices of the face (x+y+z = 1)
        public Vector3 AnchorBarycentric { get; set; } = new Vector3(0.333f, 0.333f, 0.333f);

        public float VelocityX { get; set; } = 0f;
        public float VelocityY { get; set; } = 0f;

        // Resolves the exact 3D world position dynamically from the mesh geometry
        public Vector3 GetWorldPosition(Vector3[] vertices, int[] triangles)
        {
            int triIndex = AnchorFaceIndex * 3;
            if (triIndex + 2 >= triangles.Length) return Vector3.up;

            Vector3 v0 = vertices[triangles[triIndex]];
            Vector3 v1 = vertices[triangles[triIndex + 1]];
            Vector3 v2 = vertices[triangles[triIndex + 2]];

            Vector3 precisePos = (v0 * AnchorBarycentric.x) + (v1 * AnchorBarycentric.y) + (v2 * AnchorBarycentric.z);
            return precisePos.normalized;
        }
    }
}