using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    // A generic Forge tool for rendering massive swarms/grids in the UI via Compute Buffers
    public class InstancedMeshPreviewBuilder : IGuiProvider
    {
        public string Title => "Instanced Formations Preview";

        private Mesh _mesh;
        private Material _material;
        private int _instanceCount;
        private ComputeBuffer _dataBuffer;
        private string _bufferNameInShader;

        private RenderTexture _rt;
        private Camera _previewCamera;
        private GameObject _cameraObj;

        public InstancedMeshPreviewBuilder(Mesh mesh, Material material, int count, ComputeBuffer dataBuffer, string bufferNameInShader = "_InstanceData")
        {
            _mesh = mesh;
            _material = material;
            _instanceCount = count;
            _dataBuffer = dataBuffer;
            _bufferNameInShader = bufferNameInShader;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. Setup Camera & RenderTexture 
            _rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            _rt.Create();

            _cameraObj = new GameObject("InstancedPreviewCamera");
            _cameraObj.transform.position = new Vector3(0, 20, -40); // Start slightly above and pulled back
            _cameraObj.transform.LookAt(Vector3.zero);

            _previewCamera = _cameraObj.AddComponent<Camera>();
            _previewCamera.targetTexture = _rt;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0.05f, 0.05f, 0.08f); // Deep space backdrop

            // 2. Setup UI & Interaction Map
            var imgElement = new ImageGuiBuilder(_rt).WithScaleMode(ScaleMode.ScaleToFit).CreateGui(ctx);

            Vector2 lastMousePos = Vector2.zero;
            bool isDragging = false;
            int dragButton = -1;

            imgElement.RegisterCallback<PointerDownEvent>(evt => {
                if (evt.button == 0 || evt.button == 1) // 0 = Left (Orbit), 1 = Right (Pan)
                {
                    isDragging = true;
                    dragButton = evt.button;
                    lastMousePos = evt.position;
                    imgElement.CapturePointer(evt.pointerId);
                }
            });

            imgElement.RegisterCallback<PointerMoveEvent>(evt => {
                if (isDragging && _cameraObj != null)
                {
                    Vector2 delta = (Vector2)evt.position - lastMousePos;
                    lastMousePos = evt.position;

                    if (dragButton == 0) // ORBIT
                    {
                        _cameraObj.transform.RotateAround(Vector3.zero, Vector3.up, delta.x * 0.4f);
                        _cameraObj.transform.RotateAround(Vector3.zero, _cameraObj.transform.right, delta.y * 0.4f);
                        _cameraObj.transform.LookAt(Vector3.zero);
                    }
                    else if (dragButton == 1) // PAN
                    {
                        _cameraObj.transform.Translate(-delta.x * 0.1f, delta.y * 0.1f, 0, Space.Self);
                    }
                }
            });

            imgElement.RegisterCallback<PointerUpEvent>(evt => {
                isDragging = false;
                imgElement.ReleasePointer(evt.pointerId);
            });

            // ZOOM
            imgElement.RegisterCallback<WheelEvent>(evt => {
                if (_cameraObj != null)
                    _cameraObj.transform.Translate(0, 0, -evt.delta.y * 1.5f, Space.Self);
            });

            // 3. Compile the Root UI and attach the Render Loop
            var rootBuilder = new GraphicalUserInterfaceBuilder("InstancedPreviewRoot")
                .WithBackgroundColor(Color.clear)
                .OnBuild(ve =>
                {
                    ve.style.flexGrow = 1;

                    ve.schedule.Execute(() =>
                    {
                        RenderFormation();
                        ve.MarkDirtyRepaint();
                    }).Every(16); // ~60 FPS update

                    ve.RegisterCallback<DetachFromPanelEvent>(evt => Cleanup());
                })
                .AddChild(imgElement);

            return rootBuilder.Build();
        }

        private void RenderFormation()
        {
            if (_mesh == null || _material == null || _dataBuffer == null) return;

            // Constantly ensure the material is mapped to the active buffer
            _material.SetBuffer(_bufferNameInShader, _dataBuffer);

            // A massive bounds volume so the camera doesn't accidentally cull the army
            Bounds bounds = new Bounds(Vector3.zero, new Vector3(10000, 10000, 10000));

            RenderParams rparams = new RenderParams(_material);
            rparams.worldBounds = bounds;
            rparams.camera = _previewCamera; // Lock to UI camera only!
            rparams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            rparams.receiveShadows = true;

            // Handle models with multiple materials (Submeshes) safely
            for (int i = 0; i < _mesh.subMeshCount; i++)
            {
                Graphics.RenderMeshPrimitives(rparams, _mesh, i, _instanceCount);
            }
        }

        private void Cleanup()
        {
            // CRITICAL: We do NOT release the _dataBuffer here. 
            // The logic generating the buffer owns it. We just view it.
            if (_rt != null) _rt.Release();
            if (_cameraObj != null) UnityEngine.Object.DestroyImmediate(_cameraObj);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}