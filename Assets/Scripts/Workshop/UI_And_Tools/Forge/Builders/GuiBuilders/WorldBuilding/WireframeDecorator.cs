using UnityEngine;
using System.Collections.Generic;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    public static class WireframeDecorator
    {
        public static GameObject ApplyWireframe(GameObject targetObj, Mesh mesh, float thickness = 0.01f, Color? color = null)
        {
            Color lineColor = color ?? Color.cyan;

            // Create a container for the wireframe so it can be easily toggled/destroyed
            GameObject wireframeContainer = new GameObject("Wireframe_Overlay");
            wireframeContainer.transform.SetParent(targetObj.transform, false);

            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;

            // Extract UNIQUE edges to prevent Z-fighting and double-drawing
            HashSet<long> uniqueEdges = new HashSet<long>();
            for (int i = 0; i < tris.Length; i += 3)
            {
                AddUniqueEdge(uniqueEdges, tris[i], tris[i + 1]);
                AddUniqueEdge(uniqueEdges, tris[i + 1], tris[i + 2]);
                AddUniqueEdge(uniqueEdges, tris[i + 2], tris[i]);
            }

            Material lineMat = new Material(Shader.Find("Sprites/Default")); // Fast unlit shader
            lineMat.color = lineColor;

            foreach (long edge in uniqueEdges)
            {
                int v1 = (int)(edge >> 32);
                int v2 = (int)(edge & 0xFFFFFFFF);

                GameObject edgeObj = new GameObject("Edge");
                edgeObj.transform.SetParent(wireframeContainer.transform, false);

                LineRenderer lr = edgeObj.AddComponent<LineRenderer>();
                lr.sharedMaterial = lineMat;
                lr.startWidth = thickness;
                lr.endWidth = thickness;
                lr.positionCount = 2;
                lr.SetPosition(0, verts[v1]);
                lr.SetPosition(1, verts[v2]);
                lr.useWorldSpace = false; // Ensures it rotates with the parent in previews!
            }

            return wireframeContainer;
        }

        private static void AddUniqueEdge(HashSet<long> edges, int v1, int v2)
        {
            long min = Mathf.Min(v1, v2);
            long max = Mathf.Max(v1, v2);
            long key = (min << 32) | (uint)max;
            edges.Add(key);
        }
    }
}