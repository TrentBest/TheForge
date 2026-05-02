using System;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics; // Injected for ForgeLogger
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class HermitAffordanceBuilder
    {
        public string ControlId { get; private set; }
        public string SemanticPrompt { get; private set; }
        public string RoleAssumption { get; private set; }

        public Action<string> OnHermitActuation { get; private set; }

        public HermitAffordanceBuilder(string controlId)
        {
            ControlId = controlId;

            ForgeLogger.Log($"Hermit Affordance Builder initialized for: {ControlId}")
                .WithHeader("Nervous System")
                .WithColor("#CCFF00") // Electric Lime
                .SendToUnity();
        }

        public HermitAffordanceBuilder WithRole(string rolePrompt)
        {
            RoleAssumption = rolePrompt;
            return this;
        }

        public HermitAffordanceBuilder WithPrompt(string prompt)
        {
            SemanticPrompt = prompt;
            return this;
        }

        public HermitAffordanceBuilder WithActuator(Action<string> executionLogic)
        {
            OnHermitActuation = executionLogic;
            return this;
        }

        // --- INTERNAL BUS BINDING ---
        public void BindTelemetry(VisualElement ve)
        {
            ForgeLogger.Log($"Binding Sensory Telemetry to UI Element: {ControlId}")
                .WithHeader("Hermit Core")
                .WithColor("#CCFF00")
                .SendToUnity();

            Action pushTelemetry = () =>
            {
                // 1. Create the Payload as a typed struct (Stack Allocated)
                var payload = new AffordanceTelemetryPayload
                {
                    Id = ControlId,
                    Role = RoleAssumption,
                    Prompt = SemanticPrompt,
                    State = "Examined"
                };

                if (SingularityDataBus.Instance != null)
                {
                    ForgeLogger.Log($"Sensory Focus Locked: {ControlId} | Role: {RoleAssumption}")
                        .WithHeader("Cognition")
                        .WithColor(Color.cyan)
                        .SendToUnity();

                    // 2. Direct Typed Local Broadcast (Zero Boxing)
                    SingularityDataBus.Instance.SendLocal("Hermit_Sensory_Focus", payload);

                    // 3. Queue for Binary Transmission (Zero JSON Overhead)
                    ForgeLogger.Log($"Broadcasting Binary Telemetry via UDP: {ControlId}")
                        .WithHeader("Network")
                        .WithColor("#FFA500") // Vibrant Orange
                        .SendToUnity();

                    SingularityDataBus.Instance.TransmitPackable("Hermit_UDP_Link", payload);
                }
                else
                {
                    ForgeLogger.LogError($"Nervous System Failure: DataBus offline. Dropped telemetry for {ControlId}")
                        .WithHeader("Safety")
                        .SendToUnity();
                }
            };

            ve.RegisterCallback<PointerEnterEvent>(evt => pushTelemetry());
            ve.RegisterCallback<FocusInEvent>(evt => pushTelemetry());

            HermitActuatorRegistry.Register(ControlId, OnHermitActuation);
        }

        /// <summary>
        /// Fixed: Changed from a class to a Packable Struct.
        /// Bypasses the JSON "Nightmare" by writing directly to the binary stream.
        /// </summary>
        [Serializable]
        public struct AffordanceTelemetryPayload : IBinaryPackable
        {
            public string Id;
            public string Role;
            public string Prompt;
            public string State;

            public void Pack(BinaryWriter writer)
            {
                writer.Write(Id ?? string.Empty);
                writer.Write(Role ?? string.Empty);
                writer.Write(Prompt ?? string.Empty);
                writer.Write(State ?? string.Empty);
            }
        }
    }

    public static class HermitActuatorRegistry
    {
        private static readonly System.Collections.Generic.Dictionary<string, Action<string>> _actuators = new();

        public static void Register(string id, Action<string> actuator)
        {
            _actuators[id] = actuator;

            ForgeLogger.Log($"Actuator Registry Updated: {id}")
                .WithHeader("Hermit Core")
                .WithColor("#CCFF00")
                .SendToUnity();
        }

        public static void Execute(string id, string valuePayload)
        {
            if (_actuators.TryGetValue(id, out var action))
            {
                ForgeLogger.Log($"Hermit Actuating Control: {id} | Value: {valuePayload}")
                    .WithHeader("Actuation")
                    .WithColor(Color.green)
                    .SendToUnity();

                action?.Invoke(valuePayload);
            }
            else
            {
                ForgeLogger.LogWarning($"Actuation Request Rejected: No actuator registered for {id}")
                    .WithHeader("Hermit Core")
                    .SendToUnity();
            }
        }
    }
}