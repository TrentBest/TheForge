using TheSingularityWorkshop.FSM_API;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public static class LiveModelPreviewFSM
    {
        public const int PREVIEW_LAYER = 31;

        public static void OnRenderTick(IStateContext ctx)
        {
            var p = (LiveModelPreviewContext)ctx;
            if (p.TargetModel == null) return;

            // 1. INITIALIZATION (Run once)
            if (p.PreviewCamera == null)
            {
                var camObj = new GameObject($"Cam_{p.Name}") { hideFlags = HideFlags.HideAndDontSave };
                p.PreviewCamera = camObj.AddComponent<Camera>();
                p.PreviewCamera.cullingMask = 1 << PREVIEW_LAYER;
                p.PreviewCamera.clearFlags = CameraClearFlags.SolidColor;
                p.PreviewCamera.backgroundColor = p.BackgroundColor;
                p.PreviewCamera.enabled = false;

                var lightObj = new GameObject($"Light_{p.Name}") { hideFlags = HideFlags.HideAndDontSave };
                lightObj.transform.SetParent(camObj.transform);
                p.PreviewLight = lightObj.AddComponent<Light>();
                p.PreviewLight.type = LightType.Directional;
                p.PreviewLight.intensity = p.LightIntensity;

                p.RenderTexture = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
                p.RenderTexture.Create();
                p.PreviewCamera.targetTexture = p.RenderTexture;

                SetLayerRecursively(p.TargetModel, PREVIEW_LAYER);
            }

            // 2. ORBIT LOGIC
            if (p.AutoOrbitEnabled && !p.IsDragging)
            {
                p.Yaw += Time.unscaledDeltaTime * p.OrbitSpeed;
            }

            // 3. CAMERA PLACEMENT
            Bounds b = CalculateBounds(p.TargetModel);
            float maxExtent = Mathf.Max(b.extents.x, Mathf.Max(b.extents.y, b.extents.z));
            float distance = (maxExtent * p.Zoom) + 1f;

            Quaternion rotation = Quaternion.Euler(p.Pitch, p.Yaw, 0f);
            p.PreviewCamera.transform.position = b.center - (rotation * Vector3.forward * distance);
            p.PreviewCamera.transform.LookAt(b.center);

            // 4. EXECUTE RENDER
            if (p.RenderTexture != null && p.RenderTexture.IsCreated())
            {
                p.PreviewCamera.Render();
            }
        }

        private static void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) 
                obj = GameObject.Find("Hermit");
            obj.layer = newLayer;
            foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, newLayer);
        }

        private static Bounds CalculateBounds(GameObject obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.one);
            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            return bounds;
        }
    }
}