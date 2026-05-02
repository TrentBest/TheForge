using System;
using Workshop.Core.Memory;

namespace Workshop.GURPS
{
    /// <summary>
    /// Digital Universal RolePlaying System (DURPS) Hub.
    /// The full "Big and Heavy" systemic simulation orchestrator.
    /// </summary>
    public class DURPS_Engine : IDisposable
    {
        // --- Tier 0: The Quantum Orchestrator ---
        public CampaignAPI Campaign { get; private set; }
        public BooksAPI CampaignBooks { get; private set; }

        // --- Tier 1: The Character Layer ---
        public UniverseAPI Universe { get; private set; }
        public SkillsAPI Skills { get; private set; }
        public WeaponsAPI Weapons { get; private set; }
        public TraitsApi Advantages { get; private set; }
        public EntityAPI Entities { get; private set; }

        // ALIAS for Entity Matrix access
        public EntityAPI NPCs => Entities;

        // --- Tier 2: The World Layer ---
        public SocietalAPI Nations { get; private set; }
        public CivicAPI Cities { get; private set; }
        public LogisticsAPI Transportation { get; private set; }
        public EnvironmentAPI Environment { get; private set; }
        public DemographicsAPI Demographics { get; private set; }

        // --- Tier 3: The Structural Layer ---
        public ArchitectureAPI Architecture { get; private set; }
        public EconomyAPI Economy { get; private set; }

        public TechnologyCategoryApi TechCategories { get; private set; }
        public TechnologyLevelApi TechLevels { get; private set; }

        public CombatAPI Combat { get; private set; }

        public NarrativeAPI Narrative { get; private set; }
        public GeographyAPI Geography { get; private set; }
        public HeroClassAPI HeroClasses { get; private set; }


        private readonly DataWarehouse _warehouse;

        public DURPS_Engine(DataWarehouse warehouse)
        {
            _warehouse = warehouse;

            // Initialize all horizontal APIs
            Campaign = new CampaignAPI(warehouse);
            CampaignBooks = new BooksAPI(warehouse);

            Universe = new UniverseAPI(warehouse);
            Skills = new SkillsAPI(warehouse);
            Weapons = new WeaponsAPI(warehouse);
            Advantages = new TraitsApi(warehouse);
            Entities = new EntityAPI(warehouse);

            Nations = new SocietalAPI(warehouse);
            Cities = new CivicAPI(warehouse);
            Transportation = new LogisticsAPI(warehouse);
            Environment = new EnvironmentAPI(warehouse);
            Demographics = new DemographicsAPI(warehouse);

            Architecture = new ArchitectureAPI(warehouse);
            Economy = new EconomyAPI(warehouse);

            TechCategories = new TechnologyCategoryApi(warehouse);
            TechLevels = new TechnologyLevelApi(warehouse);
            Combat = new CombatAPI(warehouse);
            InitializeAll();
        }

        private void InitializeAll()
        {
            Campaign.Initialize();
            CampaignBooks.Initialize();
            Universe.Initialize();
            Skills.Initialize();
            Weapons.Initialize();
            Advantages.Initialize();
            Entities.Initialize();
            Nations.Initialize();
            Cities.Initialize();
            Transportation.Initialize();
            Environment.Initialize();
            Architecture.Initialize();
            Economy.Initialize();
            TechCategories.Initialize();
            TechLevels.Initialize();
            Demographics.Initialize();
            Combat.Initialize();
        }

        public void SaveState()
        {
            Campaign.SaveToCache();
            CampaignBooks.SaveToCache();
            Universe.SaveToCache();
            Skills.SaveToCache();
            Weapons.SaveToCache();
            Advantages.SaveToCache();
            Entities.SaveToCache();
            Nations.SaveToCache();
            Cities.SaveToCache();
            Transportation.SaveToCache();
            Environment.SaveToCache();
            Architecture.SaveToCache();
            Economy.SaveToCache();
            TechCategories.SaveToCache();
            TechLevels.SaveToCache();
            Demographics.SaveToCache();
            Combat.SaveToCache();
        }

        public void Dispose() => SaveState();
    }
}