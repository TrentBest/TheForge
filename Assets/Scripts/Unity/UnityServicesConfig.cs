using UnityEngine;

namespace TheSingularityWorkshop.UnityServices
{
    /// <summary>
    /// The persistent data asset built by the Forge and consumed by the Runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "UnityServicesConfig", menuName = "The Singularity/Forge/Unity Services Config")]
    public class UnityServicesConfig : ScriptableObject
    {
        [Header("Initialization")]
        public bool AutoInitializeOnAwake = true;
        public int MaxInitRetries = 3;
        public float RetryBackoffSeconds = 2f;

        [Header("Service Modules")]
        public bool RequireAuthentication = true;
        public bool EnableRemoteConfig = true;
        public bool EnableCloudSave = false;
        public bool EnableLobbyServices = false;

        [Header("Authentication Settings")]
        [Tooltip("Isolate editor tests from your live production database.")]
        public string EditorDevProfile = "Forge_Editor_Dev";

        [Header("Remote Config Settings")]
        public bool AutoRefreshRemoteConfig = true;
        public float RemoteConfigRefreshIntervalSeconds = 60f;

        [Header("Proxy & Liaison")]
        public bool UseLocalDaemonProxy = false;
        public string LocalDaemonUrl = "http://localhost:5000/api/";
    }
}