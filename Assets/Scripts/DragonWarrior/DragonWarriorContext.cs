using System;
using System.Collections.Generic;
using UnityEngine;
using Workshop.Core.Memory;
using Assets.Scripts.Workshop.Gameplay.Grid;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.Domains.DragonWarrior
{
    /// <summary>
    /// The persistent state for the Dragon Warrior experience.
    /// Follows Pillar 2: Decoupled Context.
    /// </summary>
    public class DragonWarriorContext : IStateContext
    {
        // Player Stats
        public string HeroName = "LOTO";
        public int HP = 15;
        public int MaxHP = 15;
        public int MP = 0;
        public int Level = 1;
        public int XP = 0;
        public int Gold = 20;

        // Navigation State
        public Vector2Int MapPosition = new Vector2Int(10, 10);
        public Vector2Int FacingDirection = Vector2Int.up;
        public GridMapData WorldMap;

        // World State
        public bool IsMenuOpen = false;
        public string ActiveDialogue = "Welcome, descendant of Erdrick.";
        public string ActiveRoute = "EXPLORATION"; // Used by the GuiRouter

        // Combat State
        public string EnemyName = "";
        public int EnemyHP = 0;
        public int EnemyMaxHP = 0;
        public int EnemyAttack = 2;
        public List<string> CombatLog = new List<string>();
        public bool PlayerTurnActive = false;

        public DragonWarriorContext(GridMapData map)
        {
            WorldMap = map;
            Name = $"Session_{HeroName}";

            IsValid = true ;
        }

        // Rule 8: FSM only ticks if map data is present.
        public bool IsValid { get; set; } = false;

        public string Name { get; set; }
        public FSMHandle Status { get; set; }
    }
}