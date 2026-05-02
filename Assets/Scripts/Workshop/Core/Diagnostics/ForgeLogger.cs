using System;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.IO; // SingularityDataBus

namespace Workshop.Core.Diagnostics
{
    public class LogEntry
    {
        public string Message;
        public string HeaderId = "System";
        public string OverrideHexColor = null;
        public DateTime Timestamp;
    }

    public struct ForgeLogger
    {
        private LogEntry _entry;

        // 1. Instantaneous Creation
        public static ForgeLogger Log(string message)
        {
            var entry = new LogEntry
            {
                Message = message,
                Timestamp = DateTime.Now
            };

            // INSTANTLY push the reference to the Data Bus for the GUI to catch
            if (SingularityDataBus.Instance != null)
                SingularityDataBus.Instance.SendLocal("Telemetry_LogAdded", entry);

            return new ForgeLogger { _entry = entry };
        }

        // 2. Transient Modifiers (Updating the reference the GUI already holds)
        public ForgeLogger WithHeader(string headerId)
        {
            _entry.HeaderId = headerId;
            return this;
        }

        // Accepts a raw hex string (e.g., "#FF00FF")
        public ForgeLogger WithColor(string hexColor)
        {
            _entry.OverrideHexColor = hexColor;
            return this;
        }

        // NEW: Accepts a native Unity Color and converts it to a hex string
        public ForgeLogger WithColor(Color color)
        {
            _entry.OverrideHexColor = "#" + ColorUtility.ToHtmlStringRGBA(color);
            return this;
        }

        // 3. Optional: Fire to standard Unity Console (since Unity's console can't read by reference)
        public void SendToUnity()
        {
            var config = LogRegistry.GetChannel(_entry.HeaderId);
            // Use standard Unity Debug.Log here to bridge to the native console
            string color = _entry.OverrideHexColor ?? (config != null ? config.HexColor : "#FFFFFF");
            
            Debug.Log($"<color={color}>[{_entry.HeaderId}]</color> {_entry.Message}");
        }

        public static ForgeLogger LogWarning(string message)
        {
            return Log(message).WithHeader("Warning").WithColor(Color.yellow);
        }

        public static ForgeLogger LogError(string message)
        {
            return Log(message).WithHeader("Error").WithColor(Color.red);
        }
    }
}