using Assets.Scripts.Workshop.UI_And_Tools.Forge.Hermit.Actions;
using System.Collections.Generic;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge;
using Workshop.UI_And_Tools.Forge.Hermit.Directives;
using Workshop.UI_And_Tools.Forge.IO;

namespace Hermit.Core
{
    public class HermitBrain : MonoBehaviour
    {
        [SerializeField] private Transform _spawnOrigin; // Where Hermit stands

        public string Name { get; internal set; }
        public Dictionary<string, IHermitDirective> Cortex { get; internal set; } = new Dictionary<string, IHermitDirective>();

        private void Start()
        {
            // 1. Hook the physical body into the nervous system using exact types
            if (SingularityDataBus.Instance != null)
            {
                SingularityDataBus.Instance.Subscribe<string>("Hermit_Output", OnCortexIntentReceived);
            }
        }

        private void OnDestroy()
        {
            if (SingularityDataBus.Instance != null)
            {
                SingularityDataBus.Instance.Unsubscribe<string>("Hermit_Output", OnCortexIntentReceived);
            }
        }

        // 2. Parse the LLM's command directly as a string (Zero Boxing/Casting)
        private void OnCortexIntentReceived(string jsonPayload)
        {
            var directive = JsonUtility.FromJson<HermitDirectivePayload>(jsonPayload);

            if (directive != null)
            {
                ExecuteDirective(directive);
            }
        }

        private void ExecuteDirective(HermitDirectivePayload directive)
        {
            // 3. Trigger the Theater!
            switch (directive.Intent)
            {
                case "Manifest":
                    DispatchSwarm(directive.TargetId, "Build");
                    break;
                case "Trash":
                    DispatchSwarm(directive.TargetId, "Destroy");
                    break;
            }
        }

        private void DispatchSwarm(string targetId, string taskName)
        {
            // Hermit dispatches multiple Mini Hermits to do the actual work
            int swarmSize = 5;
            for (int i = 0; i < swarmSize; i++)
            {
                var miniDirective = new Directive_SpawnMini();

                HermitAction.Execute(miniDirective);
            }
        }
    }
}