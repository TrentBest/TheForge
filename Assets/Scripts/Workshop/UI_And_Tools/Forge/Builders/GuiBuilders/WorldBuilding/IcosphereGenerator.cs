using UnityEngine;
using System.Collections.Generic;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    public static class IcosphereGenerator
    {
        private struct TriangleIndices
        {
            public int v1, v2, v3;
            public TriangleIndices(int v1, int v2, int v3) { this.v1 = v1; this.v2 = v2; this.v3 = v3; }
        }

        public static Mesh Create(int recursionLevel, float radius)
        {
            Mesh mesh = new Mesh { name = $"Icosphere_Sub{recursionLevel}" };
            List<Vector3> vertList = new List<Vector3>();
            Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();

            float t = (1f + Mathf.Sqrt(5f)) / 2f;
            vertList.Add(new Vector3(-1f, t, 0f).normalized * radius);
            vertList.Add(new Vector3(1f, t, 0f).normalized * radius);
            vertList.Add(new Vector3(-1f, -t, 0f).normalized * radius);
            vertList.Add(new Vector3(1f, -t, 0f).normalized * radius);
            vertList.Add(new Vector3(0f, -1f, t).normalized * radius);
            vertList.Add(new Vector3(0f, 1f, t).normalized * radius);
            vertList.Add(new Vector3(0f, -1f, -t).normalized * radius);
            vertList.Add(new Vector3(0f, 1f, -t).normalized * radius);
            vertList.Add(new Vector3(t, 0f, -1f).normalized * radius);
            vertList.Add(new Vector3(t, 0f, 1f).normalized * radius);
            vertList.Add(new Vector3(-t, 0f, -1f).normalized * radius);
            vertList.Add(new Vector3(-t, 0f, 1f).normalized * radius);

            List<TriangleIndices> faces = new List<TriangleIndices>
            {
                new TriangleIndices(0, 11, 5), new TriangleIndices(0, 5, 1), new TriangleIndices(0, 1, 7), new TriangleIndices(0, 7, 10), new TriangleIndices(0, 10, 11),
                new TriangleIndices(1, 5, 9), new TriangleIndices(5, 11, 4), new TriangleIndices(11, 10, 2), new TriangleIndices(10, 7, 6), new TriangleIndices(7, 1, 8),
                new TriangleIndices(3, 9, 4), new TriangleIndices(3, 4, 2), new TriangleIndices(3, 2, 6), new TriangleIndices(3, 6, 8), new TriangleIndices(3, 8, 9),
                new TriangleIndices(4, 9, 5), new TriangleIndices(2, 4, 11), new TriangleIndices(6, 2, 10), new TriangleIndices(8, 6, 7), new TriangleIndices(9, 8, 1)
            };

            for (int i = 0; i < recursionLevel; i++)
            {
                List<TriangleIndices> faces2 = new List<TriangleIndices>();
                foreach (var tri in faces)
                {
                    int a = GetMiddlePoint(tri.v1, tri.v2, ref vertList, ref middlePointIndexCache, radius);
                    int b = GetMiddlePoint(tri.v2, tri.v3, ref vertList, ref middlePointIndexCache, radius);
                    int c = GetMiddlePoint(tri.v3, tri.v1, ref vertList, ref middlePointIndexCache, radius);
                    faces2.Add(new TriangleIndices(tri.v1, a, c));
                    faces2.Add(new TriangleIndices(tri.v2, b, a));
                    faces2.Add(new TriangleIndices(tri.v3, c, b));
                    faces2.Add(new TriangleIndices(a, b, c));
                }
                faces = faces2;
            }

            mesh.vertices = vertList.ToArray();
            int[] triIndices = new int[faces.Count * 3];
            for (int i = 0; i < faces.Count; i++)
            {
                triIndices[i * 3] = faces[i].v1;
                triIndices[i * 3 + 1] = faces[i].v2;
                triIndices[i * 3 + 2] = faces[i].v3;
            }
            mesh.triangles = triIndices;
            mesh.RecalculateNormals();

            return mesh;
        }

        private static int GetMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float radius)
        {
            bool firstIsSmaller = p1 < p2;
            long smallerIndex = firstIsSmaller ? p1 : p2;
            long greaterIndex = firstIsSmaller ? p2 : p1;
            long key = (smallerIndex << 32) + greaterIndex;

            if (cache.TryGetValue(key, out int ret)) return ret;

            Vector3 middle = new Vector3(
                (vertices[p1].x + vertices[p2].x) / 2f,
                (vertices[p1].y + vertices[p2].y) / 2f,
                (vertices[p1].z + vertices[p2].z) / 2f
            );

            int i = vertices.Count;
            vertices.Add(middle.normalized * radius);
            cache.Add(key, i);
            return i;
        }
    }
}