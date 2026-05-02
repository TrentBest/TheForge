using UnityEditor;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

[CustomEditor(typeof(MonoBehaviour), true)]
public class GuiProviderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the standard script fields first
        DrawDefaultInspector();

        // Check if this specific script implements your interface
        if (target is IGuiProvider provider)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Singularity Workshop - GUI Tools", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Bake to UXML (To)"))
            {
                string path = EditorUtility.SaveFilePanelInProject("Save GUI Document", provider.Title, "uxml", "Select save location");
                if (!string.IsNullOrEmpty(path)) provider.ToUIDocument(path);
            }

            if (GUILayout.Button("Hydrate from UXML (Fro)"))
            {
                string path = EditorUtility.OpenFilePanel("Select GUI Document", "Assets", "uxml");
                if (!string.IsNullOrEmpty(path)) provider.FromUIDocument(path);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}