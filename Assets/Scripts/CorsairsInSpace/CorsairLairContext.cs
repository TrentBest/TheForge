using System.Collections.Generic;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.CorsairsInSpace
{
    public class CorsairLairContext : MonoBehaviour, IStateContext
    {
        public string Name { get; set; } = "CorsairHollow_Alpha";
        public bool IsValid { get; set; } = true;

        // --- BASE STATS ---
        public int AsteroidSizeLevel { get; set; } = 1;
        public int TotalCredits { get; set; } = 5000;
        public int TotalPowerGenerated { get; set; } = 100;
        public int TotalPowerConsumed { get; set; } = 20;
        public float GlobalHeat { get; set; } = 0f;

        // --- THE MINION SWARM ---
        public int TotalMinions => Workers + Scientists + Engineers;
        public int MaxMinionCapacity { get; set; } = 300;

        // Specialized Pools
        public int Workers = 200;
        public int Scientists = 25;
        public int Engineers = 25;

        // Training Progress (0 to 100)
        public float TrainingProgress = 0f;
        public string ActiveTrainingProgram = "None";

        // --- RESEARCH & PRODUCTION ---
        public List<string> UnlockedShipClasses = new List<string> { "Shuttle" };
        public List<string> UnlockedWeapons = new List<string> { "Mass Driver" };

        public Dictionary<int, CorsairGridLevel> Levels { get; set; } = new Dictionary<int, CorsairGridLevel>();
        public List<PirateOperation> ActiveOperations { get; set; } = new List<PirateOperation>();
    }
}