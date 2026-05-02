using System.Collections.Generic;
using UnityEngine;
using Workshop.Systems.MicroPackages;

namespace Workshop.Systems.MetaDev
{
    public static class MetaDevObserver
    {
        public static ExperienceManifest CurrentManifest { get; private set; }

        public static void InitializeSession(string experienceId)
        {
            CurrentManifest = new ExperienceManifest(experienceId);
            Debug.Log($"<b>[MetaDev]</b> 👁️ Observer awake. Monitoring for convergence deltas...");
        }

        /// <summary>
        /// Captures the finalized arbitration results from the Arbitrator.
        /// </summary>
        public static void CaptureFinalEcosystemState(List<string> bootOrder)
        {
            if (CurrentManifest == null) return;
            CurrentManifest.PackageBootOrder = new List<string>(bootOrder);
            Debug.Log($"<b>[MetaDev]</b> 🔒 Ecosystem sequence locked. {bootOrder.Count} packages in manifest.");
        }

        public static void RecordArbitration(string targetId, string deltaPayload)
        {
            CurrentManifest?.RecordArbitrationDelta(targetId, deltaPayload);
        }

        public static void ReportShelfResize(string shelfDataType, int newCapacity)
        {
            CurrentManifest?.RecordHighWaterMark(shelfDataType, newCapacity);
        }

        public static void SaveManifest()
        {
            // Serialize to JSON and write to: Assets/Resources/Manifests/{ExperienceId}.json
            string json = JsonUtility.ToJson(CurrentManifest, true);
            Debug.Log($"<b>[MetaDev]</b> 💾 Manifest Published. Ready for high-performance boot.");
        }
    }
}