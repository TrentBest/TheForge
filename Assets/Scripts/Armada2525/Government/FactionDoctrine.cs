using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Armada2525.Government
{
    // --- THE MILITARY & ECONOMIC PLAYSTYLE ---
    public class FactionDoctrine : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "New Doctrine";
        public string Description { get; set; } = "How this faction approaches warfare and industry.";

        // Shipyard & Physics Modifiers! (These hook directly into our Ship Designer)
        public float HullCostMultiplier { get; set; } = 1.0f;
        public float EngineThrustMultiplier { get; set; } = 1.0f;
        public float StructuralShearTolerance { get; set; } = 1.0f; // e.g., Scrappers get 2.0x
        public float CrewVolumeRequirement { get; set; } = 1.0f; // Cybernetics get 0.1x

        // Economic Modifiers
        public float IndustrialOutputMultiplier { get; set; } = 1.0f;
        public float EspionageEfficiency { get; set; } = 1.0f;
    }
}