#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ExperienceContext))]
public class ExperienceContextEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default manifest string field
        DrawDefaultInspector();

        ExperienceContext context = (target as ExperienceContext);

        EditorGUILayout.Space();
        if (GUILayout.Button("Open Singularity Experience Editor", GUILayout.Height(30)))
        {
            // Pass the current manifest name to the window for focused editing
            ExperienceBuilderEditorWindow.ShowWindow(context.ManifestName);
        }
    }
}
#endif