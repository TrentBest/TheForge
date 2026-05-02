using TheSingularityWorkshop.FSM_API;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    public class MastersOfOrionII_Gui_GridUniverse : IGuiProvider
    {
        public string Title => "ARMADA 2525: OBSERVER STREAMING ENGINE";
        private GuiContext _lastCtx;

        private UniverseBoundaryContext _boundaryContext;
        private CosmicObserverContext _observerContext;
        private Label _dashboardLabel;
        private VisualElement _previewContainer;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            foreach (var o in Resources.FindObjectsOfTypeAll<GameObject>())
                if (o.hideFlags == HideFlags.HideAndDontSave && (o.name.StartsWith("Grid_Diorama") || o.name.StartsWith("SubCell_") || o.name.StartsWith("Univ_")))
                    UnityEngine.Object.DestroyImmediate(o);

            if (_boundaryContext == null) InitializeUniverseGrid();

            var splitPanel = new ForgeSplitPanelBuilder(sidebarWidth: 350).WithSidebar(CreateSidebar()).WithMain(CreateMainViewport());
            var rootContainer = splitPanel.CreateGui(ctx);

            // --- THE MASTER ENGINE LOOP ---
            rootContainer.schedule.Execute(() => {

                // 1. Expand the Big Bang (This was missing!)
                FSM_API.Interaction.Update("UniverseBoundaries");

                // 2. Evaluate Observer distances
                FSM_API.Interaction.Update("ObserverManager");

                // 3. Tick all cells (They will early-exit based on their LOD distance)
                FSM_API.Interaction.Update("CosmicMath");

                UpdateDashboard();

            }).Every(16);

            return rootContainer;
        }

        private IGuiProvider CreateSidebar()
        {
            return new GraphicalUserInterfaceBuilder("GridSidebar")
                .WithPadding(15).WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .AddChild(new Label("LOD STREAMING ENGINE") { style = { color = Color.cyan, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } })

                .AddChild(new GraphicalUserInterfaceBuilder("DashBox")
                    .WithPadding(10).WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                    .WithBorderWidth(1).WithBorderColor(Color.gray)
                    .OnBuild(ve => { _dashboardLabel = new Label("Initializing..") { style = { color = Color.white } }; ve.Add(_dashboardLabel); })
                    .Build()
                )

                .AddSeparator()
                .AddChild(new Label("Big Bang Expansion:") { style = { color = Color.white, marginTop = 10 } })
                .AddSliderData("Growth Speed", 0f, 1f, _boundaryContext.GridConfig.ExpansionSpeed, v => _boundaryContext.GridConfig.ExpansionSpeed = v)

                .AddSeparator()
                .AddChild(new Label("Observer Coordinates (Move Player):") { style = { color = Color.yellow, marginTop = 10 } })
                .AddIntSliderData("X Axis", -5, 5, _observerContext.ObserverGridPosition.x, v => UpdateObserverPos(new Vector3Int(v, _observerContext.ObserverGridPosition.y, _observerContext.ObserverGridPosition.z)))
                .AddIntSliderData("Y Axis", -5, 5, _observerContext.ObserverGridPosition.y, v => UpdateObserverPos(new Vector3Int(_observerContext.ObserverGridPosition.x, v, _observerContext.ObserverGridPosition.z)))
                .AddIntSliderData("Z Axis", -5, 5, _observerContext.ObserverGridPosition.z, v => UpdateObserverPos(new Vector3Int(_observerContext.ObserverGridPosition.x, _observerContext.ObserverGridPosition.y, v)));
        }

        private void UpdateObserverPos(Vector3Int newPos)
        {
            _observerContext.ObserverGridPosition = newPos;
            if (_observerContext.ObserverVisual != null) _observerContext.ObserverVisual.localPosition = (Vector3)newPos * 3.33f;
        }

        private IGuiProvider CreateMainViewport()
        {
            return new GraphicalUserInterfaceBuilder("GridViewport")
                .WithBackgroundColor(Color.black)
                .WithFlexGrow(1).WithFlexShrink(1)
                .OnBuild(ve => {
                    _previewContainer = ve;
                    var dioramaRoot = new GameObject("Grid_Diorama_Pivot");
                    dioramaRoot.hideFlags = HideFlags.HideAndDontSave;

                    if (_boundaryContext.UniverseSphereVisual != null) _boundaryContext.UniverseSphereVisual.SetParent(dioramaRoot.transform, true);
                    if (_observerContext.ObserverVisual != null) _observerContext.ObserverVisual.SetParent(dioramaRoot.transform, true);

                    foreach (var cell in _observerContext.AllCells)
                    {
                        if (cell.VisualRenderer != null) cell.VisualRenderer.transform.SetParent(dioramaRoot.transform, true);
                    }

                    var previewBuilder = new LiveModelPreviewBuilder(dioramaRoot);
                    ve.Add(new GraphicalUserInterfaceBuilder("PreviewWrapper").WithFlexGrow(1).WithFlexShrink(1).AddChild(previewBuilder).Build());
                });
        }

        private void InitializeUniverseGrid()
        {
            _boundaryContext = new UniverseBoundaryContext();
            _observerContext = new CosmicObserverContext();

            // Set up processing state visualizations
            Shader unlit = Shader.Find("Sprites/Default"); // Ignores lighting, pure glowing color
            _observerContext.MatPlayer = new Material(unlit) { color = new Color(0f, 1f, 0f, 0.8f) };    // Bright Green
            _observerContext.MatAdjacent = new Material(unlit) { color = new Color(0f, 1f, 1f, 0.5f) };  // Cyan
            _observerContext.MatDistant = new Material(unlit) { color = new Color(1f, 0f, 1f, 0.2f) };   // Dim Magenta
            _observerContext.MatInvisible = new Material(unlit) { color = new Color(0f, 0f, 0f, 0f) };   // Invisible

            // 1. Boundary Visual
            GameObject boundaryObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            boundaryObj.name = "Univ_Boundary";
            boundaryObj.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DestroyImmediate(boundaryObj.GetComponent<Collider>());
            boundaryObj.GetComponent<MeshRenderer>().sharedMaterial = new Material(unlit) { color = new Color32(50, 0, 100, 40) };
            _boundaryContext.UniverseSphereVisual = boundaryObj.transform;

            // 2. Observer Visual
            GameObject observerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            observerObj.name = "Univ_Observer";
            observerObj.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DestroyImmediate(observerObj.GetComponent<Collider>());
            observerObj.GetComponent<MeshRenderer>().sharedMaterial = new Material(unlit) { color = Color.yellow };
            observerObj.transform.localScale = Vector3.one * 1.5f;
            _observerContext.ObserverVisual = observerObj.transform;

            // Define FSMs
            if ( !FSM_API.Interaction.Exists("SubCellFSM"))
                FSM_API.Create.CreateFiniteStateMachine("SubCellFSM", -1, "CosmicMath")
                    .State("Idle", null, null, null).State("Simulating", null, OnSubCellUpdate, null)
                    .Transition("Idle", "Simulating", ctx => true).BuildDefinition();

            if ( !FSM_API.Interaction.Exists("ObserverFSM"))
                FSM_API.Create.CreateFiniteStateMachine("ObserverFSM", -1, "ObserverManager")
                    .State("Idle", null, null, null).State("Tracking", null, OnObserverTick, null)
                    .Transition("Idle", "Tracking", ctx => true).BuildDefinition();

            if ( !FSM_API.Interaction.Exists("UniverseBoundaryFSM"))
                FSM_API.Create.CreateFiniteStateMachine("UniverseBoundaryFSM", -1, "UniverseBoundaries")
                    .State("Idle", null, null, null).State("Expanding", null, OnBoundaryUpdate, null)
                    .Transition("Idle", "Expanding", ctx => true).BuildDefinition();

            // Populate Grid
            int extents = 5;
            float subSize = 3.33f;
            System.Random matterRng = new System.Random(_boundaryContext.GridConfig.Seed.GetHashCode());

            for (int x = -extents; x <= extents; x++)
            {
                for (int y = -extents; y <= extents; y++)
                {
                    for (int z = -extents; z <= extents; z++)
                    {

                        var coord = new Vector3Int(x, y, z);
                        GameObject subRoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        subRoot.name = $"SubCell_{x}_{y}_{z}";
                        subRoot.hideFlags = HideFlags.HideAndDontSave;
                        subRoot.transform.position = (Vector3)coord * subSize;
                        subRoot.transform.localScale = Vector3.one * (subSize * 0.8f);
                        UnityEngine.Object.DestroyImmediate(subRoot.GetComponent<Collider>());

                        var renderer = subRoot.GetComponent<MeshRenderer>();
                        renderer.sharedMaterial = _observerContext.MatInvisible;

                        var subCtx = new SubCellContext
                        {
                            Name = $"Sub_{x}_{y}_{z}",
                            Data = new SubCellData { LocalCoordinates = coord },
                            VisualRenderer = renderer,
                            SharedGridConfig = _boundaryContext.GridConfig,
                            ExactWorldPosition = subRoot.transform.position,
                            IsInsideUniverse = false
                        };

                        _observerContext.AllCells.Add(subCtx);
                        FSM_API.Create.CreateInstance("SubCellFSM", subCtx, "CosmicMath");
                    }
                }
            }

            FSM_API.Create.CreateInstance("UniverseBoundaryFSM", _boundaryContext, "UniverseBoundaries");
            FSM_API.Create.CreateInstance("ObserverFSM", _observerContext, "ObserverManager");
        }

        private void OnBoundaryUpdate(IStateContext ctx)
        {
            var boundary = (UniverseBoundaryContext)ctx;
            boundary.GridConfig.UniverseAgeYears += 100000;
            boundary.GridConfig.CurrentRadius += boundary.GridConfig.ExpansionSpeed;

            if (boundary.UniverseSphereVisual != null)
                boundary.UniverseSphereVisual.localScale = Vector3.one * (boundary.GridConfig.CurrentRadius * 2f);
        }

        // Calculates distances and assigns the specific Tick Throttle
        private void OnObserverTick(IStateContext ctx)
        {
            var observer = (CosmicObserverContext)ctx;

            foreach (var cell in observer.AllCells)
            {
                // Calculate grid distance (Chebyshev distance is best for grid LOD)
                int distX = Mathf.Abs(cell.Data.LocalCoordinates.x - observer.ObserverGridPosition.x);
                int distY = Mathf.Abs(cell.Data.LocalCoordinates.y - observer.ObserverGridPosition.y);
                int distZ = Mathf.Abs(cell.Data.LocalCoordinates.z - observer.ObserverGridPosition.z);
                cell.DistanceToObserver = Mathf.Max(distX, Mathf.Max(distY, distZ));

                // ASSIGN TICK THROTTLE BASED ON DISTANCE
                if (cell.DistanceToObserver == 0) cell.LodThrottle = 1;       // Player Cell: Every frame
                else if (cell.DistanceToObserver <= 2) cell.LodThrottle = 5;  // Adjacent Region: Every 5 frames
                else cell.LodThrottle = 30;                                   // Distant Universe: Every 30 frames

                // Reset material to invisible so the Cell Math can flash it when processed
                if (cell.VisualRenderer != null) cell.VisualRenderer.sharedMaterial = observer.MatInvisible;
            }
        }

        private void OnSubCellUpdate(IStateContext ctx)
        {
            var subCtx = (SubCellContext)ctx;

            // 1. THE BIG BANG CHECK
            if (!subCtx.IsInsideUniverse)
            {
                if (subCtx.ExactWorldPosition.magnitude <= subCtx.SharedGridConfig.CurrentRadius)
                {
                    subCtx.IsInsideUniverse = true;
                    System.Random matterRng = new System.Random((subCtx.Name + subCtx.SharedGridConfig.Seed).GetHashCode());
                    subCtx.HasMatter = matterRng.NextDouble() > 0.70;
                }
                return; // Nothing outside the universe processes math
            }

            // 2. THE VOID CHECK (Deep space saves CPU)
            if (!subCtx.HasMatter && subCtx.DistanceToObserver > 0) return;

            // 3. THE LOD THROTTLE (The Magic Performance Trick)
            subCtx.FrameCounter++;
            if (subCtx.FrameCounter < subCtx.LodThrottle) return; // EARLY EXIT!
            subCtx.FrameCounter = 0;

            // --- ACTUAL MATH HAPPENS HERE ---
            subCtx.Data.SubEntropy += 0.01f;

            // --- VISUAL PROOF OF PROCESSING ---
            // If the code reaches here, it just processed math. We light it up for exactly 1 frame!
            if (subCtx.VisualRenderer != null)
            {
                var obs = _observerContext;
                if (subCtx.DistanceToObserver == 0) subCtx.VisualRenderer.sharedMaterial = obs.MatPlayer;
                else if (subCtx.DistanceToObserver <= 2) subCtx.VisualRenderer.sharedMaterial = obs.MatAdjacent;
                else subCtx.VisualRenderer.sharedMaterial = obs.MatDistant;
            }
        }

        private void UpdateDashboard()
        {
            if (_dashboardLabel == null || _observerContext == null) return;

            int pCount = 0; int aCount = 0; int dCount = 0;
            foreach (var sub in _observerContext.AllCells)
            {
                if (sub.IsInsideUniverse && sub.HasMatter)
                {
                    if (sub.DistanceToObserver == 0) pCount++;
                    else if (sub.DistanceToObserver <= 2) aCount++;
                    else dCount++;
                }
            }

            _dashboardLabel.text =
                $"BIG BANG EXPANSION: {_boundaryContext.GridConfig.CurrentRadius:F1} LY\n\n" +
                $"--- LOD STREAMING QUEUE ---\n" +
                $"LOD 0 (60Hz Ticks): <color=green>{pCount}</color> Cells\n" +
                $"LOD 1 (12Hz Ticks): <color=cyan>{aCount}</color> Cells\n" +
                $"LOD 2 ( 2Hz Ticks): <color=magenta>{dCount}</color> Cells\n\n" +
                $"(Empty Void space is skipping math)";
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}