using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MastersOfOrionII
{
    public class GameData
    {
        // --- GALAXY CONFIGURATION ---
        public int GalaxySeed { get; private set; }

        // --- PLAYERS ---
        public List<Civilization> Civilizations { get; private set; } = new List<Civilization>();
        public Civilization HumanPlayer => Civilizations.Count > 0 ? Civilizations[0] : null;

        // --- LOOKUPS ---
        private Dictionary<int, Colony> _globalColonyLookup = new Dictionary<int, Colony>();

        // --- ENUMS ---
        public enum Government { Democracy, Dictatorship, Technocracy, HiveMind, Feudalism, Corporatocracy }

        // --- API CALLED BY GENERATOR ---
        public void InitializeGalaxy(int seed, Civilization humanPlayer)
        {
            GalaxySeed = seed;
            Civilizations.Clear();
            _globalColonyLookup.Clear();

            if (humanPlayer != null)
            {
                Civilizations.Add(humanPlayer);

                // Register our starting colonies (Added null checks to prevent Object Not Initialized errors)
                if (humanPlayer.Colonies != null && humanPlayer.SystemKnowledge != null)
                {
                    foreach (var col in humanPlayer.Colonies.Values)
                    {
                        _globalColonyLookup[col.PlanetId] = col;
                        humanPlayer.SystemKnowledge[col.PlanetId] = IntelLevel.Occupied;
                    }
                }
            }

            Debug.Log($"[GameData] Galaxy Initialized. Seed: {seed}");
        }

        // --- QUERY API ---
        // By returning ColonyData? (Nullable Struct), the GalaxyView's 'if (colony != null)' check will work perfectly!
        public ColonyData GetColony(int planetId)
        {
            if (_globalColonyLookup.TryGetValue(planetId, out Colony colony))
            {
                return new ColonyData
                {
                    PlanetSeed = (uint)colony.PlanetId,
                    Name = colony.Name ?? "Unnamed Colony",
                    PopulationMillions = colony.PopulationMillions,
                    IsEstablishing = colony.IsEstablishing,
                    ColonizationProgress = colony.EstablishmentProgress * 100f,
                    Morale = colony.Morale,

                    // Provide safe defaults for the Macro-Economy pipeline 
                    // so the UI doesn't crash rendering empty data
                    GrowthRate = 0.05f,
                    GovernorName = "Unassigned",
                    Focus = ColonyFocus.Manual,
                    RawResourceOutput = 100f,
                    FoundryCapacity = 50f,
                    FactoryCapacity = 25f,
                    AssemblerCapacity = 0f,
                    HasOrbitalShipyard = false,

                    // CRITICAL: Initialize public collections so the GUI loop doesn't throw a NullReferenceException
                    Buildings = new Dictionary<string, int>(),
                    BuildQueue = new List<string>()
                };
            }

            return new ColonyData();
        }

        public IntelLevel GetIntel(int id)
        {
            if (HumanPlayer != null && HumanPlayer.SystemKnowledge != null && HumanPlayer.SystemKnowledge.TryGetValue(id, out IntelLevel level))
                return level;

            return IntelLevel.Unknown;
        }

        public void StartColonization(int planetId, float funding)
        {
            if (HumanPlayer == null) return;

            var newColony = new Colony(planetId, "New Colony") {
                IsEstablishing = true,
                MonthlyFunding = (int)funding,
                PopulationMillions = 0.1f,
                Morale = 1.0f
            };

            // Guard against uninitialized dictionaries in the HumanPlayer object
            if (HumanPlayer.Colonies == null) HumanPlayer.Colonies = new Dictionary<int, Colony>();
            if (HumanPlayer.SystemKnowledge == null) HumanPlayer.SystemKnowledge = new Dictionary<int, IntelLevel>();

            HumanPlayer.Colonies[planetId] = newColony;
            _globalColonyLookup[planetId] = newColony;
            HumanPlayer.SystemKnowledge[planetId] = IntelLevel.Occupied;
        }
    }
}