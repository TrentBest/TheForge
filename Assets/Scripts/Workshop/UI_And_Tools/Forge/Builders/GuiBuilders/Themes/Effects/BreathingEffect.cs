// File: Assets/Scripts/Workshop/Forge/Builders/GuiBuilders/Themes/Effects/BreathingEffect.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes.Effects
{
    public class BreathingEffect : IStateContext
    {
        public FSMHandle Status { get; private set; }
        public bool IsValid { get; set; } = false;
        public string Name { get; set; } = "Forge_Breathing_Effect";

        // --- Debugging ---
        public bool DebugMode { get; set; } = true;
        private float _lastLogTime; // Used to throttle update logs

        // --- Visual Target & Configuration ---
        public VisualElement TargetElement { get; private set; }
        public List<Color> Colors { get; set; } = new List<Color>() { Color.cyan, Color.gray };

        public float BreathingRate { get; set; } = 2f;

        // Cranked up defaults for maximum visibility
        public float MinBorderWidth { get; set; } = 2f;
        public float MaxBorderWidth { get; set; } = 15f;
        public float MinBorderRadius { get; set; } = 0f;
        public float MaxBorderRadius { get; set; } = 30f;

        // --- Control Flags ---
        public bool IsActive { get; set; } = true;
        public bool PauseAtEquilibrium { get; set; } = false;

        public string ProcessingGroup { get; private set; }
        public int ProcessingRate { get; private set; }

        // --- public Math State ---
        private float _timeOffset;
        private bool _isAtEquilibrium;

        public BreathingEffect(VisualElement target, int processingRate = 0, float breathingRate = 2f, string processingGroup = "EditorUpdate", List<Color> breathingColors = null)
        {
            TargetElement = target;
            BreathingRate = breathingRate;
            ProcessingGroup = processingGroup;
            ProcessingRate = processingRate;
            if (breathingColors != null) Colors = breathingColors;

            _timeOffset = Time.realtimeSinceStartup;

            if (!FSM_API.Interaction.Exists(Name, processingGroup))
            {
                FSM_API.Create.CreateProcessingGroup(processingGroup);
                FSM_API.Create.CreateFiniteStateMachine(Name, ProcessingRate, ProcessingGroup)
                    .State("Equilibrium", OnEnterEquilibrium, OnUpdateEquilibrium, null)
                    .State("Flowing", OnEnterFlowing, OnUpdateFlowing, null)
                    .Transition("Equilibrium", "Flowing", ShouldFlow)
                    .Transition("Flowing", "Equilibrium", IsEquilibriumCommanded)
                    .BuildDefinition();
            }

            Status = FSM_API.Create.CreateInstance(Name, this, ProcessingGroup);
            IsValid = true;

            if (DebugMode) Debug.Log($"[BreathingEffect] INITIALIZED on target '{TargetElement?.name}'. BreathingRate: {BreathingRate}, Group: {ProcessingGroup}");
        }

        // =========================================================================
        // STATIC FSM DELEGATES
        // =========================================================================

        private static bool ShouldFlow(IStateContext context)
        {
            if (context is BreathingEffect effect)
            {
                return effect.IsActive && !effect.PauseAtEquilibrium;
            }
            return false;
        }

        private static bool IsEquilibriumCommanded(IStateContext context)
        {
            if (context is BreathingEffect effect)
            {
                if (!effect.IsActive) return true;
                if (effect.PauseAtEquilibrium && effect._isAtEquilibrium) return true;
            }
            return false;
        }

        // --- EQUILIBRIUM STATE ---

        private static void OnEnterEquilibrium(IStateContext context)
        {
            if (context is BreathingEffect effect)
            {
                if (effect.DebugMode) Debug.Log($"[BreathingEffect] ENTERED STATE: Equilibrium. (Target: {effect.TargetElement?.name})");

                effect._isAtEquilibrium = true;
                ApplyStyling(effect, 0.5f);
            }
        }

        private static void OnUpdateEquilibrium(IStateContext context) { }

        // --- FLOWING STATE ---

        private static void OnEnterFlowing(IStateContext context)
        {
            if (context is BreathingEffect effect)
            {
                if (effect.DebugMode) Debug.Log($"[BreathingEffect] ENTERED STATE: Flowing. (Target: {effect.TargetElement?.name})");

                effect._isAtEquilibrium = false;
            }
        }

        private static void OnUpdateFlowing(IStateContext context)
        {
            if (context is BreathingEffect effect)
            {
                // Safety check to ensure we aren't ticking on a dead visual element
                if (effect.TargetElement == null || effect.TargetElement.panel == null)
                {
                    if (effect.DebugMode && (Time.realtimeSinceStartup - effect._lastLogTime) > 2f)
                    {
                        Debug.LogWarning("[BreathingEffect] WARNING: FSM is still ticking but the VisualElement was removed from the UI Panel! (Potential memory leak: destroy this FSM instance)");
                        effect._lastLogTime = Time.realtimeSinceStartup;
                    }
                    return;
                }

                float t = (Time.realtimeSinceStartup - effect._timeOffset) * effect.BreathingRate;
                float wave = (Mathf.Sin(t) + 1f) / 2f;

                effect._isAtEquilibrium = Mathf.Abs(wave - 0.5f) < 0.05f;

                // Throttled Debug Logging (Once per second)
                if (effect.DebugMode && (Time.realtimeSinceStartup - effect._lastLogTime) > 1f)
                {
                    float currentWidth = Mathf.Lerp(effect.MinBorderWidth, effect.MaxBorderWidth, wave);
                    Debug.Log($"[BreathingEffect] PUMPING... WavePos: {wave:F2} | Current Thick Border: {currentWidth:F1}px");
                    effect._lastLogTime = Time.realtimeSinceStartup;
                }

                ApplyStyling(effect, wave);
            }
        }

        // --- STYLING APPLICATOR ---

        private static void ApplyStyling(BreathingEffect effect, float wavePosition)
        {
            if (effect.TargetElement == null) return;

            float thickVal = Mathf.Lerp(effect.MinBorderWidth, effect.MaxBorderWidth, wavePosition);
            float thinVal = Mathf.Lerp(effect.MaxBorderWidth, effect.MinBorderWidth, wavePosition);

            float thickRad = Mathf.Lerp(effect.MinBorderRadius, effect.MaxBorderRadius, wavePosition);
            float thinRad = Mathf.Lerp(effect.MaxBorderRadius, effect.MinBorderRadius, wavePosition);

            Color colorA = effect.Colors.Count > 0 ? effect.Colors[0] : Color.cyan;
            Color colorB = effect.Colors.Count > 1 ? effect.Colors[1] : Color.gray;

            // THE FIX: Explicitly force Alpha to 1.0f to kill the "ghost" transparency bug!
            colorA.a = 1.0f;
            colorB.a = 1.0f;

            // Actively blend the colors along the sine wave
            Color currentThickColor = Color.Lerp(colorA, colorB, wavePosition);
            Color currentThinColor = Color.Lerp(colorB, colorA, wavePosition);

            var style = effect.TargetElement.style;

            // Apply Thickness 
            style.borderTopWidth = thickVal;
            style.borderBottomWidth = thickVal;
            style.borderLeftWidth = thinVal;
            style.borderRightWidth = thinVal;

            // Pair Thickness with corresponding Radius
            style.borderTopLeftRadius = thickRad;
            style.borderBottomRightRadius = thickRad;
            style.borderTopRightRadius = thinRad;
            style.borderBottomLeftRadius = thinRad;

            // Apply Animated Colors
            style.borderTopColor = currentThickColor;
            style.borderBottomColor = currentThickColor;
            style.borderLeftColor = currentThinColor;
            style.borderRightColor = currentThinColor;
        }
    }
}