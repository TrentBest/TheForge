using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_API.Scripts;
using UnityEngine;
using Workshop;

#if UNITY_SERVICES_AVAILABLE
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.RemoteConfig;
using Unity.Services.CloudSave;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
#endif

namespace Assets.Scripts.Unity
{
    public class UnityServicesContext : MonoBehaviour, IStateContext
    {
        [Header("Configuration")]
        public UnityServicesConfig ContextConfig;

        [Header("General")]
        public string ContextName = "UnityServices";

        public List<Sense> Senses { get; private set; } = new List<Sense>();
        public bool IsValid { get; set; } = false;
        public string Name { get => ContextName; set => ContextName = value; }
        public FSMHandle Status { get; private set; }

        private bool _initializing = false;
        private bool _authenticated = false;
        private bool _servicesInitialized = false;
        private bool _degraded = false;
        private bool _shouldShutdown = false;
        private bool _hasError = false;

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private Task _remoteConfigLoopTask;
        private readonly object _lock = new object();

#if UNITY_SERVICES_AVAILABLE
        private Lobby _activeLobby;
#endif

        public event Action OnReady;
        public event Action<Exception> OnError;
        public event Action OnShutdown;
        public event Action<string> OnLog;

        private void Awake()
        {
            if (ContextConfig == null) return;

            FSM_UnityIntegrationAdvanced.Instance.AddProcessingGroup("Update", "System_Services");

            FSM_API.Create.CreateFiniteStateMachine("UnityServicesFSM", 1, "System_Services")
                .State("Idle", OnEnterIdle, null, null)
                .State("Authenticating", OnEnterAuthenticating, null, null)
                .State("InitializingServices", OnEnterInitializingServices, null, null)
                .State("Ready", OnEnterReady, null, null)
                .State("LobbyActive", OnEnterLobby, null, null)
                .State("Degraded", OnEnterDegraded, null, null)
                .State("Error", OnEnterError, null, null)
                .State("Shutdown", OnEnterShutdown, null, null)

                .Transition("Idle", "Authenticating", NeedsAuthentication)
                .Transition("Idle", "InitializingServices", NeedsInitialization)
                .Transition("Authenticating", "InitializingServices", Authenticated)
                .Transition("InitializingServices", "Ready", ServicesReady)
                .Transition("Ready", "LobbyActive", ctx => ContextConfig.EnableLobbyServices && _servicesInitialized && !_shouldShutdown)
                .Transition("Ready", "Degraded", EnterDegraded)
                .Transition("Ready", "Shutdown", ShouldShutdown)
                .BuildDefinition();

            Status = FSM_API.Create.CreateInstance("UnityServicesFSM", this, "System_Services");
            IsValid = true;

            if (ContextConfig.AutoInitializeOnAwake) StartInitialization();
        }

        private async Task AuthenticateAsync(CancellationToken ct)
        {
#if UNITY_SERVICES_AVAILABLE
            try
            {
                if (UnityServices.State == ServicesInitializationState.Uninitialized) await UnityServices.InitializeAsync();
                if (AuthenticationService.Instance.IsSignedIn) { _authenticated = true; return; }
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                _authenticated = AuthenticationService.Instance.IsSignedIn;
            }
            catch (Exception ex) { _hasError = true; OnError?.Invoke(ex); }
#else
            await Task.Delay(100, ct); _authenticated = true;
#endif
        }

        private async Task InitializeServicesAsync(CancellationToken ct)
        {
#if UNITY_SERVICES_AVAILABLE
            try
            {
                if (ContextConfig.EnableRemoteConfig)
                {
                    await RemoteConfigService.Instance.FetchConfigsAsync(new UserAttributes(), new AppAttributes());
                    RemoteConfigService.Instance.ApplyFetched();
                }
                _servicesInitialized = true;
            }
            catch (Exception ex) { _hasError = true; OnError?.Invoke(ex); }
#else
            await Task.Delay(100, ct); _servicesInitialized = true;
#endif
        }

        private async Task EstablishLobbyHandshakeAsync()
        {
#if UNITY_SERVICES_AVAILABLE
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(ContextConfig.MaxPlayers);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                var options = new CreateLobbyOptions
                {
                    IsPrivate = ContextConfig.ActiveLobbyType == LobbyType.PrivateInvitation,
                    Data = new Dictionary<string, LobbyDataObject>
                    {
                        { "RelayJoinCode", new LobbyDataObject(LobbyDataObject.VisibilityOptions.Public, joinCode) },
                        { "Skill", new LobbyDataObject(LobbyDataObject.VisibilityOptions.Public, DataShelfMappingUtility.GetSkillRating(ContextConfig.SkillDataShelfKey).ToString()) }
                    }
                };

                _activeLobby = await LobbyService.Instance.CreateLobbyAsync($"{ContextName}_Session", ContextConfig.MaxPlayers, options);
                Log($"[Handshake] Lobby Established. Join Code: {joinCode}");
            }
            catch (Exception ex) { Log($"[Handshake] Failed: {ex.Message}"); _degraded = true; }
#endif
        }

        private async Task RemoteConfigRefreshLoopAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
#if UNITY_SERVICES_AVAILABLE
                try
                {
                    await RemoteConfigService.Instance.FetchConfigsAsync(new UserAttributes(), new AppAttributes());
                    RemoteConfigService.Instance.ApplyFetched();
                    Log("RemoteConfig: Cycle Refreshed.");
                }
                catch (Exception ex) { Log($"RemoteConfig Loop Error: {ex.Message}"); }
#endif
                await Task.Delay(TimeSpan.FromSeconds(ContextConfig.RemoteConfigRefreshIntervalSeconds), _cts.Token);
            }
        }

        private void OnEnterIdle(IStateContext ctx) => Log("System Idle.");
        private void OnEnterAuthenticating(IStateContext ctx) => _ = AuthenticateAsync(_cts.Token);
        private void OnEnterInitializingServices(IStateContext ctx) => _ = InitializeServicesAsync(_cts.Token);
        private void OnEnterReady(IStateContext ctx)
        {
            OnReady?.Invoke();
            if (ContextConfig.EnableRemoteConfig && ContextConfig.AutoRefreshRemoteConfig)
                _remoteConfigLoopTask = Task.Run(RemoteConfigRefreshLoopAsync, _cts.Token);
        }
        private void OnEnterLobby(IStateContext ctx) => _ = EstablishLobbyHandshakeAsync();
        private void OnEnterDegraded(IStateContext ctx) => _degraded = true;
        private void OnEnterError(IStateContext ctx) => _hasError = true;
        private void OnEnterShutdown(IStateContext ctx) { _cts.Cancel(); OnShutdown?.Invoke(); }

        private bool NeedsAuthentication(IStateContext ctx) => _initializing && ContextConfig.RequireAuthentication && !_authenticated && !_hasError && !_shouldShutdown;
        private bool NeedsInitialization(IStateContext ctx) => _initializing && !_servicesInitialized && !_hasError && !_shouldShutdown && (!ContextConfig.RequireAuthentication || _authenticated);
        private bool Authenticated(IStateContext ctx) => _authenticated && !_hasError && !_shouldShutdown;
        private bool ServicesReady(IStateContext ctx) => _servicesInitialized && !_degraded && !_hasError && !_shouldShutdown;
        private bool EnterDegraded(IStateContext ctx) => !_shouldShutdown && !_degraded && _hasError;
        private bool ShouldShutdown(IStateContext ctx) => _shouldShutdown;

        public void StartInitialization() { lock (_lock) { _initializing = true; } }
        private void Log(string m) { Debug.Log($"[Singularity Services] {m}"); OnLog?.Invoke(m); }
        private void OnDestroy() { _cts.Cancel(); _cts.Dispose(); }
    }
}