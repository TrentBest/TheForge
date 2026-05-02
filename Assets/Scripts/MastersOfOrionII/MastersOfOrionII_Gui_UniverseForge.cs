using System;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_UniverseForge : IGuiProvider
    {
        public string Title => "ARMADA 2525: UNIVERSE FORGE";
        private GuiContext _lastCtx;

        private UniverseForgeContext _simContext;
        private VisualElement _previewContainer;
        private VisualElement _crudArea;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            // Orphan Sweeper to keep memory clean during rapid recompiles
            foreach (var o in UnityEngine.Resources.FindObjectsOfTypeAll<GameObject>())
                if (o.hideFlags == HideFlags.HideAndDontSave && o.name.StartsWith("Armada_Diorama_")) UnityEngine.Object.DestroyImmediate(o);

            if (_simContext == null)
            {
                _simContext = new UniverseForgeContext();
                InitializeFSM();
            }

            var rootBuilder = new GraphicalUserInterfaceBuilder("UniverseRoot")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100);

            // --- LEFT PANEL ---
            rootBuilder.AddChild(new GraphicalUserInterfaceBuilder("EditorPanel")
                .WithFlexGrow(1).WithFlexShrink(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)

                .AddChild(new GraphicalUserInterfaceBuilder("SimControls")
                    .WithPadding(15).WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                    .WithBorderBottomWidth(2).WithBorderBottomColor(new Color(0.2f, 0.6f, 0.8f))
                    .AddChild(new Label("MACRO COSMIC SEED") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } })
                    .AddStringData("Universal Hash", _simContext.Universe.Seed, v => {
                        _simContext.Universe.Seed = v;
                        _simContext.NeedsDioramaRebuild = true;
                    })
                    .Build())

                .AddChild(new GraphicalUserInterfaceBuilder("ActiveCrudArea")
                    .WithFlexGrow(1).WithFlexShrink(1)
                    .WithPadding(15)
                    .OnBuild(ve => {
                        _crudArea = ve;
                        RefreshToolArea();
                    })
                    .Build())
            );

            // --- RIGHT PANEL (Dynamic Square Viewport) ---
            rootBuilder.AddChild(new GraphicalUserInterfaceBuilder("SquareViewport")
                .WithBackgroundColor(Color.black)
                .WithFlexShrink(0)
                .OnBuild(ve => {
                    ve.RegisterCallback<GeometryChangedEvent>(evt => {
                        float h = evt.newRect.height;
                        if (Mathf.Abs(ve.style.width.value.value - h) > 1f) ve.style.width = h;
                    });
                    _previewContainer = ve;
                    RefreshPreviewViewport();
                })
            );

            var root = rootBuilder.Build();

            // Fast tick rate for smooth zoom animations
            root.schedule.Execute(() => FSM_API.Interaction.Update("UniverseForge")).Every(16);

            return root;
        }

        private void RefreshToolArea()
        {
            if (_crudArea == null) return;
            _crudArea.Clear();

            if (_simContext.CurrentView == CosmicViewLevel.Universe)
            {
                _crudArea.Add(new Label("GALACTIC CLUSTERS") { style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
                _crudArea.Add(BuildUniverseCRUD());
            }
            else if (_simContext.CurrentView == CosmicViewLevel.Galaxy && _simContext.ActiveGalaxy != null)
            {
                _crudArea.Add(new Label($"EXPLORING: {_simContext.ActiveGalaxy.Name.ToUpper()}") { style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
                _crudArea.Add(new Button(() => {
                    // Trigger Zoom Out
                    FSM_API.Interaction.GetInstance("UniverseForgeFSM", _simContext, "UniverseForge").TransitionTo("ZoomingOut");
                })
                { text = "<< RETURN TO UNIVERSE VIEW", style = { marginBottom = 15, backgroundColor = new Color(0.2f, 0.2f, 0.2f), color = Color.white } });

                _crudArea.Add(BuildGalaxyCRUD());
            }
        }

        private void InitializeFSM()
        {
            if ( !FSM_API.Interaction.Exists("UniverseForgeFSM"))
            {
                FSM_API.Create.CreateFiniteStateMachine("UniverseForgeFSM", -1, "UniverseForge")
                    .State("Idle", null, null, null)
                    .State("RebuildingDiorama", null, RebuildDiorama, null)

                    // The Graceful Zoom Transitions
                    .State("ZoomingIn",
                        ctx => { var c = (UniverseForgeContext)ctx; c.ZoomProgress = 0f; c.ZoomStartPos = c.DioramaRoot.transform.localPosition; c.ZoomTargetPos = -c.ActiveGalaxy.UniversePosition; },
                        AnimateZoomIn, null)

                    .State("ZoomingOut",
                        ctx => { var c = (UniverseForgeContext)ctx; c.ZoomProgress = 0f; c.ZoomStartPos = c.DioramaRoot.transform.localPosition; c.ZoomTargetPos = Vector3.zero; },
                        AnimateZoomOut, null)

                    .Transition("Idle", "RebuildingDiorama", ctx => ((UniverseForgeContext)ctx).NeedsDioramaRebuild)
                    .Transition("RebuildingDiorama", "Idle", ctx => !((UniverseForgeContext)ctx).NeedsDioramaRebuild)
                    .BuildDefinition();
            }

            FSM_API.Create.CreateInstance("UniverseForgeFSM", _simContext, "UniverseForge");
        }

        // --- THE GRACEFUL FSM ZOOM LOGIC ---
        private void AnimateZoomIn(IStateContext ctx)
        {
            var simCtx = (UniverseForgeContext)ctx;
            simCtx.ZoomProgress += 0.02f; // Drives speed of zoom

            // By moving the root in the opposite direction of the galaxy, the galaxy moves to the center (0,0,0) of the camera!
            simCtx.DioramaRoot.transform.localPosition = Vector3.Lerp(simCtx.ZoomStartPos, simCtx.ZoomTargetPos, Mathf.SmoothStep(0f, 1f, simCtx.ZoomProgress));

            if (simCtx.ZoomProgress >= 1f)
            {
                // Zoom complete! Switch contexts.
                simCtx.CurrentView = CosmicViewLevel.Galaxy;
                simCtx.NeedsDioramaRebuild = true;

                // FIX: Call our local UI refresh directly instead of the generic context delegate!
                RefreshToolArea();

                FSM_API.Interaction.GetInstance("UniverseForgeFSM", _simContext, "UniverseForge").TransitionTo("RebuildingDiorama");
            }
        }

        private void AnimateZoomOut(IStateContext ctx)
        {
            var simCtx = (UniverseForgeContext)ctx;
            simCtx.ZoomProgress += 0.02f;

            simCtx.DioramaRoot.transform.localPosition = Vector3.Lerp(simCtx.ZoomStartPos, simCtx.ZoomTargetPos, Mathf.SmoothStep(0f, 1f, simCtx.ZoomProgress));

            if (simCtx.ZoomProgress >= 1f)
            {
                simCtx.CurrentView = CosmicViewLevel.Universe;
                simCtx.NeedsDioramaRebuild = true;

                // FIX: Call our local UI refresh directly!
                RefreshToolArea();

                FSM_API.Interaction.GetInstance("UniverseForgeFSM", _simContext, "UniverseForge").TransitionTo("RebuildingDiorama");
            }
        }

        private void RebuildDiorama(IStateContext ctx)
        {
            var simCtx = (UniverseForgeContext)ctx;
            if (simCtx.DioramaRoot != null) UnityEngine.Object.DestroyImmediate(simCtx.DioramaRoot);

            simCtx.DioramaRoot = new GameObject($"Armada_Diorama_{simCtx.Universe.Seed}");
            simCtx.DioramaRoot.hideFlags = HideFlags.HideAndDontSave;

            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            Material unlitMat = new Material(unlitShader);

            // --- UNIVERSE VIEW ---
            if (simCtx.CurrentView == CosmicViewLevel.Universe)
            {
                if (simCtx.Universe.Galaxies.Count == 0) UniverseCartographer.GenerateGalaxies(simCtx.Universe, 12);

                simCtx.UniverseContainer = new GameObject("Macro_Universe");
                simCtx.UniverseContainer.transform.SetParent(simCtx.DioramaRoot.transform, false);

                foreach (var galaxy in simCtx.Universe.Galaxies)
                {
                    GameObject gObj = UniverseCartographer.RenderGalaxyMarker(galaxy, unlitMat);
                    gObj.transform.SetParent(simCtx.UniverseContainer.transform, false);
                }
            }
            // --- GALAXY VIEW ---
            else if (simCtx.CurrentView == CosmicViewLevel.Galaxy && simCtx.ActiveGalaxy != null)
            {
                // We physically destroy the Macro Universe renderers here to save rendering overhead!

                simCtx.ActiveGalaxyContainer = new GameObject($"Local_Galaxy_{simCtx.ActiveGalaxy.Name}");
                simCtx.ActiveGalaxyContainer.transform.SetParent(simCtx.DioramaRoot.transform, false);

                if (simCtx.ActiveGalaxy.StarSystems.Count == 0) UniverseCartographer.GenerateStarSystems(simCtx.ActiveGalaxy, simCtx.Universe.Seed);

                foreach (var star in simCtx.ActiveGalaxy.StarSystems)
                {
                    GameObject sObj = UniverseCartographer.RenderStarMarker(star, unlitMat);
                    sObj.transform.SetParent(simCtx.ActiveGalaxyContainer.transform, false);
                }
            }

            RefreshPreviewViewport();
            simCtx.NeedsDioramaRebuild = false;
        }

        private void RefreshPreviewViewport()
        {
            if (_previewContainer == null || _simContext.DioramaRoot == null) return;
            _previewContainer.Clear();
            var previewBuilder = new LiveModelPreviewBuilder(_simContext.DioramaRoot);
            _previewContainer.Add(new GraphicalUserInterfaceBuilder("PreviewWrapper").WithFlexGrow(1).WithFlexShrink(1).AddChild(previewBuilder).Build());
        }

        // --- DYNAMIC CRUDS ---
        private VisualElement BuildUniverseCRUD()
        {
            var crud = new CRUD_Builder<GalaxyData>("GALAXIES", () => _simContext.Universe.Galaxies, g => g.Name,
                g => new GraphicalUserInterfaceBuilder("GalForm").WithPadding(10)
                        .AddStringData("Designation", g.Name, v => g.Name = v)
                        .AddButton("🚀 EXPLORE GALAXY (ZOOM IN)", () => {
                            _simContext.ActiveGalaxy = g;
                            FSM_API.Interaction.GetInstance("UniverseForgeFSM", _simContext, "UniverseForge").TransitionTo("ZoomingIn");
                        })
                        .AddSeparator()
                        .AddChild(new Label("Morphology Settings") { style = { color = Color.gray, marginBottom = 5 } })
                        .AddSliderData("Spiral Arms", 1, 8, g.NumberOfArms, v => { g.NumberOfArms = (int)v; g.StarSystems.Clear(); })
                        .AddSliderData("Chaos Factor", 0f, 50f, g.ChaosFactor, v => { g.ChaosFactor = v; g.StarSystems.Clear(); })
                        .Build(),
                g => { }, g => { }
            );
            crud.Style.ListWidth = 140f;
            return crud.CreateGui(_lastCtx);
        }

        private VisualElement BuildGalaxyCRUD()
        {
            var crud = new CRUD_Builder<StarSystemData>("STAR SYSTEMS", () => _simContext.ActiveGalaxy.StarSystems, s => s.Name,
                s => new GraphicalUserInterfaceBuilder("StarForm").WithPadding(10)
                        .AddStringData("System Name", s.Name, v => s.Name = v)
                        .AddChild(new Label($"Galactic Coordinates: {s.LocalPosition}") { style = { color = Color.gray, marginTop = 10 } })
                        .Build(),
                s => { }, s => { }
            );
            crud.Style.ListWidth = 150f;
            crud.Style.AccentColor = _simContext.ActiveGalaxy.ThemeColor;
            return crud.CreateGui(_lastCtx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}