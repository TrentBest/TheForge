using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API;
using Assets.Scripts.CorsairsInSpace.Data;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Corsair
{
    public class CorsairsInSpace_Gui_LairPlacement : IGuiProvider
    {
        public string Title => "Lair Placement & Surface Scan";

        private readonly IGuiRouter _router;
        private readonly CosmicCellContext _cellContext;

        private float _targetPitch = 0f;
        private float _targetYaw = 0f;

        private VisualElement _mockWireframeOverlay;
        private Label _telemetryLabel;

        public CorsairsInSpace_Gui_LairPlacement(IGuiRouter router, CosmicCellContext cellContext)
        {
            _router = router;
            _cellContext = cellContext;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Fallback validation (Rule #8: Context-First Initialization)
            if (_cellContext == null || !_cellContext.IsValid)
            {
                return new Label("CRITICAL ERROR: No Cosmic Cell Context provided to Placement Engine.") { style = { color = Color.red } };
            }

            var rootBuilder = new GraphicalUserInterfaceBuilder("LairPlacementRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f))
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch);

            // LEFT PANEL: Controls & Telemetry
            var leftPanel = new GraphicalUserInterfaceBuilder("TelemetryPanel")
                .WithWidth(350)
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

            leftPanel.AddChild(new ForgeLabelBuilder("ASTEROID SURFACE SCAN")
                .WithFontSize(22)
                .WithColor(Color.cyan)
                .WithFontStyle(FontStyle.Bold)
                .WithMargin(0, 20, 0, 0));

            // Extract generic dictionary data safely
            string authPresence = _cellContext.CellData.ContainsKey("AuthorityPresence") ? _cellContext.CellData["AuthorityPresence"].ToString() : "Unknown";
            string minerals = _cellContext.CellData.ContainsKey("MineralRichness") ? _cellContext.CellData["MineralRichness"].ToString() : "Unknown";

            leftPanel.AddChild(new ForgeLabelBuilder($"SECTOR: {_cellContext.SectorId} (Heat Mod: {_cellContext.BaseHeatModifier:F2}x)\n" +
                                                     $"DIAMETER: {_cellContext.DiameterMeters}m\n" +
                                                     $"MASS: {_cellContext.MassKilotons} kT\n" +
                                                     $"AUTHORITY: {authPresence}\n" +
                                                     $"MINERALS: {minerals}")
                .WithFontSize(14)
                .WithColor(Color.white)
                .WithWhiteSpace(WhiteSpace.Normal)
                .WithMargin(0, 30, 0, 0));

            leftPanel.AddChild(CreateSliderFloat("SURFACE PITCH", -90f, 90f, _targetPitch, v => _targetPitch = v));
            leftPanel.AddChild(CreateSliderFloat("SURFACE YAW", -180f, 180f, _targetYaw, v => _targetYaw = v));

            // Handoff to Deployment
            leftPanel.AddChild(new ForgeButtonBuilder("LOCK ENTRANCE & DEPLOY SHUTTLE", () =>
            {
                // Inject the chosen placement vector into the Context dict for later FSMs
                _cellContext.CellData["EntrancePitch"] = _targetPitch;
                _cellContext.CellData["EntranceYaw"] = _targetYaw;
                _router.NavigateTo("Deployment");
            }));
          

            rootBuilder.AddChild(leftPanel);

            // RIGHT PANEL: 3D Preview (Meso-Scale)
            var rightPanel = new VisualElement { style = { flexGrow = 1, justifyContent = Justify.Center, alignItems = Align.Center } };

            // Generate a primitive to simulate the asteroid for the LiveModelPreviewBuilder
            var asteroidMock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            asteroidMock.hideFlags = HideFlags.HideAndDontSave;
            asteroidMock.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(0.3f, 0.28f, 0.35f);

            var previewCanvas = new LiveModelPreviewBuilder(asteroidMock)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.05f))
                .WithAutoRotate(false, 0f) // We control rotation manually via sliders
                .WithZoom(3.5f)
                .CreateGui(ctx);

            previewCanvas.style.flexGrow = 1;
            previewCanvas.style.width = Length.Percent(100);

            // The Targeting Reticle Overlay (Wireframe decorator surrogate)
            _mockWireframeOverlay = new Label("⌖")
            {
                style = {
                    position = Position.Absolute,
                    fontSize = 120,
                    color = new Color(0f, 1f, 0f, 0.5f),
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };

            _telemetryLabel = new Label("SCANNING CRUST INTEGRITY...") { style = { position = Position.Absolute, top = 20, right = 20, color = Color.cyan, backgroundColor = new Color(0, 0, 0, 0.5f), paddingBottom = 5, paddingTop = 5, paddingLeft = 10, paddingRight = 10 } };

            previewCanvas.Add(_mockWireframeOverlay);
            previewCanvas.Add(_telemetryLabel);
            rightPanel.Add(previewCanvas);

            rootBuilder.AddChild(rightPanel);

            var root = rootBuilder.Build();

            // CRITICAL UI TICK PATTERN
            root.schedule.Execute(() =>
            {
                // Tick localized FSM logic (e.g., surface scanner feedback)
                FSM_API.Interaction.Update("LairPlacementUI");

                // Manually rotate the mock model to match our sliders
                if (asteroidMock != null)
                {
                    asteroidMock.transform.rotation = Quaternion.Euler(_targetPitch, _targetYaw, 0f);
                }

                // Simulate scanning feedback based on rotation
                float stability = Mathf.PerlinNoise(_targetPitch * 0.1f, _targetYaw * 0.1f) * 100f;
                _telemetryLabel.text = $"CRUST STABILITY: {stability:F1}%\nDEPTH POTENTIAL: {(int)(stability * 2)}m";
                _mockWireframeOverlay.style.color = stability > 40f ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);

            }).Every(16);

            // Cleanup the mock object when this UI is detached
            root.RegisterCallback<DetachFromPanelEvent>(e => GameObject.DestroyImmediate(asteroidMock));

            return root;
        }

        private VisualElement CreateSliderFloat(string label, float min, float max, float current, Action<float> onValueChanged)
        {
            var container = new VisualElement { style = { marginBottom = 15 } };
            var lbl = new Label($"{label}: {current:F1}°") { style = { color = Color.white } };
            var slider = new Slider(min, max) { value = current };
            slider.RegisterValueChangedCallback(evt => { lbl.text = $"{label}: {evt.newValue:F1}°"; onValueChanged(evt.newValue); });
            container.Add(lbl); container.Add(slider); return container;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}