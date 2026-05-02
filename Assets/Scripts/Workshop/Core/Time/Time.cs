using System;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.Core.Chronos
{
    /// <summary>
    /// The systemic anchor for Chronos Logic.
    /// Allows the FSM engine to dictate the flow of the simulation clock.
    /// </summary>
    [Serializable]
    public class TemporalContext : IStateContext
    {
        // --- IStateContext Implementation ---
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "Global_Chronos_Anchor";

        // --- TEMPORAL VECTORS ---
        public float TimeScale = 1.0f;
        public float TargetTimeScale = 1.0f;
        public float DilationFactor = 0.0f; // For relativistic effects
        public bool IsPaused => TimeScale <= 0.001f;

        public void ApplyToUnity()
        {
            UnityEngine.Time.timeScale = Mathf.Clamp(TimeScale, 0f, 100f);
        }
    }

 
        /// <summary>
        /// The systemic anchor for Virtual Chronos.
        /// As an IStateContext, this allows FSMs to 'drive' the clock.
        /// </summary>
        public class TimeContext : IStateContext
        {
            public bool IsValid { get; set; } = true;
            public string Name { get; set; } = "Simulation_Clock";

            // --- TEMPORAL DATA ---
            public double TotalSeconds;
            public float LocalDeltaTime;
            public float LocalTimeScale = 1.0f;

            // --- TIME TRAVEL BUFFER ---
            // TODO: Implement a state-snapshot buffer here for 'Chronos Travel' rewinding.
        }
    }
