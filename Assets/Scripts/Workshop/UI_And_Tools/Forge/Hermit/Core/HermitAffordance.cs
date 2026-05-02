using System;
using Workshop.Core.Diagnostics; // Injected for ForgeLogger

namespace Workshop.UI_And_Tools.Forge.Hermit.Core
{
    /// <summary>
    /// The pure data and execution logic for a specific UI control's affordance.
    /// </summary>
    public class HermitAffordance
    {
        public string ControlId { get; private set; }
        public string RoleAssumption { get; private set; }
        public string SemanticPrompt { get; private set; }
        public Action<string> OnActuate { get; private set; }

        public HermitAffordance(string controlId, string role, string prompt, Action<string> onActuate)
        {
            ControlId = controlId;
            RoleAssumption = role;
            SemanticPrompt = prompt;
            OnActuate = onActuate;

            ForgeLogger.Log($"Cognitive Affordance Registered: [{ControlId}] assumes role '{RoleAssumption}'")
                .WithHeader("Hermit Core")
                .WithColor("#CCFF00") // Electric Lime for functional capabilities
                .SendToUnity();
        }

        // Serializable payload mapping to your bridge telemetry
        [Serializable]
        public class TelemetryPayload
        {
            public string Id;
            public string Role;
            public string Prompt;
            public string State;
        }

        public TelemetryPayload GetExaminePayload()
        {
            ForgeLogger.Log($"Hermit Brain is examining Affordance: {ControlId}")
                .WithHeader("Cognition")
                .WithColor("#CCFF00")
                .SendToUnity();

            return new TelemetryPayload
            {
                Id = ControlId,
                Role = RoleAssumption,
                Prompt = SemanticPrompt,
                State = "Examined"
            };
        }
    }
}