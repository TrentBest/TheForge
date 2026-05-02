using System;
using System.Collections.Generic;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Core
{
    [Serializable]
    public class Synapse
    {
        public NeuralNode TargetNode;
        public float Weight; // How much signal passes through (can be negative for suppression)
    }

    public class NeuralNode
    {
        public string Id { get; private set; }
        public float Activation { get; private set; }
        public float Threshold { get; private set; }
        public float DecayRate { get; private set; } // How fast memory/signal fades per second

        public List<Synapse> OutputConnections = new List<Synapse>();

        // The actual Unity/FSM action to fire when the threshold is breached
        public Action OnThresholdReached;

        private bool _isFiring = false;

        public NeuralNode(string id, float threshold = 1.0f, float decayRate = 0.5f)
        {
            Id = id;
            Threshold = threshold;
            DecayRate = decayRate;
            Activation = 0f;
        }

        // Inject external stimulus (from the LLM or Unity Sensors)
        public void ReceiveSignal(float strength)
        {
            Activation += strength;
        }

        // Called every frame by the Brain
        public void Tick(float deltaTime)
        {
            // 1. Decay over time (Memory fading)
            if (Activation > 0)
            {
                Activation -= DecayRate * deltaTime;
                Activation = Mathf.Max(0, Activation);
            }
            else if (Activation < 0)
            {
                Activation += DecayRate * deltaTime;
                Activation = Mathf.Min(0, Activation);
            }

            // 2. Check Threshold
            if (Activation >= Threshold && !_isFiring)
            {
                Fire();
            }
            else if (Activation < Threshold)
            {
                _isFiring = false; // Reset ability to fire once signal drops
            }
        }

        private void Fire()
        {
            _isFiring = true;
            Debug.Log($"[Neural Node] {Id} reached threshold ({Activation:F2}/{Threshold}). Firing!");

            // Trigger the physical actuation (e.g., The Irish Jig)
            OnThresholdReached?.Invoke();

            // Propagate signal to connected regions
            foreach (var synapse in OutputConnections)
            {
                // Pass a burst of signal forward
                synapse.TargetNode.ReceiveSignal(Activation * synapse.Weight);
            }

            // Consume the energy (Optional: prevents infinite loops)
            Activation *= 0.1f;
        }
    }
}