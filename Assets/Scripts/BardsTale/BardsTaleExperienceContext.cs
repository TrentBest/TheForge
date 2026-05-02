using UnityEngine;
using TheSingularityWorkshop.FSM_API;
using Assets.Scripts.GURPS; // Required for ActiveParty
using Workshop.BardsTale.World; // Required for CurrentMap

namespace Workshop.BardsTale
{
    /// <summary>
    /// Sovereign Context for the Bard's Tale reconstruction.
    /// Unifies navigation, party state, and map data.
    /// </summary>
    public class BardsTaleExperienceContext : MonoBehaviour, IStateContext
    {
        public string Name { get; set; } = "Bards_Tale_Session";
        public bool IsValid { get; set; } = true;

        // --- 🧭 NAVIGATION STATE (Fixed from 'int' to 'Vector2Int') ---
        public Vector2Int CurrentGridPosition { get; set; } = Vector2Int.zero;
        public Vector2Int CurrentFacingDirection { get; set; } = Vector2Int.up;
        public bool IsMoving { get; set; } = false;

        // --- ⚔️ SIMULATION DATA (Added for Guild and FSM logic) ---
        public GURPS_Party ActiveParty { get; set; }
        public DungeonMapContext CurrentMap { get; set; }

        // The "Pencil & Paper" Data
        public bool[,] ExploredMap = new bool[30, 30];

        // Standard Default Constructor
        public BardsTaleExperienceContext() { }

        // --- 🏗️ MULTI-ARG CONSTRUCTOR (Fixes FsmBuilder Error) ---
        public BardsTaleExperienceContext(GURPS_Party party, DungeonMapContext map)
        {
            ActiveParty = party;
            CurrentMap = map;
            Initialize();
        }

        public void Initialize()
        {
            // Mark starting square as explored
            if (ExploredMap != null)
                ExploredMap[CurrentGridPosition.x, CurrentGridPosition.y] = true;
        }

        // Aliases to maintain compatibility with legacy Exploration providers
        public Vector2Int PlayerPosition { get => CurrentGridPosition; set => CurrentGridPosition = value; }
        public Vector2Int PlayerDirection { get => CurrentFacingDirection; set => CurrentFacingDirection = value; }
    }
}