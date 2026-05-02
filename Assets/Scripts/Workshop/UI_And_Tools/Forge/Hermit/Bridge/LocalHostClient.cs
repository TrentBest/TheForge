using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Workshop.Core.Diagnostics;

namespace Workshop.UI_And_Tools.Forge.Hermit.Bridge
{
    public static class LocalHostClient
    {
        private static UdpClient _udpClient;
        private static Thread _receiveThread;
        private static bool _isListening;
        private static SynchronizationContext _mainThread; // Guarantees execution safely!

        public static event Action<HermitDirectivePayload> OnDirectiveReceived;

        public static void InitializeUDP()
        {
            if (_isListening) return;

            try
            {
                // Capture the Unity Main Thread so background UDP can jump back to it
                _mainThread = SynchronizationContext.Current;

                _udpClient = new UdpClient(11000);

                // Windows fix to prevent Error 10054 Connection Reset crashes
                const int SIO_UDP_CONNRESET = -1744830452;
                _udpClient.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);

                _isListening = true;
                _receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
                _receiveThread.Start();

                ForgeLogger.Log("Production Transport Online. Listening: 11001, Target: 11000.")
                    .WithHeader("DataBus")
                    .WithColor(Color.magenta)
                    .SendToUnity();
            }
            catch (Exception ex)
            {
                ForgeLogger.LogError($"[LocalHostClient] Failed to bind UDP: {ex.Message}");
            }
        }

        private static void ReceiveLoop()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (_isListening)
            {
                try
                {
                    byte[] receiveBytes = _udpClient.Receive(ref remoteEndPoint);
                    string rawIncoming = Encoding.UTF8.GetString(receiveBytes).Trim();

                    // Jump instantly to the Unity Main Thread
                    if (_mainThread != null)
                    {
                        _mainThread.Post(_ => ProcessDataOnMainThread(rawIncoming), null);
                    }
                    else
                    {
                        ProcessDataOnMainThread(rawIncoming);
                    }
                }
                catch (System.Threading.ThreadAbortException)
                {
                    // Unity stopped Play Mode. Totally benign, swallow silently.
                }
                catch (ObjectDisposedException)
                {
                    // The UDP socket was closed during shutdown. Benign.
                }
                catch (SocketException)
                {
                    // Windows ICMP connection reset. Benign.
                }
                catch (Exception ex)
                {
                    // Only log actual unexpected errors
                    ForgeLogger.LogError($"[UDP Rx Error] {ex.Message}");
                }

            }
        }

        private static void ProcessDataOnMainThread(string rawIncoming)
        {
            // Log it exactly as it arrived so we can inspect it
            ForgeLogger.Log(rawIncoming).WithHeader("Hermit Cortex").WithColor(Color.cyan).SendToUnity();

            HermitDirectivePayload payload = new HermitDirectivePayload();

            // STRICT PARSE: Only attempt JSON if it looks like JSON
            if (rawIncoming.StartsWith("{") && rawIncoming.EndsWith("}"))
            {
                try { payload = JsonUtility.FromJson<HermitDirectivePayload>(rawIncoming); }
                catch { /* Fallback */ }
            }

            // FALLBACK: If it wasn't JSON, wrap the raw string safely so the GUI accepts it
            if (string.IsNullOrEmpty(payload.Content))
            {
                payload.AgentId = "Hermit";
                payload.Intent = "ChatResponse";
                payload.Content = rawIncoming;
            }

            OnDirectiveReceived?.Invoke(payload);
        }

        public static async void SendPrompt(string jsonPayload)
        {
            if (_udpClient == null) InitializeUDP();

            try
            {
                byte[] sendBytes = Encoding.UTF8.GetBytes(jsonPayload);
                await _udpClient.SendAsync(sendBytes, sendBytes.Length, "127.0.0.1", 11001);
            }
            catch (Exception ex) { ForgeLogger.LogError($"[UDP Tx Error] {ex.Message}"); }
        }

        public static void ProcessIncomingQueue()
        {
            // Legacy hook kept empty so other files don't break. 
            // SynchronizationContext handles this natively now.
        }

        public static void Close()
        {
            _isListening = false;
            _udpClient?.Close();
            _udpClient = null;
        }
    }
}