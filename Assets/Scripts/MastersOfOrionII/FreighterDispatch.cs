using Assets.Scripts.Workshop.Core.Physics.Chemistry;
using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace Assets.Scripts.MastersOfOrionII
{
    /// <summary>
    /// The Logistics Nervous System.
    /// Manages civilian freighter traffic, resource allocation, and flight telemetry.
    /// Reforged to utilize the Logistics Pipeline pattern.
    /// </summary>
    public class FreighterDispatch : IFreighterDispatch, IStateContext
    {
        // --- ISTATECONTEXT IMPLEMENTATION ---
        public bool IsValid { get; set; } = false;
        public string Name { get; set; } = "Colonial_Freighter_Dispatch";

        // --- LOGISTICS STATE ---
        private readonly List<int> _awaitingAssignment = new List<int>();
        private readonly Dictionary<Element, Dictionary<int, int>> _resourceAssignments = new Dictionary<Element, Dictionary<int, int>>();
        private readonly CivilianFreighterFleet _fleetCommand;

        public string freighterProcessGroup { get; } = "Logistics_Early"; // Mapped to the FSM Processing Group
        public FSMHandle Status { get; private set; }
        public Dictionary<int, int> InTransit { get; private set; } = new Dictionary<int, int>();

        public FreighterDispatch(CivilianFreighterFleet civilianFleet)
        {
            _fleetCommand = civilianFleet;

            // Initialization: Verify dependencies and signal health to the Forge
            if (_fleetCommand != null)
            {
                Status = FSM_API.Create.CreateInstance("FreighterDispatchFSM", this, freighterProcessGroup);
                IsValid = true;
                Debug.Log($"[{Name}] Logistics Hub Online. Tracking {_fleetCommand.Colonies.Count} colonial ports.");
            }
        }

        // --- CARGO & ORBITAL OPERATIONS ---

        public void NotifyArrivalAtColony(int freighterId, int colonyId)
        {
            if (InTransit.ContainsKey(freighterId))
            {
                InTransit.Remove(freighterId);
                _fleetCommand.Colonies[colonyId].InOrbit.Add(freighterId);
                Debug.Log($"[{Name}] Freighter {freighterId} docked at Colony {colonyId}. Orbit stabilized.");
            }
        }

        public void NotifyCargoLoaded(int freighterId)
        {
            // Transition: Once loaded, the freighter is removed from the local "AwaitingAssignment" pool
            if (_awaitingAssignment.Contains(freighterId))
            {
                _awaitingAssignment.Remove(freighterId);
                Debug.Log($"[{Name}] Freighter {freighterId} manifests verified. Preparing for departure.");
            }
        }

        public void NotifyCargoUnloaded(int freighterId)
        {
            // Once unloaded, the freighter returns to the available pool for new resource requests
            if (!_awaitingAssignment.Contains(freighterId))
            {
                _awaitingAssignment.Add(freighterId);
                Debug.Log($"[{Name}] Freighter {freighterId} holds cleared. Available for redeployment.");
            }
        }

        // --- NAVIGATION & TRANSIT ---

        public void NotifyDeparture(int freighterId, int departureColonyId, int destinationColonyId)
        {
            // Use index checking instead of ContainsKey for Lists
            if (departureColonyId >= 0 && departureColonyId < _fleetCommand.Colonies.Count)
            {
                _fleetCommand.Colonies[departureColonyId].InOrbit.Remove(freighterId);
                InTransit[freighterId] = destinationColonyId; // Use indexer for safety
                Debug.Log($"[{Name}] Freighter {freighterId} departed port {departureColonyId}.");
            }
        }

        public void RequestAssignment(int freighterId)
        {
            if (!_awaitingAssignment.Contains(freighterId))
            {
                _awaitingAssignment.Add(freighterId);
            }
        }

        /// <summary>
        /// Generates a tactical flight plan between colonial hubs.
        /// Reforged to provide organic spline-based paths for the FSM to consume.
        /// </summary>
        public List<Vector3> RequestFlightPlan(int freighterId, int colonyId)
        {
            var plan = new List<Vector3>();
            // List index check instead of TryGetValue
            if (colonyId >= 0 && colonyId < _fleetCommand.Colonies.Count)
            {
                var destination = _fleetCommand.Colonies[colonyId];
                Vector3 currentPos = GetFreighterPosition(freighterId);
                var targetPos = destination.Position;

                plan.Add(currentPos + (Vector3.up * 50f));
                plan.Add(Vector3.Lerp(currentPos, targetPos.LocalOffset, 0.5f));
                plan.Add(targetPos.LocalOffset);
            }
            return plan;
        }

        private Vector3 GetFreighterPosition(int id)
        {
            // Internal logic to fetch current telemetry from the FSM Context
            return Vector3.zero;
        }
    }
}