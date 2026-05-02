using UnityEngine;

namespace Assets.Scripts.Unity
{
    [CreateAssetMenu(fileName = "UnityAuthConfig", menuName = "The Singularity/Forge/Modules/Unity Auth Config")]
    public class UnityAuthConfig : ScriptableObject
    {
        [Header("Project Identity")]
        public string CompanyName = "The Singularity Workshop";
        public string ProductName = "New Experience";
        [Tooltip("The Unity Cloud Project ID (GUID) found in your Unity Dashboard.")]
        public string CloudProjectID = "";

        [Header("Cloud Environment")]
        [Tooltip("Target UGS environment (e.g., 'production', 'development').")]
        public string EnvironmentName = "production";
        [Tooltip("The local profile name used when running in the Unity Editor to prevent database corruption.")]
        public string EditorSandboxProfile = "Forge_Editor_Dev";

        [Header("Authentication Methods")]
        public bool EnableAnonymousLogin = true;
        public bool EnableSteamAuth = false;
        public bool EnableGoogleAuth = false;
        public bool EnableAppleAuth = false;
        public bool EnableDiscordAuth = false;

        [Header("Provider Credentials")]
        [Tooltip("Required if using Google or Apple Sign-In")]
        public string WebClientID = "";
    }
}