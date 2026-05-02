// File: Assets/Scripts/Workshop/Forge/Hermit/Core/HermitSubconscious.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_API.Scripts;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Core;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge;
using Workshop.UI_And_Tools.Forge.Hermit.Directives;

namespace Workshop.UI_And_Tools.Forge.Hermit.Core
{
    /// <summary>
    /// The "Subconscious" layer for Hermit. 
    /// Handles directive mapping and autonomous discovery.
    /// Runs in both Play Mode and Editor Chronos.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(HermitChassis))]
    public class HermitSubconscious : MonoBehaviour
    {
        private FSMHandle _status;
        private HermitContext _context;
        [SerializeField] private HermitChassis _chassis;
        private string _processingGroup = "Hermit_Subconscious";

        // --- Autonomous Systems ---
        private HermitDiscoveryModule _discoveryModule;
        private float _lastScanTime = 0f;
        private const float SCAN_INTERVAL = 5.0f;
        private Dictionary<string, IHermitDirective> _directiveRegistry = new Dictionary<string, IHermitDirective>();

        private void Awake()
        {
            // Register the "muscles" (Directives) using their Intent string
            RegisterDirective(new Directive_Trash());
            RegisterDirective(new Directive_Restore());
            RegisterDirective(new Directive_SpawnMini());
            RegisterDirective(new Directive_MoveTo());
            RegisterDirective(new Directive_SetPosture());
            RegisterDirective(new Directive_SpawnInWorldGui());
            RegisterDirective(new Directive_Spawn());
        }

        private void OnEnable()
        {
            if (_chassis == null) _chassis = GetComponent<HermitChassis>();

            // Ensure context is ready (Bridge to SingularityBootloader memory)
            _context = _chassis.Context;
            _discoveryModule = new HermitDiscoveryModule();

            // Inject senses into the context
            if (!_context.ActiveSenses.Any(s => s is VisualScannerSense))
            {
                _context.ActiveSenses.Add(new VisualScannerSense(30f));
            }


            FSM_API.Create.CreateProcessingGroup(_processingGroup);


            // 1. Define the FSM if it hasn't been defined globally yet
            if (!FSM_API.Interaction.Exists("HermitSubconsciousDef", _processingGroup))
            {
                FSM_API.Create.CreateFiniteStateMachine("HermitSubconsciousDef", -1, _processingGroup)
                    .State("Idle", OnUpdateIdle, null, null)
                    .State("ExecutingDirective", OnEnterExecution, OnUpdateExecution, null)
                    .State("AutonomousInvestigation", OnEnterInvestigate, OnUpdateInvestigate, null)

                    // Reactive Transitions
                    .Transition("Idle", "ExecutingDirective", ctx => _context.ActiveDirective != null)
                    .Transition("AutonomousInvestigation", "ExecutingDirective", ctx => _context.ActiveDirective != null)

                    // Discovery Logic (Cortex Driven)
                    .Transition("Idle", "AutonomousInvestigation", ctx =>
                        _chassis.Brain.Cortex.ContainsKey("Discovery") &&
                        _chassis.Brain.Cortex["Discovery"].Activation > 0.7f)

                    // Return loops
                    .Transition("ExecutingDirective", "Idle", ctx => _context.DirectiveComplete)
                    .Transition("AutonomousInvestigation", "Idle", ctx => _context.DirectiveComplete)
                    .BuildDefinition();
            }

            LocalHostClient.OnDirectiveReceived += HandleDirectiveFromBridge;

            // 2. Create the instance for this specific Chassis
            _status = FSM_API.Create.CreateInstance("HermitSubconsciousDef", _context, _processingGroup);
        }

        private void OnUpdateIdle(IStateContext ctx)
        {
            // Hovering/Breathing logic for the chassis
        }

        private void OnEnterExecution(IStateContext ctx)
        {
            _context.DirectiveComplete = false;
        }

        private void HandleDirectiveFromBridge(HermitDirectivePayload payload)
        {
            if (payload.AgentId != "Broadcast" && payload.AgentId != _chassis.AgentId) return;

            // Map intent to registry
            if (_directiveRegistry.TryGetValue(payload.Intent, out var directive))
            {
                _context.ActiveDirective = directive;
                _context.CurrentPayload = payload;

                Debug.Log($"[Subconscious] Ghost assigned directive: {payload.Intent}. Transitioning FSM.");
            }
        }

        private void OnUpdateExecution(IStateContext ctx)
        {
            if (_context.ActiveDirective == null)
            {
                _context.DirectiveComplete = true;
            }
            // Logic to step the IHermitDirective IEnumerator would be called here
        }

        private void OnEnterInvestigate(IStateContext ctx)
        {
            _context.DirectiveComplete = false;
            _lastScanTime = 0f;
            Debug.Log("[Subconscious] Discovery threshold reached. Engaging autonomous sweep.");
        }

        private void OnUpdateInvestigate(IStateContext ctx)
        {
            if (Time.realtimeSinceStartup - _lastScanTime < SCAN_INTERVAL) return;
            _lastScanTime = Time.realtimeSinceStartup;

            _context.SensedEnvironment.Clear();

            foreach (var sense in _context.ActiveSenses)
            {
                _context.SensedEnvironment.AddRange(sense.Observe(_context));
            }

            if (_context.SensedEnvironment.Count > 0)
            {
                var primeTarget = _discoveryModule.GetHighestPriorityTarget(_context.SensedEnvironment);

                string sensoryReport = $"I have autonomously scanned. The most neglected affordance is '{primeTarget.Name}' ({primeTarget.Description}). What should I do?";

                var payload = new HermitPromptPayload
                {
                    Intent = "AutonomousDiscovery",
                    Prompt = sensoryReport,
                    EnvironmentContext = _context.SensedEnvironment.Select(e => $"{e.Name}: {e.Description}").ToArray()
                };

                if (_chassis.Brain.Cortex.ContainsKey("Discovery"))
                {
                    var node = _chassis.Brain.Cortex["Discovery"];
                    node.ReceiveSignal(-node.Activation);
                }

                LocalHostClient.SendPrompt(JsonUtility.ToJson(payload));
                _context.DirectiveComplete = true;
            }
            else
            {
                if (_chassis.Brain.Cortex.ContainsKey("Discovery"))
                {
                    _chassis.Brain.Cortex["Discovery"].ReceiveSignal(-0.2f);
                }
                _context.DirectiveComplete = true;
            }
        }

        public void ExecuteDirective(IHermitDirective directive, HermitDirectivePayload payload)
        {
            _context.ActiveDirective = directive;
            _context.CurrentPayload = payload;
        }

        [System.Serializable]
        private class HermitPromptPayload
        {
            public string Intent;
            public string Prompt;
            public string[] EnvironmentContext;
        }

        private void RegisterDirective(IHermitDirective directive)
        {
            // IHermitDirective uses Intent string as the key
            if (!_directiveRegistry.ContainsKey(directive.Intent))
            {
                _directiveRegistry.Add(directive.Intent, directive);
            }
        }

        private void OnDisable()
        {
            LocalHostClient.OnDirectiveReceived -= HandleDirectiveFromBridge;
            if (_status != null)
            {
                FSM_API.Interaction.DestroyInstance(_status);
                _status = null;
            }
        }
    }
}