using System;
using System.Collections.Generic;

using TheSingularityWorkshop.FSM_API;

using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace TheSingularityWorkshop.FSM_API.Scripts
{
    public class FSM_UnityIntegrationAdvanced : MonoBehaviour
    {
        [SerializeField]
        public GameObject root;

        // Private static field to hold the single instance
        private static FSM_UnityIntegrationAdvanced _instance;

        // Public static property to access the single instance.
        // It provides lazy initialization and handles finding/creating the instance.
        public static FSM_UnityIntegrationAdvanced Instance
        {
            get
            {
                // If no instance is set, try to find one in the scene first.
                // This handles cases where the component might be pre-placed in a scene.
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<FSM_UnityIntegrationAdvanced>();

                    // If still no instance (e.g., first access), create a new GameObject and add the component.
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("FSM_UnityIntegrationAdvanced"); // Name must match what tests expect
                        _instance = go.AddComponent<FSM_UnityIntegrationAdvanced>();
                        Debug.Log("FSM_UnityIntegrationAdvanced: New instance created via Instance getter.");
                    }
                }
                return _instance;
            }
        }

        // Processing group names (can be configured via Inspector if needed)
        // [SerializeField] makes them visible in the Inspector without being public.
        [SerializeField] private List<string> _updateProcessingGroup = new List<string>() { "Update" };
        [SerializeField] private List<string> _startProcessingGroup = new List<string>() { "Start" };
        [SerializeField] private List<string> _awakeProcessingGroup = new List<string>() { "Awake" };
        [SerializeField] private List<string> _fixedUpdateProcessingGroup = new List<string>() { "FixedUpdate" };
        [SerializeField] private List<string> _lateUpdateProcessingGroup = new List<string>() { "LateUpdate" };
        [SerializeField] private List<string> _onGUI_ProcessingGroup = new List<string>() { "OnGUI" };
        [SerializeField] private List<string> _onDrawGizmosProcessingGroup = new List<string>() { "OnDrawGizmos" };
        [SerializeField] private string _unityHandles = "UnityHandles";

        // Public setters to allow external (e.g., test) configuration of group names.
        public void SetUpdateProcessingGroup(string processingGroup = "Update") { _updateProcessingGroup[0] = processingGroup; }
        public void SetAwakeProcessingGroup(string processingGroup = "Awake") { _awakeProcessingGroup[0] =processingGroup; }
        public void SetFixedUpdateProcessingGroup(string processingGroup = "FixedUpdate") { _fixedUpdateProcessingGroup[0] = processingGroup; }
        public void SetLateUpdateProcessingGroup(string processingGroup = "LateUpdate") { _lateUpdateProcessingGroup[0] = processingGroup; }
        public void SetStartProcessingGroup(string processingGroup = "Start") { _startProcessingGroup[0] = processingGroup; }

        public void SetOnGUIProcessingGroup(string processingGroup = "OnGUI") { _onGUI_ProcessingGroup[0] = processingGroup; }
        public void SetOnDrawGizmosProcessingGroup(string processingGroup = "OnDrawGizmos") { _onDrawGizmosProcessingGroup[0] = processingGroup; }
       
        public void AddProcessingGroup(string unityMessage = "Update", string processingGroup = "Update")
        {
            switch (unityMessage)
            {
                case "Update":
                    if (!_updateProcessingGroup.Contains(processingGroup)) _updateProcessingGroup.Add(processingGroup);
                    break;
                case "Awake":
                    if (!_awakeProcessingGroup.Contains(processingGroup)) _awakeProcessingGroup.Add(processingGroup);
                    break;
                case "FixedUpdate":
                    if (!_fixedUpdateProcessingGroup.Contains(processingGroup)) _fixedUpdateProcessingGroup.Add(processingGroup);
                    break;
                case "LateUpdate":
                    if (!_lateUpdateProcessingGroup.Contains(processingGroup)) _lateUpdateProcessingGroup.Add(processingGroup);
                    break;
                case "Start":
                    if (!_startProcessingGroup.Contains(processingGroup)) _startProcessingGroup.Add(processingGroup);
                    break;
                case "OnGUI":
                    if (!_onGUI_ProcessingGroup.Contains(processingGroup)) _onGUI_ProcessingGroup.Add(processingGroup);
                    break;
                case "OnDrawGizmos":
                    if (!_onDrawGizmosProcessingGroup.Contains(processingGroup)) _onDrawGizmosProcessingGroup.Add(processingGroup);
                    break;
                default:
                    Debug.LogWarning($"FSM_UnityIntegrationAdvanced: Unknown Unity message '{unityMessage}' in AddProcessingGroup.");
                    break;
            }
        }
        public void SetUnityHandles(string processingGroup = "UnityHandles") { _unityHandles = processingGroup; }

        public void RemoveProcessingGroup(string unityMessage = "Update", string processingGroup = "Update")
        {
            switch (unityMessage)
            {
                case "Update":
                    _updateProcessingGroup.Remove(processingGroup);
                    break;
                case "Awake":
                    _awakeProcessingGroup.Remove(processingGroup);
                    break;
                case "FixedUpdate":
                    _fixedUpdateProcessingGroup.Remove(processingGroup);
                    break;
                case "LateUpdate":
                    _lateUpdateProcessingGroup.Remove(processingGroup);
                    break;
                case "Start":
                    _startProcessingGroup.Remove(processingGroup);
                    break;
                case "OnGUI":
                    _onGUI_ProcessingGroup.Remove(processingGroup);
                    break;
                case "OnDrawGizmos":
                    _onDrawGizmosProcessingGroup.Remove(processingGroup);
                    break;
                default:
                    Debug.LogWarning($"FSM_UnityIntegrationAdvanced: Unknown Unity message '{unityMessage}' in RemoveProcessingGroup.");
                    break;
            }
        }


        // Awake is called when the script instance is being loaded.
        // This is the correct place for all singleton setup logic.
        private void Awake()
        {
            // FIX: Check if 'root' is null before trying to use it.
            if (root != null)
            {
                root.SetActive(false);
            }

            FSM_API.Internal.ResetAPI();
            // If an instance already exists AND it's not THIS instance, destroy this new one.
            // This prevents duplicate singletons if one is created programmatically and another exists in a scene.
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("FSM_UnityIntegrationAdvanced: Duplicate instance found. Destroying new instance.", this);
                Destroy(gameObject); // Use Destroy for runtime, DestroyImmediate for editor tests if called directly.
                return;
            }

            // Set this as the singleton instance.
            _instance = this;
            // Ensure this GameObject persists across scene loads.
            DontDestroyOnLoad(gameObject);

            // Set the GameObject name for clarity and test assertions.
            gameObject.name = "FSM_UnityIntegrationAdvanced";

            // Create processing groups in FSM_API. These happen ONCE when the singleton is initialized.
            //Assuming FSM_API.Create.CreateProcessingGroup handles existing groups gracefully.
            foreach (var group in _updateProcessingGroup)
            {
                FSM_API.Create.CreateProcessingGroup(group);
            }
            foreach (var group in _awakeProcessingGroup)
            {
                FSM_API.Create.CreateProcessingGroup(group);
            }
            foreach (var group in _startProcessingGroup)
            {
                FSM_API.Create.CreateProcessingGroup(group);
            }
            foreach (var group in _fixedUpdateProcessingGroup)
            {
                FSM_API.Create.CreateProcessingGroup(group);
            }
            foreach (var group in _lateUpdateProcessingGroup)
            {
                FSM_API.Create.CreateProcessingGroup(group);
            }
            foreach (var group in _onGUI_ProcessingGroup)
            {
                FSM_API.Create.CreateProcessingGroup(group);
            }
            foreach (var group in _onDrawGizmosProcessingGroup)
            {
                FSM_API.Create.CreateProcessingGroup(group);
            }

            // Trigger the Awake processing group exactly once as part of the Awake lifecycle.
            foreach (var group in _awakeProcessingGroup)
            {
                FSM_API.Interaction.Update(group);
            }
            if (root != null)
            {
                root.SetActive(true);
            }
        }

        // Start is called once before the first execution of Update.
        void Start()
        {
            foreach (var group in _startProcessingGroup)
            {
                FSM_API.Interaction.Update(group);
            }
        }

        // Update is called once per frame.
        void Update()
        {
            foreach (var group in _updateProcessingGroup)
            {
                FSM_API.Interaction.Update(group);
            }
            FSM_API.Interaction.Update(_unityHandles);
        }

        private void FixedUpdate()
        {
            foreach (var group in _fixedUpdateProcessingGroup)
            {
                FSM_API.Interaction.Update(group);
            }
        }

        private void LateUpdate()
        {
            foreach (var group in _lateUpdateProcessingGroup)
            {
                FSM_API.Interaction.Update(group);
            }
        }

        // This method is crucial for testing to ensure a clean state between tests.
        public static void ResetInstance()
        {
            // First, destroy the GameObject associated with the instance if it exists.
            if (_instance != null)
            {
                // Use DestroyImmediate in editor (for tests), Destroy in play mode.
                // This physically removes the GameObject from the scene.
                if (Application.isEditor && !Application.isPlaying)
                {
                    //Debug.Log($"ResetInstance: Destroying GameObject '{_instance.gameObject.name}' immediately.");
                    Object.DestroyImmediate(_instance.gameObject);
                }
                else if (Application.isPlaying)
                {
                    //Debug.Log($"ResetInstance: Destroying GameObject '{_instance.gameObject.name}'.");
                    Object.Destroy(_instance.gameObject);
                }
            }

            // Then, clear the static reference to allow a new instance to be created.
            _instance = null;
            TheSingularityWorkshop.FSM_API.FSM_API.Internal.ResetAPI(true);
            _instance = new GameObject("FSM_UnityIntegrationAdvanced").AddComponent<FSM_UnityIntegrationAdvanced>();
        }

        // OnDestroy is called when the MonoBehaviour will be destroyed.
        private void OnDestroy()
        {
            //Debug.Log($"FSM_UnityIntegrationAdvanced OnDestroy() called for {gameObject.name}.");
            // If the destroyed instance *was* the singleton, clear the static reference.
            // This handles cases where Unity might destroy it for other reasons.
            if (_instance == this)
            {
                FSM_API.Internal.ResetAPI();
                _instance = null;
            }
        }

        private void OnGUI()
        {
           foreach (var group in _onGUI_ProcessingGroup)
            {
                FSM_API.Interaction.Update(group);
            }
        }


        private void OnDrawGizmos()
        {
           foreach (var group in _onDrawGizmosProcessingGroup)
            {
                FSM_API.Interaction.Update(group);
            }
        }
    }
}