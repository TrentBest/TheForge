using System.Collections;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge;

namespace Workshop.UI_And_Tools.Forge.Hermit.Directives
{
    public interface IHermitDirective
    {
        // --- Identity & FSM Integration ---
        string Intent { get; }
        string ActionType { get; }
        string Id { get; }
        float Activation { get; set; }
        float Threshold { get; set; }

        // --- Safety & Flow ---
        DirectiveSafetyLevel DefaultSafetyTier { get; }
        bool IsSafeToAutoExecute(HermitDirectivePayload payload);

        // --- Lifecycle ---
        void Initialize(string agentId);
        void ReceiveSignal(float v);

        // --- Execution (Standardized for Sovereign Chassis Data) ---
        IEnumerator Execute(HermitDirectivePayload payload, string agentId);
    }
}