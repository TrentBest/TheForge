using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

#if UNITY_SERVICES_AVAILABLE
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.RemoteConfig;
using Unity.Services.CloudSave;
#endif


/// <summary>
/// A comprehensive Unity Services lifecycle wrapper that exposes service initialization,
/// authentication, health checks, and graceful shutdown through the project's FSM_API.FSM_API.
/// Designed to be feature-rich but opt-in: actual Unity Services calls are compiled only
/// when the symbol UNITY_SERVICES_AVAILABLE is defined (add it when you install the Unity Services packages).
/// </summary>
public class UnityServicesContext : MonoBehaviour, IStateContext
{
    // Public configuration (editable in Inspector)
    [Header("General")]
    public string ContextName = "UnityServices";
    public bool AutoInitializeOnAwake = true;
    public bool RequireAuthentication = true;
    public int MaxInitRetries = 3;
    public float RetryBackoffSeconds = 2f;
    public bool AutoRefreshRemoteConfig = true;
    public float RemoteConfigRefreshIntervalSeconds = 60f;

    [Header("Optional features")]
    public bool EnableCloudSave = false;
    public bool EnableRemoteConfig = true;
    public bool EnableLobbyServices = false; // placeholder for other services

    // IStateContext members (kept similar to ExperienceContext to fit FSM_API expectations)
    public List<Sense> Senses { get; private set; } = new List<Sense>();
    public bool IsValid { get; set; } = false;
    public string Name { get => ContextName; set => ContextName = value; }
    public FSMHandle Status { get; private set; }

    // Internal FSM/state flags
    private bool _initializing = false;
    private bool _authenticated = false;
    private bool _servicesInitialized = false;
    private bool _degraded = false;
    private bool _shouldShutdown = false;
    private bool _hasError = false;

    // Async control
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private Task _remoteConfigLoopTask;
    private object _lock = new object();

    // Events consumers can subscribe to
    public event Action OnReady;
    public event Action<Exception> OnError;
    public event Action OnShutdown;
    public event Action<string> OnLog;

    private void Awake()
    {
        // Build the FSM definition for Unity Services lifecycle
        FSM_API.Create.CreateFiniteStateMachine("UnityServicesFSM", -1, "Update")
            .State("Idle", OnEnterIdle, null, null)
            .State("Authenticating", OnEnterAuthenticating, null, null)
            .State("InitializingServices", OnEnterInitializingServices, null, null)
            .State("Ready", OnEnterReady, null, OnExtReady)
            .State("Degraded", OnEnterDegraded, null, null)
            .State("Error", OnEnterError, null, null)
            .State("Shutdown", OnEnterShutdown, null, null)
            .Transition("Idle", "Authenticating", NeedsAuthentication)
            .Transition("Idle", "InitializingServices", NeedsInitialization)
            .Transition("Authenticating", "InitializingServices", Authenticated)
            .Transition("InitializingServices", "Ready", ServicesReady)
            .Transition("InitializingServices", "Degraded", ServicesDegraded)
            .Transition("InitializingServices", "Error", InitializationFailed)
            .Transition("Ready", "Degraded", EnterDegraded)
            .Transition("Degraded", "Ready", RecoverFromDegraded)
            .Transition("Ready", "Shutdown", ShouldShutdown)
            .Transition("Degraded", "Shutdown", ShouldShutdown)
            .Transition("Error", "Shutdown", ShouldShutdown)
            .BuildDefinition();

        Status = FSM_API.Create.CreateInstance("UnityServicesFSM", this, "Update");
        IsValid = true;

        Log("UnityServicesContext: FSM created.");

        if (AutoInitializeOnAwake)
        {
            // start the lifecycle asynchronously (drives flags used by transitions)
            StartInitialization();
        }
    }

    #region Public control API

    public void StartInitialization()
    {
        // Kick off initialization chain
        lock (_lock)
        {
            _initializing = true;
            _authenticated = false;
            _servicesInitialized = false;
            _degraded = false;
            _shouldShutdown = false;
            _hasError = false;
        }

        Log("UnityServicesContext: StartInitialization requested.");
        // External hook into FSM: trigger external update for the "Idle" state's external update
        FSM_API.Interaction.Update("Update");
    }

    public void ForceAuthenticate()
    {
        lock (_lock) { _authenticated = false; _hasError = false; }
        _ = AuthenticateAsync(_cts.Token);
    }

    public async Task<bool> HealthCheckAsync(TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        try
        {
#if UNITY_SERVICES_AVAILABLE
            // Optionally check Authentication and CloudSave/RemoteConfig statuses
            if (RequireAuthentication && !AuthenticationService.Instance.IsSignedIn)
            {
                Log("UnityServicesContext: HealthCheck - not authenticated.");
                return false;
            }

            // Example: check CloudSave if enabled
            if (EnableCloudSave)
            {
                // Cloud Save has no direct 'IsHealthy' API; attempt a lightweight operation with limited time
                // For safety, wrap in try/catch and treat exceptions as unhealthy.
                try
                {
                    var keys = await CloudSaveService.Instance.Data.LoadKeysAsync();
                    Log($"UnityServicesContext: HealthCheck - CloudSave keys count {keys?.Count ?? 0}");
                }
                catch (Exception ex)
                {
                    Log($"UnityServicesContext: HealthCheck - CloudSave error: {ex.Message}");
                    return false;
                }
            }
#endif
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            Log($"UnityServicesContext: HealthCheck exception: {ex}");
            return false;
        }
    }

    public void Shutdown()
    {
        lock (_lock) { _shouldShutdown = true; }
        Log("UnityServicesContext: Shutdown requested.");
    }

    #endregion

    #region FSM state hooks

    private void OnEnterIdle(IStateContext ctx)
    {
        Log("UnityServicesContext: Entering Idle.");
        // If AutoInitialize was called or flag set, set transitions' backing flags
        // Idle doesn't need to do much; transitions are evaluated by the FSM when Update runs.
    }

    private void OnEnterAuthenticating(IStateContext ctx)
    {
        Log("UnityServicesContext: Entering Authenticating.");
        _ = AuthenticateAsync(_cts.Token);
    }

    private void OnEnterInitializingServices(IStateContext ctx)
    {
        Log("UnityServicesContext: Entering InitializingServices.");
        _ = InitializeServicesAsync(_cts.Token);
    }

    private void OnEnterReady(IStateContext ctx)
    {
        Log("UnityServicesContext: Entering Ready.");
        OnReady?.Invoke();

        if (EnableRemoteConfig && AutoRefreshRemoteConfig)
        {
            _remoteConfigLoopTask = Task.Run(RemoteConfigRefreshLoopAsync, _cts.Token);
        }
    }

    private void OnExtReady(IStateContext ctx)
    {
        // place to accept external signals while in Ready
    }

    private void OnEnterDegraded(IStateContext ctx)
    {
        Log("UnityServicesContext: Entering Degraded. Some non-critical services failed.");
        _degraded = true;
    }

    private void OnEnterError(IStateContext ctx)
    {
        Log("UnityServicesContext: Entering Error. Initialization failed.");
        _hasError = true;
        OnError?.Invoke(new Exception("UnityServicesContext: Initialization failure."));
    }

    private void OnEnterShutdown(IStateContext ctx)
    {
        Log("UnityServicesContext: Entering Shutdown. Performing cleanup.");
        _cts.Cancel();

#if UNITY_SERVICES_AVAILABLE
        try
        {
            // Best effort sign out and dispose where available
            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
            }
        }
        catch (Exception ex)
        {
            Log($"UnityServicesContext: Shutdown signout exception: {ex}");
        }
#endif

        _servicesInitialized = false;
        _authenticated = false;
        _initializing = false;
        _shouldShutdown = false;
        OnShutdown?.Invoke();
    }

    #endregion

    #region Transition predicates

    private bool NeedsAuthentication(IStateContext ctx) => _initializing && RequireAuthentication && !_authenticated && !_hasError && !_shouldShutdown;
    private bool NeedsInitialization(IStateContext ctx) => _initializing && (!_servicesInitialized) && !_hasError && !_shouldShutdown && (!RequireAuthentication || _authenticated);
    private bool Authenticated(IStateContext ctx) => _authenticated && !_hasError && !_shouldShutdown;
    private bool ServicesReady(IStateContext ctx) => _servicesInitialized && !_degraded && !_hasError && !_shouldShutdown;
    private bool ServicesDegraded(IStateContext ctx) => _servicesInitialized && _degraded && !_hasError && !_shouldShutdown;
    private bool InitializationFailed(IStateContext ctx) => _hasError && !_shouldShutdown;
    private bool EnterDegraded(IStateContext ctx) => !_shouldShutdown && !_degraded && _servicesInitialized == false && _hasError == false && _initializing == false;
    private bool RecoverFromDegraded(IStateContext ctx) => _degraded && _servicesInitialized && !_hasError && !_shouldShutdown;
    private bool ShouldShutdown(IStateContext ctx) => _shouldShutdown;

    #endregion

    #region Implementation helpers

    private async Task AuthenticateAsync(CancellationToken ct)
    {
        Log("UnityServicesContext: AuthenticateAsync starting.");
        try
        {
#if UNITY_SERVICES_AVAILABLE
            // If Unity Services core isn't initialized, initialize it first.
            if (!UnityServices.State.Equals(Unity.Services.Core.InitializationOptions.Empty) && UnityServices.State != Unity.Services.Core.UnityServices.State.Initialized)
            {
                await UnityServices.InitializeAsync();
            }
            else if (UnityServices.State == Unity.Services.Core.UnityServices.State.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            if (AuthenticationService.Instance.IsSignedIn)
            {
                _authenticated = true;
                Log("UnityServicesContext: Already authenticated.");
                return;
            }

            // Example: anonymous sign-in
            var result = await AuthenticationService.Instance.SignInAnonymouslyAsync();
            _authenticated = AuthenticationService.Instance.IsSignedIn;
            Log($"UnityServicesContext: Authentication result: signedIn={_authenticated}");
#else
            // No-op stub when Unity Services packages are not present
            await Task.Delay(250, ct).ConfigureAwait(false);
            _authenticated = true;
            Log("UnityServicesContext: (stub) Authentication simulated.");
#endif
        }
        catch (Exception ex)
        {
            _hasError = true;
            Log($"UnityServicesContext: Authentication failed: {ex}");
            OnError?.Invoke(ex);
        }
    }

    private async Task InitializeServicesAsync(CancellationToken ct)
    {
        Log("UnityServicesContext: InitializeServicesAsync starting.");
        var attempt = 0;
        Exception lastEx = null;

        while (attempt < Math.Max(1, MaxInitRetries) && !_servicesInitialized && !_hasError && !_shouldShutdown)
        {
            attempt++;
            try
            {
#if UNITY_SERVICES_AVAILABLE
                // Ensure core initialized
                if (UnityServices.State == Unity.Services.Core.UnityServices.State.Uninitialized)
                {
                    await UnityServices.InitializeAsync();
                }

                // Optional service initializations (RemoteConfig, CloudSave, etc.)
                if (EnableRemoteConfig && EnableRemoteConfig)
                {
                    // Hook remote config fetch once to prime values
                    await RemoteConfigService.Instance.FetchConfigsAsync(new UserAttributes(), new AppAttributes());
                    RemoteConfigService.Instance.ApplyFetched();
                }

                if (EnableCloudSave && EnableCloudSave)
                {
                    // Cloud Save SDK typically requires no explicit init beyond UnityServices.
                    // Optionally attempt a lightweight call to validate connectivity.
                    var keys = await CloudSaveService.Instance.Data.LoadKeysAsync();
                    Log($"UnityServicesContext: CloudSave available; keys count {keys?.Count ?? 0}");
                }
#else
                // Simulate short initialization
                await Task.Delay(400, ct).ConfigureAwait(false);
                Log("UnityServicesContext: (stub) Services simulated initialization.");
#endif

                _servicesInitialized = true;
                _degraded = false;
                Log("UnityServicesContext: Services initialized successfully.");
                break;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                Log($"UnityServicesContext: Initialize attempt {attempt} failed: {ex.Message}");
                if (attempt < MaxInitRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(RetryBackoffSeconds * attempt), ct).ConfigureAwait(false);
                }
            }
        }

        if (!_servicesInitialized)
        {
            _hasError = true;
            OnError?.Invoke(lastEx ?? new Exception("Unknown initialization failure"));
        }
    }

    private async Task RemoteConfigRefreshLoopAsync()
    {
        Log("UnityServicesContext: RemoteConfig refresh loop started.");
        try
        {
            while (!_cts.IsCancellationRequested)
            {
#if UNITY_SERVICES_AVAILABLE
                try
                {
                    await RemoteConfigService.Instance.FetchConfigsAsync(new UserAttributes(), new AppAttributes());
                    RemoteConfigService.Instance.ApplyFetched();
                    Log("UnityServicesContext: RemoteConfig refreshed.");
                }
                catch (Exception ex)
                {
                    Log($"UnityServicesContext: RemoteConfig refresh error: {ex.Message}");
                    // Non-fatal: allow loop to continue
                }
#endif
                await Task.Delay(TimeSpan.FromSeconds(RemoteConfigRefreshIntervalSeconds), _cts.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Log("UnityServicesContext: RemoteConfig loop canceled.");
        }
        catch (Exception ex)
        {
            Log($"UnityServicesContext: RemoteConfig loop exception: {ex}");
        }
    }

    private void Log(string message)
    {
        Debug.Log(message);
        OnLog?.Invoke(message);
    }

    #endregion

    private void OnDestroy()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    // Keep the MonoBehaviour life-cycle methods intentionally small; FSM drives behavior
    void Start() { }

    void Update() { }
}

