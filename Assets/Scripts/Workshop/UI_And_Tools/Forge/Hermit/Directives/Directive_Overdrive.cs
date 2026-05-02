using System.Collections;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge;
using Workshop.UI_And_Tools.Forge.Hermit.Core;
using Workshop.Core;

namespace Workshop.UI_And_Tools.Forge.Hermit.Directives
{
    public class Directive_Overdrive : IHermitDirective
    {
        public string Intent => "Overdrive";
        public string ActionType => "StateShift";
        public string Id => "DIR_OVERDRIVE_01";
        public float Activation { get; set; }
        public float Threshold { get; set; } = 1.0f;

        public DirectiveSafetyLevel DefaultSafetyTier => DirectiveSafetyLevel.AutoExecute;

        public void Initialize(string agentId) { }
        public void ReceiveSignal(float v) => Activation += v;
        public bool IsSafeToAutoExecute(HermitDirectivePayload payload) => true;

        public IEnumerator Execute(HermitDirectivePayload payload, string agentId)
        {
            ApplyOverdrive(agentId);
            yield break;
        }

        private void ApplyOverdrive(string agentId)
        {
            var shelf = SingularityBootloader.MainWarehouse.GetShelf<HermitChassisData>();
            for (int i = 0; i < shelf.PageSize; i++)
            {
                if (!shelf.IsActive(i)) continue;
                ref var data = ref shelf.GetRef(i);
                if (data.AgentId.ToString() == agentId)
                {
                    data.CurrentState = HermitAIState.Executing;
                    break;
                }
            }
        }
    }
}