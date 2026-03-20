using System.Collections.Generic;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    public interface IRuleBook
    {
        string Name { get; set; }
        string Description { get; set; }

        // 0 = Core (Basic Set), 1 = Genre (Space), 2 = Setting (Armada 2525)
        int SpecificityTier { get; set; }

        // If two books have the same Specificity, the higher LoadOrder wins
        int LoadOrder { get; set; }
        
         List<string> TargetUniverseSeeds {  get; }

        
         bool DefinesNewUniverse{ get; set; }
    }
}
