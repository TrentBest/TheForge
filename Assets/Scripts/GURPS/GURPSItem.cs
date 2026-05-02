using System;

namespace Workshop.GURPS
{
    [Serializable]
    public class GURPSItem
    {
        public string Name = "New Weapon";
        public string DamageString = "1d pi";
        public int Cost = 0;

        // Required for the GURPS_CRUD_Builder Book Filter
        public string SourceBookName = "GURPS Basic Set (3rd Ed. Revised)";
    }
}