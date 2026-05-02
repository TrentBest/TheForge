using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.MastersOfOrionII.Government
{
    // --- THE PHILOSOPHICAL CORE ---
    public class FactionIdeology : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "New Ideology";
        public string Description { get; set; } = "The philosophical driving force of the people.";

        // Macro-Political Modifiers
        public float BaseLoyaltyModifier { get; set; } = 1.0f;
        public float PropagandaResistance { get; set; } = 1.0f;
        public float DiplomaticWeight { get; set; } = 1.0f;
    }
}