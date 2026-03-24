// File: Assets/Scripts/Workshop/Cast/ActorDefinition.cs
using UnityEngine;

namespace TheSingularityWorkshop.Cast
{
    /// <summary>
    /// The "Soul" of a Unit. Holds stats and links to the visual representation.
    /// </summary>
    public class ActorDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string DisplayName;
        public string PrefabResourcePath;

        [Header("Attribution")]
        public float MovementSpeed = 5f;
        public float Armor = 100f;
        public float EngagementRange = 10f;

        public byte RoleId { get; internal set; }

        // FSM Links will go here later
    }
}