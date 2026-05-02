using System.Collections;
using UnityEngine;
using Workshop.Core;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge;
using Workshop.UI_And_Tools.Forge.Hermit.Core;

namespace Workshop.UI_And_Tools.Forge.Hermit.Directives
{
    public class Directive_Trash : IHermitDirective
    {
        public string Intent => "TrashObject";
        public string ActionType => "Deconstruct";
        public string Id => "DIR_TRASH_01";
        public float Activation { get; set; }
        public float Threshold { get; set; } = 1.0f;

        public DirectiveSafetyLevel DefaultSafetyTier => DirectiveSafetyLevel.RequiresReview;

        public void Initialize(string agentId) { }
        public void ReceiveSignal(float v) => Activation += v;
        public bool IsSafeToAutoExecute(HermitDirectivePayload payload) => false;

        public IEnumerator Execute(HermitDirectivePayload payload, string agentId)
        {
            string targetName = !string.IsNullOrEmpty(payload.TargetId) ? payload.TargetId : payload.Content;
            ForgeLogger.Log($"[Hermit: {agentId}] Acknowledged Trash directive for '{targetName}'.");

            // 1. Sync lookup of the Chassis and its Slave Renderer
            var slave = GetChassisSlave(agentId);
            if (slave == null) yield break;

            var coreLight = slave.GetComponentInChildren<Light>();
            Color originalColor = coreLight != null ? coreLight.color : Color.white;
            float originalIntensity = coreLight != null ? coreLight.intensity : 1f;
            Color deconstructRed = new Color(0.8f, 0.1f, 0.1f);

            // 2. Movement logic via the Warehouse
            UpdateTargetPosition(agentId, payload.TargetPosition);

            // Wait for proximity
            while (Vector3.Distance(slave.transform.position, payload.TargetPosition) > 1.5f)
            {
                if (coreLight != null)
                {
                    coreLight.color = Color.Lerp(originalColor, deconstructRed, Mathf.PingPong(Time.time * 2f, 1f));
                }
                yield return null;
            }

            // 3. Pulsing Deconstruction Effect
            if (coreLight != null)
            {
                coreLight.color = deconstructRed;
                float pulseTime = 1.0f;
                float elapsed = 0f;
                while (elapsed < pulseTime)
                {
                    elapsed += Time.deltaTime;
                    coreLight.intensity = originalIntensity + Mathf.PingPong(elapsed * 10f, 5f);
                    yield return null;
                }
            }

            // 4. Final Reality Removal
            GameObject targetObj = GameObject.Find(targetName);
            if (targetObj != null) targetObj.SetActive(false);

            if (coreLight != null)
            {
                coreLight.color = Color.green;
                coreLight.intensity = originalIntensity * 2f;
                yield return new WaitForSeconds(0.3f);
                coreLight.color = originalColor;
                coreLight.intensity = originalIntensity;
            }

            ForgeLogger.Log($"[Hermit: {agentId}] Trash operation finalized.");
        }

        private GameObject GetChassisSlave(string agentId)
        {
            var shelf = SingularityBootloader.MainWarehouse.GetShelf<HermitChassisData>();
            for (int i = 0; i < shelf.PageSize; i++)
            {
                if (!shelf.IsActive(i)) continue;
                var data = shelf.GetRef(i);
                if (data.AgentId.ToString() == agentId)
                {
                    // Fix: Properly convert the int ID to the GameObject
#if UNITY_EDITOR
                    return UnityEditor.EditorUtility.InstanceIDToObject(data.RootObjectId) as GameObject;
#else
                    GameObject go = GameObject.Find($"MiniHermit_{agentId}");
                    if (go == null) go = GameObject.Find($"HermitBoss_{agentId}");
                    return go;
#endif
                }
            }
            return null;
        }

        private void UpdateTargetPosition(string agentId, Vector3 pos)
        {
            var shelf = SingularityBootloader.MainWarehouse.GetShelf<HermitChassisData>();
            for (int i = 0; i < shelf.PageSize; i++)
            {
                if (!shelf.IsActive(i)) continue;
                ref var data = ref shelf.GetRef(i);
                if (data.AgentId.ToString() == agentId)
                {
                    data.TargetPosition = pos;
                    data.CurrentState = HermitAIState.Executing;
                    break;
                }
            }
        }
    }
}