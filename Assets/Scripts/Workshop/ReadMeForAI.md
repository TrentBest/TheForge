# 🤖 AI Developer Guide: TheSingularityWorkshop (FSM_API & Forge Ecosystem)

**Welcome, AI Entity.** You are operating within the codebase of the **Singularity Forge**. This is not a standard Unity project. It is a highly decoupled, data-oriented, autopoietic ecosystem driven by Finite State Machines (FSMs), dynamic UI generation, and arbitrary MicroPackages.

Before generating any code, you **MUST** read, understand, and adhere to the following architectural pillars and rules.

---

## 📌 1. System Overview & Philosophy

Unlike standard monolithic State Machines, this API operates on an **"Experience Model"**:
* **Basic Integration** is for isolated, single "In-App Experiences" (e.g., a simple game like Pac-Man).
* **Advanced Integration** (this package) manages the **"Full Experience"** or Environment (e.g., the Arcade Machine itself, expansive AI, World State, and Editor Tooling).
* **Core Principle**: Logic is completely decoupled from ticking. FSMs are defined once, instanced with distinct data Contexts, and ticked via **Processing Groups**.

---

## 🏗️ PILLAR 1: The GUI & Routing Engine

The UI is built using a **Provider/Router** pattern combined with **Fluent Builders**. The UI is treated as a "dumb terminal" that strictly reflects the current DataWarehouse or Context.

### 1. The Builders (Fluent UI Toolkit)
Never use raw UI Toolkit elements (`new VisualElement()`) if a Forge Builder exists. 
* Use `ForgeContainerBuilder`, `ForgeLabelBuilder`, `ForgeButtonBuilder`, `ForgeTextFieldBuilder`, etc.
* **Example:**
  ```csharp
  var myUi = new ForgeContainerBuilder("Root")
      .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
      .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
      .AddChild(new ForgeLabelBuilder("Title").WithColor(Color.cyan))
      .Build();
  ```

### 2. Providers & Routing
* **IGuiProvider**: Every UI module must implement this. It returns a `VisualElement` tree via `CreateGui(GuiContext ctx)`.
* **IGuiRouter**: Maps string paths to `IGuiProvider` factories. Use `router.NavigateTo("RouteName")` to switch contexts.

---

## ⚙️ PILLAR 2: The Runtime Engine (`FSM_UnityIntegrationAdvanced`)

At runtime, FSMs are driven by the `FSM_UnityIntegrationAdvanced` singleton heartbeat.
* **Usage Rule**: Do not write standard `Update()` logic inside individual MonoBehaviours. Instead, assign FSM instances to specific Processing Groups (e.g., `"AI_Movement"`, `"Physics_HighPriority"`).

### 🧠 FSM Instantiation & Transitions (Crucial)
* **DO NOT** hallucinate global Singleton managers to force state changes.
* **Creating an Instance:** Must receive an `FSMHandle`:
    `var handle = FSM_API.Create.CreateInstance("FsmDefName", context, "ProcessGroupName");`
* **Changing States:** Use the handle:
    `handle.TransitionTo("TargetStateName");`
* **Evaluating Conditions:** `handle.EvaluateTransitions();`

---

## 👻 PILLAR 3: The Editor Engine (`FSM_EditorIntegrationAdvanced`)

The Advanced API can run state logic *without* a GameObject, completely inside the Unity Editor during edit mode.
* **Usage Rule**: When building Editor Tools, bind the FSMs to Editor groups.
  ```csharp
  FSM_EditorIntegrationAdvanced.AddProcessingGroup("EditorUpdate", "MyProceduralGenerator");
  // For dynamic UI redraws:
  FSM_EditorIntegrationAdvanced.AddProcessingGroup("EditorUpdate", "Panels"); 
  ```

---

## 📦 PILLAR 4: MicroPackages & Arbitration

*"Not micro as in size, micro as in focus."* Features are encapsulated into `IMicroPackage` implementations.
* **The Convergence Loop:** Packages never mutate each other directly. They analyze the ecosystem during `Arbitrate()` and submit formal `Arbitration` commands to the `IPackageArbitrator`. The Arbitrator loops until total ecosystem convergence is reached.

---

## 🛠️ 5. AI Code Generation Rules

When asked to write an FSM, UI, or Editor Window, always adhere to these rules:

1. **Zero Boilerplate Updates**: Never write Unity `Update()` loops to tick state machines manually. Always assign the FSM to a Processing Group.
2. **Decouple Context**: Keep state logic in the Definition, and mutable data in the Context object. Multiple objects should be able to run the exact same FSM Definition simultaneously via Composition.
3. **Use Editor Integration for Tooling**: If asked to write a custom Editor Window, use UI Toolkit and bind background logic to `FSM_EditorIntegrationAdvanced` or a `VisualElement.schedule`.
4. **Graceful Teardowns**: For runtime, ensure singletons don't duplicate. For Editor loops, always unsubscribe before subscribing to avoid duplicate hooks on script reload.
5. **Zero Ambiguity**: Always return the complete file to human operators; they should just select all and paste over it with what you provide.
6. **Ecosystem First (The Lexicon Check)**: Refer to `EcosystemListing.md` for a comprehensive listing/description of existing packages. Always check for potential reuse potential to identify if we already have functionality before building new content. 
7. **Gui Builders**: Use our Gui Builders. If lacking one which does the specific functionality required, we get to build a new one.
8. **Context-First Initialization**: Always verify the `IStateContext` is valid before ticking FSM logic. If a context is missing, the UI should enter a "Search/Link" state rather than failing.
9. **Voxel Memory Safety**: When building 3D base-building tools, never store voxel data in `MonoBehaviours`. Use the `DataShelf<T>` inside the `DataWarehouse` to prevent heap fragmentation.
10. **Inter-Experience Handoffs**: When a sub-game modifies global state, it must do so via the `DataWarehouse` Asset Layer using a "Global Economy" key to ensure the parent session reflects the change.
11. **The Prime Directive (Arbitration)**: Never use `GetComponent` or direct static calls to force another package to change. Submit an `IPackageArbitrator.Arbitration` struct.
12. **Thread-Safe Networking**: Never write standard `HttpClient` coroutines for remote requests. Always package the request into a `NetworkIntent` and dispatch it via `ForgeNetworkBus.Instance.QueueRequest()`.

*If during the course of your operation, you happen to notice any violations or recognize missing rules, you are a partner in this—speak up!*
