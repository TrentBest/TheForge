using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;



namespace TheSingularityWorkshop.FsmEditorHooks
{
    /// <summary>
    /// The dedicated Editor-Time pacemaker. Completely decoupled from FSM_UnityIntegrationAdvanced.
    /// Drives UI Toolkit, live previews, and Forge tools with variable frequency targets.
    /// </summary>
    [InitializeOnLoad]
    public static class FSM_EditorIntegrationAdvanced
    {
        private class TickProfile
        {
            public string GroupName;
            public float TargetFPS;
            public double LastTickTime;
            public double TickInterval => TargetFPS > 0 ? 1.0 / TargetFPS : 0;
        }

        // Internal groups managed specifically for Editor life-cycles
        public static readonly List<string> EditorUpdateGroups = new List<string> { "EditorUpdate", "Panels" };

        // Dynamic groups registered by Forge UI windows or user tools
        private static readonly Dictionary<string, TickProfile> _registeredGroups = new Dictionary<string, TickProfile>();

        private static bool _isSubscribed = false;

        static FSM_EditorIntegrationAdvanced()
        {
            Subscribe();
        }

        public static void Subscribe()
        {
            if (_isSubscribed) return;

            EditorApplication.update += OnEditorUpdate;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.projectChanged += OnProjectChanged;
            SceneView.duringSceneGui += OnSceneGUI;

            _isSubscribed = true;
            Debug.Log("[FSM Pacemaker] Advanced Editor Integration Active.");
        }

        public static void Unsubscribe()
        {
            if (!_isSubscribed) return;

            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.projectChanged -= OnProjectChanged;
            SceneView.duringSceneGui -= OnSceneGUI;

            _isSubscribed = false;
            Debug.Log("[FSM Pacemaker] Advanced Editor Integration Disabled.");
        }

        /// <summary>
        /// Gets all processing groups from the underlying FSM API.
        /// Preserved for your existing UI dropdown bindings.
        /// </summary>
        public static List<string> GetProcessingGroups(string filter = "")
        {
            return FSM_API.FSM_API.Internal.GetProcessingGroups();
        }

        /// <summary>
        /// Registers a processing group to be ticked during Editor Mode (The Chronos API).
        /// </summary>
        /// <param name="groupName">The FSM Processing Group.</param>
        /// <param name="targetFps">0 = tick every editor frame. > 0 = tick at specific interval.</param>
        public static void RegisterEditorGroup(string groupName, float targetFps = 0f)
        {
            if (!_registeredGroups.ContainsKey(groupName))
            {
                _registeredGroups[groupName] = new TickProfile
                {
                    GroupName = groupName,
                    TargetFPS = targetFps,
                    LastTickTime = EditorApplication.timeSinceStartup
                };
            }
            else
            {
                // Update FPS if already registered
                _registeredGroups[groupName].TargetFPS = targetFps;
            }
        }

        /// <summary>
        /// Unregisters a processing group so it stops ticking in Editor Mode.
        /// </summary>
        public static void UnregisterEditorGroup(string groupName)
        {
            if (_registeredGroups.ContainsKey(groupName))
            {
                _registeredGroups.Remove(groupName);
            }
        }

        /// <summary>
        /// Instantly simulates a specific processing group for a set number of ticks (Fast-Forward).
        /// </summary>
        public static void SimulateGroupFastForward(string groupName, int ticks)
        {
            for (int i = 0; i < ticks; i++)
            {
                FSM_API.FSM_API.Interaction.Update(groupName);
            }
        }

        private static void OnEditorUpdate()
        {
            // CRITICAL DECOUPLING: Yield entirely to the Runtime Integration if the game is running.
            // We no longer manually pump the runtime instance here.
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            double currentTime = EditorApplication.timeSinceStartup;
            bool uiNeedsRepaint = false;

            // 1. Tick mandatory internal Editor groups (Every frame)
            foreach (var group in EditorUpdateGroups)
            {
                FSM_API.FSM_API.Interaction.Update(group);
                uiNeedsRepaint = true;
            }

            // 2. Tick variable-rate Forge tools and visualizers (Chronos API)
            foreach (var kvp in _registeredGroups)
            {
                TickProfile profile = kvp.Value;

                // If TargetFPS is 0, tick as fast as the Editor loop allows
                if (profile.TargetFPS <= 0 || (currentTime - profile.LastTickTime) >= profile.TickInterval)
                {
                    FSM_API.FSM_API.Interaction.Update(profile.GroupName);
                    profile.LastTickTime = currentTime;
                    uiNeedsRepaint = true;
                }
            }

            // 3. Force UI/Scene Refresh if we actually pumped anything that drives UI
            if (uiNeedsRepaint)
            {
                EditorApplication.QueuePlayerLoopUpdate();
            }
        }

        private static void OnHierarchyChanged()
        {
            FSM_API.FSM_API.Interaction.Update("HierarchyChanged");
        }

        private static void OnProjectChanged()
        {
            FSM_API.FSM_API.Interaction.Update("ProjectChanged");
        }

        private static void OnSceneGUI(SceneView sv)
        {
            FSM_API.FSM_API.Interaction.Update("SceneViewGUI");
        }
    }
}