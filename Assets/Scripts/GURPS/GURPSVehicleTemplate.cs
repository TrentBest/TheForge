using System;
using UnityEngine;

namespace Workshop.GURPS
{
    /// <summary>
    /// The Atomic Blueprint for all vehicles (Hulls, Frames, Chasis).
    /// Expanded with GURPS-compliant performance and logistics metrics.
    /// </summary>
    [Serializable]
    public class GURPSVehicleTemplate
    {
        public string Id = Guid.NewGuid().ToString();
        public string Name = "New Vehicle Design";
        public int TechLevel = 8;

        // --- PERFORMANCE (GURPS STANDARDS) ---
        public float MaxVelocity;    // Move units or LY/day
        public float Acceleration;   // Gs or move/turn increment
        public int Handling;         // +/- modifier to piloting rolls
        public int Stability;        // Resistance to wipeouts/turbulence

        // --- LOGISTICS & CAPACITY ---
        public float FuelCapacity;
        public float FuelConsumptionRate; // Units per LY or Hour
        public int CargoCapacityTons;
        public int CrewRequired = 1;
        public int PassengerCapacity = 0;

        // --- SURVIVABILITY ---
        public int DR = 20;          // Damage Resistance
        public int HP = 100;         // Structural Hit Points
        public float SM = 0;         // Size Modifier

        // --- VISUAL & ENGINE DATA ---
        public string PrefabAddress; // Addressable link for the manifestor
        public Color HullTint = Color.white;
    }
}