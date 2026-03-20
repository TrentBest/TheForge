using TheSingularityWorkshop.FSM_API;

public class FactionTrait : IStateContext
{
    public bool IsValid { get; set; } = true;
    public string Name { get; set; }
    public TraitDomain Domain { get; set; }
    public bool IsDetrimental { get; set; }

    // Math Hooks for the Engine
    public float CostModifier { get; set; } = 1.0f;
    public float MoraleModifier { get; set; } = 1.0f;
    public float ScienceModifier { get; set; } = 1.0f;
    public float StructuralModifier { get; set; } = 1.0f;
}

public enum TraitDomain { Economic, Military, Science, Society }