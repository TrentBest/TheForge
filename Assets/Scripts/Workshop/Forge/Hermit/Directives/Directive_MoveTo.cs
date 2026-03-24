// File: Assets/Scripts/Workshop/Forge/Hermit/Directives/Directive_MoveTo.cs
using Assets.Scripts.Workshop.Forge.Hermit.Core;
using System.Collections;
using TheSingularityWorkshop.Forge.Hermit.Core;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Hermit.Directives
{
    public class Directive_MoveTo : IHermitDirective
    {
        // This links {"type": "MoveTo"} from the JSON to this script
        public string ActionType => "MoveTo";

        
        public IEnumerator Execute(HermitAction payload, HermitChassis body)
        {
            Vector3 targetPos = new Vector3(payload.Target.x, payload.Target.y, payload.Target.z);
            body.SetTargetPosition(targetPos);

            // Wait until my chassis physically arrives at the coordinates
            while (Vector3.Distance(body.transform.position, targetPos) > 1f)
            {
                yield return null;
            }
        }
    }
}