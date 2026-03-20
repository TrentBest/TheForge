
using System;
using System.Collections.Generic;
using System.Linq;
using TheSingularityWorkshop.FSM_API;
using Unity.Collections;
using UnityEngine;

namespace TheSingularityWorkshop.Armada2525
{
    /// <summary>
    /// Represents the entire container.
    /// </summary>
    public struct Galaxy
    {
        public int Seed; // The Master Seed
        public int RadiusInSectors; // Size of the map
        public int AgeInBillionsYears; // Affects star metallicity (resource abundance)
    }

    /// <summary>
    /// The container for a solar system. 
    /// This is what you click on in the "Galaxy View".
    /// </summary>
    public struct StarSystem
    {
        public int Id;          // Unique Hash
        public int Seed;        // Specific seed for this system
        public Vector2Int GridCoordinate; // Where it is on the map

        // System wide aggregates (useful for AI or quick scanning)
        public int StarCount;
        public int PlanetCount;
        public bool HasAnomalies;
    }

    /// <summary>
    /// The central body. Determines the "Goldilocks Zone" for planets.
    /// </summary>
    public struct Star
    {
        public int Id;

        // Physical Properties
        public StarSpectralClass Class;
        public float Mass;          // Relative to Sol (1.0 = Sun)
        public float Radius;        // Relative to Sol
        public float Luminosity;    // Energy output (critical for planet temp)
        public float TemperatureK;  // Surface temp in Kelvin

        // Visuals
        public Color ChromaticAberration; // For the renderer
    }

    /// <summary>
    /// The primary gameplay surface.
    /// </summary>
    public struct Planet
    {
        public int Id;
        public int SystemIndex; // 1st planet, 2nd planet, etc.

        // Classification
        public PlanetType Type;

        // Orbital Physics
        public float SemiMajorAxisAU; // Distance from Star (Astronomical Units)
        public float OrbitalPeriodDays; // How long is a year?

        // Surface Physics
        public float RadiusEarth;    // Relative to Earth
        public float GravityG;       // 1.0 = Earth Gravity (Affects movement/costs)
        public float SurfaceTempK;   // Average temperature

        // Habitability
        public AtmosphereComposition Atmosphere;
        public float AtmosphereDensity; // 0.0 (Vacuum) to 100.0 (Crushing)
        public float HydrospherePct;    // 0.0 to 1.0 (OpenWater coverage)

        // Economy / 4X Stats (Generated from Seed)
        public ResourceRichness MineralRichness;
        public float BioDiversity;      // Bonus to food/research
        public float EnergyPotential;   // Solar/Wind/Geothermal rating

        public int MaxPopulationMillions; // Calculated based on size/habitability

        public uint Seed { get; internal set; }
        public string Name { get; internal set; }
    }

    public struct Moon
    {
        public int Id;
        public int ParentPlanetIndex;
        public PlanetType Type;
        public float RadiusEarth;
        public bool IsTidallyLocked;
        public ResourceRichness MineralRichness;
    }

    /// <summary>
    /// Sources of raw materials but no population.
    /// </summary>
    public struct AsteroidBelt
    {
        public int Id;
        public float DistanceFromStarAU;
        public float Density; // How hard is it to navigate ships through?
        public ResourceRichness MineralRichness;
        public bool HasRareIsotopes; // Special strategic resource?
    }

    // --- ANOMALIES ---

    /// <summary>
    /// Fast travel points.
    /// </summary>
    public struct Wormhole
    {
        public int Id;
        public Vector2Int DestinationCoordinate; // Where does it spit you out?
        public bool IsStable; // Can it collapse?
        public int MaxMassTransit; // Max ship size allowed?
    }

    /// <summary>
    /// Dangerous terrain or high-science locations.
    /// </summary>
    public struct Blackhole
    {
        public int Id;
        public float EventHorizonRadiusAU;
        public float AccretionDiskRadiation; // Damage per second to ships
        public bool LeadsToUnknown; // Maybe it's a wormhole in disguise?
    }

    public struct Mine
    {
        public int Id;
        public Element element;
        public float miningRate;
    }

    public enum StarSpectralClass
    {
        O, B, A, F, G, K, M, // Standard Harvard classification
        WhiteDwarf,
        NeutronStar,
        Pulsar
    }

    public enum PlanetType
    {
        GasGiant,
        IceGiant,
        Terrestrial, // Earth-like
        Desert,
        Oceanic,
        Tundra,
        Molten,
        Barren,
        Ice
    }

    public enum AtmosphereComposition
    {
        None,
        Breathable,     // Nitrogen/Oxygen
        Toxic,          // High CO2/Sulfur
        Corrosive,      // Acidic
        Inert           // Helium/Argon
    }

    public enum ResourceRichness
    {
        Poor,
        Average,
        Abundant,
        UltraRich
    }

    // ----------------------------------------------------------------------
    // 1. THE EMPIRE (Top Level)
    // ----------------------------------------------------------------------
    [Serializable]
    public class Civilization
    {
        public int PlayerId;        // 0 = Human, 1+ = AI
        public string Name;         // "Terran Federation"
        public Color Color;         // Empire color on map
        public bool IsHuman;

        // --- ECONOMY ---
        public long Treasury;           // Current Money (Credits)
        public float TaxRate;           // 0.0 to 1.0
        public float GlobalMorale;      // Average morale across empire

        // --- ASSETS ---
        // We track colonies by the PlanetID they sit on
        public Dictionary<int, Colony> Colonies = new Dictionary<int, Colony>();

        // We track fleets by a unique FleetID
        public List<Fleet> Fleets = new List<Fleet>();

        // Leaders and Advisors
        public List<Leader> Leaders = new List<Leader>();

        // --- KNOWLEDGE ---
        public ResearchState Research = new ResearchState();

        // Which systems have we visited? (Fog of War)
        // Key: SystemID, Value: Intel Level
        public Dictionary<int, IntelLevel> SystemKnowledge = new Dictionary<int, IntelLevel>();

        // Diplomatic relations
        // Key: Other PlayerID, Value: Relation State
        public Dictionary<int, DiplomaticRelation> Diplomacy = new Dictionary<int, DiplomaticRelation>();
    }

    // ----------------------------------------------------------------------
    // 2. COLONIES (Planetary Management)
    // ----------------------------------------------------------------------
    [Serializable]
    public class Colony
    {
        public int PlanetId;        // The physical location (Key)
        public string Name;         // Custom name given by player

        // --- POPULATION ---
        public float PopulationMillions;
        public float MaxPopulation;      // Cap based on planet size/tech
        public float Morale;             // 0.0 to 1.0 (Affects production/revolt)
        public float GrowthRate;         // Calculated per turn

        // --- INFRASTRUCTURE ---
        // Key: BuildingID/Name, Value: Count (e.g., "Factory": 5)
        public Dictionary<string, int> Buildings = new Dictionary<string, int>();

        public List<Mine> Mines = new List<Mine>();

        // What is currently being built?
        public List<ProductionItem> BuildQueue = new List<ProductionItem>();

        // --- LOGISTICS (The Mechanics we discussed) ---
        public bool IsEstablishing;        // True if still a "Camp"
        public float EstablishmentProgress; // 0.0 to 1.0
        public int MonthlyFunding;         // How much treasury flows here per turn

        // --- OUTPUTS (Calculated per turn) ---
        public float NetProduction;  // Industry points
        public float NetScience;     // Research points
        public float NetIncome;      // Tax revenue
        public float NetDefense;     // Planetary shield/ground strength
        public List<int> InOrbit = new List<int>(); 
    }

    [Serializable]
    public class ProductionItem
    {
        public string ItemId;        // "Building_Factory" or "Ship_Cruiser"
        public bool IsUnit;          // True if Ship/Troop, False if Building
        public float ProductionCost; // Total industry needed
        public float Progress;       // Current industry invested
    }

    // ----------------------------------------------------------------------
    // 3. FLEETS & MILITARY
    // ----------------------------------------------------------------------
    [Serializable]
    public class Fleet
    {
        public int Id;
        public string Name;          // "1st Strike Group"
        public int OwnerPlayerId;

        // --- POSITION ---
        public int CurrentSystemId;  // -1 if in deep space
        public Vector2 DeepSpacePosition; // If travelling between stars
        public bool IsMoving;
        public int DestinationSystemId;
        public float TurnsToArrival;

        // --- COMPOSITION ---
        public List<Ship> Ships = new List<Ship>();
        public FleetStance Stance;   // Aggressive, Defensive, Patrol
    }

    [Serializable]
    public struct Ship
    {
        public int Id;
        public int DesignId;


        // --- STATE ---
        public float CurrentHP;
        public float Experience;     // 0 to 100 (Crew Veternancy)
        public float Fuel;           // Range limit


    }

    [Serializable]
    public class ShipDesign
    {
        public int DesignId;      // Unique ID
        public string DisplayName;
        public HullSize Size;        // Frigate, Cruiser, Dreadnought

        // Components
        public int ArmorLevel;
        public int ShieldLevel;
        public int EngineLevel;
        public int ComputerLevel;

        // Weapons (List of weapon IDs)
        public List<string> WeaponMounts = new List<string>();

        // Stats (Calculated)
        public int ConstructionCost;
        public int MaintenanceCost;
        public int CombatPower;
    }

    // ----------------------------------------------------------------------
    // 4. RESEARCH & TECH
    // ----------------------------------------------------------------------
    [Serializable]
    public class ResearchState
    {
        // Unlocked Tech IDs
        public HashSet<string> UnlockedTechs = new HashSet<string>();

        // Current Focus
        public string CurrentProjectID;
        public float CurrentProgress;
        public float ProjectCost;

        // Spending Allocation
        public float ResearchBudgetPct; // % of Empire income to Science
    }

    [Serializable]
    public struct Freighter
    {
        public int Id;
        public int carryingCapacity;
        public int load;
        public Element loadType;
        public int destinationColonyID;
        public HullSize hullSize;
    }



    [Serializable]
    public class CivilianFreighterFleet : IStateContext
    {
        public bool IsValid { get; set; } = false;
        public string Name { get; set; }

        public List<Freighter> Fleet { get; private set; } = new List<Freighter>();

        /// <summary>
        /// Supply available, where we order by Element, then colony ID then quantity available.
        /// </summary>
        public Dictionary<Element, Dictionary<int, int>> Supply { get; private set; } = new Dictionary<Element, Dictionary<int, int>>();
        /// <summary>
        /// Demand where we order by element, then by colony ID, then quantity needed.
        /// </summary>
        public Dictionary<Element, Dictionary<int, int>> Demand { get; private set; } = new Dictionary<Element, Dictionary<int, int>>();

        public IFreighterDispatch Dispatcher { get; private set; }
        public List<Colony> Colonies { get; internal set; } = new List<Colony>();
    }


    public interface IFreighterDispatch : IStateContext
    {
        void NotifyArrivalAtColony(int freighterId, int colonyId);
        void NotifyDeparture(int freighterId, int departureColonyId, int destinationColonyId);

        void NotifyCargoLoaded(int freighterId);
        void NotifyCargoUnloaded(int freighterId);

        void RequestAssignment(int freighterId);
        List<Vector3> RequestFlightPlan(int freighterId, int colonyId);
    }

    public class FreighterDispatch : IFreighterDispatch
    {
        public bool IsValid { get; set; } = false;
        public string Name { get; set; }

        List<int> AwaitingAssignment { get; set; } = new List<int>();
        Dictionary<Element, Dictionary<int, int>> Assignments { get; set; } = new Dictionary<Element, Dictionary<int, int>>();

        List<Tuple<Element, int, int>> AvailableAssignments { get; set; } = new List<Tuple<Element, int, int>>();
        public string freighterProcessGroup { get; } = "Freighters";
        public FSMHandle Status { get; private set; }

        private CivilianFreighterFleet fleetCommand;
        public Dictionary<int, int> InTransit { get; private set; } = new Dictionary<int, int>();

        public FreighterDispatch(CivilianFreighterFleet civilianFleet)
        {
            fleetCommand = civilianFleet;
            Name = "FreighterDispatch";
            if ( !FSM_API.FSM_API.Interaction.Exists("FreighterDispatchFSM", freighterProcessGroup))
            {

            }
            Status = FSM_API.FSM_API.Create.CreateInstance("FreighterDispatchFSM", this, freighterProcessGroup);
            IsValid = true;
        }

        public void NotifyArrivalAtColony(int freighterId, int colonyId)
        {
            InTransit.Remove(freighterId);
            fleetCommand.Colonies[colonyId].InOrbit.Add(freighterId);
        }

        public void NotifyCargoLoaded(int freighterId)
        {
            throw new NotImplementedException();
        }

        public void NotifyCargoUnloaded(int freighterId)
        {
            throw new NotImplementedException();
        }

        public void NotifyDeparture(int freighterId, int departureColonyId, int destinationColonyId)
        {

            fleetCommand.Colonies[departureColonyId].InOrbit.Remove(freighterId);
            InTransit.Add(freighterId, destinationColonyId);
        }

        public void RequestAssignment(int freighterId)
        {
            AwaitingAssignment.Add(freighterId);
        }

        public List<Vector3> RequestFlightPlan(int freighterId, int colonyId)
        {
            throw new NotImplementedException();
        }
    }


    // ----------------------------------------------------------------------
    // 5. LEADERS & ADVISORS
    // ----------------------------------------------------------------------
    [Serializable]
    public class Leader
    {
        public int Id;
        public string Name;
        public LeaderType Type;      // Governor, Admiral, Scientist
        public int Level;            // 1-10
        public int AssignedTargetId; // ColonyID (if Governor) or FleetID (if Admiral)

        // Traits (e.g., "LogisticsExpert", "Charismatic")
        public List<string> Traits = new List<string>();
    }

    // ----------------------------------------------------------------------
    // ENUMS
    // ----------------------------------------------------------------------

    public enum HullSize { Scout, Frigate, Destroyer, Cruiser, Battleship, Titan, ColonyShip }

    public enum FleetStance { Passive, Patrol, Aggressive, Blockade, Invasion }

    public enum LeaderType { Governor, Admiral, General, Scientist, Spy }

    public enum DiplomaticRelation { Peace, War, Ceasefire, Alliance, Vassal }

    public enum IntelLevel { Unknown, Detected, Surveyed, Occupied }
}


//***********************************************************
//                  GURPS
//***********************************************************

public enum GURPSAttributeType { ST, DX, IQ, HT }
public enum GURPSDifficulty { Easy, Average, Hard, VeryHard }

[Serializable]
public class GURPSSkillDefinition
{
    public string Name;
    public GURPSAttributeType BaseAttribute;
    public GURPSDifficulty Difficulty;
    public string Description;

    // The CRUD Builder requires this to map to the 1st Column (Book Filter)
    public string SourceBookName = "GURPS Basic Set (3rd Ed. Revised)";

    public string TypeString { get;  set; }
}


public class GURPSCharacterSkill
{
    public GURPSSkillDefinition Definition;
    public float PointsInvested = 0.5f;

    // GURPS 3e Skill Calculation Logic
    public int GetRelativeLevelOffset()
    {
        int step = 0;
        if (PointsInvested >= 0.5f) step = 1;
        if (PointsInvested >= 1.0f) step = 2;
        if (PointsInvested >= 2.0f) step = 3;
        if (PointsInvested >= 4.0f) step = 4;
        if (PointsInvested >= 8.0f) step = 4 + Mathf.FloorToInt((PointsInvested - 4f) / 2f); // +1 per 2 pts after 4

        int difficultyPenalty = Definition.Difficulty switch
        {
            GURPSDifficulty.Easy => -2,
            GURPSDifficulty.Average => -3,
            GURPSDifficulty.Hard => -4,
            GURPSDifficulty.VeryHard => -5,
            _ => -3
        };

        // Physical skills advance slightly differently in 3e, but for simplicity, we use the unified offset table
        return difficultyPenalty + step;
    }

    public int GetActualLevel(GURPS3eCharacterData character)
    {
        int baseAttr = Definition.BaseAttribute switch
        {
            GURPSAttributeType.ST => character.ST,
            GURPSAttributeType.DX => character.DX,
            GURPSAttributeType.IQ => character.IQ,
            GURPSAttributeType.HT => character.HT,
            _ => 10
        };
        return baseAttr + GetRelativeLevelOffset();
    }

    public string RelativeLevelString
    {
        get
        {
            int offset = GetRelativeLevelOffset();
            string attr = Definition.BaseAttribute.ToString();
            if (offset == 0) return attr;
            return offset > 0 ? $"{attr}+{offset}" : $"{attr}{offset}";
        }
    }
}


public class GURPS3eCharacterData
{
    public string CharacterName = "Nameless Adventurer";
    public string PlayerName = "Player";
    public int StartingPoints = 100;

    // Core Attributes (3e baseline is 10)
    public int ST = 10;
    public int DX = 10;
    public int IQ = 10;
    public int HT = 10;

    // Derived Characteristics (GURPS 3rd Edition Rules)
    public int HP => HT;
    public int Fatigue => ST;
    public float BasicSpeed => (HT + DX) / 4f;
    public int BasicMove => Mathf.FloorToInt(BasicSpeed);
    public int Dodge => Mathf.FloorToInt(BasicSpeed);

    // Freeform Text Areas for the Sheet
    public string Advantages = "";
    public string Disadvantages = "";
    public string Quirks = "";
    public List<GURPSCharacterSkill> Skills = new List<GURPSCharacterSkill>();
    public string Inventory = "";

    // 3rd Edition Attribute Cost Scale
    public int GetAttributeCost(int level)
    {
        int diff = level - 10;
        if (diff == 0) return 0;

        // Simplified 3e cost progression (10, 20, 30, 45, 60, 80, 100, 125..)
        int absDiff = Mathf.Abs(diff);
        int cost = 0;

        if (absDiff == 1) cost = 10;
        else if (absDiff == 2) cost = 20;
        else if (absDiff == 3) cost = 30;
        else if (absDiff == 4) cost = 45;
        else if (absDiff == 5) cost = 60;
        else if (absDiff == 6) cost = 80;
        else if (absDiff == 7) cost = 100;
        else if (absDiff == 8) cost = 125;
        else cost = 125 + ((absDiff - 8) * 25); // Rough linear scaling beyond 18

        return diff < 0 ? -cost : cost;
    }

    public int TotalAttributePoints =>
        GetAttributeCost(ST) + GetAttributeCost(DX) + GetAttributeCost(IQ) + GetAttributeCost(HT);
    public float TotalSkillPoints => Skills.Sum(s => s.PointsInvested);
}