# TheForge

### *Where Logic Meets Construction*

Welcome to **TheForge**, the central development environment and architectural foundation for **The Singularity Workshop**. This project serves as a high-level "Meta-Framework" for Unity, designed to bridge the gap between complex software patterns (Finite State Machines, Micro Packages) and intuitive, data-driven editor tools.

TheForge isn't just a collection of scripts; it is an automated pipeline for building, simulating, and documenting complex systems—from architectural BIM-inspired structures to autonomous drone swarms.

---

## 🏗 Core Philosophy: The "Micro Package"

TheForge is built upon the **Micro Package** architecture. Instead of monolithic systems, functionality is broken down into atomic, independent units that are:

* **Self-Describing:** Using `ISingularityDefinition` to manage their own metadata and library paths.
* **Registry-Backed:** Automatically serialized and tracked via the `SingularityRegistry<T>`, which manages JSON persistence within Unity's `StreamingAssets`.
* **Arbitrated:** Using an `IPackageArbitrator` to resolve dependencies and interactions between disparate packages at runtime.

## 🛠 Key Systems

### 1. The Singularity Workshop Hub

The **Hub** (`SingularityWorkshopHub.cs`) is your central nervous system within the Unity Editor. It provides a unified docking station for all development activities, including:

* **System Metrics:** Real-time tracking of FSM Definitions and active Handles.
* **Integrated CRUD Tabs:** Dedicated interfaces for managing FSMs, States, Logic Methods, and Transitions without leaving the Hub.
* **Dynamic Viewports:** A custom-built navigation system that allows for seamless switching between different workshop tools.

### 2. Graphical User Interface Builder (OneGUI)

To support the complex data requirements of TheForge, we developed a fluent **GUI Builder** API (`GraphicalUserInterfaceBuilder.cs`). This system allows us to:

* **Build via Code:** Define complex UIElements layouts using a chainable, readable API.
* **Context-Aware UI:** Toggle between "Editor Mode" (for data entry/creation) and "Simulation Mode" (for runtime visualization).
* **Modular Layouts:** Features like `WithAutoGrow`, `WithScrollable`, and nested `WithPanel` support create a responsive, modern editor experience.

### 3. Experience & Forge Builders

The project introduces the concept of **Manifest-Driven Development**:

* **Experience Builder:** A tool to define "Experience Manifests" that combine different sensory inputs (Vision, Audio, Touch) and providers into a single JSON-backed configuration.
* **The Forge Contract:** Interfaces like `IForgeBuilder` and `IBuilder` ensure that every system in the project knows how to generate its own runtime instance and its own Editor UI.

---

## 🚀 Projects Powered by TheForge

* **raWWar:** A tactical showcase utilizing the `FSM_API` for advanced unit AI.
* **Builders:** A world-building application applying BIM (Building Information Modeling) principles to game environments.
* **SpaceShip Sim:** A celestial body and ship design simulation utilizing the modularity of Forge Builders.

---

## 📂 Project Structure

| Directory | Purpose |
| --- | --- |
| `Editor/` | The Hub, Experience Manifest Editors, and custom Inspector windows. |
| `Scripts/Builders/` | Implementation of the Fluent UI API and system-specific constructors. |
| `Scripts/MicroPackages/` | The interfaces and logic for the atomic "Singularity" architecture. |
| `Scripts/FSMs/` | (Integration) Bridges the core FSM_API with the Unity environment. |

---

## ⚡ Quick Start for Developers

1. Open the **Singularity Hub** via `Singularity > Workshop Hub` to see the current system stats.
2. Use the **Experience Manifest Editor** (`Tools > Builders > Experience Manifest Editor`) to define new sensory configurations.
3. All data is saved to `StreamingAssets/Singularity/` in a clean, version-control-friendly JSON format.

---

*“We are not just coding games; we are forging the architectures of the future.”* — **Trent Best**, The Singularity Workshop