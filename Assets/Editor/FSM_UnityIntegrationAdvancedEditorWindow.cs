#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.FSM_API.Editor
{
    /// <summary>
    /// An Editor-time integration that drives FSM processing groups within the Unity Editor environment.
    /// This mirrors the runtime FSM_UnityIntegrationAdvanced logic but hooks into Editor-specific events.
    /// </summary>
    public class FSM_UnityIntegrationAdvancedEditorWindow : EditorWindow
    {
        [MenuItem("Tools/The Singularity Workshop/FSM Editor Engine")]
        public static void ShowWindow() => GetWindow<FSM_UnityIntegrationAdvancedEditorWindow>("FSM Engine");

        // The list of strings to iterate over for Editor-specific updates
        [SerializeField] private List<string> _editorProcessingGroups = new List<string> { "EditorUpdate", "Heuristics" };

        private void OnEnable()
        {
            // Connect to every available Unity Editor message loop
            EditorApplication.update += TickEditorUpdate;
            EditorApplication.hierarchyChanged += TickHierarchyChanged;
            EditorApplication.projectChanged += TickProjectChanged;

            // Re-bind SceneView GUI if you want to drive FSMs during scene manipulation
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            // Clean up hooks to prevent memory leaks or background processing
            EditorApplication.update -= TickEditorUpdate;
            EditorApplication.hierarchyChanged -= TickHierarchyChanged;
            EditorApplication.projectChanged -= TickProjectChanged;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void TickEditorUpdate() => ProcessGroups("EditorUpdate");
        private void TickHierarchyChanged() => ProcessGroups("HierarchyChanged");
        private void TickProjectChanged() => ProcessGroups("ProjectChanged");
        private void OnSceneGUI(SceneView sceneView) => ProcessGroups("SceneViewGUI");

        /// <summary>
        /// Iterates over the defined groups and triggers the FSM_API Interaction update.
        /// </summary>
        private void ProcessGroups(string currentContext)
        {
            // We can dynamically add the context name to our processing list 
            // if we want specific FSMs to respond to specific Unity Editor messages.
            foreach (var group in _editorProcessingGroups)
            {
                FSM_API.Interaction.Update(group);
            }

            // Always process the specific message context if it exists in the API
            FSM_API.Interaction.Update(currentContext);
        }

        private void OnGUI()
        {
            GUILayout.Label("FSM Editor Engine Status", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // metaDev: Display the same numerical data and performance metrics found in the Hub
            GUILayout.Label($"Total Definitions: {FSM_API.Internal.TotalFsmDefinitionCount}");
            GUILayout.Label($"Active Handles: {FSM_API.Internal.TotalFsmHandleCount}");

            if (GUILayout.Button("Hard Reset API"))
            {
                FSM_API.Internal.ResetAPI(true);
                Debug.Log("FSM_API: Hard Reset triggered from Editor Integration.");
            }

            // Allow the user to define more groups via the script-centric preference
            SerializedObject so = new SerializedObject(this);
            EditorGUILayout.PropertyField(so.FindProperty("_editorProcessingGroups"), true);
            so.ApplyModifiedProperties();
        }
    }
}
#endif