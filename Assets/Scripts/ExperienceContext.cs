using System;
using System.Collections.Generic;
using System.Linq;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

public class ExperienceContext : MonoBehaviour, IStateContext
{
    // When empty, the builder will attempt to discover a manifest adjacent to the executable.
    // If you set an explicit name, the builder will look adjacent to the executable for that name first,
    // then fall back to StreamingAssets/Experiences/<name>.json and cached manifests.
    public string ManifestName = "";

    public List<Sense> Senses { get; private set; } = new List<Sense>();
    public bool IsValid { get; set; } = false;
    public string Name { get => name; set => name = value; }
    public FSMHandle Status { get; private set; }

    private ExperienceBuilder _builder;
    private Experience _currentExperience;

    // simple state flags used by transitions
    private bool _loaded = false;
    private bool _initialized = false;
    private bool _shouldShutdown = false;

    private void Awake()
    {
        _builder = new ExperienceBuilder();

        FSM_API.Create.CreateFiniteStateMachine("ExperienceFSM", -1, "Update")
            .State("Loading", OnEnterLoading, null, null).State("Initializing", OnEnterInitializing, null, null)
            .State("Acting", OnEnterActing, OnUpdateActing, OnExtActing).State("Shutdown", OnEnterShutdown, null, null)
            .Transition("Loading", "Initializing", Loaded)
            .Transition("Initializing", "Acting", Initialized)
            .Transition("Acting", "Shutdown", ShouldShutdown)
            .BuildDefinition();

        Status = FSM_API.Create.CreateInstance("ExperienceFSM", this, "Update");
        IsValid = true;
    }

    private void OnEnterLoading(IStateContext context)
    {
        _loaded = false;
        _initialized = false;
        _shouldShutdown = false;
        Senses.Clear();

        try
        {
            // Builder.Load will now attempt executable-adjacent discovery first when ManifestName is empty,
            // or try a named file adjacent to the executable before StreamingAssets/cache when a name is provided.
            _currentExperience = _builder.Load(string.IsNullOrWhiteSpace(ManifestName) ? null : ManifestName);
            if (_currentExperience != null)
            {
                // Fix: map SenseDescriptor -> Sense rather than attempting an invalid cast
                Senses.AddRange(_currentExperience.Senses.Select(sd => sd.Sense));
                Debug.Log($"ExperienceContext: Loaded experience '{_currentExperience.Name}' (id: {_currentExperience.Id}) with {Senses.Count} senses.");
                _loaded = true;
            }
            else
            {
                Debug.LogError($"ExperienceContext: failed to load experience '{ManifestName}'");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"ExperienceContext: exception while loading manifest '{ManifestName}': {ex}");
        }
    }

    private void OnEnterInitializing(IStateContext context)
    {
        _initialized = false;

        // perform any initialization depending on the senses
        Debug.Log($"ExperienceContext: Initializing experience '{_currentExperience?.Name ?? ManifestName}'");
        foreach (var s in Senses)
            Debug.Log($" - Sense: {s}");

        // mark initialized (non-blocking); in a real system you might wait for subsystems to signal readiness
        _initialized = true;
    }

    private void OnEnterActing(IStateContext context)
    {
        Debug.Log("ExperienceContext: Entering Acting state.");
    }

    private void OnUpdateActing(IStateContext context)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ExperienceContext: Escape pressed, scheduling shutdown.");
            _shouldShutdown = true;
        }
    }

    private void OnExtActing(IStateContext context)
    {
        // Hook for external signals
    }

    private void OnEnterShutdown(IStateContext context)
    {
        Debug.Log("ExperienceContext: Entering Shutdown state. Cleaning up.");
        Senses.Clear();
        _currentExperience = null;
        _loaded = false;
        _initialized = false;
        _shouldShutdown = false;
    }

    private bool Loaded(IStateContext context) => _loaded;

    private bool Initialized(IStateContext context) => _initialized;

    private bool ShouldShutdown(IStateContext context) => _shouldShutdown;

    void Start() { }

    void Update() { }
}

public enum Sense
{
    Vision = 0,
    Audio = 1,
    Touch = 2,
    Smell = 3,
    Taste = 4,
    Custom = 5,
    Custom01 = 6,
    Custom02 = 7,
    Custom03 = 8,
    Custom04 = 9,
    Custom05 = 10,
    Custom06 = 11,
    Custom07 = 12,
    Custom08 = 13,
    Custom09 = 14,
    Custom10 = 15,
}