// File: Assets/Scripts/Workshop/Stage/PropDefinition.cs
using UnityEngine;

namespace Workshop.Gameplay.Stage
{
    /// <summary>
    /// The "Soul" of a Building or Scenery.
    /// </summary>
    public class PropDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string DisplayName;
        public string PrefabResourcePath;

        [Header("Physicality")]
        public Vector3 BoundingBox;
        public bool IsObstacle = true;
        public bool IsDestructible = false;
    }
}