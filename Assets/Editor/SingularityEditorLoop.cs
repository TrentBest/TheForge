#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Editor
{
    /// <summary>
    /// The "Editor Kernel" for the Singularity Hub.
    /// It assumes control of the FSM API execution pathways within the Unity Editor.
    /// This runs AUTOMATICALLY (No GameObject or Window required).
    /// </summary>
    [InitializeOnLoad]
    public static class SingularityEditorLoop
    {
        // The specific "Frequency" we broadcast on for Editor-time FSMs
        public const string EDITOR_PATHWAY = "EditorUpdate";

        static SingularityEditorLoop()
        {
            // 1. Hook into the Editor's main update loop
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            // 2. Hook into Scene View for input processing if needed
            // SceneView.duringSceneGui += ..

           

        }

        private static void OnEditorUpdate()
        {
            // Only step if we are NOT playing (Runtime handles itself)
            if (!Application.isPlaying)
            {
                // This forces the API to process any FSM registered to "EditorUpdate"
                FSM_API.FSM_API.Interaction.Update(EDITOR_PATHWAY);

                // Optional: Force UI Repaint for smooth animations
                // ForceGuiRefresh(); 
            }
        }

        // Helper to wake up lazy UIs
        public static void ForceGuiRefresh()
        {
            var builders = Object.FindObjectsByType<TheSingularityWorkshop.Forge.Builders.GuiBuilders.InWorldGuiBuilder>(FindObjectsSortMode.None);
            foreach (var b in builders) b.MarkDirty();
        }
    }
}
#endif