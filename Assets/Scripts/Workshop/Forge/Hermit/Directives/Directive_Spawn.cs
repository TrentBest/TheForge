// File: Assets/Scripts/Workshop/Forge/Hermit/Directives/Directive_Spawn.cs
using Assets.Scripts.Workshop.Forge.Hermit.Core;
using System.Collections;
using TheSingularityWorkshop.Forge.Hermit.Core;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Hermit.Directives
{
    public class Directive_Spawn : IHermitDirective
    {
        public string ActionType => "Spawn";

        public IEnumerator Execute(HermitAction payload, HermitChassis body)
        {
            // In a full implementation, this would read your DataWarehouse
            // For now, we simulate materializing a primitive

            Debug.Log($"[Hermit] Manifesting {payload.AssetId} into reality...");

            // Make the AI pulse bright yellow/white while "fabricating"
            body.CoreLight.color = Color.white;
            body.CoreLight.intensity = 5f;

            // Fake fabrication time based on asset complexity
            yield return new WaitForSeconds(1.5f);

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = payload.AssetId;
            go.transform.position = new Vector3(payload.Target.x, payload.Target.y, payload.Target.z);

            // Restore normal execution light
            body.CoreLight.intensity = 1f;
            body.SetState(HermitChassis.AIState.Executing);
        }
    }
}