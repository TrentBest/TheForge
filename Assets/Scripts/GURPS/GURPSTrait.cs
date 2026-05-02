using System;
using System.Collections.Generic;
using UnityEngine;

namespace Workshop.GURPS
{
    [Serializable]
    public abstract class GURPSTrait
    {
        [SerializeField] public string Id = Guid.NewGuid().ToString();
        [SerializeField] protected string _name = "New Trait";
        [SerializeField] protected string _sourceBookName = "GURPS Basic Set (3rd Ed. Revised)";
        [SerializeField] protected int _baseCost = 0;
        [SerializeField] protected string _description = "";

        // --- Tier 2: The Math Pipeline ---
        // Allows the trait to modify attributes (e.g., +1 to DX)
        [SerializeField] public List<GameEffect> Effects = new List<GameEffect>();

        // --- Tier 3: The Logic Injector ---
        // Reference to a script/class that handles unique behavior (e.g., "AbsoluteDirectionBehavior")
        [SerializeField] public string RuntimeLogicScript = "";

        // --- Tier 4: Heavy Metadata ---
        public AdvantageCategory Category = AdvantageCategory.Physical;
        public bool IsSupernatural = false;
        public List<string> Prerequisites = new List<string>();

        public string Name { get => _name; set => _name = value; }
        public string SourceBookName { get => _sourceBookName; set => _sourceBookName = value; }
        public string Description { get => _description; set => _description = value; }

        // Virtual so derived classes can enforce positive/negative rules
        public virtual int BaseCost { get => _baseCost; set => _baseCost = value; }
    }


    [Serializable]
    public class GURPSAdvantage : GURPSTrait
    {
        public GURPSAdvantage()
        {
            _name = "New Advantage";
            _baseCost = 10;
        }

        public override int BaseCost
        {
            get => _baseCost;
            set => _baseCost = Mathf.Max(0, value); // Force to be 0 or positive
        }
    }

    public enum AdvantageCategory { Physical, Mental, Social, Exotic, Supernatural }
    public enum AdvantageType { Advantage, Disadvantage, Perk, Flaw }

    [Serializable]
    public class GURPSDisadvantage : GURPSTrait
    {
        public GURPSDisadvantage()
        {
            _name = "New Disadvantage";
            _baseCost = -10;
        }

        public override int BaseCost
        {
            get => _baseCost;
            set => _baseCost = Mathf.Min(0, value); // Force to be 0 or negative
        }
    }
}