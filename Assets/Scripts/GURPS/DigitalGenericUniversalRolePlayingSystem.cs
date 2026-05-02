using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using Workshop.Core.Memory; // Added for engine-level logging


namespace Workshop.GURPS
{
    public static class DigitalGenericUniversalRolePlayingSystem
    {
        private static DURPS_Engine _activeEngine;

        // Safety check for UI modules that need to verify boot state
        public static bool IsInitialized => _activeEngine != null;

        public static DURPS_Engine Engine => _activeEngine;

        public static void InitializeCoreSystem(DataWarehouse warehouse)
        {
            if (_activeEngine != null)
            {
                Debug.LogWarning("[DURPS API] Core System is already active. Ignoring duplicate boot request.");
                return;
            }

            if (warehouse == null)
            {
                Debug.LogError("[DURPS API] CRITICAL: Cannot boot the Core System without a valid DataWarehouse context.");
                return;
            }

            Debug.Log("[DURPS API] Initializing Digital Universal RolePlaying System...");
            _activeEngine = new DURPS_Engine(warehouse);
        }

        internal static void InitializeCoreSystem()
        {
            // Graceful error logging instead of throwing an unhandled exception that could crash editor tools
            Debug.LogError("[DURPS API] Headless initialization attempted. A DataWarehouse must be provided to boot the system.");
        }

        /// <summary>
        /// Flushes the active engine, freeing up memory and allowing for a clean reboot of the simulation state.
        /// </summary>
        public static void Shutdown()
        {
            if (_activeEngine == null) return;

            Debug.Log("[DURPS API] Shutting down Core System and releasing DataWarehouse hooks...");
            // If your DURPS_Engine implements an IDisposable or internal Teardown, call it here:
            // _activeEngine.Teardown(); 

            _activeEngine = null;
        }

        // --- SUB-SYSTEM ACCESSORS ---

        // Books & Tech
        public static BooksAPI Books => _activeEngine?.CampaignBooks;
        public static TechnologyCategoryApi TechCategories => _activeEngine?.TechCategories;
        public static TechnologyLevelApi TechLevels => _activeEngine?.TechLevels;


        // Core Mechanics
        public static CampaignAPI Campaign => _activeEngine?.Campaign;
        public static UniverseAPI Universes => _activeEngine?.Universe;

        // REFACTORED: Unified Traits API
        public static TraitsApi Traits => _activeEngine?.Advantages;
        public static TraitsApi Advantages => Traits;
        public static TraitsApi Disadvantages => Traits;

        // Entities & Combat
        public static WeaponsAPI Weapons => _activeEngine?.Weapons;
        public static SkillsAPI Skills => _activeEngine?.Skills;

        // Assuming this gets initialized by a specific builder later
        public static ArchitectureAPI Architecture { get; internal set; }

    }
}