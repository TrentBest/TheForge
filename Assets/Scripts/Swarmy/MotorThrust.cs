using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_API.Scripts;
using UnityEngine;

// Expanded Enum for finer GUI control
public enum ThrustLevel
{
    // Reverse Stages
    Reverse100, Reverse75, Reverse50, Reverse25,

    // Idle
    Off,

    // Forward Stages (10% increments)
    Thrust10, Thrust20, Thrust30, Thrust40, Thrust50,
    Thrust60, Thrust70, Thrust80, Thrust90, Thrust100
}

public class MotorThrust : MonoBehaviour, IStateContext
{
    [Header("Configuration")]
    [Tooltip("Maximum force this motor can apply in Newtons")]
    public float maxThrustForce = 100f;
    public string motorID;

    [Header("Runtime Status")]
    // The FSM writes to this value ONCE when entering a state.
    // FixedUpdate reads this every physics tick.
    [SerializeField] private float currentThrottle = 0f;

    // API Handle - This gives us "Status.TransitionTo(..)"
    public FSMHandle Status;

    // Cache the Rigidbody for performance
    private Rigidbody droneRigidbody;

    // --- IStateContext Implementation ---
    public string Name
    {
        get => motorID;
        set => motorID = value;
    }

    // Start false. The API won't update this instance until we flip this true.
    public bool IsValid { get; set; } = false;

    private void Awake()
    {
        // 1. Setup Data
        if (string.IsNullOrEmpty(motorID)) motorID = System.Guid.NewGuid().ToString();
        droneRigidbody = GetComponentInParent<Rigidbody>();

        // 2. Define Shared FSM (First One Wins)
        if ( !FSM_API.Interaction.Exists("MotorThrust", "Thrusters"))
        {
            // Only grab the integration the one time we fail to find the FSM
            var unityIntegration = FindFirstObjectByType<FSM_UnityIntegrationAdvanced>();
            unityIntegration.AddProcessingGroup("LateUpdate", "Thrusters");

            // Define the Logic ONCE
            // Note: ProcessRate is 0 (Event Driven). 
            // We pass Logic to 'onEnter' (2nd param), and leave 'onUpdate' (3rd param) null.
            FSM_API.Create.CreateFiniteStateMachine("MotorThrust", 0, "Thrusters")
                // Reverse States
                .State("Reverse100", (ctx) => SetPower(ctx, -1.0f), null, null)
                .State("Reverse75", (ctx) => SetPower(ctx, -0.75f), null, null)
                .State("Reverse50", (ctx) => SetPower(ctx, -0.5f), null, null)
                .State("Reverse25", (ctx) => SetPower(ctx, -0.25f), null, null)

                // Off State
                .State("Off", (ctx) => SetPower(ctx, 0f), null, null)

                // Forward States
                .State("Thrust10", (ctx) => SetPower(ctx, 0.1f), null, null)
                .State("Thrust20", (ctx) => SetPower(ctx, 0.2f), null, null)
                .State("Thrust30", (ctx) => SetPower(ctx, 0.3f), null, null)
                .State("Thrust40", (ctx) => SetPower(ctx, 0.4f), null, null)
                .State("Thrust50", (ctx) => SetPower(ctx, 0.5f), null, null)
                .State("Thrust60", (ctx) => SetPower(ctx, 0.6f), null, null)
                .State("Thrust70", (ctx) => SetPower(ctx, 0.7f), null, null)
                .State("Thrust80", (ctx) => SetPower(ctx, 0.8f), null, null)
                .State("Thrust90", (ctx) => SetPower(ctx, 0.9f), null, null)
                .State("Thrust100", (ctx) => SetPower(ctx, 1.0f), null, null)
                .BuildDefinition();
        }

        // 3. Create the Instance for THIS motor
        Status = FSM_API.Create.CreateInstance("MotorThrust", this, "Thrusters");

        // 4. Engage! 
        IsValid = true;
    }

    // --- THE MUSCLE (FixedUpdate) ---
    // Runs on Physics Clock. Applies the force set by the last FSM State Entry.
    private void FixedUpdate()
    {
        // Safety: Don't run physics if we aren't valid yet or have no output
        if (!IsValid || currentThrottle == 0 || droneRigidbody == null) return;

        Vector3 force = transform.up * (maxThrustForce * currentThrottle);
        droneRigidbody.AddForceAtPosition(force, transform.position);
    }

    // --- THE BRAIN (Shared Static FSM Logic) ---
    // Only runs when a Transition occurs (Event Driven).
    private static void SetPower(IStateContext context, float throttlePercent)
    {
        MotorThrust motor = context as MotorThrust;
        if (motor == null) return;

        // We simply update the float. FixedUpdate handles the rest.
        motor.currentThrottle = throttlePercent;
    }

    // --- CONTROL METHOD (For the Drone/GUI) ---
    /// <summary>
    /// Call this to drive the motor. It triggers the FSM Transition.
    /// </summary>
    public void SetThrustLevel(ThrustLevel level)
    {
        if (Status != null)
        {
            // This is the Event that wakes the FSM
            Status.TransitionTo(level.ToString());
        }
    }
}