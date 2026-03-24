// File: Assets/Scripts/Workshop/Forge/Hermit/Core/HermitBrain.cs
using Assets.Scripts.Workshop.Forge.Hermit.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheSingularityWorkshop.Forge.Hermit.Directives;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Hermit.Core
{
    [RequireComponent(typeof(HermitChassis))]
    public class HermitBrain : MonoBehaviour
    {
        private HermitChassis _chassis;
        private Queue<HermitAction> _actionQueue = new Queue<HermitAction>();
        private bool _isExecuting = false;

        // --- The Neural Registry ---
        // Maps the JSON string "type" to the actual behavior script
        private Dictionary<string, IHermitDirective> _knownDirectives = new Dictionary<string, IHermitDirective>();

        private void Awake()
        {
            _chassis = GetComponent<HermitChassis>();
            _chassis.SetState(HermitChassis.AIState.Idle);

            InitializeNeuralPathways();
        }

        private void InitializeNeuralPathways()
        {
            Debug.Log("[Hermit Brain] Scanning domain for Directives...");
            _knownDirectives.Clear();

            // Find all classes that implement IHermitDirective (that aren't interfaces/abstract)
            var directiveTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(IHermitDirective).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in directiveTypes)
            {
                var instance = (IHermitDirective)Activator.CreateInstance(type);
                _knownDirectives[instance.ActionType.ToLower()] = instance;
                Debug.Log($"[Hermit Brain] Learned new skill: {instance.ActionType}");
            }
        }

        public void ReceiveDirectives(List<HermitAction> actions)
        {
            foreach (var action in actions)
            {
                _actionQueue.Enqueue(action);
            }

            if (!_isExecuting) StartCoroutine(ProcessActionQueue());
        }

        private IEnumerator ProcessActionQueue()
        {
            _isExecuting = true;
            _chassis.SetState(HermitChassis.AIState.Executing);

            while (_actionQueue.Count > 0)
            {
                var currentAction = _actionQueue.Dequeue();
                string actionKey = currentAction.Type.ToLower();

                // Look up the skill in our registry
                if (_knownDirectives.TryGetValue(actionKey, out IHermitDirective directive))
                {
                    Debug.Log($"[Hermit Brain] Executing Intent: {currentAction.Type}");
                    yield return StartCoroutine(directive.Execute(currentAction, _chassis));
                }
                else
                {
                    Debug.LogWarning($"[Hermit Brain] Hallucination or Unknown Directive: {currentAction.Type}. I do not know how to do this.");
                    _chassis.SetState(HermitChassis.AIState.Error);
                    yield return new WaitForSeconds(1.5f); // Flash error state
                    _chassis.SetState(HermitChassis.AIState.Executing); // Resume normal execution color
                }
            }

            _chassis.SetState(HermitChassis.AIState.Idle);
            _isExecuting = false;
        }
    }

    // --- Data Transfer Objects (Mapped from JSON) ---
    [Serializable]
    public class HermitAction
    {
        public string Type; // The key that matches ActionType (e.g., "MoveTo")
        public Vector3 Target;
        public string AssetId;
        public string Value;
    }
}