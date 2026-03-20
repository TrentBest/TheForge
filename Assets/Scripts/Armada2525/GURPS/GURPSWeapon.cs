using System;

[Serializable]
public class GURPSWeapon
{
    public string Name = "New Weapon";
    public string SourceBookName = "GURPS Basic Set";
    public string Description = "";
    public string ImagePath = "";
    public string SchematicPath = "";

    // Core Stats
    public int TechLevel = 7;
    public string DamageString = "1d pi";
    public int Cost = 0;
    public float Weight = 1.0f;
    public int MinimumStrength = 10;
    public int LegalityClass = 4;

    // 3rd Edition Ultra-Tech Combat Stats
    public string Malfunction = "Crit"; // "Crit", "17", "Ver", etc.
    public string SnapShot = "10";      // SS
    public string Accuracy = "0";       // Acc
    public string HalfDamageRange = "100"; // 1/2D
    public string MaxRange = "1000";    // Max
    public string RateOfFire = "1";     // RoF
    public string Shots = "1";          // Shots
    public string Recoil = "1";         // Rcl

    // Logistical / Contextual
    public string ReloadTime = "3";
    public string UsageBehavior = "One-handed";

    public string Range { get; internal set; }
    public string Bulk { get; internal set; }
}