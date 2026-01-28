#if UNITY_EDITOR
using System.Collections.Generic;

[System.Serializable]
public class GurpsCharacterDefinition
{
    public string Guid = System.Guid.NewGuid().ToString();
    public string Name = "New Hero";
    public int Strength = 10;
    public int Dexterity = 10;
    public int Intelligence = 10;
    public int Health = 10;
    public List<string> Advantages = new();
    public List<string> Disadvantages = new();
    public List<string> Possessions = new();
    public Dictionary<string, int> Skills = new();
}
#endif