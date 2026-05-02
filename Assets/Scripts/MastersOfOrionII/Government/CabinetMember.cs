using TheSingularityWorkshop.FSM_API;


namespace Assets.Scripts.MastersOfOrionII.Government
{
    public class CabinetMember : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "Unknown Bureaucrat";
        public MinistryDepartment Department { get; set; }

        // Cost to keep them loyal per month
        public long Salary { get; set; } = 150000;
        public float Loyalty { get; set; } = 1.0f;
    }
}