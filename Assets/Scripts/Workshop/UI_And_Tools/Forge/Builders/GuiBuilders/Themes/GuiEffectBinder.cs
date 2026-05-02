using TheSingularityWorkshop.FSM_API;
using UnityEngine.UIElements;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes
{
    public static class GuiEffectBinder
    {
        /// <summary>
        /// Reads the GuiEffect definitions from a StyleProfile, registers them if missing, 
        /// and spins up instances tied to the target VisualElement.
        /// </summary>
        public static void ApplyThemeEffects(VisualElement target, StyleProfile profile, GuiContext baseCtx)
        {
            if (profile.AttachedEffects == null || profile.AttachedEffects.Count == 0) return;

            foreach (var effect in profile.AttachedEffects)
            {
                // 1. Ensure the FSM Definition exists in the API
                if (!FSM_API.Interaction.Exists(effect.FsmTemplateName, effect.ProcessingGroup))
                {
                    Debug.Log($"[GuiEffectBinder] Auto-Registering missing Theme FSM: {effect.FsmTemplateName}");

                    // Note: In a real scenario, you'd likely fetch the complex FSM blueprint from the DataWarehouse.
                    // For now, we scaffold a generic UI effect structure.
                    FSM_API.Create.CreateFiniteStateMachine(effect.FsmTemplateName, -1, effect.ProcessingGroup)
                        .State(effect.InitialState, null, null, null)
                        .State("Hover", null, null, null)
                        .State("Active", null, null, null)
                        .WithInitialState(effect.InitialState)
                        .BuildDefinition();
                }

                // 2. Create a specialized Context that bridges the FSM to this specific VisualElement
                var effectContext = new GuiEffectContext
                {
                    TargetElement = target,
                    BaseContext = baseCtx,
                    ThemeProfile = profile
                };

                // 3. Spin up the Instance!
                var fsmHandle = FSM_API.Create.CreateInstance(effect.FsmTemplateName, effectContext, effect.ProcessingGroup);

                // 4. Hook up basic UI Toolkit events to drive the FSM state changes automatically
                target.RegisterCallback<MouseEnterEvent>(evt => fsmHandle.TransitionTo("Hover"));
                target.RegisterCallback<MouseLeaveEvent>(evt => fsmHandle.TransitionTo(effect.InitialState));
                target.RegisterCallback<PointerDownEvent>(evt => fsmHandle.TransitionTo("Active"));
                target.RegisterCallback<PointerUpEvent>(evt => fsmHandle.TransitionTo("Hover"));

                // Cleanup when the UI element is destroyed
                target.RegisterCallback<DetachFromPanelEvent>(evt => {
                    // Tell FSM API to tear down this instance
                    fsmHandle.TransitionTo("Teardown");
                });
            }
        }
    }

    /// <summary>
    /// This context is passed into the FSM's OnUpdate/OnEnter methods.
    /// It allows your FSM logic to manipulate the actual UI Toolkit properties (like border colors/widths).
    /// </summary>
    public class GuiEffectContext : IStateContext
    {
        public VisualElement TargetElement;
        public GuiContext BaseContext;
        public StyleProfile ThemeProfile;

        public bool IsValid { get; set; } = false;
        public string Name { get; set; } = "Effect Without a Name";
    }
}