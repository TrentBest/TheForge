using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase
{
    public static class ExperienceRegistry
    {
        private static SortedDictionary<string, List<IShowcasePortal>> _cachedPortals;

        public static SortedDictionary<string, List<IShowcasePortal>> GetDiscoveredPortals(IGuiRouter router)
        {
            // If the WebGL Bootstrapper already injected our portals, just inject the router and return!
            if (_cachedPortals != null && _cachedPortals.Count > 0)
            {
                foreach (var category in _cachedPortals.Values)
                {
                    foreach (var portal in category) portal.InjectRouter(router);
                }
                return _cachedPortals;
            }

            _cachedPortals = new SortedDictionary<string, List<IShowcasePortal>>();
            var interfaceType = typeof(IShowcasePortal);

            // ====================================================================
            // EDITOR AUTO-DISCOVERY (Reflection)
            // This runs in the Editor if manual registration wasn't used.
            // ====================================================================
            var portalTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => interfaceType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

            foreach (var type in portalTypes)
            {
                try
                {
                    var portal = (IShowcasePortal)Activator.CreateInstance(type);
                    if (!portal.IsDiscoverable) continue;

                    portal.InjectRouter(router);

                    string category = string.IsNullOrEmpty(portal.Category) ? "UNCLASSIFIED DOMAINS" : portal.Category;

                    if (!_cachedPortals.ContainsKey(category))
                        _cachedPortals[category] = new List<IShowcasePortal>();

                    _cachedPortals[category].Add(portal);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ExperienceRegistry] Failed to mount portal '{type.Name}'. Error: {ex.Message}");
                }
            }

            return _cachedPortals;
        }

        // Optional: Flush cache if you are using Hot Reloading and want to force a re-scan
        public static void FlushRegistry() => _cachedPortals = null;

        // ====================================================================
        // THE WEBGL LIFESAVER (Manual Injection)
        // Auto-generated code calls this to force IL2CPP to keep the classes!
        // ====================================================================
        public static void RegisterManual(IShowcasePortal portal)
        {
            if (_cachedPortals == null) _cachedPortals = new SortedDictionary<string, List<IShowcasePortal>>();

            string category = string.IsNullOrEmpty(portal.Category) ? "UNCLASSIFIED DOMAINS" : portal.Category;

            if (!_cachedPortals.ContainsKey(category))
                _cachedPortals[category] = new List<IShowcasePortal>();

            // Prevent duplicates
            if (!_cachedPortals[category].Any(p => p.GetType() == portal.GetType()))
            {
                _cachedPortals[category].Add(portal);
            }
        }
    }
}





//using System.Collections.Generic;
//using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

//namespace Workshop.UI_And_Tools.Showcase
//{
//    /// <summary>
//    /// The master curator for the Singularity Forge. 
//    /// Automatically populates the Lobby with all established domains.
//    /// </summary>
//    public static class ExperienceRegistry
//    {
//        private static readonly List<ShowcaseExperienceDef> _availableExperiences = new();

//        public static IEnumerable<ShowcaseExperienceDef> All => _availableExperiences;

//        public static void Initialize()
//        {
//            _availableExperiences.Clear();

//            // 1. ANTS: High-Performance GPU Simulation
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "Ants",
//                Overview = "Technical showcase for Data-Oriented Design (DOD). Simulates millions of entities via Compute Shaders.",
//                CoreTechnologies = new List<string> { "HLSL Compute", "Blittable Structs", "Pheromone Mapping" },
//                OnLaunchExperience = () => { /* Load Ants_Playable_Alpha */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer for Ants Domain */ }
//            });

//            // 2. ARMADA 2525: Macro-Scale 4X Strategy
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "Armada 2525",
//                Overview = "Flagship for macroscopic 'Everything Simulations.' Abstract flows of power, wealth, and policy.",
//                CoreTechnologies = new List<string> { "FSM_API", "GURPS Engine", "Economic Stock Exchange" },
//                OnLaunchExperience = () => { /* Load Armada2525_Game */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer for Armada Domain */ }
//            });

//            // 3. ASTEROIDS: 2D Kinematics & Modular Assembly
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "Asteroids",
//                Overview = "High-frequency update loops and UI-driven entity authoring. Modernizes classic arcade physics.",
//                CoreTechnologies = new List<string> { "2D Kinematics", "CRUD_Builder", "Painter2D Vector Graphics" },
//                OnLaunchExperience = () => { /* Load Asteroids Game */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer for Asteroids Domain */ }
//            });

//            // 4. BARD'S TALE: Grid-Based RPG
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "Bard's Tale",
//                Overview = "Step-based movement and dungeon crawling. Zero-boilerplate party management.",
//                CoreTechnologies = new List<string> { "Grid Navigation", "Cardinal Math", "Tavern CRUD" },
//                OnLaunchExperience = () => { /* Load BardsTale experience */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer */ }
//            });

//            // 5. CORSAIRS IN SPACE: Macro-Tycoon
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "Corsairs In Space",
//                Overview = "Evil Genius style lair construction and minion management on an astronomical scale.",
//                CoreTechnologies = new List<string> { "Voxel Management", "Cosmic Cells", "Blueprint Texture Rendering" },
//                OnLaunchExperience = () => { /* Load CorsairLair experience */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer */ }
//            });

//            // 6. EMPIRE: Tile-Based 4X
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "Empire",
//                Overview = "Naval and land-based conquest. Entirely UI-driven world grid and Fog of War.",
//                CoreTechnologies = new List<string> { "Perlin Map Gen", "Unit Definition Bible", "Dumb UI Patterns" },
//                OnLaunchExperience = () => { /* Load Empire experience */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer */ }
//            });

//            // 7. FRACTALS: GPU Math
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "Fractals",
//                Overview = "Real-time Mandelbrot set rendering. Bridges UI controls directly to HLSL kernels.",
//                CoreTechnologies = new List<string> { "Compute Shader", "Complex Plane Mapping", "Atomic FSM Gates" },
//                OnLaunchExperience = () => { /* Load Fractals experience */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer */ }
//            });

//            // 8. MASTERS OF ORION II: Galactic Strategic Orchestration
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "Masters of Orion II",
//                Overview = "Persistent universe state and multi-layered strategic decision making.",
//                CoreTechnologies = new List<string> { "Experience Orchestration", "Colony Packing", "Roman Numeral Extensions" },
//                OnLaunchExperience = () => { /* Load MOO2 experience */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer */ }
//            });

//            // 9. GURPS: Rule Engine
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "GURPS Engine",
//                Overview = "Standalone 'Physics of Rules.' Codifies tabletop mechanics for digital resolution.",
//                CoreTechnologies = new List<string> { "Probability Graphs", "Modular Rulebooks", "Trait Resolution" },
//                OnLaunchExperience = () => { /* Load GURPS Sandbox */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer */ }
//            });

//            // 10. PAC-MAN: Pathfinding
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "Pac-Man",
//                Overview = "Grid-constrained pathfinding and asynchronous AI state management.",
//                CoreTechnologies = new List<string> { "Maze Pathfinding", "Global State Inversion", "Corridor Wrap" },
//                OnLaunchExperience = () => { /* Load Pacman experience */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer */ }
//            });

//            // 11. PILL VIRUS: Causality Engine
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "Pill Virus",
//                Overview = "Strict pattern matching and cascading grid resolution. Tests microsecond state reactions.",
//                CoreTechnologies = new List<string> { "Cascading Gravity", "Color-Swap Rotation", "Quadrant Menus" },
//                OnLaunchExperience = () => { /* Load PillVirus experience */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer */ }
//            });

//            // 12. PONG: Autopoietic Gestalt
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "Pong",
//                Overview = "High-speed UI feedback and event-driven input. Pure 60fps UI game loop.",
//                CoreTechnologies = new List<string> { "UI Collision", "English Physics", "Timer Delta Integration" },
//                OnLaunchExperience = () => { /* Load Pong experience */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer */ }
//            });

//            // 13. SOLAR SYSTEM EXPLORER: Orbital Mechanics
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "Solar System Explorer",
//                Overview = "Large-scale coordinate spaces and interactive planetary body inspection.",
//                CoreTechnologies = new List<string> { "Orbital Sim", "Isolated Viewports", "Dynamic Material Swapping" },
//                OnLaunchExperience = () => { /* Load Explorer experience */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer */ }
//            });

//            // 14. SWARMY: Drone Multi-Agent Robotics
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "Swarmy",
//                Overview = "Autonomous flight stabilization and PID-controlled drone coordination.",
//                CoreTechnologies = new List<string> { "Thrust Physics", "PID Control", "IFF Classification" },
//                OnLaunchExperience = () => { /* Load Swarmy experience */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer */ }
//            });

//            // 15. UNITY SERVICES: Cloud Integration
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "Unity Services",
//                Overview = "Identity management and cloud persistence. Connects creators to player environments.",
//                CoreTechnologies = new List<string> { "UGS Authentication", "Remote Config", "Asynchronous Initialization" },
//                OnLaunchExperience = () => { /* Load Services Hub */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer */ }
//            });

//            // 16. WARLORDS: Strategy RPG
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "Warlords",
//                Overview = "Hybrid 2D/3D map presentation and deep entity customization via image ingestion.",
//                CoreTechnologies = new List<string> { "Pluggable Presenters", "Image-to-Map Ingestion", "Logistics Armory" },
//                OnLaunchExperience = () => { /* Load Warlords experience */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer */ }
//            });

//            // 17. THE FORGE: Autopoietic Meta-Tooling
//            _availableExperiences.Add(new ShowcaseExperienceDef
//            {
//                Title = "The Forge",
//                Overview = "The environment you are currently standing in. Recursive suite of UI and Logic builders.",
//                CoreTechnologies = new List<string> { "Reflective UI", "FSM API", "Data Warehouse" },
//                OnLaunchExperience = () => { /* Show Builder Registry */ },
//                OnInspectArchitecture = () => { /* Open ArchitectureExplorer for The Forge itself */ }
//            });
//        }
//    }
//}