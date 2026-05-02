using Assets.Scripts.MastersOfOrionII;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GalaxyConfigData
{
    public float PhysicsStability = 1.0f;
    public float UniverseAge = 5.0f; // Billions of years
    public int OpponentCount = 4;
    public float Abundance = 0.5f; // Resource density
    public uint MasterSeed = 42;
    public string Shape = "Spiral";
    public float MagicAbundance = 0f; // 0 (Hard Sci-Fi) to 1 (High Fantasy)

    public int ScaleIndex { get; internal set; }
    public GalaxyMorphology Morphology { get; internal set; }
    public object LifeSupportingPlanetaryAbundance { get; internal set; }

    /// <summary>
    /// Calculates the Race Creation budget based on universe difficulty.
    /// Higher resource abundance typically results in a lower point budget to maintain challenge.
    /// </summary>
    public int CalculatePoints()
    {
        return Mathf.FloorToInt(150 + (UniverseAge * 5) - (Abundance * 30));
    }
}

