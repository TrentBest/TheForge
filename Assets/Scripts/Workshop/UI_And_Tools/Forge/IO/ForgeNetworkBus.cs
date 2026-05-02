using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;
using Workshop.Core.Diagnostics; // Injected for ForgeLogger
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.REST;

namespace Workshop.UI_And_Tools.Forge.IO
{
    /// <summary>
    /// The Central Data Nervous System.
    /// Thread-safe HttpClient bus that decouples network latency from the simulation tick.
    /// Reforged to follow the Experience Model: No internal Update loops.
    /// </summary>
    public class ForgeNetworkBus : MonoBehaviour, IStateContext
    {
        public static ForgeNetworkBus Instance { get; private set; }

        // --- ISTATECONTEXT IMPLEMENTATION ---
        public bool IsValid { get; set; } = false;
        public string Name { get; set; } = "Forge_Network_Bus";

        // Native HttpClient handles connection pooling automatically.
        private static readonly HttpClient _client = new HttpClient();

        // Lock-less concurrent queues for cross-thread synchronization.
        private readonly ConcurrentQueue<NetworkIntent> _outboundQueue = new ConcurrentQueue<NetworkIntent>();
        private readonly ConcurrentQueue<NetworkResult> _inboundQueue = new ConcurrentQueue<NetworkResult>();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _client.Timeout = TimeSpan.FromSeconds(15);
                IsValid = true; // Signals the Forge that the bus is online

                ForgeLogger.Log($"{Name} Online. Connection pooling and async bridge established.")
                    .WithHeader("Network Bus")
                    .WithColor("#00FFCC") // Neon Cyan for Init
                    .SendToUnity();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Public Gateway: Queues a request from an FSM State or GUI Builder.
        /// Thread-safe and non-blocking.
        /// </summary>
        public void QueueRequest(NetworkIntent intent)
        {
            if (!IsValid)
            {
                ForgeLogger.LogError($"Request Aborted: Bus is offline. Target: {intent.FullUrl}")
                    .WithHeader("Network Bus")
                    .SendToUnity();
                return;
            }

            _outboundQueue.Enqueue(intent);

            ForgeLogger.Log($"Intent Queued: {intent.Method} -> {intent.FullUrl}")
                .WithHeader("Network Bus")
                .WithColor("#FFA500") // Network Orange
                .SendToUnity();
        }

        // --- THE DECOUPLED LIFECYCLE SYNC ---

        /// <summary>
        /// Should be called via the "Logic_Early" Processing Group in FSM_UnityIntegrationAdvanced.
        /// Delivers completed data results to requesters at the start of the frame.
        /// </summary>
        public void TickInbound()
        {
            while (_inboundQueue.TryDequeue(out NetworkResult result))
            {
                try
                {
                    ForgeLogger.Log($"Delivering Result: {result.StatusCode} | Success: {result.IsSuccess}")
                        .WithHeader("Inbound")
                        .WithColor("#FF00FF") // Magenta for Manifestation/Delivery
                        .SendToUnity();

                    result.OriginalIntent.OnComplete?.Invoke(result);
                }
                catch (Exception ex)
                {
                    ForgeLogger.LogError($"Callback delivery failure: {ex.Message}")
                        .WithHeader("Network Bus")
                        .SendToUnity();
                }
            }
        }

        /// <summary>
        /// Should be called via the "Logic_Late" Processing Group in FSM_UnityIntegrationAdvanced.
        /// Dispatches all queued outbound intents to the background thread pool.
        /// </summary>
        public void TickOutbound()
        {
            if (_outboundQueue.IsEmpty) return;

            int count = 0;
            while (_outboundQueue.TryDequeue(out NetworkIntent intent))
            {
                count++;
                _ = DispatchAsync(intent);
            }

            ForgeLogger.Log($"Flushing Outbound Bus: {count} tasks dispatched to .NET ThreadPool.")
                .WithHeader("Outbound")
                .WithColor("#FFA500")
                .SendToUnity();
        }

        // --- ASYNC BACKGROUND EXECUTION ---

        private async Task DispatchAsync(NetworkIntent intent)
        {
            var result = new NetworkResult { OriginalIntent = intent };

            try
            {
                string httpMethod = intent.Endpoint != null ? intent.Endpoint.Method : intent.Method;

                using (var requestMessage = new HttpRequestMessage(new HttpMethod(httpMethod), intent.FullUrl))
                {
                    // Body Injection
                    if (!string.IsNullOrEmpty(intent.Payload) &&
                        (httpMethod == "POST" || httpMethod == "PUT" || httpMethod == "PATCH"))
                    {
                        requestMessage.Content = new StringContent(intent.Payload, Encoding.UTF8, "application/json");
                    }

                    // REST Auth Hook
                    if (intent.Auth != null && intent.Auth.Type != AuthConfiguration.AuthType.None && !string.IsNullOrEmpty(intent.Auth.TokenOrKey))
                    {
                        requestMessage.Headers.Add(intent.Auth.AuthHeaderName, $"{intent.Auth.AuthHeaderPrefix}{intent.Auth.TokenOrKey}");
                    }

                    using (var response = await _client.SendAsync(requestMessage))
                    {
                        result.StatusCode = (long)response.StatusCode;
                        result.IsSuccess = response.IsSuccessStatusCode;
                        result.Payload = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Payload = $"Network Bus Failure: {ex.Message}";

                ForgeLogger.LogError($"Async Dispatch Failure: {ex.Message} at {intent.FullUrl}")
                    .WithHeader("Network Bus")
                    .SendToUnity();
            }

            // Sync back to the simulation boundary
            _inboundQueue.Enqueue(result);
        }
    }

    // --- DATA CONTRACTS ---

    public class NetworkIntent
    {
        public string FullUrl;
        public RestEndpoint Endpoint;
        public AuthConfiguration Auth;
        public string Payload;
        public string Method = "GET";

        // Callback fired at the START of the next frame via TickInbound()
        public Action<NetworkResult> OnComplete;
    }

    public class NetworkResult
    {
        public NetworkIntent OriginalIntent;
        public bool IsSuccess;
        public long StatusCode;
        public string Payload;
    }
}