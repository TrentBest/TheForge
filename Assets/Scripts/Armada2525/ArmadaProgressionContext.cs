// Armada2525/ArmadaProgressionContext.cs
using Assets.Scripts.MastersOfOrionII;
using Assets.Scripts.MastersOfOrionII.Economy;
using Assets.Scripts.MastersOfOrionII.Government;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.Armada2525
{
    public class ArmadaProgressionContext : IStateContext
    {
        public string Name { get; set; } = "Armada_Progression";
        public bool IsValid { get; set; } = true;

        // Scavenged MoOII Infrastructure
        public GalacticIndustry Industry; // From MoOII/Economy/GalacticIndustry.cs
        public GovernmentContext Politics; // From MoOII/Government/GovernmentContext.cs

        // Armada Specifics
        public int CurrentTurn = 1;
        public float GlobalTechModifier = 1.0f;
    }
}