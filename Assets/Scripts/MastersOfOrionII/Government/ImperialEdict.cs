using TheSingularityWorkshop.FSM_API;


namespace Assets.Scripts.MastersOfOrionII.Government
{
    public class ImperialEdict : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "New Law";
        public string Description { get; set; } = "A policy affecting the empire.";

        // Edicts cost political capital or raw credits to maintain
        public long UpkeepCost { get; set; } = 0;
    }
}