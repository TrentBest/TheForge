using System;
using System.Collections.Generic;

[Serializable]
public class GURPSSkillDefinition
{
    public string Id = Guid.NewGuid().ToString();
    public string Name;
    public string Description;
    public string SourceBookName = "GURPS Basic Set";

    public GURPSAttributeType BaseAttribute = GURPSAttributeType.IQ;
    public GURPSDifficulty Difficulty = GURPSDifficulty.Average;
    public SkillType Type = SkillType.Mental;

    // --- Tier 2: Simulation Data ---
    public int PageReference;
    public bool RequiresSpecialization = false;
    public string Specialization;
    public List<string> Prerequisites = new List<string>();

    // --- Tier 3: Tech Level Constraints ---
    public bool IsTechLevelDependent = false;
    public int TechLevel = 3;

    public string TypeString { get; internal set; }
}
