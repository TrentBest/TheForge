using System;
using System.Collections.Generic;

namespace Assets.Scripts.CorsairsInSpace
{
    [Serializable]
    public class PirateOperation
    {
        public string OperationName { get; set; } = "Unnamed Raid";
        public string Status { get; set; } = "Idle";
        public string TargetIdentifier { get; set; } = "Unknown";

        // GURPS Mechanics
        public int RequiredPilotSkill { get; set; } = 12;
        public int RequiredTacticsSkill { get; set; } = 10;

        // Results
        public int CreditsStolen { get; set; }
        public float HeatGenerated { get; set; }
        public bool WasSuccessful { get; set; }
        public List<string> Salvage { get; set; } = new List<string>();

        // FSM Timers
        public float StartTime { get; set; }
        public float Duration { get; set; } = 300f; // 5 minutes standard raid
    }
}