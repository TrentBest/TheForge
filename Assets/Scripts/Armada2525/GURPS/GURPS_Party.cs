using System.Collections.Generic;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    public class GURPS_Party
    {
        public string Name { get; set; }
        public List<GURPS3eCharacterData> Members { get; set; } = new List<GURPS3eCharacterData>();
    }
}
