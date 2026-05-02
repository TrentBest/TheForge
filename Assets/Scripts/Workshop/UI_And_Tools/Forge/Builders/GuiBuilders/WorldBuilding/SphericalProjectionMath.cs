using UnityEngine;
using System.Collections.Generic;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    public static class SphericalProjectionMath
    {
        /// <summary>
        /// Deforms a target icosphere mesh using a 2D MapDataTexture, localized to a specific centroid.
        /// </summary>
        /// <param name="baseMesh">The raw Icosphere mesh.</param>
        /// <param name="mapData">The RGBAFloat texture from Workshop_Gui_MapBuilder.</param>
        /// <param name="centroidDirection">The normalized 3D vector pointing to the center of the projection.</param>
        /// <param name="coverageAngle">How much of the sphere the map covers (e.g., 30 degrees).</param>
        /// <param name="maxExtrusion">The maximum height to displace the vertices.</param>
        public static Mesh ProjectMapOntoSphericalCap(Mesh baseMesh, Texture2D mapData, Vector3 centroidDirection, float coverageAngle, float maxExtrusion)
        {
            Vector3[] vertices = baseMesh.vertices;
            Color[] colors = new Color[vertices.Length];

            // We use the dot product to find the angle between the centroid and any vertex
            float cosCoverage = Mathf.Cos(coverageAngle * Mathf.Deg2Rad);

            // Create a tangent space at the centroid to map X/Y of the texture
            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(centroidDirection, up)) > 0.99f) up = Vector3.forward;
            Vector3 tangent = Vector3.Cross(up, centroidDirection).normalized;
            Vector3 bitangent = Vector3.Cross(centroidDirection, tangent).normalized;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertDir = vertices[i].normalized;
                float dot = Vector3.Dot(centroidDirection, vertDir);

                // Check if the vertex falls within our map's coverage area on the sphere
                if (dot >= cosCoverage)
                {
                    // 1. Calculate the local 2D projection (Azimuthal Equidistant)
                    float angle = Mathf.Acos(dot); // Angle from center
                    float radiusMapping = angle / (coverageAngle * Mathf.Deg2Rad); // 0 at center, 1 at edge

                    // Project the vertex direction onto the tangent plane
                    Vector3 localProj = vertDir - (dot * centroidDirection);
                    localProj.Normalize();

                    float xOnPlane = Vector3.Dot(localProj, tangent);
                    float yOnPlane = Vector3.Dot(localProj, bitangent);

                    // 2. Convert to UV space (0.0 to 1.0)
                    float u = 0.5f + (xOnPlane * radiusMapping * 0.5f);
                    float v = 0.5f + (yOnPlane * radiusMapping * 0.5f);

                    // Clamp to prevent edge bleeding
                    u = Mathf.Clamp01(u);
                    v = Mathf.Clamp01(v);

                    // 3. Sample the Master Map Builder Texture
                    Color mapPixel = mapData.GetPixelBilinear(u, v);

                    // r = Height, g = Biome Type
                    float heightDisplacement = mapPixel.r;

                    // Extrude the vertex outward along its normal based on the map's height
                    vertices[i] += vertDir * (heightDisplacement * maxExtrusion);

                    // Store the biome data in the vertex color for the shader to parse
                    colors[i] = mapPixel;
                }
                else
                {
                    // Ocean / Default Crust fallback for vertices outside the map
                    colors[i] = new Color(0, 0, 0, 0);
                }
            }

            Mesh projectedMesh = new Mesh();
            projectedMesh.name = baseMesh.name + "_Projected";
            projectedMesh.vertices = vertices;
            projectedMesh.triangles = baseMesh.triangles;
            projectedMesh.colors = colors;
            projectedMesh.RecalculateNormals(); // Crucial for lighting the new topography

            return projectedMesh;
        }
    }
}