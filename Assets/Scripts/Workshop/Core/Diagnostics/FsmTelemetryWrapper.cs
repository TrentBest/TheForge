// File: Assets/Scripts/Workshop/Core/Diagnostics/FsmTelemetryWrapper.cs
using System;
using System.Diagnostics;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.Core.Diagnostics
{
    /// <summary>
    /// Intercepts FSM execution to gather timing, execution counts, and telemetry
    /// for the Singularity Workshop Profiler and Hermit analysis.
    /// </summary>
    public static class FsmTelemetryWrapper
    {
        // Threshold where Hermit flags an operation (e.g., ~16ms for 60fps budget)
        public const long CRITICAL_TICK_WARNING_THRESHOLD = 160000; // Ticks

        /// <summary>
        /// Wraps a standard FSM action with high-precision diagnostics.
        /// </summary>
        public static Action<IStateContext> Wrap(string fsmName, string stateName, string actionType, Action<IStateContext> originalAction)
        {
            if (originalAction == null) return null;

            return (ctx) =>
            {
                var sw = Stopwatch.StartNew();

                try
                {
                    originalAction.Invoke(ctx);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[FSM Crash Prevented] {fsmName}::{stateName} threw an exception: {ex.Message}");
                    // Here we can route the crash data directly to Hermit's context
                }
                finally
                {
                    sw.Stop();
                    long ticks = sw.ElapsedTicks;

                    // Log to the telemetry bus for the sequence diagrams
                    ForgeTelemetryBus.RecordTick(fsmName, stateName, actionType, ticks);

                    if (ticks > CRITICAL_TICK_WARNING_THRESHOLD)
                    {
                        // "Scream Bloody Murder"
                        UnityEngine.Debug.LogWarning($"[Hermit Alert] {fsmName}::{stateName} is choking the thread! ({sw.ElapsedMilliseconds}ms). Chronos to optimize.");
                    }
                }
            };
        }
    }

    // Stub for the bus that the Diagnostics GUI will read from
    public static class ForgeTelemetryBus
    {
        public static void RecordTick(string fsmName, string stateName, string actionType, long ticks)
        {
            // Future: Push to a Circular Buffer or DataWarehouse so the UI Toolkit charts can map it.
        }
    }
}