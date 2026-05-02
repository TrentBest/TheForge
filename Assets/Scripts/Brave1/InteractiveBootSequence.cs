using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.Staging
{
    public class InteractiveBootSequence : IGuiProvider
    {
        public string Title => "INTERACTIVE BOOT SEQUENCE";

        private Action _onCompleteCallback;
        private List<ISequenceNode> _sequence;

        private Label _telemetryHeader;
        private Label _telemetryData;
        private Slider _scrubSlider;
        private Button _enterButton;

        public InteractiveBootSequence(Action onCompleteCallback)
        {
            _onCompleteCallback = onCompleteCallback;
            _sequence = new OpeningSequenceBuilder().Build();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    backgroundColor = new Color(0.05f, 0.02f, 0.08f, 0.9f), // Deep dark purple
                    justifyContent = Justify.FlexEnd,
                    paddingBottom = 40, paddingLeft = 40, paddingRight = 40
                }
            };

            var telemetryBox = new VisualElement
            {
                style =
                {
                    backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f),
                    borderLeftWidth = 4, borderLeftColor = Color.cyan,
                    paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20,
                    width = 450, marginBottom = 20
                }
            };

            _telemetryHeader = new Label("AWAITING MANUAL OVERRIDE") { style = { color = Color.cyan, fontSize = 22, unityFontStyleAndWeight = FontStyle.Bold } };
            _telemetryData = new Label("Drag the slider to traverse architectural scale.") { style = { color = Color.white, fontSize = 14, whiteSpace = WhiteSpace.Normal, marginTop = 10 } };

            telemetryBox.Add(_telemetryHeader);
            telemetryBox.Add(_telemetryData);
            root.Add(telemetryBox);

            // THE INTERACTIVE SLIDER
            _scrubSlider = new Slider("DIMENSIONAL SCALE", 0, 100) { value = 0 };
            _scrubSlider.style.color = Color.yellow;
            _scrubSlider.RegisterValueChangedCallback(evt => OnSliderScrubbed(evt.newValue));
            root.Add(_scrubSlider);

            _enterButton = new Button(() => _onCompleteCallback?.Invoke())
            {
                text = "INITIALIZE WORKSHOP LOBBY",
                style = { marginTop = 20, height = 40, backgroundColor = new Color(0.8f, 0.1f, 0.8f), color = Color.white, display = DisplayStyle.None }
            };
            root.Add(_enterButton);

            // Initialize state
            OnSliderScrubbed(0);

            return root;
        }

        private void OnSliderScrubbed(float value)
        {
            if (_sequence.Count == 0) return;

            // Map 0-100 to the sequence indices
            int nodeIndex = Mathf.Clamp(Mathf.FloorToInt((value / 100f) * _sequence.Count), 0, _sequence.Count - 1);

            // If they drag to the very end, show the enter button
            if (value >= 99f) _enterButton.style.display = DisplayStyle.Flex;
            else _enterButton.style.display = DisplayStyle.None;

            var activeNode = _sequence[nodeIndex];

            // Update Diegetic UI
            _telemetryHeader.text = $"CONTEXT: {activeNode.NodeName}";
            _telemetryData.text = $"Engine bound to abstraction layer: {activeNode.TargetDataContext.Name}\nDeterministic State: Locked.";

            // INJECT GRAPHICS HOOK HERE:
            // e.g., RenderEngine.LoadContext(activeNode.TargetDataContext);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}