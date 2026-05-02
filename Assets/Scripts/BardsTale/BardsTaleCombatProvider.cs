

using Assets.Scripts.GURPS;
using System.Collections.Generic;
using UnityEngine;
using Workshop.BardsTale;
using Workshop.Core.Diagnostics;

namespace Workshop.BardsTale
{
    public class BardsTaleCombatProvider
    {
        public enum CombatPhase { Input, Resolution, End }
        public CombatPhase CurrentPhase { get; private set; }

        public void Tick(BardsTaleExperienceContext ctx)
        {
            // TODO: Process initiative based on GURPS DX
            // Phase 1: Player Input (Attack, Cast, Hide, Song)
            // Phase 2: Action Resolution
            // Phase 3: Cleanup
        }

        public void StartEncounter(GURPS_Party party, List<string> enemies)
        {
            CurrentPhase = CombatPhase.Input;
            ForgeLogger.Log("A group of monsters approach!");
        }
    }
}