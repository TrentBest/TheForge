// File: Assets/Scripts/Workshop/Cast/Gestures/EquationOfMotion.cs
using System;
using UnityEngine;

namespace Workshop.Gameplay.Cast.Gestures
{


    [Serializable]
    public class EquationOfMotion
    {
        public enum EaseProfile { Linear, SmoothStep, Snap, Anticipate, Spring }

        [Header("Timing")]
        public float Duration = 1.0f;
        public EaseProfile Easing = EaseProfile.SmoothStep;

        [Header("Fuzzification")]
        public float MaxPositionalError = 0.1f; // How far off path can it get?
        public float MaxRotationalError = 15f;  // Degrees of wobble
        public float HesitationFactor = 0.2f;   // How much time distortion occurs (stuttering)

        // The pure mathematical curve
        public float EvaluateIdeal(float t)
        {
            return Easing switch
            {
                EaseProfile.Linear => t,
                EaseProfile.SmoothStep => t * t * (3f - 2f * t),
                EaseProfile.Snap => t < 0.9f ? t * 0.1f : 1f, // Holds then snaps
                EaseProfile.Anticipate => t * t * (2.7f * t - 1.7f), // Pulls back before moving
                _ => t
            };
        }
    }
}