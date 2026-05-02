using Hermit.Core;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.UI_And_Tools.Forge.Hermit.Core
{
    /// <summary>
    /// The decoupled data context required to tick the HermitBrain's FSM safely,
    /// avoiding standard Monobehaviour updates.
    /// </summary>
    public class HermitCognitiveContext : IStateContext
    {
        public HermitBrain Brain { get; private set; }
        public bool IsValid { get; set; } = true;
        public string Name { get; set; }

        public HermitCognitiveContext(HermitBrain brain)
        {
            Brain = brain;
            Name = $"CognitiveContext_{brain.Name}";
        }
    }
}