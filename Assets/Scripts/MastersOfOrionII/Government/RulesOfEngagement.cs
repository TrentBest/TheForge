using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.MastersOfOrionII.Government
{
    public class RulesOfEngagement : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public bool TakesPrisoners { get; set; } = true;
        public bool AllowsOrbitalBombardment { get; set; } = false;
        public float RetreatHullThreshold { get; set; } = 0.25f; // 25%
        public string Name { get; set; } = "Default RulesOfEngagement";
    }
}