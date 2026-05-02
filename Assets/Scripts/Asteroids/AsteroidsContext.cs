using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace Workshop.Asteroids
{
    public enum AsteroidsDifficulty
    {
        Easy = 4,
        Medium = 8,
        Hard = 16,
        Difficult = 32,
        Impossible = 64,
        Insane = 128,
        BeyondHuman = 256,
    }
    public class AsteroidsContext : MonoBehaviour, IStateContext
    {
        public FSMHandle Status { get; private set; }
        public bool IsValid { get; set; } = false;
        public string Name { get; set; } = "Asteroids";

        public List<AsteroidContext> Asteroids { get; set; } = new List<AsteroidContext>();
        public AsteroidFighter Fighter { get; set; }
        public List<GameObject> fighterPrefabs;
        public List<GameObject> fighterPartPrefabs;
        public AsteroidScore Score { get; set; }
        public Vector2 ScreenBounds { get;  set; }
        public int Level { get; set; }
        public int Difficulty { get; private set; }

        public void Awake()
        {
            if( !FSM_API.Interaction.Exists("Asteroids", "Update"))
            {
                FSM_API.Create.CreateFiniteStateMachine("Asteroids", -1, "Update")
                    .State("Initializing", OnEnterInitializing, null, null)
                    .State("Running", OnEnterRunning, OnUpdateRunning, OnExitRunning)
                    .State("Shutdown", OnEnterShutdown, null, null)
                    .Transition("Initializing", "Running", (ctx) => true)
                    .Transition("Running", "Initializing", ShouldReset)
                    .Transition("Running", "Shutdown", ShouldShutdown)
                    .BuildDefinition();
            }
            Status = FSM_API.Create.CreateInstance("Asteroids", this, "Update");
            IsValid = true;
        }

        private void OnEnterInitializing(IStateContext context)
        {
            if (context is AsteroidsContext asteroids)
            {
                var level = asteroids.Level++;
                var asteroidCount = asteroids.Difficulty * level;
            }
        }

        private void OnEnterRunning(IStateContext context)
        {
            if (context is AsteroidsContext asteroids)
            {

            }
        }

        private void OnUpdateRunning(IStateContext context)
        {
            if (context is AsteroidsContext asteroids)
            {

            }
        }

        private void OnExitRunning(IStateContext context)
        {
            if (context is AsteroidsContext asteroids)
            {

            }
        }

        private void OnEnterShutdown(IStateContext context)
        {
            if (context is AsteroidsContext asteroids)
            {

            }
        }

        private bool ShouldReset(IStateContext context)
        {
            if(context is AsteroidsContext asteroids)
            {
                return asteroids.Asteroids.Count == 0;
            }
            return false;
        }

        private bool ShouldShutdown(IStateContext context)
        {
            return false;
        }
    }
    public class ModularFighterDesign : AsteroidFighter
    {
        public bool UsePreAssembled { get; set; } = true;
        public string PreAssembledName { get; set; } = "StarSparrow_Example_01";

        // Modular Components
        public string CoreStyle { get; set; } = "Core_01";
        public string EngineStyle { get; set; } = "Engine_01";
        public string FinStyle { get; set; } = "Fin_01";
        public string PlasmaStyle { get; set; } = "Plasma_01";
        public string TailStyle { get; set; } = "Tail_01";
        public string ThrusterStyle { get; set; } = "Thruster_01";
        public string WeaponStyle { get; set; } = "Weapon_01";
        public string WingStyle { get; set; } = "Wing_01";

        public Color HullPaint { get; set; } = new Color(0.12f, 0.16f, 0.10f); // Olive Drab
        public Color EmissionColor { get; set; } = new Color(0.85f, 0.40f, 0.10f); // Copper Glow
    }
    public class AsteroidContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "Asteroid";

        // --- 2D Physics ---
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Rotation { get; set; }
        public float RotationSpeed { get; set; }

        // --- Gameplay Data ---
        public int SizeTier { get; set; } = 3; // e.g., 3 = Large, 2 = Medium, 1 = Small
        public int PointValue { get; set; } = 100;
        public bool IsDestroyed { get; set; } = false;

        private void OnUpdateRunning(IStateContext context)
        {
            if (context is AsteroidsContext asteroids)
            {
                float dt = Time.deltaTime;

                // --- 1. Fighter Flight Physics ---
                var ship = asteroids.Fighter;
                if (ship != null)
                {
                    // Apply thrust from input (Arrow keys mapped to ThrustInput)
                    ship.Velocity += ship.ThrustInput * ship.ThrustPower * dt;

                    // Apply Drag & Clamp
                    ship.Velocity *= ship.Drag;
                    if (ship.Velocity.magnitude > ship.MaxSpeed)
                        ship.Velocity = ship.Velocity.normalized * ship.MaxSpeed;

                    ship.Position += ship.Velocity * dt;
                    ship.Position = Wrap(ship.Position, asteroids.ScreenBounds);
                }

                // --- 2. Asteroid Movement ---
                foreach (var asteroid in asteroids.Asteroids)
                {
                    asteroid.Position += asteroid.Velocity * dt;
                    asteroid.Rotation += asteroid.RotationSpeed * dt;
                    asteroid.Position = Wrap(asteroid.Position, asteroids.ScreenBounds);
                }
            }
        }

        // Helper to keep everything inside the viewport
        private Vector2 Wrap(Vector2 pos, Vector2 bounds)
        {
            if (pos.x > bounds.x) pos.x = -bounds.x;
            else if (pos.x < -bounds.x) pos.x = bounds.x;

            if (pos.y > bounds.y) pos.y = -bounds.y;
            else if (pos.y < -bounds.y) pos.y = bounds.y;

            return pos;
        }
    }

    public class AsteroidFighter : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "AsteroidFighter";

        // --- 2D Physics & Movement ---
        public Vector2 Position { get; set; } = Vector2.zero;
        public Vector2 Velocity { get; set; } = Vector2.zero;
        public Vector2 ThrustInput { get; set; } = Vector2.zero; // X and Y thrust applied by player

        public float ThrustPower { get; set; } = 5f;
        public float MaxSpeed { get; set; } = 8f;
        public float Drag { get; set; } = 0.98f; // Slows the ship down when not thrusting

        // --- Subsystems (Ready for 3D/Advanced Phases) ---
        public AsteroidFighterPointDriverWeapon Weapon { get; set; } = new AsteroidFighterPointDriverWeapon();
        public AsteroidFighterGravityBombWeapon GravityBombWeapon { get; set; }
        public AsteroidFighterSmartBombWeapon SmartBombWeapon { get; set; }
        public AsteroidFighterArmor Armor { get; set; }
        public AsteroidFighterShield Shields { get; set; }
    }

    public class AsteroidScore : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "AsteroidScore";

        public int CurrentScore { get; set; } = 0;
        public int Lives { get; set; } = 3;
        public int Multiplier { get; set; } = 1;
    }

    public class AsteroidFighterPointDriverWeapon : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "AsteroidFighterPointDriverWeapon";

        // Example properties for the active 2D weapon
        public float FireRate { get; set; } = 0.25f;
        public float CooldownTimer { get; set; } = 0f;
    }

    public class AsteroidFighterGravityBombWeapon : IStateContext
    {
        public bool IsValid { get; set; } = false;
        public string Name { get; set; } = "AsteroidFighterGravityBombWeapon";
    }

    public class AsteroidFighterSmartBombWeapon : IStateContext
    {
        public bool IsValid { get; set; } = false;
        public string Name { get; set; } = "AsteroidFighterSmartBombWeapon";
    }

    public class AsteroidFighterArmor : IStateContext
    {
        public bool IsValid { get; set; } = false;
        public string Name { get; set; } = "AsteroidFighterArmor";
    }

    public class AsteroidFighterShield : IStateContext
    {
        public bool IsValid { get; set; } = false;
        public string Name { get; set; } = "AsteroidFighterShield";
    }
}
