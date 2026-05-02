using System;
using UnityEngine;

namespace Workshop.GURPS
{
    /// <summary>
    /// The Atomic Blueprint for all structural entities in the Workshop.
    /// Expanded with "God Mode" properties for high-fidelity simulation.
    /// </summary>
    [Serializable]
    public class GURPSBuildingTemplate
    {
        public string Id = Guid.NewGuid().ToString();
        public string Name = "New Blueprint";

        // --- GURPS STATS ---
        public int TechLevel = 3;
        public int DR = 10;        // Damage Resistance
        public int HP = 1000;      // Structural Hit Points
        public float SquareFootage = 1000f;
        public string Purpose = "General"; // Residential, Military, etc.

        // --- ECONOMY & LOGISTICS ---
        public int ConstructionCost = 5000;
        public int MonthlyMaintenance = 50;
        public float PowerRequired = 0f;

        // --- VISUAL LINK ---
        public string PrefabAddress; // For the addressable system to spawn the mesh
    }
}