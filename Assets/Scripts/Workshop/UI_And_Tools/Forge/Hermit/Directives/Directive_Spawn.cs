using System.Collections;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge;
using Workshop.UI_And_Tools.Forge.Hermit.Core;

namespace Workshop.UI_And_Tools.Forge.Hermit.Directives
{
    public class Directive_Spawn : IHermitDirective
    {
        public string Intent => "SpawnObject";
        public string ActionType => "Manifestation";
        public string Id => "DIR_SPAWN_01";
        public float Activation { get; set; }
        public float Threshold { get; set; } = 1.0f;

        public DirectiveSafetyLevel DefaultSafetyTier => DirectiveSafetyLevel.RequiresReview;

        public void Initialize(string agentId) { }
        public void ReceiveSignal(float v) => Activation += v;
        public bool IsSafeToAutoExecute(HermitDirectivePayload payload) => false;

        public IEnumerator Execute(HermitDirectivePayload payload, string agentId)
        {
            ForgeLogger.Log($"[Hermit: {agentId}] Spawning manifest: {payload.Content}")
                .WithHeader("Manifestation")
                .SendToUnity();
            yield break;
        }
    }
}