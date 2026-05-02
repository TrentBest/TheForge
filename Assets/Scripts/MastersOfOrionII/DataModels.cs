using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Workshop.GURPS;

namespace Assets.Scripts.MastersOfOrionII
{
}


public class GURPS3eCharacterData
{
    public string CharacterName = "Nameless Adventurer";
    public string PlayerName = "Player";
    public int StartingPoints = 100;

    // Core Attributes (3e baseline is 10)
    public int ST = 10;
    public int DX = 10;
    public int IQ = 10;
    public int HT = 10;

    // Derived Characteristics (GURPS 3rd Edition Rules)
    public int HP => HT;
    public int Fatigue => ST;
    public float BasicSpeed => (HT + DX) / 4f;
    public int BasicMove => Mathf.FloorToInt(BasicSpeed);
    public int Dodge => Mathf.FloorToInt(BasicSpeed);

    // Freeform Text Areas for the Sheet
    public string Advantages = "";
    public string Disadvantages = "";
    public string Quirks = "";
    public List<GURPSCharacterSkill> Skills = new List<GURPSCharacterSkill>();
    public string Inventory = "";

    // 3rd Edition Attribute Cost Scale
    public int GetAttributeCost(int level)
    {
        int diff = level - 10;
        if (diff == 0) return 0;

        // Simplified 3e cost progression (10, 20, 30, 45, 60, 80, 100, 125..)
        int absDiff = Mathf.Abs(diff);
        int cost = 0;

        if (absDiff == 1) cost = 10;
        else if (absDiff == 2) cost = 20;
        else if (absDiff == 3) cost = 30;
        else if (absDiff == 4) cost = 45;
        else if (absDiff == 5) cost = 60;
        else if (absDiff == 6) cost = 80;
        else if (absDiff == 7) cost = 100;
        else if (absDiff == 8) cost = 125;
        else cost = 125 + ((absDiff - 8) * 25); // Rough linear scaling beyond 18

        return diff < 0 ? -cost : cost;
    }

    public int TotalAttributePoints =>
        GetAttributeCost(ST) + GetAttributeCost(DX) + GetAttributeCost(IQ) + GetAttributeCost(HT);
    public float TotalSkillPoints => Skills.Sum(s => s.PointsInvested);
}