using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

using Workshop.Asteroids; // For AsteroidsContext

namespace Assets.Scripts.SolarSystem
{
    public class SolarSystem_Gui_Explorer : IGuiProvider, IDisposable
    {
        // This prefix ensures it gets its own category foldout in your Forge Hub
        public string Title => "SOLAR SYSTEM: EXPLORER";

        private GuiContext _guiContext;
        private AsteroidsContext _asteroidsContext;

        private VisualElement _previewContainer;
        private LiveModelPreviewBuilder _lmpBuilder;

        // 3D Scene state
        private GameObject _dioramaRoot;
        private GameObject _activeFighterInstance;
        private string _focusedPlanet = "Earth";

        // Hardcoded list for the UI, but you can drive this from a SolarSystemContext later
        private readonly List<string> _planets = new List<string>
        {
            "Sun", "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune"
        };

        public SolarSystem_Gui_Explorer()
        {
            _asteroidsContext = new AsteroidsContext();
        }

      
        public SolarSystem_Gui_Explorer(AsteroidsContext asteroidsContext)
        {
            _asteroidsContext = asteroidsContext ?? new AsteroidsContext();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _guiContext = ctx;

            var rootBuilder = new GraphicalUserInterfaceBuilder("SolarSystemRoot")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch)
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.05f)); // Deep space background

            // 1. LEFT PANE: Planet Selection
            rootBuilder.AddChild(BuildPlanetSelector());

            // 2. CENTER PANE: 3D Viewport
            rootBuilder.AddChild(new GraphicalUserInterfaceBuilder("ViewportContainer")
                .WithFlexGrow(1)
                .WithBackgroundColor(Color.black)
                .OnBuild(ve => {
                    ve.RegisterCallback<GeometryChangedEvent>(evt => {
                        // Keep viewport somewhat square or handle resize
                        float h = evt.newRect.height;
                        if (Mathf.Abs(ve.style.width.value.value - h) > 1f) ve.style.width = h;
                    });
                    _previewContainer = ve;
                    RefreshDiorama();
                })
            );

            // 3. RIGHT PANE: Asteroids Fighter Selection
            rootBuilder.AddChild(BuildFighterSelector());

            var root = rootBuilder.Build();

            // Lifecycle hook to ensure we clean up the LiveModelPreviewBuilder and Cameras
            root.RegisterCallback<DetachFromPanelEvent>(evt => Dispose());

            return root;
        }

        private VisualElement BuildPlanetSelector()
        {
            var panel = new GraphicalUserInterfaceBuilder("PlanetPanel")
                .WithWidth(250).WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderRightWidth(2).WithBorderRightColor(new Color(0.2f, 0.6f, 0.8f))
                .WithPadding(15)
                .AddChild(new Label("CELESTIAL BODIES") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10, letterSpacing = 2 } })
                .Build();

            var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };

            // 1. Existing Planet Tabs
            foreach (var planet in _planets)
            {
                bool isSelected = _focusedPlanet == planet;
                var btn = new Button(() => {
                    _focusedPlanet = planet;
                    RefreshDiorama();
                })
                {
                    text = planet.ToUpper(),
                    style = {
                        unityTextAlign = TextAnchor.MiddleLeft, height = 30,
                        backgroundColor = isSelected ? new Color(0.2f, 0.6f, 0.8f) : Color.clear,
                        color = isSelected ? Color.black : Color.white,
                        borderLeftWidth = isSelected ? 4 : 0, borderLeftColor = Color.white
                    }
                };
                scroll.Add(btn);
            }

            // Visual Divider
            scroll.Add(new VisualElement { style = { height = 2, backgroundColor = new Color(0.3f, 0.3f, 0.4f), marginTop = 15, marginBottom = 15 } });
            scroll.Add(new Label("SIMULATION MODES") { style = { color = new Color(0.8f, 0.8f, 0.2f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10, letterSpacing = 1 } });

            // 2. ORBIT SIMULATION BUTTON
            var orbitBtn = new Button(() => ActivateOrbitSimulation())
            {
                text = "▶ RUN ORBIT SIM",
                style = { height = 35, backgroundColor = new Color(0.2f, 0.5f, 0.3f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold }
            };

            // 3. FREE FLY BUTTON
            var freeFlyBtn = new Button(() => ActivateFreeFlyMode())
            {
                text = "🚀 FREE FLY MODE",
                style = { height = 35, backgroundColor = new Color(0.6f, 0.3f, 0.6f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 5 }
            };

            scroll.Add(orbitBtn);
            scroll.Add(freeFlyBtn);

            panel.Add(scroll);
            return panel;
        }

        private void ActivateOrbitSimulation()
        {
            _focusedPlanet = "FULL_SYSTEM"; // Create a special case in RefreshDiorama to spawn everything
            RefreshDiorama();
            // TODO: Trigger your global FSM to start orbiting the planets around the sun
            Debug.Log("[SolarSystem] Orbit Simulation Activated!");
        }

        private void ActivateFreeFlyMode()
        {
            // TODO: Swap out the LiveModelPreviewBuilder's camera controller for your FirstPersonController or Drone Controller
            Debug.Log("[SolarSystem] Free Fly Mode Activated! (Camera unbound)");
        }

        private VisualElement BuildFighterSelector()
        {
            var panel = new GraphicalUserInterfaceBuilder("FighterPanel")
                .WithWidth(300).WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderLeftWidth(2).WithBorderLeftColor(new Color(0.8f, 0.4f, 0.1f)) // Asteroids orange/amber accent
                .WithPadding(15)
                .AddChild(new Label("HANGAR: SELECT FIGHTER") { style = { color = new Color(0.8f, 0.4f, 0.1f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15, letterSpacing = 2 } })
                .Build();

            var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };

            if (_asteroidsContext != null && _asteroidsContext.fighterPartPrefabs != null)
            {
                foreach (var shipPrefab in _asteroidsContext.fighterPartPrefabs)
                {
                    if (shipPrefab == null) continue;

                    var card = new VisualElement { style = { flexDirection = FlexDirection.Row, backgroundColor = new Color(0.12f, 0.12f, 0.15f), marginBottom = 10, paddingLeft = 10, paddingTop = 10, paddingBottom = 10, alignItems = Align.Center } };

                    card.Add(new Label(shipPrefab.name) { style = { color = Color.white, flexGrow = 1, unityFontStyleAndWeight = FontStyle.Bold } });

                    var deployBtn = new Button(() => DeployFighter(shipPrefab))
                    {
                        text = "DEPLOY",
                        style = { backgroundColor = new Color(0.8f, 0.4f, 0.1f), color = Color.black, unityFontStyleAndWeight = FontStyle.Bold }
                    };
                    card.Add(deployBtn);

                    scroll.Add(card);
                }
            }
            else
            {
                scroll.Add(new Label("No Asteroids fighters found in context.") { style = { color = Color.gray, whiteSpace = WhiteSpace.Normal } });
            }

            panel.Add(scroll);
            return panel;
        }
        private void FrameCameraOnBounds(GameObject targetObject, Camera previewCamera)
        {
            Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            // 1. Calculate the encapsulating bounds of all child renderers
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            // 2. Use trigonometry to find the perfect camera distance based on the Field of View
            float maxExtent = bounds.extents.magnitude;
            float minDistance = maxExtent / Mathf.Tan(previewCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

            // 3. Position the camera slightly further back (1.2f padding) and look at the center
            previewCamera.transform.position = bounds.center - (previewCamera.transform.forward * (minDistance * 1.2f));
            previewCamera.transform.LookAt(bounds.center);
        }
        private void DeployFighter(GameObject fighterPrefab)
        {
            if (_dioramaRoot == null) return;

            // Remove existing fighter if one is deployed
            if (_activeFighterInstance != null)
            {
                UnityEngine.Object.DestroyImmediate(_activeFighterInstance);
            }

            // Spawn the new fighter
            _activeFighterInstance = UnityEngine.Object.Instantiate(fighterPrefab, _dioramaRoot.transform);

            // Offset it slightly so it appears to be orbiting or flying near the planet
            _activeFighterInstance.transform.localPosition = new Vector3(2f, 1f, -2f);
            _activeFighterInstance.transform.localRotation = Quaternion.Euler(0, -45, 0);

            // Re-render the viewport so the camera picks up the new object
            RefreshViewport();
        }

        private void RefreshDiorama()
        {
            // 1. Clean up old diorama
            if (_dioramaRoot != null) UnityEngine.Object.DestroyImmediate(_dioramaRoot);

            // 2. Create new diorama root
            _dioramaRoot = new GameObject($"SolarSystem_Diorama_{_focusedPlanet}");
            _dioramaRoot.hideFlags = HideFlags.HideAndDontSave;

            // 3. Route to the correct generator
            if (_focusedPlanet == "Asteroid Belt")
            {
                GenerateAsteroidBelt();
            }
            else
            {
                GeneratePlanet();
            }

            // 4. Basic lighting to illuminate the materials
            Light sunLight = _dioramaRoot.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(1f, 0.95f, 0.9f);
            sunLight.intensity = 1.5f;
            sunLight.transform.rotation = Quaternion.Euler(30, -30, 0);

            RefreshViewport();
        }

        private void GeneratePlanet()
        {
            GameObject planetObj = null;

#if UNITY_EDITOR
            // Attempt to load the asset pack prefab
            string prefabPath = $"Assets/Planets of the Solar System 3D/Prefabs/{_focusedPlanet}.prefab";
            GameObject planetPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (planetPrefab != null)
            {
                planetObj = UnityEngine.Object.Instantiate(planetPrefab, _dioramaRoot.transform);

                // Attach the FSM Rotator
                var spinner = planetObj.AddComponent<PlanetSpinBehavior>();
                spinner.Speed = 5.0f;
            }
            else
            {
                Debug.LogWarning($"[The Forge] Could not locate planet prefab at: {prefabPath}");
            }
#endif

            // Fallback
            if (planetObj == null)
            {
                planetObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                planetObj.transform.SetParent(_dioramaRoot.transform, false);
                planetObj.transform.localScale = Vector3.one * 5f;
                UnityEngine.Object.DestroyImmediate(planetObj.GetComponent<Collider>());
            }
        }

        private void GenerateAsteroidBelt()
        {
#if UNITY_EDITOR
            string folderPath = "Assets/BreakableAsteroids/Asteroids";

            // Find all GameObjects in the target folder
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:GameObject", new[] { folderPath });
            List<GameObject> asteroidPrefabs = new List<GameObject>();

            foreach (var guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

                // Exclude the fractured prefabs so we only get the whole rocks
                if (!path.ToLower().Contains("fractured"))
                {
                    var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null) asteroidPrefabs.Add(prefab);
                }
            }

            if (asteroidPrefabs.Count > 0)
            {
                // Spawn a dense ring of asteroids
                int asteroidCount = 150;
                for (int i = 0; i < asteroidCount; i++)
                {
                    GameObject randomPrefab = asteroidPrefabs[UnityEngine.Random.Range(0, asteroidPrefabs.Count)];
                    GameObject inst = UnityEngine.Object.Instantiate(randomPrefab, _dioramaRoot.transform);

                    // Position in a circular belt
                    float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                    float radius = UnityEngine.Random.Range(15f, 25f); // Belt thickness
                    float yOffset = UnityEngine.Random.Range(-3f, 3f); // Vertical variance

                    inst.transform.localPosition = new Vector3(Mathf.Cos(angle) * radius, yOffset, Mathf.Sin(angle) * radius);

                    // Random scale and rotation
                    float scale = UnityEngine.Random.Range(0.5f, 3.0f);
                    inst.transform.localScale = Vector3.one * scale;
                    inst.transform.localRotation = UnityEngine.Random.rotation;

                    // Make every single asteroid tumble on its own unique axis
                    var spinner = inst.AddComponent<PlanetSpinBehavior>();
                    spinner.Speed = UnityEngine.Random.Range(2f, 15f);
                    spinner.Axis = UnityEngine.Random.onUnitSphere; // Random tumble axis
                }
            }
            else
            {
                Debug.LogWarning($"[The Forge] No asteroid prefabs found in {folderPath}");
                GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                placeholder.transform.SetParent(_dioramaRoot.transform, false);
            }
#endif
        }

        private void RefreshViewport()
        {
            if (_previewContainer == null || _dioramaRoot == null) return;

            // Dispose old previewer so we don't leak render textures or cameras
            _lmpBuilder?.Dispose();
            _previewContainer.Clear();

            // Relational Staging Pipeline Integration
            _lmpBuilder = new LiveModelPreviewBuilder(_dioramaRoot)
                .WithBackgroundColor(new Color(0.01f, 0.01f, 0.02f)) // Deep space
                .WithMouseControl(true);

            _previewContainer.Add(new GraphicalUserInterfaceBuilder("PreviewWrapper")
                .WithFlexGrow(1).WithFlexShrink(1)
                .AddChild(_lmpBuilder)
                .Build());
        }

        public void Dispose()
        {
            _lmpBuilder?.Dispose();
            if (_dioramaRoot != null) UnityEngine.Object.DestroyImmediate(_dioramaRoot);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}