using System;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    [Serializable]
    public class TechnologyCategory
    {
        public string Name = "New Category";
        public string Description = "";
        public string SourceBookName = "GURPS Basic Set (3rd Ed. Revised)";

        public TechnologyCategory() { }
        public TechnologyCategory(string name, string desc = "", string sourceBook = "GURPS Basic Set (3rd Ed. Revised)")
        {
            Name = name;
            Description = desc;
            SourceBookName = sourceBook;
        }

        public override string ToString() => Name;
    }
}