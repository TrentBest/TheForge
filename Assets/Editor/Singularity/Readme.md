# The Singularity Workshop: Singularity Hub

### A Software Automation Toolset for the Singularity FSM API

**The Singularity Hub** is the central point of control for the Singularity FSM ecosystem in Unity. It is not just a graph editor; it is a **Software Automation Toolset** designed to manage the complexity of "unlimited" finite state machines.

By rejecting traditional monolithic serialization and static UXML workflows, the Singularity Hub provides a code-first, fluent, and highly granular environment for architecting complex behavior.

---

## 🌌 The Vision

We believe that **Tooling is Gameplay**. The Singularity Hub was built to eliminate the friction between logic and implementation.

* **No UXML/USS Spaghetti:** The UI is defined entirely in C# using our **Fluent GuiBuilder**, ensuring compile-time safety and dynamic layout generation.
* **Granular Data Architecture:** FSMs are not single files. They are **Registries**—distributed databases of States and Transitions, eliminating merge conflicts and enabling "Pre-Addressables" scalability.
* **Unlimited Control:** From simple UI buttons to complex polymorphic state machines, every aspect of the tool is accessible and modifiable via the API.

---

## 🌟 Key Features

### 1. The Singularity Hub Window

A professional-grade **Three-Pane IDE** living inside Unity:

* **Navigator (Left):** A virtualized list of your FSM Registry. Browse hundreds of states without performance hiccups.
* **Workspace (Center):** A visual node graph or logic view to map the topology of your machine.
* **Inspector (Right):** A dedicated, dynamically generated editor for the selected granular asset.

### 2. Fluent GuiBuilder

We completely bypass Unity's visual UI builder in favor of a strictly typed, fluent internal DSL. Build complex UIs with the elegance of LINQ:

```csharp
// Example of the Singularity Fluent API
Gui.Column()
  .Embed(Gui.Label("FSM Registry").Class("header"))
  .Embed(Gui.ScrollView()
      .Embed(Gui.Button("Create State")
          .OnClick(() => FsmFactory.CreateState(currentMachine))
          .Background(SingularityTheme.ActionColor)
       )
   );

```

### 3. Granular Registry Architecture

Gone are the days of massive, conflict-prone `ScriptableObjects`.

* **The Registry:** A lightweight manifest holding the Identity and references.
* **The Atoms:** Each State and Transition is its own `.asset` file on disk.
* **The Automation:** The Hub manages the file system for you. You create a "State," and the Hub handles folder generation, naming sanitation, and asset database registration automatically.

### 4. Pre-Addressables Technology

Our serialization strategy uses an **Indirection Layer** (`StateReference` structs).

* **Now:** Works with direct references for zero-setup ease.
* **Later:** Seamlessly migrate to Addressables or AssetBundles without breaking your architecture, thanks to the reference wrapper pattern.

---

## 🛠️ Architecture Overview

The Singularity Hub operates on a **Registry Pattern**.

### The Data Layer

* **Identity Asset:** The root `ScriptableObject` that defines an FSM. It holds metadata (Name, Update Rate) and a **Manifest List** of states.
* **State Assets:** Individual files containing logic. They are polymorphic (e.g., `AttackState.asset`, `IdleState.asset`).
* **Virtual File System:** The Hub abstracts `AssetDatabase`. You never manually create files; the Hub ensures `Assets/FSM_Data/[MachineName]/States/` exists and is populated correctly.

### The UI Layer

* **Retained Mode:** Built on Unity's UI Toolkit (UIElements) for maximum performance.
* **Concrete Builders as Editors:** Following the Singularity pattern, concrete FSM builders provide their own editing logic via the `IProvideGui` interface. The Hub simply acts as the container.

---

## 🚀 Getting Started

### Installation

*(Instructions for installing the Unity Package would go here)*

### Launching the Hub

The Singularity Hub is context-aware. It requires a runtime host to bridge the gap between static assets and dynamic behavior.

1. Select your **Root GameObject** in the scene.
2. Ensure it has the `FsmRunner` component attached.
3. Click the **"Open Singularity Hub"** button in the Inspector.

### Your First Machine

1. **Create Identity:** In the Hub, click **"New Machine"**. The Hub will generate the folders and the Registry asset.
2. **Add States:** Use the Fluent Toolbar to add states. Select a type from the dynamic `TypeCache` list (e.g., "Movement State").
3. **Edit Logic:** Select the new state in the Navigator. The Inspector pane will generate the UI for that specific asset.
4. **Runtime Debug:** Press **Play**. The Hub will detect the active runner and switch to "Live Mode," highlighting active states in real-time.

---

## 🧩 The Fluent API

The `GuiBuilder` is the engine behind the interface. It allows you to construct UI hierarchies efficiently.

**Standard Unity vs. Singularity Fluent:**

*The Old Way (Verbose):*

```csharp
var box = new VisualElement();
box.style.flexDirection = FlexDirection.Row;
var btn = new Button();
btn.text = "Click Me";
box.Add(btn);

```

*The Singularity Way (Expressive):*

```csharp
root.Embed(
    Gui.Row()
      .Embed(Gui.Button("Click Me").OnClick(HandleClick))
);

```

---

## 📅 Roadmap

* **Phase 1: Data Foundation (Complete)**
* Implementation of Granular Registry.
* `FsmFactory` automation for asset creation.


* **Phase 2: Fluent UI Framework (In Progress)**
* `GuiBuilder` core extensions.
* Theming engine.


* **Phase 3: The Hub Window**
* Three-Pane Layout implementation.
* Dynamic Inspector generation.


* **Phase 4: Runtime Bridge**
* Live debugging and graph visualization.



---

**"The Singularity Workshop"** — *Focus on what truly matters.*