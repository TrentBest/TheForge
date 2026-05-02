using System.Collections.Concurrent;
using UnityEngine;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.Core.Diagnostics
{
    /// <summary>
    /// Swaps native Unity logging into the Forge's SingularityDataBus.
    /// EXECUTED: 100% Static. Zero GameObject allocation.
    /// </summary>
    public static class UnityLogToForgeBridge
    {
        // Thread-safe queue for logs coming from background tasks
        private static ConcurrentQueue<LogEntry> _pendingLogs = new ConcurrentQueue<LogEntry>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeBridge()
        {
            // Clean up existing subscriptions to prevent memory leaks during domain reloads in the editor
            Application.logMessageReceivedThreaded -= HandleUnityLog;
            Application.logMessageReceivedThreaded += HandleUnityLog;

            // Hook the main thread pump to the render loop instead of an Update() method
            Application.onBeforeRender -= PumpLogQueue;
            Application.onBeforeRender += PumpLogQueue;

            Debug.Log("<b>[Diagnostics]</b> Native Unity Log Bridge Awakened.");
        }

        private static void HandleUnityLog(string logString, string stackTrace, LogType type)
        {
            // Prevent infinite loops and double-logging: 
            // If it already has our ForgeLog color tags, ignore it (we already pushed it to the bus).
            if (logString.StartsWith("<color=")) return;

            string headerId = type switch
            {
                LogType.Error => "Unity Error",
                LogType.Exception => "Unity Exception",
                LogType.Warning => "Unity Warning",
                _ => "Unity System"
            };

            string hexColor = type switch
            {
                LogType.Error => "#FF3333", // Light Red
                LogType.Exception => "#FF0000", // Solid Red
                LogType.Warning => "#FFCC00", // Yellow
                _ => "#AAAAAA" // Light Grey
            };

            _pendingLogs.Enqueue(new LogEntry
            {
                Message = type == LogType.Exception ? $"{logString}\n{stackTrace}" : logString,
                HeaderId = headerId,
                OverrideHexColor = hexColor,
                Timestamp = System.DateTime.Now
            });
        }

        private static void PumpLogQueue()
        {
            // Dispatch pending logs to the nervous system on the main thread right before we render
            while (_pendingLogs.TryDequeue(out var entry))
            {
                if (SingularityDataBus.Instance != null)
                {
                    // FIX: Replaced obsolete 'Send' with the O(1) unboxed 'SendLocal'
                    SingularityDataBus.Instance.SendLocal("Telemetry_LogAdded", entry);
                }
            }
        }
    }
}