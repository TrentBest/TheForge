using System;
using System.Collections.Generic;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.IO; // Added for DataWarehouse Extensions
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.Memory;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders.WorldBuilding
{
    public class GameMastersCompanion_Gui_StarSystemForge : IGuiProvider, IDisposable
    {
        public string Title => "GM COMPANION: ORBITAL DIORAMA FORGE";
        private GuiContext _lastCtx;

        private StarSystemForgeContext _simContext;
        private TextField _seedField;
        private VisualElement _previewContainer;
        private VisualElement _crudArea;

        // Relational Staging Provider
        private LiveModelPreviewBuilder _lmpBuilder;

        private string _activeSubTool = "Planetary Roster";
        private readonly List<string> _subTools = new List<string> { "Stellar Core", "Planetary Roster", "Active Planet: Tectonics", "Active Planet: Moons" };

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            string cacheKey = "STAR_SYSTEM_FORGE_STATE";

            // --- REHYDRATION STATE ---
            if (ctx.TryGetService<DataWarehouse>(out var dw))
            {
                if (dw.TryRetrieveTemporary(cacheKey, out string json))
                {
                    _simContext = JsonUtility.FromJson<StarSystemForgeContext>(json);
                }
            }

            if (_simContext == null)
            {
                _simContext = new StarSystemForgeContext();
                InitializeFSM();
            }
            else if (!FSM_API.FSM_API.Interaction.Exists("SystemForgeFSM"))
            {
                // Ensure FSM is rebuilt if we rehydrated from a fresh boot
                InitializeFSM();
            }

            var rootBuilder = new GraphicalUserInterfaceBuilder("SystemForgeRoot")
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
                    .AddChild(new Label("1. STAR SYSTEM SEED") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } })
                    .AddChild(guiCtx => {
                        _seedField = new TextField("Galactic Hash") { value = _simContext.System.SystemSeed };
                        _seedField.RegisterValueChangedCallback(evt => {
                            _simContext.System.SystemSeed = evt.newValue;
                            _simContext.System.Planets.Clear();
                            _simContext.NeedsMeshRebuild = true;
                        });
                        return _seedField;
                    })
                    .AddButton("🎲 JUMP TO RANDOM SYSTEM", () => {
                        _simContext.System.SystemSeed = $"System_{UnityEngine.Random.Range(10000, 999999)}";
                        if (_seedField != null) _seedField.value = _simContext.System.SystemSeed;
                        _simContext.System.Planets.Clear();
                        _simContext.NeedsMeshRebuild = true;
                    })
                    .Build())

                .AddChild(new GraphicalUserInterfaceBuilder("ToolSwitcher")
                    .WithPadding(15).WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                    .WithBorderBottomWidth(1).WithBorderBottomColor(Color.gray)
                    .AddChild(new Label("2. SYSTEM INSPECTOR") { style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } })

                    // REFACTOR: Replaced manual visual element with pure GUI builder method
                    .AddDropdownData("Inspect:", _subTools, _subTools.IndexOf(_activeSubTool), evtValue => {
                        _activeSubTool = evtValue;
                        RefreshToolArea();
                    })
                    .Build())

                .AddChild(new GraphicalUserInterfaceBuilder("ActiveCrudArea")
                    .WithFlexGrow(1).WithFlexShrink(1)
                    .OnBuild(ve => {
                        _crudArea = ve;
                        RefreshToolArea();
                    })
                    .Build())
            );

            // --- RIGHT PANEL (Square Viewport) ---
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

            // --- LIFECYCLE HANDSHAKE ---
            root.RegisterCallback<DetachFromPanelEvent>(evt => {
                if (ctx.TryGetService<DataWarehouse>(out var warehouse))
                {
                    warehouse.StoreTemporary(cacheKey, JsonUtility.ToJson(_simContext));
                }
                Dispose(); // Enforces the relational stage cleanup via LMP
            });

            root.schedule.Execute(() => FSM_API.FSM_API.Interaction.Update("SystemForge")).Every(100);
            return root;
        }

        private void RefreshToolArea()
        {
            if (_crudArea == null) return;
            _crudArea.Clear();

            if (_activeSubTool == "Stellar Core") _crudArea.Add(BuildStarEditor());
            else if (_activeSubTool == "Planetary Roster") _crudArea.Add(BuildPlanetCRUD());
            else if (_activeSubTool == "Active Planet: Tectonics") _crudArea.Add(BuildContinentCRUD());
            else if (_activeSubTool == "Active Planet: Moons") _crudArea.Add(BuildMoonCRUD());
        }

        private void InitializeFSM()
        {
            if (!FSM_API.FSM_API.Interaction.Exists("SystemForgeFSM"))
            {
                FSM_API.FSM_API.Create.CreateFiniteStateMachine("SystemForgeFSM", -1, "SystemForge")
                    .State("Idle", null, null, null)
                    .State("Rebuilding", null, RebuildSystemDiorama, null)
                    .Transition("Idle", "Rebuilding", ctx => ((StarSystemForgeContext)ctx).NeedsMeshRebuild)
                    .Transition("Rebuilding", "Idle", ctx => !((StarSystemForgeContext)ctx).NeedsMeshRebuild)
                    .BuildDefinition();
            }

            FSM_API.FSM_API.Create.CreateInstance("SystemForgeFSM", _simContext, "SystemForge");
        }

        private void RebuildSystemDiorama(IStateContext ctx)
        {
            var simCtx = (StarSystemForgeContext)ctx;

            if (simCtx.GeneratedSystemPrefab != null) UnityEngine.Object.DestroyImmediate(simCtx.GeneratedSystemPrefab);

            // 1. GENERATE PLANETS FIRST (Fixes the vanishing bug!)
            if (simCtx.System.Planets.Count == 0)
            {
                System.Random rng = new System.Random(simCtx.System.SystemSeed.GetHashCode());
                int numPlanets = rng.Next(2, 9);
                for (int i = 0; i < numPlanets; i++)
                {
                    simCtx.System.Planets.Add(new PlanetData { Name = $"Planet {i + 1}", OrbitDistance = 20f + (i * 15f) });
                }
            }

            // 2. NOW GET THE ACTIVE PLANET
            PlanetData activePlanet = simCtx.GetActivePlanet();

            GameObject systemRoot = new GameObject($"Temp_System_{simCtx.System.SystemSeed}");
            systemRoot.hideFlags = HideFlags.HideAndDontSave;

            if (activePlanet != null)
            {
                Mesh rawMesh = IcosphereGenerator.Create(3, activePlanet.Radius);
                Vector3[] verts = rawMesh.vertices;
                int[] tris = rawMesh.triangles;
                int totalFaces = tris.Length / 3;

                if (activePlanet.Continents.Count == 0)
                {
                    System.Random rng = new System.Random((simCtx.System.SystemSeed + activePlanet.Id).GetHashCode());
                    int numPlates = rng.Next(6, 12);
                    float baseMass = 100f / numPlates;

                    for (int i = 0; i < numPlates; i++)
                    {
                        activePlanet.Continents.Add(new ContinentData
                        {
                            Name = $"Plate {i + 1}",
                            MassPercentage = baseMass,
                            PlateColor = new Color((float)rng.NextDouble() * 0.8f + 0.2f, (float)rng.NextDouble() * 0.8f + 0.2f, (float)rng.NextDouble() * 0.8f + 0.2f, 1f),
                            AnchorFaceIndex = rng.Next(0, totalFaces)
                        });
                    }
                }

                rawMesh = ApplyFlatShading(rawMesh);
                activePlanet.PlanetMesh = rawMesh;

                GameObject planetObj = new GameObject("Active_Planet");
                planetObj.transform.SetParent(systemRoot.transform, false);
                planetObj.AddComponent<MeshFilter>().sharedMesh = rawMesh;

                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                Material planetMat = new Material(urpShader);
                planetMat.color = new Color(0.15f, 0.15f, 0.17f);
                planetObj.AddComponent<MeshRenderer>().sharedMaterial = planetMat;

                simCtx.CentroidMarkers.Clear();
                foreach (var plate in activePlanet.Continents)
                {
                    Vector3 surfacePos = plate.GetWorldPosition(verts, tris);
                    GameObject centroidMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    centroidMarker.transform.SetParent(planetObj.transform, false);
                    centroidMarker.transform.localPosition = surfacePos * 1.02f;
                    UnityEngine.Object.DestroyImmediate(centroidMarker.GetComponent<Collider>());

                    Material markerMat = new Material(urpShader);
                    markerMat.color = plate.PlateColor;
                    centroidMarker.GetComponent<MeshRenderer>().sharedMaterial = markerMat;
                    simCtx.CentroidMarkers.Add(plate.Id, centroidMarker.transform);
                }
                simCtx.UpdateCentroidVisuals(activePlanet);

                foreach (var moon in activePlanet.Moons)
                {
                    GameObject moonObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    moonObj.transform.SetParent(systemRoot.transform, false);
                    moonObj.transform.localPosition = new Vector3(moon.OrbitDistance, 0, 0);
                    moonObj.transform.localScale = Vector3.one * (moon.Radius * 2f);
                    UnityEngine.Object.DestroyImmediate(moonObj.GetComponent<Collider>());

                    Material moonMat = new Material(urpShader);
                    moonMat.color = Color.gray;
                    moonObj.GetComponent<MeshRenderer>().sharedMaterial = moonMat;
                }
            }

            GameObject starObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            starObj.transform.SetParent(systemRoot.transform, false);
            starObj.transform.localPosition = new Vector3(0, 0, simCtx.System.LocalStar.Radius * 5f);
            starObj.transform.localScale = Vector3.one * simCtx.System.LocalStar.Radius;
            UnityEngine.Object.DestroyImmediate(starObj.GetComponent<Collider>());

            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            Material starMat = new Material(unlitShader);
            Color starColor = new Color(1f, 0.95f, 0.8f);
            starMat.color = starColor;
            starObj.GetComponent<MeshRenderer>().sharedMaterial = starMat;

            Light sunLight = starObj.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = starColor;
            sunLight.intensity = simCtx.System.LocalStar.LightIntensity;

            if (activePlanet != null) sunLight.transform.LookAt(Vector3.zero);

            simCtx.GeneratedSystemPrefab = systemRoot;
            RefreshPreviewViewport();
            simCtx.NeedsMeshRebuild = false;
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
            flatMesh.vertices = flatVerts;
            flatMesh.triangles = flatTriangles;
            flatMesh.RecalculateNormals();
            return flatMesh;
        }

        private void RefreshPreviewViewport()
        {
            if (_previewContainer == null || _simContext.GeneratedSystemPrefab == null) return;

            // Explicitly dispose old LMP FSMs and Cameras before clearing visuals
            _lmpBuilder?.Dispose();
            _previewContainer.Clear();

            // Relational Staging Pipeline Integration
            _lmpBuilder = new LiveModelPreviewBuilder(_simContext.GeneratedSystemPrefab)
                .WithBackgroundColor(Color.black)
                .WithMouseControl(true);

            _previewContainer.Add(new GraphicalUserInterfaceBuilder("PreviewWrapper")
                .WithFlexGrow(1).WithFlexShrink(1)
                .AddChild(_lmpBuilder)
                .Build());
        }

        // --- SUB-EDITORS ---

        private VisualElement BuildStarEditor()
        {
            return new GraphicalUserInterfaceBuilder("StarEditor")
                .WithPadding(20).WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .AddStringData("Star Name", _simContext.System.LocalStar.Name, v => _simContext.System.LocalStar.Name = v)
                .AddSeparator()
                .AddSliderData("Visual Radius", 5f, 50f, _simContext.System.LocalStar.Radius, v => { _simContext.System.LocalStar.Radius = v; _simContext.NeedsMeshRebuild = true; })
                .AddSliderData("Light Intensity", 0f, 5f, _simContext.System.LocalStar.LightIntensity, v => { _simContext.System.LocalStar.LightIntensity = v; _simContext.NeedsMeshRebuild = true; })
                .Build();
        }

        private VisualElement BuildPlanetCRUD()
        {
            var crud = new CRUD_Builder<PlanetData>("PLANETS", () => _simContext.System.Planets, p => p.Name,
                p => new GraphicalUserInterfaceBuilder("PlanetForm").WithPadding(10)
                        .AddStringData("Planet Name", p.Name, v => p.Name = v)
                        .AddSliderData("Orbit Distance", 10f, 200f, p.OrbitDistance, v => p.OrbitDistance = v)
                        .AddButton(" [ SET DIORAMA FOCUS ] ", () => {
                            _simContext.ActivePlanetIndex = _simContext.System.Planets.IndexOf(p);
                            _simContext.NeedsMeshRebuild = true;
                        })
                        .Build(),
                p => { if (!_simContext.System.Planets.Contains(p)) _simContext.System.Planets.Add(p); _simContext.NeedsMeshRebuild = true; },
                p => { _simContext.System.Planets.Remove(p); _simContext.NeedsMeshRebuild = true; }
            );
            crud.Style.ListWidth = 140f;
            return crud.CreateGui(_lastCtx);
        }

        private VisualElement BuildContinentCRUD()
        {
            PlanetData p = _simContext.GetActivePlanet();
            if (p == null) return new Label("No planet active.");

            var crud = new CRUD_Builder<ContinentData>($"PLATES ({p.Name})", () => p.Continents, c => $"{(c.IsLocked ? "🔒 " : "")}{c.Name}",
                c => new GraphicalUserInterfaceBuilder("ContinentForm").WithPadding(10)
                        .AddStringData("Name", c.Name, v => c.Name = v)
                        .AddToggleData("Lock Mass", c.IsLocked, v => c.IsLocked = v)
                        .OnBuild(ve => {
                            var massLabel = new Label($"{c.MassPercentage:F1}% of Surface") { style = { color = Color.white, marginTop = 5 } };
                            var massSlider = new Slider(0f, 100f) { value = c.MassPercentage };
                            massSlider.RegisterValueChangedCallback(evt => {
                                if (c.IsLocked) { massSlider.SetValueWithoutNotify(evt.previousValue); return; }
                                _simContext.BalanceMasses(c, evt.newValue);
                                massLabel.text = $"{c.MassPercentage:F1}% of Surface";
                            });
                            ve.Add(massLabel); ve.Add(massSlider);
                        }).Build(),
                c => { if (!p.Continents.Contains(c)) p.Continents.Add(c); },
                c => { p.Continents.Remove(c); }
            );
            crud.Style.ListWidth = 140f;
            crud.Style.AccentColor = new Color(0.7f, 0.4f, 0.1f);
            return crud.CreateGui(_lastCtx);
        }

        private VisualElement BuildMoonCRUD()
        {
            PlanetData p = _simContext.GetActivePlanet();
            if (p == null) return new Label("No planet active.");

            var crud = new CRUD_Builder<MoonData>($"MOONS ({p.Name})", () => p.Moons, m => m.Name,
                m => new GraphicalUserInterfaceBuilder("MoonForm").WithPadding(10)
                        .AddStringData("Moon Name", m.Name, v => m.Name = v)
                        .AddSliderData("Radius (Size)", 0.05f, 0.8f, m.Radius, v => { m.Radius = v; _simContext.NeedsMeshRebuild = true; })
                        .AddSliderData("Orbit Distance", 1.5f, 10f, m.OrbitDistance, v => { m.OrbitDistance = v; _simContext.NeedsMeshRebuild = true; })
                        .Build(),
                m => { if (!p.Moons.Contains(m)) p.Moons.Add(m); _simContext.NeedsMeshRebuild = true; },
                m => { p.Moons.Remove(m); _simContext.NeedsMeshRebuild = true; }
            );
            crud.Style.ListWidth = 140f;
            crud.Style.AccentColor = Color.white;
            return crud.CreateGui(_lastCtx);
        }

        public void Dispose()
        {
            // Triggers the cascading cleanup of FSMs, Cameras, and the actual +/- 500m Models
            _lmpBuilder?.Dispose();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}