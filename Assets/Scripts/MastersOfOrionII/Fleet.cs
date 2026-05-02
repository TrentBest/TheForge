using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MastersOfOrionII
{
    // ----------------------------------------------------------------------
    // 3. FLEETS & MILITARY
    // ----------------------------------------------------------------------
    [Serializable]
    public class Fleet
    {
        public int Id;
        public string Name;          // "1st Strike Group"
        public int OwnerPlayerId;

        // --- POSITION ---
        public int CurrentSystemId;  // -1 if in deep space
        public Vector2 DeepSpacePosition; // If travelling between stars
        public bool IsMoving;
        public int DestinationSystemId;
        public float TurnsToArrival;

        // --- COMPOSITION ---
        public List<Ship> Ships = new List<Ship>();
        public FleetStance Stance;   // Aggressive, Defensive, Patrol
    }
}
