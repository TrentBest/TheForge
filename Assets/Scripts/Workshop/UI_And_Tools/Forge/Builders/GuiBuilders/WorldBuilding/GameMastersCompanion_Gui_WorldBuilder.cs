using TheSingularityWorkshop.FSM_API;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    public class GameMastersCompanion_Gui_WorldBuilder : IGuiProvider
    {
        public string Title => "GM COMPANION: TECTONIC WORLD FORGE";
        private GuiContext _lastCtx;

        private WorldBuilderContext _simContext;
        private VisualElement _previewContainer;
        private VisualElement _crudArea;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            if (_simContext == null)
            {
                _simContext = new WorldBuilderContext();
                InitializeFSM();
            }

            var splitPanel = new ForgeSplitPanelBuilder(sidebarWidth: 450);

            splitPanel.WithSidebar(new GraphicalUserInterfaceBuilder("WorldBuilder_Sidebar")
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)

                .AddChild(new GraphicalUserInterfaceBuilder("SimControls")
                    .WithPadding(15).WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                    .WithBorderBottomWidth(2).WithBorderBottomColor(new Color(0.2f, 0.6f, 0.8f))

                    // REFACTORED: Using ForgeLabelBuilder instead of raw Label
                    .AddChild(new ForgeLabelBuilder("1. PLANETARY SEED")
                        .WithColor(Color.cyan)
                        .WithBold()
                        .WithMarginBottom(5))

                    // REFACTORED: Using GraphicalUserInterfaceBuilder's native data binding instead of raw TextField
                    .AddStringData("Core Seed", _simContext.Seed, val => {
                        _simContext.Seed = val;
                        _simContext.Continents.Clear();
                        _simContext.NeedsMeshRebuild = true;
                    })

                    .AddButton("🎲 RANDOMIZE SEED", () => {
                        _simContext.Seed = $"Planet_{UnityEngine.Random.Range(1000, 99999)}";
                        _simContext.Continents.Clear();
                        _simContext.NeedsMeshRebuild = true;

                        // Force a full UI repaint to update the StringData field
                        if (_lastCtx?.OnBuilt != null) _lastCtx.OnBuilt.Invoke(CreateGui(_lastCtx));
                    })
                    .Build())

                .AddChild(new GraphicalUserInterfaceBuilder("DataHub")
                    .WithPadding(15).WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                    .WithFlexGrow(1)
                    .OnBuild(ve => {
                        _crudArea = new VisualElement { style = { flexGrow = 1 } };
                        _crudArea.Add(BuildContinentCRUD());
                        ve.Add(_crudArea);
                    })
                    .Build())
            );

            splitPanel.WithMain(new GraphicalUserInterfaceBuilder("PlanetViewport")
                .WithBackgroundColor(Color.black)
                .WithFlexGrow(1)
                .OnBuild(ve => {
                    _previewContainer = ve;
                    RefreshPreviewViewport();
                })
            );

            var root = splitPanel.CreateGui(ctx);

            root.schedule.Execute(() => {
                FSM_API.Interaction.Update("WorldBuilder");
            }).Every(100);

            return root;
        }

        private void InitializeFSM()
        {
            if (!FSM_API.Interaction.Exists("WorldBuilderFSM"))
            {
                FSM_API.Create.CreateFiniteStateMachine("WorldBuilderFSM", -1, "WorldBuilder")
                    .State("Idle", null, null, null)
                    .State("Rebuilding", null, RebuildPlanetMesh, null)
                    .Transition("Idle", "Rebuilding", ctx => ((WorldBuilderContext)ctx).NeedsMeshRebuild)
                    .Transition("Rebuilding", "Idle", ctx => !((WorldBuilderContext)ctx).NeedsMeshRebuild)
                    .BuildDefinition();
            }

            FSM_API.Create.CreateInstance("WorldBuilderFSM", _simContext, "WorldBuilder");
        }

        private void RebuildPlanetMesh(IStateContext ctx)
        {
            var simCtx = (WorldBuilderContext)ctx;

            if (simCtx.GeneratedPlanetPrefab != null) UnityEngine.Object.DestroyImmediate(simCtx.GeneratedPlanetPrefab);

            Mesh rawMesh = IcosphereGenerator.Create(3, 1f);
            Vector3[] verts = rawMesh.vertices;
            int[] tris = rawMesh.triangles;
            int totalFaces = tris.Length / 3;

            // REFACTORED: Removed the .Data layer to work directly with the context wrapper
            if (simCtx.Continents.Count == 0)
            {
                System.Random rng = new System.Random(simCtx.Seed.GetHashCode());
                int numPlates = rng.Next(8, 16);
                float baseMass = 100f / numPlates;

                for (int i = 0; i < numPlates; i++)
                {
                    simCtx.Continents.Add(new ContinentData
                    {
                        Name = $"Plate {i + 1}",
                        MassPercentage = baseMass,
                        PlateColor = new Color((float)rng.NextDouble() * 0.8f + 0.2f, (float)rng.NextDouble() * 0.8f + 0.2f, (float)rng.NextDouble() * 0.8f + 0.2f, 1f),
                        AnchorFaceIndex = rng.Next(0, totalFaces),
                        AnchorBarycentric = new Vector3(0.333f, 0.333f, 0.333f)
                    });
                }
            }

            rawMesh = ApplyFlatShading(rawMesh);
            simCtx.PlanetMesh = rawMesh;

            simCtx.GeneratedPlanetPrefab = new GameObject($"Temp_World_Seed_{simCtx.Seed}");
            simCtx.GeneratedPlanetPrefab.hideFlags = HideFlags.HideAndDontSave;

            var meshFilter = simCtx.GeneratedPlanetPrefab.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = rawMesh;

            var renderer = simCtx.GeneratedPlanetPrefab.AddComponent<MeshRenderer>();
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material planetMat = new Material(urpShader);
            planetMat.color = new Color(0.15f, 0.15f, 0.17f);
            renderer.sharedMaterial = planetMat;

            simCtx.CentroidMarkers.Clear();
            foreach (var plate in simCtx.Continents)
            {
                Vector3 surfacePos = plate.GetWorldPosition(verts, tris);

                GameObject centroidMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                centroidMarker.name = $"Centroid_{plate.Name}";
                centroidMarker.transform.SetParent(simCtx.GeneratedPlanetPrefab.transform, false);
                centroidMarker.transform.localPosition = surfacePos * 1.02f;

                UnityEngine.Object.DestroyImmediate(centroidMarker.GetComponent<Collider>());

                Material markerMat = new Material(urpShader);
                if (markerMat.HasProperty("_BaseColor")) markerMat.SetColor("_BaseColor", plate.PlateColor);
                else markerMat.color = plate.PlateColor;
                centroidMarker.GetComponent<MeshRenderer>().sharedMaterial = markerMat;

                simCtx.CentroidMarkers.Add(plate.Id, centroidMarker.transform);
            }

            simCtx.UpdateCentroidVisuals();
            RefreshPreviewViewport();

            simCtx.NeedsMeshRebuild = false;

            if (_crudArea != null) { _crudArea.Clear(); _crudArea.Add(BuildContinentCRUD()); }
        }

        private Mesh ApplyFlatShading(Mesh smoothMesh)
        {
            Vector3[] oldVerts = smoothMesh.vertices;
            int[] triangles = smoothMesh.triangles;
            Vector3[] flatVerts = new Vector3[triangles.Length];
            int[] flatTriangles = new int[triangles.Length];

            for (int i = 0; i < triangles.Length; i++)
            {
                flatVerts[i] = oldVerts[triangles[i]];
                flatTriangles[i] = i;
            }

            Mesh flatMesh = new Mesh();
            flatMesh.name = smoothMesh.name + "_Flat";
            flatMesh.vertices = flatVerts;
            flatMesh.triangles = flatTriangles;
            flatMesh.RecalculateNormals();
            return flatMesh;
        }

        private void RefreshPreviewViewport()
        {
            if (_previewContainer == null) return;
            _previewContainer.Clear();

            if (_simContext.GeneratedPlanetPrefab == null) return;

            var previewBuilder = new LiveModelPreviewBuilder(_simContext.GeneratedPlanetPrefab);

            _previewContainer.Add(new GraphicalUserInterfaceBuilder("PreviewWrapper")
                .WithFlexGrow(1).WithFlexShrink(1)
                .AddChild(previewBuilder)
                .Build());
        }

        private VisualElement BuildContinentCRUD()
        {
            var crud = new CRUD_Builder<ContinentData>(
                "PLATES",
                () => _simContext.Continents,
                c => $"{(c.IsLocked ? "🔒 " : "")}{c.Name}",
                c => new GraphicalUserInterfaceBuilder("ContinentForm")
                        .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                        .AddStringData("Name", c.Name, v => c.Name = v)

                        // REFACTORED: Using Builder for the Color Box
                        .AddChild(new GraphicalUserInterfaceBuilder("ColorBox")
                            .WithHeight(20)
                            .WithBackgroundColor(c.PlateColor)
                            .WithMarginTop(10).WithMarginBottom(10)
                            .WithBorderRadius(4)
                            .WithBorderAllColor(Color.white)
                            .WithBorderWidth(1))

                        // REFACTORED: ForgeLabelBuilder
                        .AddChild(new ForgeLabelBuilder("Mass Distribution")
                            .WithColor(Color.yellow)
                            .WithMarginTop(5)
                            .WithMarginBottom(5))

                        .AddToggleData("Lock Mass", c.IsLocked, v => c.IsLocked = v)

                        // REFACTORED: Keeping the logic tight, but removing raw Label instantiation
                        .AddChild(guiCtx => {
                            var container = new VisualElement();
                            var labelWrapper = new ForgeLabelBuilder($"{c.MassPercentage:F1}% of Surface")
                                .WithColor(Color.white)
                                .WithBold()
                                .WithMarginTop(5)
                                .Build() as Label;

                            var massSlider = new Slider(0f, 100f) { value = c.MassPercentage };

                            massSlider.RegisterValueChangedCallback(evt => {
                                if (c.IsLocked) { massSlider.SetValueWithoutNotify(evt.previousValue); return; }

                                _simContext.BalanceMasses(c, evt.newValue);
                                if (labelWrapper != null) labelWrapper.text = $"{c.MassPercentage:F1}% of Surface";
                            });

                            container.Add(labelWrapper);
                            container.Add(massSlider);
                            return container;
                        })
                        .Build(),
                c => { if (!_simContext.Continents.Contains(c)) _simContext.Continents.Add(c); },
                c => { _simContext.Continents.Remove(c); }
            );

            crud.Style.ListWidth = 130f;
            crud.Style.AccentColor = new Color(0.7f, 0.4f, 0.1f);

            return crud.CreateGui(_lastCtx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}