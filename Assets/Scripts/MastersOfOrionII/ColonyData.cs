using System.Collections.Generic;

namespace Assets.Scripts.MastersOfOrionII
{
    public enum ColonyFocus { Manual, Balanced, Extraction, Industrial, Research, Military, Logistics, Leisure }

    public struct ColonyData
    {
        public uint PlanetSeed;
        public string Name;

        // Demographics
        public float PopulationMillions;
        public float GrowthRate;
        public float Morale;

        // Governance
        public string GovernorName;
        public ColonyFocus Focus;

        // The Macro-Economy Pipeline
        public float RawResourceOutput;    // Base mining yield
        public float FoundryCapacity;      // Smelts Raw -> Refined Materials
        public float FactoryCapacity;      // Forges Refined -> Components
        public float AssemblerCapacity;    // Assembles Components -> Ship Modules
        public bool HasOrbitalShipyard;    // Can it do final ship construction?

        // Infrastructure State
        public Dictionary<string, int> Buildings;
        public List<string> BuildQueue;

        // Colonization Phase
        public bool IsEstablishing;
        public float ColonizationProgress;

        public int MonthlyBudget { get; set; }
    }
}