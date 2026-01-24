using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_API.Scripts;
using UnityEngine;

public class DroneController : MonoBehaviour, IStateContext
{
    [Header("Drone Identity")]
    [Tooltip("Changing this string creates a completely separate FSM Definition (e.g. 'ScoutBehavior', 'HeavyLifter').")]
    public string behaviorID = "StandardDrone";
    public string droneID;

    [Header("Configuration")]
    [SerializeField] private ThrustData motors;

    [Header("Runtime Data")]
    public Vector3 targetPosition;
    public FSMHandle Status { get; private set; }

    // --- IStateContext Implementation ---
    public bool IsValid { get; set; } = false;
    public string Name { get => droneID; set => droneID = value; }

    void Awake()
    {
        if (string.IsNullOrEmpty(droneID)) droneID = System.Guid.NewGuid().ToString();

        // Auto-assign motors if they are children and not assigned
        if (motors == null || motors.FWD_RT == null) AutoDiscoverMotors();

        // 1. Define the specific "Class" of behavior if it doesn't exist yet
        if (!FSM_API.Interaction.Exists(behaviorID, "DroneLogic"))
        {
            DefineDroneBehavior(behaviorID);
        }

        // 2. Instantiate the FSM for THIS drone
        Status = FSM_API.Create.CreateInstance(behaviorID, this, "DroneLogic");

        // 3. Activate
        IsValid = true;
    }

    // --- Command Methods (API for your GUI/Squad Manager) ---

    public void TakeOff(float altitude = 10f)
    {
        targetPosition = new Vector3(transform.position.x, altitude, transform.position.z);
        Status.TransitionTo("TakeOff");
    }

    public void MoveTo(Vector3 destination)
    {
        targetPosition = destination;
        Status.TransitionTo("Moving");
    }

    public void LandAt(Vector3 landingSpot)
    {
        targetPosition = landingSpot;
        Status.TransitionTo("Landing");
    }

    // --- Behavior Definitions (The "Classes") ---

    private void DefineDroneBehavior(string id)
    {
        // Ensure the processing group exists
        var unityIntegration = FindFirstObjectByType<FSM_UnityIntegrationAdvanced>();
        if (unityIntegration != null) unityIntegration.AddProcessingGroup("Update", "DroneLogic");

        // We can switch on the ID to create completely different FSM structures
        // This allows "Scout" to have different states than "Kamikaze"
        switch (id)
        {
            case "AggressiveDrone":
                DefineAggressiveBehavior(id);
                break;
            case "StandardDrone":
            default:
                DefineStandardBehavior(id);
                break;
        }
    }

    private void DefineStandardBehavior(string fsmName)
    {
        FSM_API.Create.CreateFiniteStateMachine(fsmName, 0, "DroneLogic") // Rate 0 = Event Driven (Optimization for massive numbers)

            // 1. Idle: Motors Off
            .State("Idle", (ctx) => SetAllMotors(ctx, ThrustLevel.Off), null, null)

            // 2. TakeOff: Motors 70% until altitude reached
            .State("TakeOff",
                (ctx) => SetAllMotors(ctx, ThrustLevel.Thrust70), // On Enter
                (ctx) => CheckAltitudeReached(ctx, "Hover"),       // On Update
                null)

            // 3. Hover: Motors 50% (Gravity compensation)
            .State("Hover", (ctx) => SetAllMotors(ctx, ThrustLevel.Thrust50), null, null)

            // 4. Moving: Tilt forward (Simplified for example)
            .State("Moving",
                (ctx) => SetAllMotors(ctx, ThrustLevel.Thrust80),
                (ctx) => CheckDestinationReached(ctx, "Hover"),
                null)

            // 5. Landing: Low thrust
            .State("Landing",
                (ctx) => SetAllMotors(ctx, ThrustLevel.Thrust30),
                (ctx) => CheckGrounded(ctx, "Idle"),
                null)

            .BuildDefinition();
    }

    // You can define completely different state machines for other types
    private void DefineAggressiveBehavior(string fsmName)
    {
        FSM_API.Create.CreateFiniteStateMachine(fsmName, 0, "DroneLogic")
            .State("Idle", (ctx) => SetAllMotors(ctx, ThrustLevel.Off), null, null)
            .State("AttackRun", (ctx) => SetAllMotors(ctx, ThrustLevel.Thrust100), null, null)
            // ... distinct logic ...
            .BuildDefinition();
    }

    // --- Shared Logic Helpers (Static for Performance) ---

    private static void SetAllMotors(IStateContext ctx, ThrustLevel level)
    {
        var drone = ctx as DroneController;
        if (drone == null || !drone.IsValid) return;

        // Command the 4 separate Motor FSMs
        drone.motors.FWD_LT.SetThrustLevel(level);
        drone.motors.FWD_RT.SetThrustLevel(level);
        drone.motors.AFT_LT.SetThrustLevel(level);
        drone.motors.AFT_RT.SetThrustLevel(level);
    }

    private static void CheckAltitudeReached(IStateContext ctx, string nextState)
    {
        var drone = ctx as DroneController;
        if (drone.transform.position.y >= drone.targetPosition.y)
        {
            drone.Status.TransitionTo(nextState);
        }
    }

    private static void CheckDestinationReached(IStateContext ctx, string nextState)
    {
        var drone = ctx as DroneController;
        if (Vector3.Distance(drone.transform.position, drone.targetPosition) < 1.0f)
        {
            drone.Status.TransitionTo(nextState);
        }
    }

    private static void CheckGrounded(IStateContext ctx, string nextState)
    {
        var drone = ctx as DroneController;
        // Simple check: if y is near 0 or raycast hits ground
        if (drone.transform.position.y < 0.5f)
        {
            drone.Status.TransitionTo(nextState);
        }
    }

    // --- Utilities ---

    private void AutoDiscoverMotors()
    {
        // Helper to find motors if not manually assigned in Inspector
        // Assumes child naming convention or order
        var allMotors = GetComponentsInChildren<MotorThrust>();
        if (allMotors.Length >= 4)
        {
            motors = new ThrustData();
            motors.FWD_RT = allMotors[0]; // You might want to match by name here
            motors.FWD_LT = allMotors[1];
            motors.AFT_RT = allMotors[2];
            motors.AFT_LT = allMotors[3];
        }
    }
}

