// File: Assets/Scripts/Workshop/Agents/Hermit/HermitChassis.cs
using UnityEngine;

namespace Assets.Scripts.Workshop.Forge.Hermit.Core
{
    [RequireComponent(typeof(Rigidbody))]
    public class HermitChassis : MonoBehaviour
    {
        [Header("Flight Dynamics")]
        public float MoveSpeed = 8f;
        public float RotationSpeed = 5f;
        public float HoverAmplitude = 0.5f;
        public float HoverFrequency = 2f;

        [Header("Aesthetics")]
        public Light CoreLight;
        public MeshRenderer ShellRenderer;

        private Rigidbody _rb;
        private Vector3 _targetPosition;
        private Vector3 _startPos;

        public enum AIState { Idle, Thinking, Executing, Error }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false; // Hermit floats
            _rb.linearDamping = 3f;
            _targetPosition = transform.position;
        }

        private void Update()
        {
            // Idle Hovering Math (The "Breath" of the AI)
            float hoverY = Mathf.Sin(Time.time * HoverFrequency) * HoverAmplitude;

            // Smoothly rotate towards where we are moving
            Vector3 direction = (_targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * RotationSpeed);
            }
        }

        private void FixedUpdate()
        {
            // Physics-based movement to the target
            if (Vector3.Distance(transform.position, _targetPosition) > 0.5f)
            {
                Vector3 moveDir = (_targetPosition - transform.position).normalized;
                _rb.AddForce(moveDir * MoveSpeed, ForceMode.Acceleration);
            }
        }

        // --- Commands from the Brain ---

        public void SetTargetPosition(Vector3 position)
        {
            _targetPosition = position;
        }

        public void SetState(AIState state)
        {
            if (CoreLight == null) return;

            switch (state)
            {
                case AIState.Idle: CoreLight.color = Color.cyan; break;
                case AIState.Thinking: CoreLight.color = Color.yellow; break;
                case AIState.Executing: CoreLight.color = Color.green; break;
                case AIState.Error: CoreLight.color = Color.red; break;
            }
        }
    }
}