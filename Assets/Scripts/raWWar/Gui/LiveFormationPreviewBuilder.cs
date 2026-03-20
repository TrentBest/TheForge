using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

using System.Runtime.InteropServices;

namespace Assets.Scripts.raWWar.Editors
{
    public class LiveFormationPreviewBuilder : IGuiProvider
    {
        public string Title => "Formation Preview";

        private Mesh _unitMesh;
        private Material _unitMaterial; // MUST have an instancing shader!
        private int _unitCount;

        private ComputeBuffer _soldierBuffer;
        private ComputeBuffer _argsBuffer; // For DrawMeshInstancedIndirect

        private RenderTexture _rt;
        private Camera _previewCamera;
        private GameObject _cameraObj;

        public LiveFormationPreviewBuilder(Mesh mesh, Material material, int count)
        {
            _unitMesh = mesh;
            _unitMaterial = material;
            _unitCount = count;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. Setup Camera & RenderTexture 
            _rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            _rt.Create();

            _cameraObj = new GameObject("FormationPreviewCamera");
            // Start the camera a bit closer and angled down
            _cameraObj.transform.position = new Vector3(0, 20, -40);
            _cameraObj.transform.LookAt(Vector3.zero);

            _previewCamera = _cameraObj.AddComponent<Camera>();
            _previewCamera.targetTexture = _rt;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0.1f, 0.1f, 0.12f);

            // 2. Initialize GPU Buffers
            InitializeGPUFormation();

            // 3. Setup UI & Interaction
            var imgElement = new ImageGuiBuilder(_rt).WithScaleMode(ScaleMode.ScaleToFit).CreateGui(ctx);

            // --- CAMERA INTERACTION LOGIC ---
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
            // --------------------------------

            var rootBuilder = new GraphicalUserInterfaceBuilder("FormationRoot")
                .WithBackgroundColor(Color.black)
                .OnBuild(ve =>
                {
                    ve.style.flexGrow = 1;
                    ve.schedule.Execute(() =>
                    {
                        RenderFormation();
                        ve.MarkDirtyRepaint();
                    }).Every(16);
                    ve.RegisterCallback<DetachFromPanelEvent>(evt => Cleanup());
                })
                .AddChild(imgElement); // Add our newly interactive image

            return rootBuilder.Build();
        }

        private void InitializeGPUFormation()
        {
            // Allocate memory on the GPU (Count * Size of Struct)
            int stride = Marshal.SizeOf(typeof(SoldierGPUData));
            _soldierBuffer = new ComputeBuffer(_unitCount, stride);

            // Generate formation positions (e.g., a massive grid)
            SoldierGPUData[] armyData = new SoldierGPUData[_unitCount];
            int columns = Mathf.CeilToInt(Mathf.Sqrt(_unitCount));
            float spacing = 2.5f;

            for (int i = 0; i < _unitCount; i++)
            {
                int row = i / columns;
                int col = i % columns;

                armyData[i] = new SoldierGPUData
                {
                    Position = new Vector3(col * spacing - (columns * spacing / 2), 0, row * spacing),
                    Facing = 0f,
                    StateId = 0,
                    Health = 100f
                };
            }

            // Push data to GPU
            _soldierBuffer.SetData(armyData);

            // Bind the buffer to the Material so the Shader can read the positions!
            _unitMaterial.SetBuffer("_SoldierBuffer", _soldierBuffer);
        }

        private void RenderFormation()
        {
            if (_unitMesh == null || _unitMaterial == null || _soldierBuffer == null) return;

            Bounds bounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000));

            // 1. Setup the modern RenderParams
            RenderParams rparams = new RenderParams(_unitMaterial);
            rparams.worldBounds = bounds;

            // CRITICAL: Bind it exclusively to our UI preview camera!
            rparams.camera = _previewCamera;

            // Optional: Turn on shadows for the UI preview
            rparams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            rparams.receiveShadows = true;

            // 2. The Submesh Loop
            // This safely handles models with 1 material OR 10 materials.
            for (int submeshIndex = 0; submeshIndex < _unitMesh.subMeshCount; submeshIndex++)
            {
                // NOTE ON MULTIPLE MATERIALS: 
                // If your Soldier requires different materials for different submeshes, 
                // you would pass a Material[] to this Builder instead of a single Material.
                // Then, you would update the material right here before drawing:
                // rparams.material = _unitMaterials[submeshIndex];

                Graphics.RenderMeshPrimitives(rparams, _unitMesh, submeshIndex, _unitCount);
            }
        }

        private void Cleanup()
        {
            if (_soldierBuffer != null) _soldierBuffer.Release();
            if (_argsBuffer != null) _argsBuffer.Release();
            if (_rt != null) _rt.Release();
            if (_cameraObj != null) UnityEngine.Object.DestroyImmediate(_cameraObj);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}