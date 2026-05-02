using System.Collections.Generic;

namespace Workshop.GURPS
{
    public class GURPS_Gui_PartyBuilder
    {
        public string Name { get; set; }
        public List<GURPS3eCharacterData> Members { get; set; } = new List<GURPS3eCharacterData>();
    }
}
