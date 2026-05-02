using Assets.Scripts.MastersOfOrionII.Economy;
using TheSingularityWorkshop.FSM_API;


namespace Assets.Scripts.MastersOfOrionII.Government
{
    public class MinistryDepartment : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "New Ministry";
        public string Description { get; set; } = "Area of governmental oversight.";

        // Links the ministry to an economic sector so they can report on it
        public GalacticIndustry OversightSector { get; set; }
    }
}