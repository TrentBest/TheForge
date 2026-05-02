using System.Collections;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge;
using Workshop.UI_And_Tools.Forge.Hermit.Core;
using Workshop.Core;

namespace Workshop.UI_And_Tools.Forge.Hermit.Directives
{
    public class Directive_MoveTo : IHermitDirective
    {
        public string Intent => "MoveTo";
        public string ActionType => "Locomotion";
        public string Id => "DIRECTIVE_MOVE_01";
        public float Activation { get; set; }
        public float Threshold { get; set; }

        public DirectiveSafetyLevel DefaultSafetyTier => DirectiveSafetyLevel.AutoExecute;

        public void Initialize(string agentId) { }
        public void ReceiveSignal(float v) { }
        public bool IsSafeToAutoExecute(HermitDirectivePayload payload) => true;

        public IEnumerator Execute(HermitDirectivePayload payload, string agentId)
        {
            ApplyMovement(payload, agentId);
            yield break;
        }

        private void ApplyMovement(HermitDirectivePayload payload, string agentId)
        {
            var shelf = SingularityBootloader.MainWarehouse.GetShelf<HermitChassisData>();
            for (int i = 0; i < shelf.PageSize; i++)
            {
                if (!shelf.IsActive(i)) continue;

                ref var data = ref shelf.GetRef(i);
                if (data.AgentId.ToString() == agentId)
                {
                    data.TargetPosition = payload.TargetPosition;
                    data.CurrentState = HermitAIState.Executing;
                    break;
                }
            }
        }
    }
}