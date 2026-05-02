using Assets.Scripts.Asteroids;
using System;
using UnityEngine;

namespace Assets.Scripts.Builders.GuiBuilders
{
    /// <summary>
    /// Specialized helper for drone prototypes.
    /// Bridges the gap between the raw DroneBuilder and the Swarm Architect.
    /// </summary>
    [Serializable]
    public class DroneHelper : DroneBuilder // Inheriting to satisfy the SwarmGroupGuiBuilder's list
    {
        public DroneHelper() : base() { }

        // --- FLUENT OVERRIDES ---
        public string DroneName { get; set; }
        public string BehaviorID { get; set; }

        public new DroneHelper WithName(string name)
        {
            DroneName = name;
            return this;
        }

        public new DroneHelper WithBehavior(string behaviorId)
        {
            this.BehaviorID = behaviorId;
            return this;
        }

        // Additional helper logic for "Massive Quantity" boid telemetry
        public Vector3 FlockingParameters = new Vector3(1f, 1f, 1f); // Separation, Alignment, Cohesion
    }
}