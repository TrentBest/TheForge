using System;

namespace TheSingularityWorkshop.Ontology
{
    // 1. The Metadata Attribute (Visuals for the Editor)
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false)]
    public class ExperienceMetaAttribute : Attribute
    {
        public string DisplayName;
        public string Description;
        public string IconPath; // e.g., "Icons/Galactic"

        public ExperienceMetaAttribute(string name, string desc)
        {
            DisplayName = name;
            Description = desc;
        }
    }

    // 2. The Root Ancestor (The "Quark")
    [ExperienceMeta("VOID", "The raw fabric of existence. No physics, no time, just State.")]
    public interface IExperience { }

    // 3. First-Generation Descendants
    [ExperienceMeta("Simulation", "Adds linear time, causality, and object permanence.")]
    public interface ISimulation : IExperience { }

    [ExperienceMeta("Narrative", "Adds dramatic structure, cameras, and scripted sequencing.")]
    public interface INarrative : IExperience { }

    // 4. Second-Generation (Specific Genres/Domains)
    [ExperienceMeta("Game", "Adds win/loss states, input loops, and player agency.")]
    public interface IGame : ISimulation { }

    // 5. Concrete Implementations (The "Templates" users can actually build)

    [ExperienceMeta("Galactic Empire (Armada 2525)", "A 4X strategy foundation. Explore, Expand, Exploit, Exterminate.")]
    [Serializable]
    public class GalacticStrategyBase : IGame
    {
        // --- 4X Configuration Parameters ---
        // Armada 2525 used a grid system, but modern implementations might use continuous space.
        // We will start with a defined abstract 'Size' and 'Density'.

        public string GalaxyName = "Omega Sector";
        public int GalaxySize = 100;      // Representing Light Years or Grid Units
        public int StarDensity = 25;      // Percentage or Count
        public int OpponentCount = 4;     // Number of AI Civilizations
        public bool FogOfWar = true;      // Hidden information
        public bool RandomEvents = true;  // Space monsters, anomalies, etc.
        public string StartingTech = "Hyperspace Era";
    }

    [ExperienceMeta("First Person Shooter", "Standard WASD movement + Projectile Physics.")]
    public class FPSBase : IGame
    {
        public int MaxPlayers = 16;
        public int FragLimit = 25;
        public bool FriendlyFire = false;
    }

    [ExperienceMeta("Interactive Movie", "Branching narrative nodes with high-fidelity rendering.")]
    public class InteractiveMovieBase : INarrative
    {
        public bool EnableQTE = true;
        public float DialogueTimer = 5.0f;
    }
}