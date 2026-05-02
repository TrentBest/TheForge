using UnityEngine;
using UnityEngine.Rendering;

namespace Workshop.UI_And_Tools.Showcase
{
    [RequireComponent(typeof(MeshRenderer))]
    public class CosmicSphereRenderer : MonoBehaviour
    {
        [Header("GPU Compute")]
        public ComputeShader CosmicBaker;
        public int TextureResolution = 2048;

        [Header("Materials (Pre-configured Cull Modes)")]
        public Material OutsideMaterialTemplate; // Cull Back
        public Material InsideMaterialTemplate;  // Cull Front

        public float SphereRadius { get; private set; } = 1000f;
        public Vector3 BrightestPoint { get; private set; }
        public bool IsGenerationComplete { get; private set; }

        private RenderTexture _cosmicTexture;
        private Material _outsideMaterialInst;
        private Material _insideMaterialInst;
        private MeshRenderer _meshRenderer;
        private Camera _targetCamera;
        private bool _isInsideEventHorizon = false;

        public void InitializeAndBake(int scaleLevel, float radius, Camera cam)
        {
            _targetCamera = cam;
            SphereRadius = radius;
            transform.localScale = Vector3.one * SphereRadius;
            _meshRenderer = GetComponent<MeshRenderer>();

            // 1. Setup the GPU Texture
            _cosmicTexture = new RenderTexture(TextureResolution, TextureResolution, 0, RenderTextureFormat.ARGBHalf)
            {
                enableRandomWrite = true
            };
            _cosmicTexture.Create();

            // 2. Clone the material templates and assign the freshly baked universe texture
            _outsideMaterialInst = new Material(OutsideMaterialTemplate) { mainTexture = _cosmicTexture };
            _insideMaterialInst = new Material(InsideMaterialTemplate) { mainTexture = _cosmicTexture };

            // Start on the outside looking at the event horizon
            _meshRenderer.material = _outsideMaterialInst;

            // 3. Dispatch Compute
            int kernel = CosmicBaker.FindKernel("BakeCosmicSphere");
            CosmicBaker.SetTexture(kernel, "OutputTexture", _cosmicTexture);
            CosmicBaker.SetInt("_ScaleLevel", scaleLevel);
            CosmicBaker.SetInt("_Seed", 1984);
            CosmicBaker.SetInt("_TextureResolution", TextureResolution);

            // Buffer for brightest point (float3 direction + uint intensity)
            var resultBuffer = new ComputeBuffer(1, 16);
            resultBuffer.SetData(new[] { new { Direction = Vector3.forward, MaxLightIntensity = 0u } });
            CosmicBaker.SetBuffer(kernel, "ResultBuffer", resultBuffer);

            // Dispatch 2D thread groups
            int threadGroups = Mathf.CeilToInt(TextureResolution / 8f);
            CosmicBaker.Dispatch(kernel, threadGroups, threadGroups, 1);

            // Async Readback for the absolute brightest point
            AsyncGPUReadback.Request(resultBuffer, request =>
            {
                if (!request.hasError)
                {
                    // We extract the float3 and scale it to rest exactly on the sphere's surface
                    var data = request.GetData<Vector3>();
                    BrightestPoint = data[0].normalized * SphereRadius;
                    IsGenerationComplete = true;
                }
                resultBuffer.Release();
            });
        }

        private void Update()
        {
            if (_targetCamera == null) return;

            // Simple distance check to flip materials
            float distToCenter = Vector3.Distance(_targetCamera.transform.position, transform.position);

            if (!_isInsideEventHorizon && distToCenter < SphereRadius)
            {
                _isInsideEventHorizon = true;
                _meshRenderer.material = _insideMaterialInst; // Swap to interior
            }
            else if (_isInsideEventHorizon && distToCenter > SphereRadius)
            {
                _isInsideEventHorizon = false;
                _meshRenderer.material = _outsideMaterialInst; // Swap to exterior
            }
        }

        private void OnDestroy()
        {
            if (_cosmicTexture != null) _cosmicTexture.Release();
            if (_outsideMaterialInst != null) Destroy(_outsideMaterialInst);
            if (_insideMaterialInst != null) Destroy(_insideMaterialInst);
        }
    }
}