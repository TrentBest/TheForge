using System;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    [RequireComponent(typeof(Collider))]
    public class DiegeticPortalTrigger : MonoBehaviour
    {
        public Action OnPlayerEnter;
        public Action OnPlayerExit;

        // Optional filter to ensure random physics objects don't trigger the UI transition
        [SerializeField] private string targetTag = "Player";

        private void Awake()
        {
            // Ensure the collider is actually a trigger
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            // If you have a specific Player setup, verify it here.
            // Checking for the tag or an attached Camera is usually safest.
            if (string.IsNullOrEmpty(targetTag) || other.CompareTag(targetTag) || other.GetComponentInChildren<Camera>() != null)
            {
                OnPlayerEnter?.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (string.IsNullOrEmpty(targetTag) || other.CompareTag(targetTag) || other.GetComponentInChildren<Camera>() != null)
            {
                OnPlayerExit?.Invoke();
            }
        }
    }
}