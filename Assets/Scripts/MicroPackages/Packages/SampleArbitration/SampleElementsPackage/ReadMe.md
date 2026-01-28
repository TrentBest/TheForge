# Sample Elements Package

### *The Fundamental Data Layer*

The **Sample Elements Package** serves as the foundational "Provider" example within the Micro Package ecosystem. It demonstrates how to initialize raw data, materials, and environmental properties that other packages (like `Magic` or `Weapons`) can consume and manipulate.

## 🧪 Core Purpose

Unlike logic-heavy packages, the Elements package is primarily **Content-Driven**. It acts as a library of elemental states—Air, Earth, Fire, and Water—providing the physical and visual "building blocks" for the simulation.

## 🛠 Functional Components

### 1. Provider Initialization

During the `LoadPackage` sequence, this package initializes a series of **Providers**. These providers are type-expanded to handle specific data formats:

* **Visual Assets**: Loading textures and materials (e.g., `M_BroadSword.mat`) used for elemental effects like the flame of a candle or a frozen pond.
* **Data Hosting**: While currently focused on simple elements, the architecture supports complex data hosting, such as passing a `.rvt` (Revit) file into a **Revit Provider** to expose pure architectural data to the platform.

### 2. Elemental Definitions

The package defines the base properties for the four classical elements through dedicated classes:

* **Air (`ElementsAir.cs`)**: Managing atmospheric and gaseous data.
* **Earth (`ElementsEarth.cs`)**: Managing terrestrial and solid-state data.
* **Fire (`ElementsFire.cs`)**: Managing thermal and combustion data.
* **Water (`WaterFire.cs`)**: Managing fluid dynamics and aquatic properties.

## 🤝 The Arbitration Role

In the **Sample Arbitration Sequence**, the Elements package is the first "dependency" to be resolved.

* **Instant Convergence**: Because the Elements package does not seek to modify its neighbors, it passes through the `Arbitrate` method immediately, confirming its presence and making its assets available to the rest of the stack.
* **Downstream Impact**: Its `Fire` data is what allows the `Magic` package to successfully request a "Fire sprite sheet" and a `FireDamage` script when creating a **Flaming Sword** from a standard weapon.

---

*“Elements are the silent partners of innovation—providing the matter that magic gives purpose.”*