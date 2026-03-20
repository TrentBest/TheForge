
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class GURPSTrait
{
    [SerializeField] protected string _name = "New Trait";
    [SerializeField] protected string _sourceBookName = "GURPS Basic Set (3rd Ed. Revised)";
    [SerializeField] protected int _baseCost = 0;
    [SerializeField] protected string _description = "";

    // The Math Pipeline
    [SerializeField] public List<GameEffect> Effects = new List<GameEffect>();

    // NEW: The Logic Injector! (e.g., "AbsoluteDirectionBehavior")
    [SerializeField] public string RuntimeLogicScript = "";

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
        _baseCost = 10; // Default to positive
    }

    public override int BaseCost
    {
        get => _baseCost;
        set => _baseCost = Mathf.Max(0, value); // Force to be 0 or positive!
    }
}

[Serializable]
public class GURPSDisadvantage : GURPSTrait
{
    public GURPSDisadvantage()
    {
        _name = "New Disadvantage";
        _baseCost = -10; // Default to negative
    }

    public override int BaseCost
    {
        get => _baseCost;
        set => _baseCost = Mathf.Min(0, value); // Force to be 0 or negative!
    }
}