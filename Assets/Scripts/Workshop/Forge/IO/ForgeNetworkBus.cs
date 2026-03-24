// File: Assets/Scripts/Workshop/Forge/IO/ForgeNetworkBus.cs
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders.REST;

namespace TheSingularityWorkshop.Forge.IO
{
    /// <summary>
    /// Replaces UnityWebRequest. 
    /// Handles all external data intents on background threads, syncing results to the FSM at frame boundaries.
    /// </summary>
    public class ForgeNetworkBus : MonoBehaviour
    {
        public static ForgeNetworkBus Instance { get; private set; }

        // Native C# HttpClient is designed to be a long-lived singleton for connection pooling
        private static readonly HttpClient _client = new HttpClient();

        // Queues that can be safely accessed across threads
        private ConcurrentQueue<NetworkIntent> _outboundQueue = new ConcurrentQueue<NetworkIntent>();
        private ConcurrentQueue<NetworkResult> _inboundQueue = new ConcurrentQueue<NetworkResult>();

        void Awake()
        {
            if (Instance != null && Instance != this) Destroy(gameObject);
            else Instance = this;

            // Set up timeouts suitable for VR/Forge expectations
            _client.Timeout = TimeSpan.FromSeconds(15);
        }

        /// <summary>
        /// Called by FSM States or GUI Builders to request data. Does NOT block the thread.
        /// </summary>
        public void QueueRequest(NetworkIntent intent)
        {
            _outboundQueue.Enqueue(intent);
        }

        // --- THE LIFECYCLE SYNC ---

        void Update()
        {
            // FRONT OF FRAME: Deliver completed data to the FSM Contexts
            while (_inboundQueue.TryDequeue(out NetworkResult result))
            {
                // Deliver the payload to the original requester's callback or context
                result.OriginalIntent.OnComplete?.Invoke(result);
            }
        }

        void LateUpdate()
        {
            // END OF FRAME: Dispatch all queued requests to the background
            while (_outboundQueue.TryDequeue(out NetworkIntent intent))
            {
                // Fire and forget onto the ThreadPool. Unity is not blocked!
                _ = DispatchAsync(intent);
            }
        }

        // --- BACKGROUND THREAD EXECUTION ---

        private async Task DispatchAsync(NetworkIntent intent)
        {
            var result = new NetworkResult { OriginalIntent = intent };

            try
            {
                using (var requestMessage = new HttpRequestMessage(new HttpMethod(intent.Endpoint.Method), intent.FullUrl))
                {
                    // Apply Payload
                    if (!string.IsNullOrEmpty(intent.Payload) &&
                       (intent.Endpoint.Method == "POST" || intent.Endpoint.Method == "PUT" || intent.Endpoint.Method == "PATCH"))
                    {
                        requestMessage.Content = new StringContent(intent.Payload, Encoding.UTF8, "application/json");
                    }

                    // Apply Auth Header
                    if (intent.Auth != null && intent.Auth.Type != AuthConfiguration.AuthType.None && !string.IsNullOrEmpty(intent.Auth.TokenOrKey))
                    {
                        requestMessage.Headers.Add(intent.Auth.AuthHeaderName, $"{intent.Auth.AuthHeaderPrefix}{intent.Auth.TokenOrKey}");
                    }

                    // EXECUTE NATIVE HTTP REQUEST
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
                result.Payload = $"Forge Network Exception: {ex.Message}";
            }

            // Push to inbound queue for the main thread to pick up next frame
            _inboundQueue.Enqueue(result);
        }
    }

    // --- DATA STRUCTURES ---

    public class NetworkIntent
    {
        public string FullUrl;
        public RestEndpoint Endpoint;
        public AuthConfiguration Auth;
        public string Payload;

        // The callback fired at the START of the next frame when data is ready
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