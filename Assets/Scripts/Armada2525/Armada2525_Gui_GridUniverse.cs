using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Armada2525
{
    public class Armada2525_Gui_GridUniverse : IGuiProvider
    {
        public string Title => "ARMADA 2525: SECTOR COMMAND";

        private ArmadaGalaxyContext _galaxyContext;
        private VisualElement _viewportContainer;
        private GameObject _tacticalMapRoot;

        // Constructor injection from your Bootloader Router!
        public Armada2525_Gui_GridUniverse(ArmadaGalaxyContext galaxyContext)
        {
            _galaxyContext = galaxyContext;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Destroy any previous tactical map roots to prevent stacking
            var oldRoot = GameObject.Find("Armada_Tactical_Root");
            if (oldRoot != null) UnityEngine.Object.DestroyImmediate(oldRoot);

            _tacticalMapRoot = new GameObject("Armada_Tactical_Root");
            _tacticalMapRoot.hideFlags = HideFlags.HideAndDontSave;

            return new ForgeSplitPanelBuilder(sidebarWidth: 350, Side.Left)
                .WithSidebar(BuildEmpireDashboard(ctx))
                .WithMain(BuildTacticalViewport(ctx))
                .CreateGui(ctx);
        }

        private IGuiProvider BuildEmpireDashboard(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("EmpireDash")
                .WithPadding(15)
                .WithBackgroundColor(new Color(0.05f, 0.08f, 0.12f)) // Deep Navy Blue

                .AddHeader($"SECTOR: {_galaxyContext.Name.ToUpper()}")
                .AddSeparator(new Color(0.2f, 0.6f, 0.9f), 2)

                .AddChild(new Label("GALACTIC TELEMETRY") { style = { color = new Color(0.5f, 0.5f, 0.7f), fontSize = 11, letterSpacing = 2, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 10 } })

                .AddChild(new Label($"Universal Seed: {_galaxyContext.UniversalSeed}") { style = { color = Color.white, marginBottom = 5 } })
                .AddChild(new Label($"Known Systems: {_galaxyContext.StarSystems.Count}") { style = { color = Color.white, marginBottom = 5 } })

                .AddSeparator(new Color(0.3f, 0.3f, 0.3f), 1)

                .AddChild(new Label("SYSTEMS DETECTED") { style = { color = new Color(0.5f, 0.5f, 0.7f), fontSize = 11, letterSpacing = 2, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 10 } })

                // Read the actual generated systems and create a quick list!
                .OnBuild(ve =>
                {
                    var scroll = new ScrollView { style = { flexGrow = 1, marginTop = 5, marginBottom = 10 } };

                    foreach (var kvp in _galaxyContext.StarSystems)
                    {
                        var sys = kvp.Value;
                        string indicator = sys.IsHomeworld ? "<color=#FFAA00>[CAPITAL]</color> " : "";
                        var lbl = new Label($"{indicator}{sys.Name} ({sys.Planets.Count} planets)")
                        {
                            style = { color = sys.IsHomeworld ? new Color(1f, 0.8f, 0.2f) : new Color(0.7f, 0.8f, 0.9f), marginBottom = 3 }
                        };
                        scroll.Add(lbl);
                    }

                    ve.Add(scroll);
                });
        }

        private IGuiProvider BuildTacticalViewport(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("TacticalViewport")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.01f, 0.01f, 0.02f)) // Pure Space Black
                .OnBuild(ve =>
                {
                    _viewportContainer = ve;

                    // Build the 3D representation of the generated grid!
                    ConstructTacticalGrid3D();

                    // Inject the standard LiveModelPreviewBuilder so the user can pan/zoom around the galaxy
                    var previewBuilder = new LiveModelPreviewBuilder(_tacticalMapRoot)
                        .WithBackgroundColor(new Color(0.01f, 0.01f, 0.02f))
                        .WithMouseControl(true)
                        .WithGizmos(false); // Turn off the center grid

                    ve.Add(previewBuilder.CreateGui(ctx));
                });
        }

        private void ConstructTacticalGrid3D()
        {
            // Standard Unlit material for glowing stars
            Material starMat = new Material(Shader.Find("Sprites/Default"));

            foreach (var kvp in _galaxyContext.StarSystems)
            {
                var sys = kvp.Value;

                // Create a basic sphere to represent the star system
                var starObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                starObj.name = $"Star_{sys.Name}";
                starObj.transform.SetParent(_tacticalMapRoot.transform);

                // Spread them out using their generated GridCoordinates
                starObj.transform.localPosition = sys.GridCoordinate;

                // Scale up homeworlds so they are obvious
                starObj.transform.localScale = sys.IsHomeworld ? Vector3.one * 3f : Vector3.one * 1.5f;

                // Colorize based on Homeworld status
                var renderer = starObj.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = new Material(starMat);
                renderer.sharedMaterial.color = sys.IsHomeworld ? new Color(1f, 0.6f, 0f) : new Color(0.5f, 0.8f, 1f);

                // Remove the collider so it doesn't mess with the UI preview raycasting
                UnityEngine.Object.DestroyImmediate(starObj.GetComponent<Collider>());
            }
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}