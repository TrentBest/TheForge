using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Corsair
{
    public class CorsairsInSpace_Gui_Deployment : IGuiProvider
    {
        public string Title => "Shuttle Deployment Sequence";

        private readonly IGuiRouter _router;
        private float _progress = 0f;
        private VisualElement _viewport;
        private Label _statusLabel;
        private ProgressBar _shuttleTelemetry;

        public CorsairsInSpace_Gui_Deployment(IGuiRouter router)
        {
            _router = router;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("DeploymentRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(Color.black)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            _viewport = new VisualElement
            {
                style = { flexGrow = 1, width = Length.Percent(100), justifyContent = Justify.Center, alignItems = Align.Center }
            };

            var retroIcon = new Label("🚀") { style = { fontSize = 120, position = Position.Absolute } };
            _statusLabel = new Label("INITIALIZING DEPLOYMENT VECTORS...") { style = { color = Color.yellow, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 150 } };

            _shuttleTelemetry = new ProgressBar { style = { width = 400, marginTop = 20 } };

            _viewport.Add(retroIcon);
            _viewport.Add(_statusLabel);
            _viewport.Add(_shuttleTelemetry);
            rootBuilder.AddChild(_viewport);

            var root = rootBuilder.Build();

            // Sequence Tick Pattern
            root.schedule.Execute(() =>
            {
                if (_progress < 1f)
                {
                    _progress += Time.deltaTime * 0.2f; // ~5 seconds total
                    _shuttleTelemetry.value = _progress * 100f;

                    UpdateSequenceState(retroIcon);
                }
                else
                {
                    // Handoff to the 3D City Builder (Lair Architect)
                    _router.NavigateTo("Architect");
                }
            }).Every(16);

            return root;
        }

        private void UpdateSequenceState(VisualElement icon)
        {
            // 1. Telemetry Readout
            if (_progress < 0.2f) _statusLabel.text = "DEPARTURE: CLEARING FLAGSHIP DOCK...";
            else if (_progress < 0.5f) _statusLabel.text = "FLIGHT PATH ADHERENCE: ALIGNING WITH ASTEROID EQUATOR...";
            else if (_progress < 0.8f) _statusLabel.text = "RETRO BURN: DECELERATING FOR SURFACE IMPACT...";
            else _statusLabel.text = "IMPACT: CORING CREW DEPLOYED. ESTABLISHING VAULT.";

            // 2. Camera Shake Effect (Manipulating the Viewport container)
            if (_progress > 0.75f && _progress < 0.95f)
            {
                // Intense shake during impact/retro burn
                float intensity = Mathf.InverseLerp(0.95f, 0.75f, _progress) * 20f;
                float shakeX = UnityEngine.Random.Range(-intensity, intensity);
                float shakeY = UnityEngine.Random.Range(-intensity, intensity);
                _viewport.style.translate = new Translate(shakeX, shakeY, 0);
                icon.style.color = Color.red; // Heating up
            }
            else
            {
                _viewport.style.translate = new Translate(0, 0, 0);
            }

            // 3. Icon animation
            icon.style.scale = new Scale(new Vector2(Mathf.Lerp(1f, 0.1f, _progress), Mathf.Lerp(1f, 0.1f, _progress)));
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}