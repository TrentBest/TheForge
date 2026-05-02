using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Core
{
    [CreateAssetMenu(fileName = "HermitSettings", menuName = "TheSingularityWorkshop/Hermit Settings")]
    public class HermitSettings : ScriptableObject
    {
        public string AgentExecutablePath = "";

        public static HermitSettings LoadOrCreate()
        {
            // Try to load from Resources
            var settings = Resources.Load<HermitSettings>("HermitSettings");

            if (settings == null)
            {
#if UNITY_EDITOR
                settings = ScriptableObject.CreateInstance<HermitSettings>();

                // Ensure directory exists
                if (!System.IO.Directory.Exists(Application.dataPath + "/Resources"))
                    System.IO.Directory.CreateDirectory(Application.dataPath + "/Resources");

                UnityEditor.AssetDatabase.CreateAsset(settings, "Assets/Resources/HermitSettings.asset");
                UnityEditor.AssetDatabase.SaveAssets();
#endif
            }
            return settings;
        }
    }
}