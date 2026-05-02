using System.Collections.Generic;

namespace Assets.Scripts.SC2
{
    public class SC2BotBlueprint
    {
        public string BotName { get; set; } = "New Swarm Entity";
        public SC2Race PlayableRace { get; set; } = SC2Race.Random;

        // The list of modular behaviors assigned to this bot
        public List<IBotBehaviorData> Behaviors { get; set; } = new List<IBotBehaviorData>();
    }

    public enum SC2Race { Terran, Zerg, Protoss, Random }

    // Interface for different modules (Macro, Micro, Scouting)
    public interface IBotBehaviorData
    {
        string ModuleName { get; }
    }

    public class CartographyBehaviorData : IBotBehaviorData
    {
        public string ModuleName => "Cartography & Scouting";
        public int AggressionBias { get; set; } = 50; // 0-100
        public int FallbackThreshold { get; set; } = 30; // % of enemy strength
    }
}