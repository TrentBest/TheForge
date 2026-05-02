using System.Collections.Generic;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public static class EditorStagingManager
    {
        private static float _lastSwapSign = 1f;
        private static List<GameObject> _activeGhosts = new List<GameObject>();

        public static void RegisterGhost(GameObject ghost) => _activeGhosts.Add(ghost);
        public static Vector3 GetGuiStagePosition(Transform playerTransform)
        {
            return playerTransform.position - (playerTransform.forward * 500f);
        }

        public static Vector3 GetNextLmpPosition(Vector3 currentGuiPos)
        {
            // Swap between -500 and 500 relative to the GUI Stage to prevent ghosting
            _lastSwapSign *= -1f;
            return currentGuiPos + (Vector3.right * 500f * _lastSwapSign);
        }

        public static void CleanupCurrentStage()
        {
            foreach (var obj in _activeGhosts)
            {
                if (obj != null)
                {
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(obj);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(obj);
                    }
                }
            }
            _activeGhosts.Clear();
        }
    }
}