# 🤖 AI Developer Guide: TheSingularityWorkshop FSM_API (Advanced Integration)

## 📌 1. System Overview & Philosophy

You are generating code for a Unity project that uses **TheSingularityWorkshop FSM_API Advanced Integration**.

Unlike standard monolithic State Machines, this API operates on an **"Experience Model"**:

* **Basic Integration** is for isolated, single "In-App Experiences" (e.g., a simple game like Pac-Man).
* **Advanced Integration** (this package) manages the **"Full Experience"** or Environment (e.g., the Arcade Machine itself, expansive AI, World State, and Editor Tooling).
* **Core Principle**: Logic is completely decoupled from ticking. FSMs are defined once, instanced with distinct data Contexts, and ticked via **Processing Groups**.

## ⚙️ 2. The Runtime Engine: `FSM_UnityIntegrationAdvanced`

At runtime, FSMs are driven by the `FSM_UnityIntegrationAdvanced` class.

* **What it is**: A Singleton `MonoBehaviour` that acts as the heartbeat bridging the engine-agnostic FSM_API and Unity's main thread.
* **How it works**: It maps Unity lifecycle events (`Awake`, `Start`, `Update`, `FixedUpdate`, `LateUpdate`, `OnGUI`, `OnDrawGizmos`) to **Processing Groups**.
* **Usage Rule**: Do not write standard `Update()` logic inside individual MonoBehaviours. Instead, assign FSM instances to specific Processing Groups (e.g., `"AI_Movement"`, `"Physics_HighPriority"`).

**Code Generation Example (Runtime):**

```csharp
// 1. Add a custom group to Unity's Update loop via the singleton
FSM_UnityIntegrationAdvanced.Instance.AddProcessingGroup("Update", "AI_Movement");

// 2. Define the FSM and assign it to that group
FSM_API.Create.CreateFiniteStateMachine("CharacterAI", processRate: 1, processingGroup: "AI_Movement")
    /* states... */
    .BuildDefinition();

// 3. Create instance
FSM_API.Create.CreateInstance("CharacterAI", new CharacterContext(), "AI_Movement");

```

## 👻 3. The Editor Engine: `FSM_EditorIntegrationAdvanced` (The "Ghost in the Machine")

The Advanced API can run state logic *without* a GameObject, completely inside the Unity Editor during edit mode.

* **What it is**: A static `[InitializeOnLoad]` class that hooks FSM Processing Groups directly into Unity Editor events (`EditorApplication.update`, `hierarchyChanged`, `projectChanged`, `SceneView.duringSceneGui`).
* **Usage Rule**: When building Editor Tools, custom inspectors, or procedural generation tools, bind the FSMs to Editor groups.

**Code Generation Example (Editor Tooling):**

```csharp
// Add an FSM group to run every time the Editor Updates
FSM_EditorIntegrationAdvanced.AddProcessingGroup("EditorUpdate", "MyProceduralGenerator");

// If your UI needs to force Unity to redraw the Scene View/Panels dynamically, 
// bind to the special "Panels" group:
FSM_EditorIntegrationAdvanced.AddProcessingGroup("EditorUpdate", "Panels"); 

```

## 🎨 4. UI Toolkit & Custom Editor Windows (`Asteroids_Gui_Hangar` Pattern)

When generating UI using Unity's **UI Toolkit (UIElements)** alongside the FSM_API, do not rely on standard Unity Monobehaviour updates. Instead, use the UI Element's built-in `schedule` API to tick a specific FSM processing group.

**The "Asteroids Hangar" Pattern for UI:**
When an AI needs to generate a dynamic UI class (like `IGuiProvider` implementations), it must encapsulate the FSM tick directly within the VisualElement's schedule:

```csharp
public VisualElement CreateGui(GuiContext context)
{
    var rootContainer = new VisualElement { style = { flexGrow = 1 } };
    
    // Build your UI...
    // rootContainer.Add(...);

    // --- CRITICAL UI TICK PATTERN ---
    // Drive the FSM_API locally for this specific UI window/element
    rootContainer.schedule.Execute(() => {
        // 1. Tick the specific FSM group for this UI
        FSM_API.Interaction.Update("HangarPreview");
        
        // 2. Trigger any necessary local visual refreshes
        _mainOrbitView?.ForceRender();
    }).Every(16); // ~60fps target

    return rootContainer;
}

```

## 🛠️ 5. AI Code Generation Rules for this API

When asked to write an FSM or an Editor Window using this API, always adhere to the following rules:

1. **Zero Boilerplate Updates**: Never write Unity `Update()` loops to tick state machines manually. Always assign the FSM to a Processing Group.
2. **Decouple Context**: Keep state logic in the Definition, and mutable data in the Context object. Multiple objects should be able to run the exact same FSM Definition simultaneously via Composition.
3. **Use Editor Integration for Tooling**: If asked to write a custom Editor Window (e.g., using `FsmEngineDashboardCard` as a reference), use UI Toolkit and bind background logic to `FSM_EditorIntegrationAdvanced` or a `VisualElement.schedule`.
4. **Graceful Teardowns**: For runtime, ensure singletons don't duplicate. `FSM_UnityIntegrationAdvanced` handles its own `DontDestroyOnLoad` and duplication destruction. For Editor loops, always unsubscribe before subscribing to avoid duplicate hooks on script reload.
