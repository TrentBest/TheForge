using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MastersOfOrionII
{
    // ----------------------------------------------------------------------
    // 1. THE EMPIRE (Top Level)
    // ----------------------------------------------------------------------
    [Serializable]
    public class Civilization
    {
        public int PlayerId;        // 0 = Human, 1+ = AI
        public string Name;         // "Terran Federation"
        public Color Color;         // Empire color on map
        public bool IsHuman;

        // --- ECONOMY ---
        public long Treasury;           // Current Money (Credits)
        public float TaxRate;           // 0.0 to 1.0
        public float GlobalMorale;      // Average morale across empire

        // --- ASSETS ---
        // We track colonies by the PlanetID they sit on
        public Dictionary<int, Colony> Colonies = new Dictionary<int, Colony>();

        // We track fleets by a unique FleetID
        public List<Fleet> Fleets = new List<Fleet>();

        // Leaders and Advisors
        public List<Leader> Leaders = new List<Leader>();

        // --- KNOWLEDGE ---
        public ResearchState Research = new ResearchState();

        // Which systems have we visited? (Fog of War)
        // Key: SystemID, Value: Intel Level
        public Dictionary<int, IntelLevel> SystemKnowledge = new Dictionary<int, IntelLevel>();

        // Diplomatic relations
        // Key: Other PlayerID, Value: Relation State
        public Dictionary<int, DiplomaticRelation> Diplomacy = new Dictionary<int, DiplomaticRelation>();
    }
}
