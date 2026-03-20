using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders.WorldBuilding
{
    public class GameMastersCompanion_Gui_WorldBuilder : IGuiProvider
    {
        public string Title => "GM COMPANION: TECTONIC WORLD FORGE";
        private GuiContext _lastCtx;

        private WorldBuilderContext _simContext;
        private Label _yearLabel;
        private TextField _seedField;
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

            var splitPanel = new SplitPanelBuilder(sidebarWidth: 450);

            splitPanel.WithSidebar(new GraphicalUserInterfaceBuilder("WorldBuilder_Sidebar")
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)

                .AddChild(new GraphicalUserInterfaceBuilder("SimControls")
                    .WithPadding(15).WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                    .WithBorderBottomWidth(2).WithBorderBottomColor(new Color(0.2f, 0.6f, 0.8f))
                    .AddChild(new Label("1. PLANETARY SEED") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } })
                    .AddChild(guiCtx => {
                        _seedField = new TextField("Core Seed") { value = _simContext.Data.Seed };
                        _seedField.RegisterValueChangedCallback(evt => {
                            _simContext.Data.Seed = evt.newValue;
                            _simContext.Data.Continents.Clear();
                            _simContext.NeedsMeshRebuild = true;
                        });
                        return _seedField;
                    })
                    .AddButton("🎲 RANDOMIZE SEED", () => {
                        _simContext.Data.Seed = $"Planet_{UnityEngine.Random.Range(1000, 99999)}";
                        if (_seedField != null) _seedField.value = _simContext.Data.Seed;
                        _simContext.Data.Continents.Clear();
                        _simContext.NeedsMeshRebuild = true;
                    })
                    .Build())

                .AddChild(new GraphicalUserInterfaceBuilder("DataHub")
                    .WithPadding(15).WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                    .OnBuild(ve => {
                        ve.style.flexGrow = 1;

                        _crudArea = new VisualElement { style = { flexGrow = 1 } };
                        _crudArea.Add(BuildContinentCRUD());
                        ve.Add(_crudArea);
                    })
                    .Build())
            );

            splitPanel.WithMain(new GraphicalUserInterfaceBuilder("PlanetViewport")
                .WithBackgroundColor(Color.black)
                .OnBuild(ve => {
                    ve.style.flexGrow = 1;
                    _previewContainer = ve;
                    RefreshPreviewViewport();
                })
            );

            var root = splitPanel.CreateGui(ctx);

            root.schedule.Execute(() => {
                FSM_API.FSM_API.Interaction.Update("WorldBuilder");
            }).Every(100);

            return root;
        }

        private void InitializeFSM()
        {
            if ( !FSM_API.FSM_API.Interaction.Exists("WorldBuilderFSM"))
            {
                FSM_API.FSM_API.Create.CreateFiniteStateMachine("WorldBuilderFSM", -1, "WorldBuilder")
                    .State("Idle", null, null, null)
                    .State("Rebuilding", null, RebuildPlanetMesh, null)
                    .Transition("Idle", "Rebuilding", ctx => ((WorldBuilderContext)ctx).NeedsMeshRebuild)
                    .Transition("Rebuilding", "Idle", ctx => !((WorldBuilderContext)ctx).NeedsMeshRebuild)
                    .BuildDefinition();
            }

            FSM_API.FSM_API.Create.CreateInstance("WorldBuilderFSM", _simContext, "WorldBuilder");
        }

        private void RebuildPlanetMesh(IStateContext ctx)
        {
            var simCtx = (WorldBuilderContext)ctx;

            if (simCtx.GeneratedPlanetPrefab != null) UnityEngine.Object.DestroyImmediate(simCtx.GeneratedPlanetPrefab);

            // Fetch Base Geometry (Level 3 is a great balance of detail and performance)
            Mesh rawMesh = IcosphereGenerator.Create(3, 1f);
            Vector3[] verts = rawMesh.vertices;
            int[] tris = rawMesh.triangles;
            int totalFaces = tris.Length / 3;

            // Seed the Plates if starting fresh
            if (simCtx.Data.Continents.Count == 0)
            {
                System.Random rng = new System.Random(simCtx.Data.Seed.GetHashCode());
                int numPlates = rng.Next(8, 16);
                float baseMass = 100f / numPlates;

                for (int i = 0; i < numPlates; i++)
                {
                    simCtx.Data.Continents.Add(new ContinentData
                    {
                        Name = $"Plate {i + 1}", // Shortened name for UI
                        MassPercentage = baseMass,
                        PlateColor = new Color((float)rng.NextDouble() * 0.8f + 0.2f, (float)rng.NextDouble() * 0.8f + 0.2f, (float)rng.NextDouble() * 0.8f + 0.2f, 1f),
                        AnchorFaceIndex = rng.Next(0, totalFaces), // ZERO-DRIFT ANCHOR
                        AnchorBarycentric = new Vector3(0.333f, 0.333f, 0.333f)
                    });
                }
            }

            // Assemble Base Planet Mesh
            rawMesh = ApplyFlatShading(rawMesh);
            simCtx.Data.PlanetMesh = rawMesh;

            simCtx.GeneratedPlanetPrefab = new GameObject($"Temp_World_Seed_{simCtx.Data.Seed}");
            simCtx.GeneratedPlanetPrefab.hideFlags = HideFlags.HideAndDontSave;

            var meshFilter = simCtx.GeneratedPlanetPrefab.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = rawMesh;

            var renderer = simCtx.GeneratedPlanetPrefab.AddComponent<MeshRenderer>();
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material planetMat = new Material(urpShader);
            planetMat.color = new Color(0.15f, 0.15f, 0.17f); // Dark crust
            renderer.sharedMaterial = planetMat;

            // Spawn the Centroid Spheres and Cache Them
            simCtx.CentroidMarkers.Clear();
            foreach (var plate in simCtx.Data.Continents)
            {
                Vector3 surfacePos = plate.GetWorldPosition(verts, tris);

                GameObject centroidMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                centroidMarker.name = $"Centroid_{plate.Name}";
                centroidMarker.transform.SetParent(simCtx.GeneratedPlanetPrefab.transform, false);

                // Push perfectly above the crust so it's visible, regardless of mesh size
                centroidMarker.transform.localPosition = surfacePos * 1.02f;

                UnityEngine.Object.DestroyImmediate(centroidMarker.GetComponent<Collider>());

                Material markerMat = new Material(urpShader);
                if (markerMat.HasProperty("_BaseColor")) markerMat.SetColor("_BaseColor", plate.PlateColor);
                else markerMat.color = plate.PlateColor;
                centroidMarker.GetComponent<MeshRenderer>().sharedMaterial = markerMat;

                simCtx.CentroidMarkers.Add(plate.Id, centroidMarker.transform);
            }

            // Scale them based on their mass
            simCtx.UpdateCentroidVisuals();
            RefreshPreviewViewport();

            simCtx.NeedsMeshRebuild = false;

            // Refresh CRUD to show newly generated plates
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
                () => _simContext.Data.Continents,
                c => $"{(c.IsLocked ? "🔒 " : "")}{c.Name}", // Simplified label
                c => new GraphicalUserInterfaceBuilder("ContinentForm")
                        .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                        .AddStringData("Name", c.Name, v => c.Name = v)

                        .OnBuild(ve => {
                            var colorBox = new VisualElement
                            {
                                style = { height = 20, backgroundColor = c.PlateColor, marginTop = 10, marginBottom = 10, borderTopLeftRadius = 4, borderBottomLeftRadius = 4, borderBottomRightRadius = 4, borderTopRightRadius = 4,
                                          borderBottomWidth=1, borderTopWidth=1, borderLeftWidth=1, borderRightWidth=1, borderBottomColor=Color.white, borderTopColor=Color.white, borderLeftColor=Color.white, borderRightColor=Color.white }
                            };
                            ve.Add(colorBox);
                        })

                        .AddChild(new Label("Mass Distribution") { style = { color = Color.yellow, marginTop = 5, marginBottom = 5 } })
                        .AddToggleData("Lock Mass", c.IsLocked, v => c.IsLocked = v)

                        // The Linked Slider
                        .OnBuild(ve => {
                            var massLabel = new Label($"{c.MassPercentage:F1}% of Surface") { style = { color = Color.white, marginTop = 5, unityFontStyleAndWeight = FontStyle.Bold } };
                            var massSlider = new Slider(0f, 100f) { value = c.MassPercentage };

                            massSlider.RegisterValueChangedCallback(evt => {
                                if (c.IsLocked) { massSlider.SetValueWithoutNotify(evt.previousValue); return; }

                                _simContext.BalanceMasses(c, evt.newValue);
                                massLabel.text = $"{c.MassPercentage:F1}% of Surface";
                            });

                            ve.Add(massLabel);
                            ve.Add(massSlider);
                        })
                        .Build(),
                c => { if (!_simContext.Data.Continents.Contains(c)) _simContext.Data.Continents.Add(c); },
                c => { _simContext.Data.Continents.Remove(c); }
            );

            // THE FIX: Constrain the left panel so the editor gets breathing room!
            crud.Style.ListWidth = 130f;
            crud.Style.AccentColor = new Color(0.7f, 0.4f, 0.1f);

            return crud.CreateGui(_lastCtx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}