using System;
using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.raWWar.Drill;
using Assets.Scripts.Workshop.Gameplay.Grid;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.raWWar.Gui
{
    public class LiveFormationPreviewBuilder : IGuiProvider
    {
        public string Title => "Live Formation Preview";

        private DrillGroundsContext _ctx;
        private InstancedGridPresenter _gridPresenter; // The generic map renderer

        // Rendering Assets
        private Mesh _unitMesh;
        private Material _unitMaterial;
        private Material _ghostMaterial;

        private RenderTexture _rt;
        private Camera _previewCamera;
        private GameObject _cameraObj; // Pure transform container, no MonoBehaviours!

        // Constructor injects the Context, Map Presenter, and Soldier Assets
        public LiveFormationPreviewBuilder(DrillGroundsContext ctx, InstancedGridPresenter gridPresenter, Mesh unitMesh, Material unitMaterial, Material ghostMaterial)
        {
            _ctx = ctx;
            _gridPresenter = gridPresenter;
            _unitMesh = unitMesh;
            _unitMaterial = unitMaterial;
            _ghostMaterial = ghostMaterial;
        }

        public VisualElement CreateGui(GuiContext guiCtx)
        {
            _rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            _rt.Create();

            _cameraObj = new GameObject("FormationPreviewCamera");

            // Start right down in the dirt with the troops
            _cameraObj.transform.position = new Vector3(0, 8, -15);
            _cameraObj.transform.LookAt(Vector3.zero);

            _previewCamera = _cameraObj.AddComponent<Camera>();
            _previewCamera.targetTexture = _rt;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0.08f, 0.08f, 0.1f);

            var imgElement = new ImageGuiBuilder(_rt).WithScaleMode(ScaleMode.ScaleToFit).CreateGui(guiCtx);

            // --- CAMERA INTERACTION LOGIC ---
            Vector2 lastMousePos = Vector2.zero;
            bool isDragging = false;
            int dragButton = -1;

            imgElement.RegisterCallback<PointerDownEvent>(evt => {
                if (evt.button == 0 || evt.button == 1)
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

            imgElement.RegisterCallback<WheelEvent>(evt => {
                if (_cameraObj != null)
                    _cameraObj.transform.Translate(0, 0, -evt.delta.y * 1.5f, Space.Self);
            });

            var rootBuilder = new GraphicalUserInterfaceBuilder("FormationRoot")
                .WithBackgroundColor(Color.black)
                .OnBuild(ve =>
                {
                    ve.style.flexGrow = 1;
                    ve.schedule.Execute(() =>
                    {
                        UpdateCameraScaling();
                        RenderScene();
                        ve.MarkDirtyRepaint();
                    }).Every(16);
                    ve.RegisterCallback<DetachFromPanelEvent>(evt => Cleanup());
                })
                .AddChild(imgElement);

            return rootBuilder.Build();
        }

        private void UpdateCameraScaling()
        {
            if (_ctx == null || _cameraObj == null) return;

            float baseHeight = 15f;
            float zoomOutThreshold = 50f;
            float growthFactor = Mathf.Max(0, _ctx.CurrentArmySize - zoomOutThreshold) * 0.4f;

            float desiredHeight = Mathf.Clamp(baseHeight + growthFactor, baseHeight, _ctx.MapWidth * 1.5f);
            float desiredZ = -desiredHeight * 0.8f;

            Vector3 targetPos = new Vector3(_cameraObj.transform.position.x, desiredHeight, desiredZ);

            _cameraObj.transform.position = Vector3.Lerp(_cameraObj.transform.position, targetPos, Time.deltaTime * 1.5f);
            _cameraObj.transform.LookAt(Vector3.zero);
        }

        private void RenderScene()
        {
            if (_ctx == null) return;
            Bounds bounds = new Bounds(Vector3.zero, new Vector3(10000, 10000, 10000));

            // --- 1. RENDER TERRAIN (Via Generic GPU Presenter) ---
            if (_gridPresenter != null)
            {
                _gridPresenter.RenderMap(_previewCamera);
            }

            // --- 2. RENDER ARMY (Via Compute Buffer) ---
            if (_unitMesh != null && _unitMaterial != null && _ctx.CurrentArmySize > 0)
            {
                ComputeBuffer readBuffer = _ctx.GetReadBuffer();
                if (readBuffer != null)
                {
                    _unitMaterial.SetBuffer("_SoldierBuffer", readBuffer);

                    RenderParams rparams = new RenderParams(_unitMaterial);
                    rparams.worldBounds = bounds;
                    rparams.camera = _previewCamera;
                    rparams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    rparams.receiveShadows = true;

                    for (int submeshIndex = 0; submeshIndex < _unitMesh.subMeshCount; submeshIndex++)
                    {
                        Graphics.RenderMeshPrimitives(rparams, _unitMesh, submeshIndex, _ctx.CurrentArmySize);
                    }
                }
            }

            // --- 3. RENDER GHOST RECRUIT ---
            if (_ghostMaterial != null && _unitMesh != null)
            {
                Matrix4x4 ghostMatrix = Matrix4x4.TRS(_ctx.RecruitPosition, Quaternion.identity, Vector3.one);
                RenderParams ghostParams = new RenderParams(_ghostMaterial);
                ghostParams.worldBounds = bounds;
                ghostParams.camera = _previewCamera;

                Graphics.RenderMesh(ghostParams, _unitMesh, 0, ghostMatrix);
            }
        }

        private void Cleanup()
        {
            if (_rt != null) _rt.Release();
            if (_cameraObj != null) UnityEngine.Object.DestroyImmediate(_cameraObj);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}