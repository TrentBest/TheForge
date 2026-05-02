using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Workshop.Core;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge; // Required for the Hail Mary payload

namespace Workshop.UI_And_Tools.Forge.IO
{
    /// <summary>
    /// Binary UDP Transport for the Hermit Neural Bridge.
    /// Acts as an IDomainPassenger to gracefully sever and re-establish ports across Domain Reloads.
    /// </summary>
    public class UdpDataTransport : IDataTransport, IDomainPassenger, IDisposable
    {
        public string TransportId { get; private set; }

        private UdpClient _client;
        private string _targetIp;
        private int _listenPort;
        private int _targetPort;
        private bool _isOnline = false;

        public event Action<byte[]> OnDataReceived;

        public UdpDataTransport(string name, string ip, int listenPort, int targetPort)
        {
            TransportId = name;
            _targetIp = ip;
            _listenPort = listenPort;
            _targetPort = targetPort;

            InitializeSocket();

            // Hop aboard the Domain Conductor immediately
            Workshop.UI_And_Tools.Forge.Core.ForgeDomainConductor.IssueTicket(this);
        }

        private void InitializeSocket()
        {
            try
            {
                _client = new UdpClient(_listenPort);

                // Windows fix to prevent Error 10054 Connection Reset crashes when the external app drops
                const int SIO_UDP_CONNRESET = -1744830452;
                _client.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);

                _isOnline = true;

                // Spin up the async receive loop
                _client.BeginReceive(OnUdpReceive, null);
            }
            catch (Exception ex)
            {
                ForgeLogger.LogError($"[UDP Transport] Socket Initialization Fault: {ex.Message}").SendToUnity();
            }
        }

        private void OnUdpReceive(IAsyncResult ar)
        {
            if (!_isOnline || _client == null) return;

            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _client.EndReceive(ar, ref remoteEP);

                OnDataReceived?.Invoke(data);

                // Re-arm the listener
                if (_isOnline) _client.BeginReceive(OnUdpReceive, null);
            }
            catch (ObjectDisposedException) { /* Expected during Domain Reload / Dispose */ }
            catch (Exception ex) { ForgeLogger.LogError($"[UDP Rx Error] {ex.Message}").SendToUnity(); }
        }

        public void Transmit(byte[] payload)
        {
            if (!_isOnline || _client == null) return;
            _client.Send(payload, payload.Length, _targetIp, _targetPort);
        }

        // ==========================================
        // --- IDOMAINPASSENGER IMPLEMENTATION ---
        // ==========================================

        public void OnSuspend()
        {
            if (!_isOnline) return;

            ForgeLogger.Log("Network Transport Suspending. Firing Hail Mary...")
                .WithHeader("UDP Link").WithColor("#FF00FF").SendToUnity();

            // 1. Compile the Hail Mary Package (The Castle of Aarrgh Protocol)
            var hailMary = new HermitDirectivePayload
            {
                AgentId = "System",
                Intent = "System_Suspend",
                Content = "Domain Reload Impending! I want you to know this and that and this and this and that and arrrrrrrrg"
            };

            // You can pack current stats, unsaved telemetry, or current active FSM states here
            hailMary.MetaData.Add("Timestamp", DateTime.UtcNow.ToString("O"));
            hailMary.MetaData.Add("Reason", "AssemblyReload");

            string json = JsonUtility.ToJson(hailMary);
            byte[] warningPayload = Encoding.UTF8.GetBytes(json);

            // 2. Blast the warning to the Console App (Synchronous execution)
            _client.Send(warningPayload, warningPayload.Length, _targetIp, _targetPort);

            // 3. Immediately sever the socket so Unity can reload without Port Occupied errors
            Dispose();
        }

        public void OnResume()
        {
            ForgeLogger.Log("Network Transport Rehydrating...")
                .WithHeader("UDP Link").WithColor("#32CD32").SendToUnity();

            // 1. Rebind the socket to the previously cached port
            InitializeSocket();

            // 2. Alert Hermit that the brain has returned
            var wakeup = new HermitDirectivePayload
            {
                AgentId = "System",
                Intent = "System_Resume",
                Content = "Domain Reload Complete. Sovereign Context Restored."
            };
            wakeup.MetaData.Add("Timestamp", DateTime.UtcNow.ToString("O"));

            string json = JsonUtility.ToJson(wakeup);
            byte[] resumePayload = Encoding.UTF8.GetBytes(json);

            _client.Send(resumePayload, resumePayload.Length, _targetIp, _targetPort);
        }

        public void Dispose()
        {
            _isOnline = false;
            if (_client != null)
            {
                _client.Close();
                _client.Dispose();
                _client = null;
            }
        }
    }
}