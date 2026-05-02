using System.Collections;
using UnityEngine;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge;
using Workshop.UI_And_Tools.Forge.Hermit.Core;
using Workshop.Core;

namespace Workshop.UI_And_Tools.Forge.Hermit.Directives
{
    public class Directive_SpawnMini : IHermitDirective
    {
        public string Intent => "SpawnMini";
        public string ActionType => "Spawning";
        public string Id => "DIR_SPAWN_MINI";
        public float Activation { get; set; }
        public float Threshold { get; set; } = 1.0f;

        public DirectiveSafetyLevel DefaultSafetyTier => DirectiveSafetyLevel.AutoExecute;

        public void Initialize(string agentId) { }
        public void ReceiveSignal(float v) => Activation += v;
        public bool IsSafeToAutoExecute(HermitDirectivePayload payload) => true;

        public IEnumerator Execute(HermitDirectivePayload payload, string agentId)
        {
            // 1. Determine Spatial Coordinates
            Vector3 spawnPos = payload.TargetPosition;
            if (spawnPos == Vector3.zero)
            {
                if (Camera.main != null)
                {
                    spawnPos = Camera.main.transform.position + (Camera.main.transform.forward * 2.0f);
                    spawnPos.y -= 0.5f;
                }
                else
                {
                    spawnPos = new Vector3(Random.Range(-2f, 2f), 0.125f, Random.Range(-2f, 2f));
                }
            }

            // 2. The Task - If no explicit task, default to Idle Compute Mocking
            string minionTask = string.IsNullOrEmpty(payload.Content)
                ? "Diegetic Idle: Dusting the Forge"
                : payload.Content;

            // 3. Synchronous memory manifestation (Bypasses C# 9.0 'ref' iterator limits)
            string newMinionId = ManifestSovereignMinion(agentId, spawnPos, minionTask);

            if (newMinionId != "ERR_NO_MEMORY")
            {
                ForgeLogger.Log($"<color=#00FFFF>[Hermit DataBus]</color> Minion '{newMinionId}' manifested at {spawnPos}. Task: {minionTask}");
            }
            else
            {
                ForgeLogger.LogWarning($"<color=#FF0000>[Hermit DataBus]</color> Minion manifestation failed. Warehouse shelf is full.");
            }

            yield break;
        }

        /// <summary>
        /// Synchronous worker method to allocate the Unmanaged Data and conjure the Slave Renderer.
        /// </summary>
        private string ManifestSovereignMinion(string bossId, Vector3 spawnPos, string task)
        {
            var shelf = SingularityBootloader.MainWarehouse.GetShelf<HermitChassisData>();

            // Find the first free slot in the unmanaged memory pool
            for (int i = 0; i < shelf.PageSize; i++)
            {
                if (!shelf.IsActive(i))
                {
                    // We found an empty slot! Let's build the Minion.
                    string minionId = $"Mini_{System.Guid.NewGuid().ToString().Substring(0, 5)}";

                    // Initialize the pure POD struct
                    var minionData = HermitChassisData.Create(minionId, spawnPos);

                    // Assign state based on task
                    if (task.Contains("Dusting") || task.Contains("Cleaning") || task.Contains("Idle"))
                    {
                        // They are just busy-bodies roaming the Forge
                        minionData.CurrentState = HermitAIState.Idle;
                    }
                    else
                    {
                        // They are assigned to a pending bus proposal waiting for human approval
                        minionData.CurrentState = HermitAIState.Executing;
                    }

                    // CRITICAL: We tell the Physical System to build the visual Slave Renderer.
                    // (This handles the 1/8th scale, the Amber safety colors, and the instance ID tracking)
                    HermitPhysicalSystem.ManifestChassis(ref minionData, isMinion: true);

                    // Lock the data into the Sovereign Warehouse
                    shelf.Store(minionData);

                    return minionId;
                }
            }

            return "ERR_NO_MEMORY";
        }
    }
}