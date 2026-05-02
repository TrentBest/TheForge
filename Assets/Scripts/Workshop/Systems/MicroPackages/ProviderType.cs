namespace Workshop.Systems.MicroPackages
{
    public enum ProviderType
    {
        None = 0,

        // --- Core Logic & Compute (FSM & Math) ---
        StateOnEnter = 1,
        StateOnUpdate = 2,
        StateOnExit = 3,
        TransitionCondition = 4,
        Transition = 5,
        State = 6,
        FiniteStateMachine = 7,
        Context = 11,
        MathExpression = 100,
        LogicGate = 101,
        ProceduralRule = 102,

        // --- UI & Interaction ---
        GuiElement = 8,
        GUI = 9,               // Legacy/Base GUI
        PackagePreview = 13,
        CommonGUI = 14,        // Standardized fluent UI interfaces
        DiegeticTerminal = 15, // In-world interactive surfaces
        InputGesture = 16,     // Mapped input profiles

        // --- Media & Physical Assets ---
        Image = 12,
        AudioClip = 20,
        MeshAsset = 21,
        MaterialAsset = 22,
        ShaderVariant = 23,
        AnimationRig = 24,
        VfxPayload = 25,
        SpatialAnchor = 26,

        // --- Integration, APIs, & Data Bridges ---
        GurpsApi = 10,
        RestEndpoint = 30,
        NormalBimSchema = 31,  // Architectural/CAD data bridging
        DataWarehouseSync = 32,
        DynamicLink = 33,      // Remote URIs or cross-package references
        BinaryBlob = 34,

        // --- Simulation & Gameplay Systems ---
        AgentProfile = 40,     // Swarm/100k agent definitions
        PathfindingNavMesh = 41,
        LootManifest = 42,
        TechTreeNode = 43,
        BallisticsProfile = 44,
        FactionAlignment = 45,

        // --- MetaDev, Tooling & Analytics ---
        TelemetryHook = 50,    // Live metrics and performance shunts
        MigrationRule = 51,    // Logic for breaking changes across version barriers
        ValidationLedger = 52, // Logical integrity checks
        LogStream = 53,

        // --- Sovereign Rendering Pipeline ---
        RenderPass = 60,       // Standard draw manifestation (DrawMeshInstancedIndirect)
        ComputePass = 61,      // GPU compute shunts (Nebula, Swarm Physics, Environmental logic)
        CullingPass = 62,      // Visibility and frustum sorting logic
        PostProcessPass = 63   // Full-screen, temporal, or color-grading manifestations
    }
}