using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Hermit.Core
{
    /// <summary>
    /// Metadata woven into Forge objects that Hermit "sees".
    /// Prioritizes least-interacted objects to drive discovery.
    /// </summary>
    public interface IAffordance
    {
        string Description { get; }
        int InteractionCount { get; set; }
        float LastInteractionTime { get; set; }

        // Quantified neglect: Higher score means it's prime for "testing"
        float GetNeglectScore();
        bool ValidateEffect(); // Regression check: Did the button actually work?
        void OnInteraction();
    }
}