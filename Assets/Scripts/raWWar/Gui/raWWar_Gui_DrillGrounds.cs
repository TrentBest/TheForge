using Assets.Scripts.raWWar.Drill;
using Assets.Scripts.Workshop.Gameplay.Grid;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.raWWar.Gui
{
    public class raWWar_Gui_DrillGrounds : IGuiProvider
    {
        public string Title => "Live Drill Grounds (Commander View)";

        private DrillGroundsContext _ctx;
        private LiveModelPreviewBuilder _previewRenderer;
        private DrillGroundsLogic _logic;
        private InstancedGridPresenter _gridPresenter;

        // GPU Rendering Assets
        private Mesh _unitMesh;
        private Material _unitMaterial;
        private Material _ghostMaterial;

        public raWWar_Gui_DrillGrounds()
        {
            _ctx = new DrillGroundsContext();

            // 1. Resolve Safe Materials (Fixes the Pink Material Issue!)
            Shader safeShader = GraphicsSettings.currentRenderPipeline != null
                ? Shader.Find("Universal Render Pipeline/Lit")
                : Shader.Find("Standard");

            if (safeShader == null) safeShader = Shader.Find("Hidden/InternalErrorShader");

            _unitMaterial = new Material(safeShader) { enableInstancing = true, color = new Color(0.3f, 0.4f, 0.2f) };
            _ghostMaterial = new Material(safeShader) { enableInstancing = true, color = new Color(0.2f, 0.6f, 1.0f, 0.5f) };

            // Fix alpha blending for ghost recruit dynamically
            _ghostMaterial.SetFloat("_Surface", 1); // 1 = Transparent in URP
            _ghostMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            _ghostMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            _ghostMaterial.SetInt("_ZWrite", 0);
            _ghostMaterial.renderQueue = 3000;

            Material terrainMat = new Material(safeShader) { enableInstancing = true, color = new Color(0.15f, 0.15f, 0.15f) };

            // 2. Generate Primitives
            GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _unitMesh = tempCube.GetComponent<MeshFilter>().sharedMesh;
            GameObject.DestroyImmediate(tempCube);

            GameObject tempPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Mesh terrainMesh = tempPlane.GetComponent<MeshFilter>().sharedMesh;

            Vector3[] vertices = terrainMesh.vertices;
            for (int i = 0; i < vertices.Length; i++) vertices[i] *= 10f;
            terrainMesh.vertices = vertices;
            terrainMesh.RecalculateBounds();
            GameObject.DestroyImmediate(tempPlane);

            // 3. Grid Presenter Initialization
            GridTilePalette palette = new GridTilePalette();
            palette.RegisterTile(0, terrainMesh, terrainMat);
            _gridPresenter = new InstancedGridPresenter(_ctx.MapGrid, palette);

            // 4. GPU Buffers
            int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(SoldierGPUData));
            _ctx.BufferA = new ComputeBuffer(_ctx.MaxSoldiers, stride);
            _ctx.BufferB = new ComputeBuffer(_ctx.MaxSoldiers, stride);

            SoldierGPUData[] initialData = new SoldierGPUData[_ctx.MaxSoldiers];
            initialData[0] = new SoldierGPUData { Position = Vector3.zero, Facing = 0, Health = 100 };
            _ctx.BufferA.SetData(initialData);
            _ctx.BufferB.SetData(initialData);

            _logic = new DrillGroundsLogic(_ctx);

            // 5. Build the Target Node for LiveModelPreviewBuilder
            GameObject drillTargetRoot = new GameObject("DrillGrounds_Target");
            drillTargetRoot.hideFlags = HideFlags.HideAndDontSave;

            // Add a dummy MeshRenderer so LMPB's CalculateBounds frames the entire map perfectly
            var dummyFilter = drillTargetRoot.AddComponent<MeshFilter>();
            dummyFilter.sharedMesh = terrainMesh;
            var dummyRenderer = drillTargetRoot.AddComponent<MeshRenderer>();
            dummyRenderer.sharedMaterial = terrainMat;

            // Scale the dummy to match the Map Grid dimensions so the camera zooms exactly right
            drillTargetRoot.transform.localScale = new Vector3(_ctx.MapWidth / 10f, 1f, _ctx.MapHeight / 10f);

            // Initialize the robust LMPB
            _previewRenderer = new LiveModelPreviewBuilder(drillTargetRoot)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithControls(false)
                .WithAutoRotate(false)
                .WithZoom(2.5f)
                .WithPitch(45f)
                .WithOriginalModel(true) // Crucial: Tell LMPB not to strip our renderer
                .OnBuild(ve =>
                {
                    // Inject our GPU drawing into the UI schedule loop
                    ve.schedule.Execute(() => RenderGpuEntities()).Every(16);
                });
        }

        public VisualElement CreateGui(GuiContext guiCtx)
        {
            var viewportVe = _previewRenderer.CreateGui(guiCtx);
            viewportVe.style.flexGrow = 1;

            var commandPanel = new ForgeContainerBuilder("CommandPanel")
                .WithFlexLayout(FlexDirection.Row, Justify.Center, Align.Center)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                .AddChild(new ForgeButtonBuilder("LEFT FLANK (A)").WithBackgroundColor(new Color(0.6f, 0.2f, 0.2f)).OnClick(() => CommandTurn(-90)))
                .AddChild(new ForgeButtonBuilder("HALF STEP (S)").WithBackgroundColor(new Color(0.4f, 0.4f, 0.4f)).OnClick(() => CommandSpeed(-0.5f)))
                .AddChild(new ForgeButtonBuilder("DOUBLE TIME (W)").WithBackgroundColor(new Color(0.2f, 0.6f, 0.2f)).OnClick(() => CommandSpeed(0.5f)))
                .AddChild(new ForgeButtonBuilder("RIGHT FLANK (D)").WithBackgroundColor(new Color(0.2f, 0.2f, 0.6f)).OnClick(() => CommandTurn(90)))
                .Build();

            commandPanel.style.height = 80;

            var root = new ForgeContainerBuilder("DrillGroundsRoot")
                .WithFlexLayout(FlexDirection.Column, Justify.SpaceBetween, Align.Stretch)
                .WithBackgroundColor(Color.black)
                .AddChild(viewportVe)
                .AddChild(commandPanel)
                .Build();

            // --- FULLSCREEN ISOLATION ---
            // This is how we eliminate the In-World GUI and Hermit Clutter entirely.
            root.style.position = Position.Absolute;
            root.style.top = 0;
            root.style.bottom = 0;
            root.style.left = 0;
            root.style.right = 0;
            root.BringToFront();

            root.focusable = true;
            root.RegisterCallback<AttachToPanelEvent>(evt => root.Focus());

            root.schedule.Execute(() => { if (_logic != null) _logic.OnUpdate(0.016f); }).Every(16);

            root.RegisterCallback<KeyDownEvent>(evt =>
            {
                switch (evt.keyCode)
                {
                    case KeyCode.LeftArrow:
                    case KeyCode.A: CommandTurn(-90); break;
                    case KeyCode.RightArrow:
                    case KeyCode.D: CommandTurn(90); break;
                    case KeyCode.UpArrow:
                    case KeyCode.W: CommandSpeed(0.5f); break;
                    case KeyCode.DownArrow:
                    case KeyCode.S: CommandSpeed(-0.5f); break;
                    case KeyCode.Escape:
                        // Press Escape to tear down the fullscreen view and return to normal UI
                        root.RemoveFromHierarchy();
                        _previewRenderer?.Dispose();
                        break;
                }
            });

            root.RegisterCallback<NavigationMoveEvent>(evt => evt.PreventDefault());

            return root;
        }

        private void RenderGpuEntities()
        {
            if (_ctx == null || _previewRenderer == null) return;

            Camera cam = _previewRenderer.GetCamera();
            if (cam == null) return;

            Bounds bounds = new Bounds(Vector3.zero, new Vector3(10000, 10000, 10000));

            // 1. Draw Instanced Grid Map
            if (_gridPresenter != null) _gridPresenter.RenderMap(cam);

            // 2. Draw Compute Buffer Soldiers
            if (_unitMesh != null && _unitMaterial != null && _ctx.CurrentArmySize > 0)
            {
                ComputeBuffer readBuffer = _ctx.GetReadBuffer();
                if (readBuffer != null)
                {
                    _unitMaterial.SetBuffer("_SoldierBuffer", readBuffer);
                    RenderParams rparams = new RenderParams(_unitMaterial)
                    {
                        worldBounds = bounds,
                        camera = cam,
                        shadowCastingMode = ShadowCastingMode.On,
                        receiveShadows = true
                    };

                    for (int i = 0; i < _unitMesh.subMeshCount; i++)
                    {
                        Graphics.RenderMeshPrimitives(rparams, _unitMesh, i, _ctx.CurrentArmySize);
                    }
                }
            }

            // 3. Draw Ghost Recruit
            if (_ghostMaterial != null && _unitMesh != null)
            {
                Matrix4x4 ghostMatrix = Matrix4x4.TRS(_ctx.RecruitPosition, Quaternion.identity, Vector3.one);
                RenderParams ghostParams = new RenderParams(_ghostMaterial)
                {
                    worldBounds = bounds,
                    camera = cam
                };
                Graphics.RenderMesh(ghostParams, _unitMesh, 0, ghostMatrix);
            }
        }

        private void CommandTurn(float angleDegrees)
        {
            _ctx.CurrentDirection = Quaternion.Euler(0, angleDegrees, 0) * _ctx.CurrentDirection;
            _ctx.CurrentDirection.x = Mathf.Round(_ctx.CurrentDirection.x);
            _ctx.CurrentDirection.z = Mathf.Round(_ctx.CurrentDirection.z);
        }

        private void CommandSpeed(float speedDelta)
        {
            _ctx.SpeedMultiplier = Mathf.Clamp(_ctx.SpeedMultiplier + speedDelta, 0.5f, 2.0f);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}