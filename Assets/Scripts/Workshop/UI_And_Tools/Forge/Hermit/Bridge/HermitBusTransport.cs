using System;
using System.Text;
using UnityEngine;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Core;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Forge.Hermit.Bridge
{
    /// <summary>
    /// The Neural Gateway Adapter.
    /// Standardizes communication between the external AI (Hermit) and the Singularity Data Bus.
    /// Reforged for high-fidelity manifestation and telemetry tracking using raw byte pipelines.
    /// </summary>
    public class HermitBusTransport : IDataTransport
    {
        public string TransportId => "HERMIT_NEURAL_BRIDGE_01";

        // 1. SATISFY THE INTERFACE: The overarching DataBus demands raw bytes.
        public event Action<byte[]> OnDataReceived;

        // Overload to accept the HermitContext natively
        public void Transmit(string userMessage, HermitContext currentContext)
        {
            if (string.IsNullOrEmpty(userMessage)) return;

            try
            {
                // 1. Compile the environmental snapshot
                string envSummary = currentContext != null
                    ? $"Active Senses: {currentContext.ActiveSenses.Count}. Sensed Entities: {currentContext.SensedEnvironment.Count}."
                    : "Environment data unavailable.";

                // 2. Compile Tool Availability (Can hook into BuilderRegistry later)
                string toolSummary = "Available Directives: [MoveTo, Spawn, SetPosture, SpawnInWorldGui]";

                // 3. Package the Prompt
                CortexPromptData promptData = new CortexPromptData
                {
                    UserMessage = userMessage,
                    AvailableTools = toolSummary,
                    SensedEnvironment = envSummary,
                    TargetPosition = currentContext != null ? currentContext.TargetPosition : Vector3.zero
                };

                string jsonPayload = JsonUtility.ToJson(promptData);

                // Assuming LocalHostClient handles strings natively for the UDP/TCP socket.
                LocalHostClient.SendPrompt(jsonPayload);
                ForgeLogger.Log($"[HermitBridge] Signal Transmitted to Cortex. Size: {jsonPayload.Length} bytes.");
            }
            catch (Exception ex)
            {
                ForgeLogger.LogError($"[HermitBridge] Transmission Fault: {ex.Message}");
            }
        }

        // Standard string fallback for legacy Hermit calls
        public void Transmit(string data)
        {
            Transmit(data, null);
        }

        // 2. SATISFY THE INTERFACE: When the core bus tries to transmit raw bytes through this transport.
        public void Transmit(byte[] payload)
        {
            if (payload == null || payload.Length == 0) return;

            try
            {
                // Translate the raw memory bus bytes back into Hermit's JSON format
                string decodedJson = Encoding.UTF8.GetString(payload);
                Transmit(decodedJson, null);
            }
            catch (Exception ex)
            {
                ForgeLogger.LogError($"[HermitBridge] Byte Decoding Fault: {ex.Message}");
            }
        }

        // 3. INJECT TO BUS: When LocalHostClient receives the AI's response, we convert it 
        // to bytes so it seamlessly travels the SingularityDataBus like any other MicroPackage.
        public void InjectIntoBus(string jsonPayload)
        {
            if (string.IsNullOrEmpty(jsonPayload)) return;

            byte[] rawPayload = Encoding.UTF8.GetBytes(jsonPayload);
            OnDataReceived?.Invoke(rawPayload);
        }
    }
}