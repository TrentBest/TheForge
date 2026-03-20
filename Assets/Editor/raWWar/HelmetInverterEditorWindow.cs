using UnityEngine;
using UnityEditor;
using System.Linq;

public class HelmetInverterEditorWindow : EditorWindow
{
    [MenuItem("TheSingularityWorkshop/NonSupported/Create Internal Helmet Mesh")]
    public static void InvertHelmetMesh()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null || !selected.GetComponent<MeshFilter>()) return;

        MeshFilter mf = selected.GetComponent<MeshFilter>();
        Mesh originalMesh = mf.sharedMesh;

        // 1. Create a copy so we don't wreck the original soldier mesh
        Mesh invertedMesh = Instantiate(originalMesh);
        invertedMesh.name = originalMesh.name + "_InternalView";

        // 2. Flip the Normals
        Vector3[] normals = invertedMesh.normals;
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -normals[i];
        }
        invertedMesh.normals = normals;

        // 3. Flip the Triangle Winding (The "Inversion")
        for (int m = 0; m < invertedMesh.subMeshCount; m++)
        {
            int[] triangles = invertedMesh.GetTriangles(m);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                // Swap two indices to flip the face direction
                int temp = triangles[i];
                triangles[i] = triangles[i + 1];
                triangles[i + 1] = temp;
            }
            invertedMesh.SetTriangles(triangles, m);
        }

        // 4. Save and Assign
        AssetDatabase.CreateAsset(invertedMesh, "Assets/" + invertedMesh.name + ".asset");
        mf.mesh = invertedMesh;

        Debug.Log("Helmet Mesh Re-rolled for internal view!");
    }
}