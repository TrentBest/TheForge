using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public enum ViewGizmoStyle { AutodeskCube, BlenderAxis }

    public class RuntimeViewGizmo : IDisposable
    {
        private RenderTexture _gizmoRT;
        private Camera _gizmoCamera;
        private GameObject _gizmoRoot;
        public Image GizmoUIElement { get; private set; }

        private const int GIZMO_LAYER = 30;
        private List<Material> _trackedMaterials = new List<Material>();

        public RuntimeViewGizmo(ViewGizmoStyle style = ViewGizmoStyle.BlenderAxis, Material overrideGizmoMaterial = null)
        {
            _gizmoRT = new RenderTexture(128, 128, 16, RenderTextureFormat.ARGB32);

            GizmoUIElement = new Image
            {
                image = _gizmoRT,
                style = { width = 100, height = 100, position = Position.Absolute, top = 10, right = 10 }
            };

            SetupGizmoScene(style, overrideGizmoMaterial);
        }

        private void SetupGizmoScene(ViewGizmoStyle style, Material overrideMat)
        {
            GameObject camObj = new GameObject("RuntimeGizmoCamera");
            camObj.hideFlags = HideFlags.HideAndDontSave;
            _gizmoCamera = camObj.AddComponent<Camera>();
            _gizmoCamera.targetTexture = _gizmoRT;
            _gizmoCamera.clearFlags = CameraClearFlags.SolidColor;
            _gizmoCamera.backgroundColor = new Color(0, 0, 0, 0);
            _gizmoCamera.cullingMask = 1 << GIZMO_LAYER;
            _gizmoCamera.orthographic = true;
            _gizmoCamera.orthographicSize = 1.5f;

            // Fix for the invisible clipping plane if the camera gets too close
            _gizmoCamera.nearClipPlane = 0.01f;

            _gizmoRoot = new GameObject("GizmoRoot");
            _gizmoRoot.hideFlags = HideFlags.HideAndDontSave;

            if (style == ViewGizmoStyle.AutodeskCube) GenerateAutodeskCube(overrideMat);
            else GenerateBlenderAxis();

            SetLayerRecursively(_gizmoRoot.transform, GIZMO_LAYER);

            GameObject lightObj = new GameObject("GizmoLight");
            lightObj.transform.SetParent(_gizmoRoot.transform);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Directional;
            l.cullingMask = 1 << GIZMO_LAYER;
            l.intensity = 1.5f;
        }

        // --- THE BULLETPROOF FIX ---
        private Shader GetPipelineAgnosticShader()
        {
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null || UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null)
            {
                Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLit != null) return urpLit;

                Shader hdrpLit = Shader.Find("HDRP/Lit");
                if (hdrpLit != null) return hdrpLit;
            }
            return Shader.Find("Standard");
        }

        private void ApplyColorSafely(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer == null) return;

            Shader targetShader = GetPipelineAgnosticShader();
            if (targetShader == null) return;

            Material mat = new Material(targetShader);

            // Map the colors depending on what the shader actually supports
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color); // URP/HDRP
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);         // Standard

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.8f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.8f);

            renderer.sharedMaterial = mat;
            _trackedMaterials.Add(mat);
        }

        private void GenerateBlenderAxis()
        {
            CreateAxisArrow("X", Color.red, Vector3.right);
            CreateAxisArrow("Y", Color.green, Vector3.up);
            CreateAxisArrow("Z", new Color(0.2f, 0.4f, 1f), Vector3.forward);

            GameObject center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            center.transform.SetParent(_gizmoRoot.transform);
            center.transform.localScale = Vector3.one * 0.3f;
            ApplyColorSafely(center, Color.white);
        }

        private void CreateAxisArrow(string name, Color color, Vector3 direction)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.transform.SetParent(_gizmoRoot.transform);
            cylinder.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
            cylinder.transform.position = direction * 0.5f;
            cylinder.transform.up = direction;
            ApplyColorSafely(cylinder, color);

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(_gizmoRoot.transform);
            sphere.transform.localScale = Vector3.one * 0.35f;
            sphere.transform.position = direction * 1.0f;
            ApplyColorSafely(sphere, color);
        }

        private void GenerateAutodeskCube(Material overrideMat)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(_gizmoRoot.transform);
            cube.transform.localScale = Vector3.one * 1.5f;

            if (overrideMat != null)
            {
                cube.GetComponent<MeshRenderer>().sharedMaterial = overrideMat;
            }
            else
            {
                ApplyColorSafely(cube, new Color(1, 1, 1, 1));
            }
        }

        public void SyncRotation(Quaternion mainCameraRotation)
        {
            _gizmoCamera.transform.position = mainCameraRotation * new Vector3(0, 0, -3f);
            _gizmoCamera.transform.LookAt(Vector3.zero);
            _gizmoCamera.Render();
        }

        private void SetLayerRecursively(Transform trans, int layer)
        {
            trans.gameObject.layer = layer;
            foreach (Transform child in trans) SetLayerRecursively(child, layer);
        }

        public void Dispose()
        {
            if (_gizmoCamera) UnityEngine.Object.DestroyImmediate(_gizmoCamera.gameObject);
            if (_gizmoRoot) UnityEngine.Object.DestroyImmediate(_gizmoRoot);
            if (_gizmoRT) { _gizmoRT.Release(); UnityEngine.Object.DestroyImmediate(_gizmoRT); }

            foreach (var mat in _trackedMaterials)
            {
                if (mat != null) UnityEngine.Object.DestroyImmediate(mat);
            }
            _trackedMaterials.Clear();
        }
    }
}