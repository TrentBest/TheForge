### Simple Light Demo 💡

The Simple Light Demo is a foundational example demonstrating how to use the Finite State Machine (FSM) API to control a simple object's state based on user input.

#### Prerequisites

* **FSM API Package:** Ensure you have the main FSM API package installed in your project.

#### Contents

The demo is organized into the following folders for clarity:

* **`Rendering/`**: Contains the Universal Render Pipeline (URP) assets specific to this demo, including the `FSM_UniversalRenderPipelineSettings` asset.
* **`Scenes/`**: Holds the `SimpleLight.unity` scene file.
* **`Scripts/`**: Includes the `LightFSM.cs` script that implements the FSM logic.
* **`ReadMe.md`**: This file.

#### Getting Started

1.  **Open the Scene:** Navigate to the `Scenes` folder and open the `SimpleLight.unity` scene.
2.  **Run the Demo:** Press **Play** to run the scene.
3.  **Control the Light:** Click anywhere on the screen or press any key to toggle the directional light on and off.

#### How It Works

This demo uses a single FSM with two states: **On** and **Off**.

* The **`LightFSM.cs`** script acts as the FSM's context.
* The `Awake()` method creates the FSM with the "On" and "Off" states and sets the initial state to "Off."
* The `Update()` method listens for any key press. When a key is pressed, it checks the current state and transitions to the opposing state (`"On"` transitions to `"Off"` and vice-versa).
* The `OnEnterOn()` and `OnEnterOff()` methods are called when the FSM enters their respective states. These methods contain the logic to set the light's active state, turning it on or off.

This demo provides a solid foundation for understanding the core FSM concepts: **states**, **transitions**, and **context**.

[**💖 Support Us**](https://www.paypal.com/donate/?hosted_button_id=3Z7263LCQMV9J)

## 🧠 Brought to you by

**The Singularity Workshop – Tools for the curious, the bold, and the systemically inclined.**

<a href="https://www.patreon.com/TheSingularityWorkshop" target="_blank">
    <img src="Branding/TheSingularityWorkshop.png" alt="Support The Singularity Workshop on Patreon" height="200" style="display: block;">
</a>

Because state shouldn’t be a mess.
