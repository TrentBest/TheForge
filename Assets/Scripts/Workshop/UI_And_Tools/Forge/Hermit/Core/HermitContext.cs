using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge;
using Workshop.UI_And_Tools.Forge.Hermit.Directives;

namespace Workshop.UI_And_Tools.Forge.Core
{
    public class HermitContext : IStateContext
    {
        public string AgentId { get; set; }
        public Transform Transform { get; set; }
        public Rigidbody Rigidbody { get; set; }
        public Light CoreLight { get; set; }

        public bool IsValid { get; set; } = true;
        public string Name { get; set; }

        // --- Required for Subconscious Execution ---
        public IHermitDirective ActiveDirective { get; set; }
        public HermitDirectivePayload CurrentPayload { get; set; }
        public bool DirectiveComplete { get; set; }

        // --- Discovery & Senses ---
        public List<ISense> ActiveSenses { get; set; } = new List<ISense>();
        public List<PerceptionData> SensedEnvironment { get; set; } = new List<PerceptionData>();
        public Vector3 TargetPosition { get; internal set; }

        public HermitContext(string agentId, Transform transform, Rigidbody rb, Light coreLight)
        {
            AgentId = agentId;
            Transform = transform;
            Rigidbody = rb;
            CoreLight = coreLight;
            Name = $"Hermit_{agentId.Substring(0, 4)}";
        }
    }
}