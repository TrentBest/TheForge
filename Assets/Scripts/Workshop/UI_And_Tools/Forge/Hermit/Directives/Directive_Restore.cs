using System.Collections;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge;
using Workshop.UI_And_Tools.Forge.Hermit.Core;
using Workshop.Core;

namespace Workshop.UI_And_Tools.Forge.Hermit.Directives
{
    public class Directive_Restore : IHermitDirective
    {
        public string Intent => "RestoreState";
        public string ActionType => "Temporal";
        public string Id => "DIR_RESTORE_01";
        public float Activation { get; set; }
        public float Threshold { get; set; } = 1.0f;

        public DirectiveSafetyLevel DefaultSafetyTier => DirectiveSafetyLevel.RequiresReview;

        public void Initialize(string agentId) { }
        public void ReceiveSignal(float v) => Activation += v;
        public bool IsSafeToAutoExecute(HermitDirectivePayload payload) => false;

        public IEnumerator Execute(HermitDirectivePayload payload, string agentId)
        {
            PerformRestore(agentId);
            yield break;
        }

        private void PerformRestore(string agentId)
        {
            var shelf = SingularityBootloader.MainWarehouse.GetShelf<HermitChassisData>();
            for (int i = 0; i < shelf.PageSize; i++)
            {
                if (!shelf.IsActive(i)) continue;
                ref var data = ref shelf.GetRef(i);
                if (data.AgentId.ToString() == agentId)
                {
                    if (data.HistoryCount > 0)
                    {
                        data.CurrentHistoryIndex = (data.CurrentHistoryIndex - 1 + 50) % 50;
                    }
                    break;
                }
            }
        }
    }
}