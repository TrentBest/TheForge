using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public static class LiveModelPreviewFSM
    {
        public const int PREVIEW_LAYER = 31;

        public static void OnPrimeTick(IStateContext ctx)
        {
            var p = (LiveModelPreviewContext)ctx;
            if (p.TargetModel == null) return;

            InitializePreviewScene(p);
            ForceRender(p);
        }

        public static void OnRenderTick(IStateContext ctx)
        {
            var p = (LiveModelPreviewContext)ctx;
            if (p.TargetModel == null || p.PreviewCamera == null) return;

            if (p.AutoOrbitEnabled && !p.IsDragging)
            {
                p.Yaw += Time.unscaledDeltaTime * p.OrbitSpeed;
            }

            UpdateCameraTransform(p);

            if (p.RenderTexture != null && p.RenderTexture.IsCreated())
            {
                p.PreviewCamera.Render();
            }
        }

        private static void InitializePreviewScene(LiveModelPreviewContext p)
        {
            if (p.PreviewCamera != null) return;

            var camObj = new GameObject($"Cam_{p.Name}") { hideFlags = HideFlags.HideAndDontSave };
            p.PreviewCamera = camObj.AddComponent<Camera>();

            if (p.UseOriginalModel)
            {
                p.PreviewCamera.cullingMask = 1 << p.TargetModel.layer;
            }
            else
            {
                p.PreviewCamera.cullingMask = 1 << PREVIEW_LAYER;
                SetLayerRecursively(p.TargetModel, PREVIEW_LAYER);
            }

            p.PreviewCamera.clearFlags = CameraClearFlags.SolidColor;
            p.PreviewCamera.backgroundColor = p.BackgroundColor;
            p.PreviewCamera.nearClipPlane = 0.01f;
            p.PreviewCamera.enabled = false;

            var lightObj = new GameObject($"Light_{p.Name}") { hideFlags = HideFlags.HideAndDontSave };
            lightObj.transform.SetParent(camObj.transform);
            p.PreviewLight = lightObj.AddComponent<Light>();
            p.PreviewLight.type = LightType.Directional;
            p.PreviewLight.intensity = p.LightIntensity;

            p.RenderTexture = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            p.RenderTexture.Create();
            p.PreviewCamera.targetTexture = p.RenderTexture;
        }

        private static void UpdateCameraTransform(LiveModelPreviewContext p)
        {
            Bounds b = CalculateBounds(p.TargetModel);
            float maxExtent = Mathf.Max(b.extents.x, Mathf.Max(b.extents.y, b.extents.z));
            float distance = (maxExtent * p.Zoom) + 0.5f;

            Quaternion rotation = Quaternion.Euler(p.Pitch, p.Yaw, 0f);
            p.PreviewCamera.transform.position = b.center - (rotation * Vector3.forward * distance);
            p.PreviewCamera.transform.LookAt(b.center);
        }

        private static void ForceRender(LiveModelPreviewContext p)
        {
            if (p.PreviewCamera != null && p.TargetModel != null)
            {
                UpdateCameraTransform(p);
                p.PreviewCamera.Render();
            }
        }

        private static void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;
            obj.layer = newLayer;
            foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, newLayer);
        }

        private static Bounds CalculateBounds(GameObject obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.one * 0.1f);
            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            return bounds;
        }
    }
}