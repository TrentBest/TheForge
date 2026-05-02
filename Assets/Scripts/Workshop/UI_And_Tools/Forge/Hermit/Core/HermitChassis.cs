using Hermit.Core;
using System;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_API.Scripts;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Core;

namespace Workshop.UI_And_Tools.Forge.Hermit.Core
{


    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HermitBrain))] // Ensure the body always has a brain attached
    public class HermitChassis : MonoBehaviour
    {
        public enum AIState { Idle, Executing }
        public AIState CurrentState { get; private set; }
        public string AgentId { get; private set; }

        [Header("Aesthetics")]
        public Light CoreLight;
        public MeshRenderer ShellRenderer;

        public HermitContext Context { get; private set; }
        public HermitBrain Brain { get; private set; }

        private void Awake()
        {
            AgentId = Guid.NewGuid().ToString();
            Context = new HermitContext(AgentId, transform, GetComponent<Rigidbody>(), CoreLight);

            // Re-establish the neural link
            Brain = GetComponent<HermitBrain>();
        }

        private void Start()
        {
            // Register with the FSM Heartbeat
            FSM_UnityIntegrationAdvanced.Instance.AddProcessingGroup("Update", "Hermit_Subconscious");
            FSM_UnityIntegrationAdvanced.Instance.AddProcessingGroup("FixedUpdate", "Hermit_Physics");
        }

        public void SetState(AIState state) => CurrentState = state;

        /// <summary>
        /// Updates the movement target in the context. 
        /// This is the "lever" the Directive pulls to move the physical body.
        /// </summary>
        public void SetTargetPosition(Vector3 targetPos)
        {
            if (Context != null)
            {
                Context.TargetPosition = targetPos;
                SetState(AIState.Executing);
            }
        }

        /// <summary>
        /// Fabricates a scaled-down minion unit and returns its Chassis for commanding.
        /// </summary>
        internal HermitChassis SpawnMinion()
        {
            Debug.Log($"[Hermit Boss: {AgentId}] Ejecting Minion Scribe Drone.");

            // 1. Create the physical body
            GameObject minionObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            minionObj.name = $"MiniHermit_{Guid.NewGuid().ToString().Substring(0, 4)}";

            // Eject in front of the Boss and scale down
            minionObj.transform.position = transform.position + (transform.forward * 1.5f);
            minionObj.transform.localScale = transform.localScale * 0.125f;

            // 2. Setup Worker Aesthetics (Safety Amber)
            Color workerAmber = new Color(1f, 0.5f, 0f);
            MeshRenderer shell = minionObj.GetComponent<MeshRenderer>();
            if (shell != null)
            {
                Material workerMat = new Material(Shader.Find("Standard"));
                workerMat.color = workerAmber;
                workerMat.EnableKeyword("_EMISSION");
                workerMat.SetColor("_EmissionColor", workerAmber * 1.5f);
                shell.sharedMaterial = workerMat;
            }

            // Convert collider to trigger so it doesn't bump the Boss around upon spawning
            Collider col = minionObj.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            // 3. Setup the Core Light for aesthetic pulsing
            Light light = minionObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = workerAmber;
            light.intensity = 2f;
            light.range = 5f;

            // 4. Attach the Chassis (Rigidbody & Brain are auto-added via RequireComponent)
            HermitChassis minionChassis = minionObj.AddComponent<HermitChassis>();
            minionChassis.CoreLight = light;
            minionChassis.ShellRenderer = shell;

            // 5. Configure Physics for a floating drone
            Rigidbody rb = minionObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true; // Minions are purely script-driven
            }

            return minionChassis;
        }
    }
}