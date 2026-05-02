using System;
using System.Collections.Generic;
using UnityEngine;

namespace Workshop.Systems.MetaDev
{
    [Serializable]
    public class ExperienceManifest
    {
        public string ExperienceId;
        public string Version;

        // --- THE PUBLISHED ARRAYS ---
        // The dependency-ordered list of Package IDs required for boot
        public List<string> PackageBootOrder = new List<string>();

        // Maps PackageId -> A list of specific Deltas (Arbitration outcomes) to apply on load
        public Dictionary<string, List<string>> PackageConfigurationDeltas = new Dictionary<string, List<string>>();

        // --- PERFORMANCE TELEMETRY ---
        public Dictionary<string, int> AllocationHighWaterMarks = new Dictionary<string, int>();
        public Dictionary<string, long> ComputeTickBudgets = new Dictionary<string, long>();

        public ExperienceManifest(string id)
        {
            ExperienceId = id;
            Version = Guid.NewGuid().ToString().Substring(0, 8);
        }

        public void RecordHighWaterMark(string typeName, int count)
        {
            if (!AllocationHighWaterMarks.ContainsKey(typeName) || count > AllocationHighWaterMarks[typeName])
            {
                AllocationHighWaterMarks[typeName] = count;
                Debug.Log($"<b>[MetaDev]</b> 📈 High-Water Mark: {typeName} reached {count}");
            }
        }

        public void RecordArbitrationDelta(string targetPackageId, string payload)
        {
            if (!PackageConfigurationDeltas.ContainsKey(targetPackageId))
                PackageConfigurationDeltas[targetPackageId] = new List<string>();

            if (!PackageConfigurationDeltas[targetPackageId].Contains(payload))
            {
                PackageConfigurationDeltas[targetPackageId].Add(payload);
                Debug.Log($"<b>[MetaDev]</b> 🖋️ Recorded Config Delta for {targetPackageId}: {payload}");
            }
        }
    }
}