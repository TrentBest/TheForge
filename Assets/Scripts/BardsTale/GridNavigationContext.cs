using UnityEngine;

namespace Workshop.BardsTale.Navigation
{
    /// <summary>
    /// The decoupled data payload for grid-based movement.
    /// Operates entirely on integers and cardinal directions.
    /// </summary>
    public class GridNavigationContext
    {
        public Vector2Int CurrentPosition { get; set; } = Vector2Int.zero;

        // Represents North (0,1), East (1,0), South (0,-1), West (-1,0)
        public Vector2Int FacingDirection { get; set; } = new Vector2Int(0, 1);

        // State flags for the FSM to process transitions
        public bool IsTransitioning { get; set; }
        public Vector2Int TargetPosition { get; set; }
        public Vector2Int TargetFacing { get; set; }

        // Optional: Interpolation data if we want smooth camera slides instead of instant snaps
        public float TransitionProgress { get; set; }
        public float MoveSpeed { get; set; } = 3.0f; // Blocks per second
        public float TurnSpeed { get; set; } = 90.0f; // Degrees per second
    }
}