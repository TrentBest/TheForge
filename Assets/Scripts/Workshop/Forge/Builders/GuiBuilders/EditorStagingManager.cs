using System.Collections.Generic;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
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
            for (int i = _activeGhosts.Count - 1; i >= 0; i--)
            {
                if (_activeGhosts[i] != null) GameObject.Destroy(_activeGhosts[i]);
            }
            _activeGhosts.Clear();
        }
    }
}