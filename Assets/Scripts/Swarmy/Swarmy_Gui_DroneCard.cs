using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Swarmy.UI
{
    public class Swarmy_Gui_DroneCard : IGuiProvider
    {
        public string Title => "Drone Telemetry Card";
        private DroneController _targetDrone;

        // UI Elements we need to update
        private Label _statusLabel;
        private Label _altitudeLabel;
        private ProgressBar _thrustPowerBar;

        public Swarmy_Gui_DroneCard(DroneController target)
        {
            _targetDrone = target;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder($"DroneCard_{_targetDrone.droneID}")
                .WithFlexLayout(FlexDirection.Column)
                .WithBackgroundColor(new Color(0.1f, 0.15f, 0.2f, 0.9f))
                .WithMargin(5)
                .WithPadding(10);

            // Header
            root.AddChild(new ForgeLabelBuilder($"UNIT: {_targetDrone.behaviorID}")
                .WithColor(new Color(0.4f, 0.8f, 1.0f))
                .WithFontStyle(FontStyle.Bold)
                .WithFontSize(14));

            root.AddChild(new ForgeLabelBuilder($"ID: {_targetDrone.droneID.Substring(0, 8)}...")
                .WithColor(Color.gray)
                .WithFontSize(10));

            // Status Readouts
            _statusLabel = new Label("State: OFFLINE");
            _statusLabel.style.color = Color.white;
            _statusLabel.style.marginTop = 10;

            _altitudeLabel = new Label("Altitude: 0.0m");
            _altitudeLabel.style.color = Color.white;

            // Using standard UIElements for dynamic updating
            var uiRoot = root.Build();
            uiRoot.Add(_statusLabel);
            uiRoot.Add(_altitudeLabel);

            // Bind the update loop (Ticks with Unity's UI system)
            uiRoot.schedule.Execute(UpdateTelemetry).Every(100); // 10 ticks per second

            return uiRoot;
        }

        private void UpdateTelemetry()
        {
            if (_targetDrone == null) return;

            // Read the FSM State directly from your API handle
            string currentState = _targetDrone.Status != null ? _targetDrone.Status.CurrentState : "Offline";
            _statusLabel.text = $"State: {currentState}";

            // Read Physics
            _altitudeLabel.text = $"Altitude: {_targetDrone.transform.position.y:F2}m";

            // Change color based on state
            if (currentState == "AttackRun") _statusLabel.style.color = Color.red;
            else if (currentState == "Hover") _statusLabel.style.color = Color.green;
            else _statusLabel.style.color = Color.white;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}