using System.Collections;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge;
using Workshop.UI_And_Tools.Forge.Hermit.Core;
using Workshop.Core;

namespace Workshop.UI_And_Tools.Forge.Hermit.Directives
{
    public class Directive_ScribeScript : IHermitDirective
    {
        // --- Interface Implementation ---
        public string Intent => "ProposeScript";
        public string ActionType => "ScribeScript";
        public string Id => "DIRECTIVE_SCRIBE_01";
        public float Activation { get; set; }
        public float Threshold { get; set; }

        public DirectiveSafetyLevel DefaultSafetyTier => DirectiveSafetyLevel.RequiresReview;

        public void Initialize(string agentId) { }
        public void ReceiveSignal(float v) { }
        public bool IsSafeToAutoExecute(HermitDirectivePayload payload) => false;

        public IEnumerator Execute(HermitDirectivePayload payload, string agentId)
        {
            // 1. Diegetic setup: The Scribe Drone routes to the Code Forge
            // We pull metadata from the warehouse synchronously before yielding
            string droneIdentity = GetDroneLogName(agentId);

            ForgeLogger.Log($"[Minion: {droneIdentity}] Routing to Code Forge to scribe {payload.TargetId}...");

            // Simulate travel/processing time
            yield return new WaitForSeconds(1.5f);

            // 2. The actual file I/O (Your Core Logic)
            string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, payload.TargetId));
            string directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                File.WriteAllText(fullPath, payload.Content);

                ForgeLogger.Log($"Minion {droneIdentity} successfully scribed script at: {payload.TargetId}")
                           .WithHeader("Code Forge")
                           .SendToUnity();

#if UNITY_EDITOR
                // 3. Force Unity to detect the new file
                AssetDatabase.Refresh();
#endif
            }
            catch (System.Exception ex)
            {
                ForgeLogger.Log($"Scribe Failure for {payload.TargetId}: {ex.Message}")
                           .WithHeader("Network Fault")
                           .SendToUnity();
            }
        }

        /// <summary>
        /// Synchronous worker to get the AgentId from the Warehouse without 
        /// triggering the C# 9.0 iterator 'ref' restriction.
        /// </summary>
        private string GetDroneLogName(string agentId)
        {
            var shelf = SingularityBootloader.MainWarehouse.GetShelf<HermitChassisData>();
            for (int i = 0; i < shelf.PageSize; i++)
            {
                if (!shelf.IsActive(i)) continue;

                // Read-only access is safe here
                if (shelf.GetRef(i).AgentId.ToString() == agentId)
                {
                    return agentId;
                }
            }
            return "Unknown_Drone";
        }
    }
}