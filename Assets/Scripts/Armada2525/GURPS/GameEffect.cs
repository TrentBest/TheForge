using System;

[Serializable]
public class GameEffect
{
    public string TargetTag = ""; // e.g., "stat.strength", "ship.evasion", "colony.production"
    public GameEffectMath MathType = GameEffectMath.FlatBonus;
    public float ValuePerLevel = 1.0f;
}