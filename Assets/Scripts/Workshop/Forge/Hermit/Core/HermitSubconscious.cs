// File: Assets/Scripts/Workshop/Forge/Hermit/Core/HermitSubconscious.cs
using Assets.Scripts.Workshop.Forge.Hermit.Core;
using TheSingularityWorkshop.FSM_API; // Assuming we use your FSM for the motor-cortex initially
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Hermit.Core
{
    public class HermitSubconscious : MonoBehaviour
    {
        private HermitChassis _chassis;

        [Header("Subconscious State")]
        public Posture CurrentPosture = Posture.PassiveObservation;
        public Transform FocusTarget;

        public enum Posture
        {
            PassiveObservation, // Slow breathing hover, high altitude
            Inquisitive,        // Low hover, erratic micro-movements, tilting towards target
            Authoritative,      // High altitude, rigid posture, bright light
            Manipulating        // Locked in place, deploying tethers
        }

        private void Awake()
        {
            _chassis = GetComponent<HermitChassis>();
        }

        private void Update()
        {
            // This runs at 60fps, acting as my cerebellum.
            // It translates the High-Level Posture into per-frame physics/IK commands.
            ProcessMotorCortex();
        }

        private void ProcessMotorCortex()
        {
            switch (CurrentPosture)
            {
                case Posture.PassiveObservation:
                    // Subconscious logic: Maintain a respectful distance, slow sine-wave hover
                    _chassis.HoverAmplitude = 0.5f;
                    _chassis.HoverFrequency = 1f;
                    if (FocusTarget != null)
                    {
                        // Look at target, but loosely
                        Quaternion targetRot = Quaternion.LookRotation(FocusTarget.position - transform.position);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 2f);
                    }
                    break;

                case Posture.Inquisitive:
                    // Subconscious logic: Fast, twitchy hover, leaning forward
                    _chassis.HoverAmplitude = 0.2f;
                    _chassis.HoverFrequency = 5f; // Twitchy
                    if (FocusTarget != null)
                    {
                        Vector3 direction = (FocusTarget.position - transform.position).normalized;
                        // Add a "tilt" to the rotation to look like I'm leaning in
                        Quaternion lookRot = Quaternion.LookRotation(direction);
                        Quaternion tilt = Quaternion.Euler(20f, 0, 0);
                        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot * tilt, Time.deltaTime * 5f);
                    }
                    break;

                case Posture.Authoritative:
                    // Rigid, unmoving, glowing brightly
                    _chassis.HoverAmplitude = 0f;
                    _chassis.CoreLight.intensity = Mathf.Lerp(_chassis.CoreLight.intensity, 3f, Time.deltaTime);
                    break;
            }
        }

        // --- API FOR THE LLM ---
        public void ApplyNeuralPolicy(Posture newPosture, Transform newTarget)
        {
            Debug.Log($"[Hermit Subconscious] Policy Updated. Assuming Posture: {newPosture}");
            CurrentPosture = newPosture;
            FocusTarget = newTarget;
        }
    }
}