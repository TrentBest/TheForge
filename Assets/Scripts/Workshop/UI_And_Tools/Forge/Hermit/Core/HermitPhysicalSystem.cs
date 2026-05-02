using UnityEngine;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Forge.Hermit.Core
{
    public static class HermitPhysicalSystem
    {
        public static void ManifestChassis(ref HermitChassisData data, bool isMinion = false)
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            root.name = isMinion ? $"MiniHermit_{data.AgentId}" : $"HermitBoss_{data.AgentId}";
            root.transform.position = data.Position;
            root.transform.localScale = isMinion ? Vector3.one * 0.125f : Vector3.one;

            Color themeColor = isMinion ? new Color(1f, 0.5f, 0f) : Color.cyan;

            MeshRenderer shell = root.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = themeColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", themeColor * 1.5f);
            shell.sharedMaterial = mat;

            GameObject lightObj = new GameObject("CoreLight");
            lightObj.transform.SetParent(root.transform);
            lightObj.transform.localPosition = Vector3.zero;
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = themeColor;
            light.intensity = 2f;
            light.range = 5f;

            data.RootObjectId = root.GetInstanceID();
            data.CoreLightId = light.GetInstanceID();
            data.ShellRendererId = shell.GetInstanceID();

            if (root.TryGetComponent<Rigidbody>(out var rb)) Object.Destroy(rb);
            Object.DontDestroyOnLoad(root);
        }

        public static void Tick(ref HermitChassisData data, float deltaTime)
        {
            if (data.CurrentState == HermitAIState.Executing)
            {
                data.Position = Vector3.Lerp(data.Position, data.TargetPosition, deltaTime * 5f);

                // Temporal recording for "Back up... right there"
                data.RecordSnapshot();

                // Safe manifestation update - only if in the Editor or Runtime
#if UNITY_EDITOR
                var root = UnityEditor.EditorUtility.InstanceIDToObject(data.RootObjectId) as GameObject;
#else
                // In standalone, we'd need a simple Dictionary<int, GameObject> registry
                var root = GameObject.Find(data.RootObjectId.ToString()); 
#endif

                if (root != null) root.transform.position = data.Position;

                if (Vector3.Distance(data.Position, data.TargetPosition) < 0.01f)
                    data.CurrentState = HermitAIState.Idle;
            }
        }
    }
}