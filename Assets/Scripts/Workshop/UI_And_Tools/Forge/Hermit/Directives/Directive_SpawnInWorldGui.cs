using System.Collections;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge;
using Workshop.UI_And_Tools.Forge.Hermit.Core;

namespace Workshop.UI_And_Tools.Forge.Hermit.Directives
{
    public class Directive_SpawnInWorldGui : IHermitDirective
    {
        public string Intent => "SpawnInWorldGui";
        public string ActionType => "Interface";
        public string Id => "DIR_WORLDGUI_01";
        public float Activation { get; set; }
        public float Threshold { get; set; } = 1.0f;

        public DirectiveSafetyLevel DefaultSafetyTier => DirectiveSafetyLevel.InertDisplay;

        public void Initialize(string agentId) { }
        public void ReceiveSignal(float v) => Activation += v;
        public bool IsSafeToAutoExecute(HermitDirectivePayload payload) => true;

        public IEnumerator Execute(HermitDirectivePayload payload, string agentId)
        {
            yield break;
        }
    }
}