using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_API.Scripts;
using UnityEngine;
using Workshop.Systems.MicroPackages;

namespace Workshop.Systems.MicroPackages.Packages.DesignPatterns.Factory
{
    /// <summary>
    /// The strict contract for any Factory operating within the Singularity ecosystem.
    /// </summary>
    public interface IForgeFactory<T>
    {
        string FactoryName { get; }
        T Manufacture(string blueprintId, IStateContext contextOverride = null);
        bool CanManufacture(string blueprintId);
    }

    /// <summary>
    /// Holds the exact FSM routing data required to spawn and wire an entity into the ecosystem.
    /// </summary>
    public class EntityBlueprint
    {
        public Func<GameObject> Constructor;
        public string FsmName;
        public string ProcessGroup;
        public string InitialState;

        public EntityBlueprint(Func<GameObject> constructor, string fsmName, string processGroup, string initialState)
        {
            Constructor = constructor;
            FsmName = fsmName;
            ProcessGroup = processGroup;
            InitialState = initialState;
        }
    }

    /// <summary>
    /// The standard implementation for generating Unity GameObjects tightly coupled to the FSM API.
    /// </summary>
    public class ForgeEntityFactory : IForgeFactory<GameObject>
    {
        public string FactoryName { get; }
        private readonly Dictionary<string, EntityBlueprint> _blueprints = new();

        public ForgeEntityFactory(string name)
        {
            FactoryName = name;
        }

        public void RegisterBlueprint(string blueprintId, EntityBlueprint blueprint)
        {
            FSM_API.Create.CreateProcessingGroup(blueprint.ProcessGroup);
            _blueprints[blueprintId] = blueprint;
        }

        public GameObject Manufacture(string id, IStateContext contextOverride = null)
        {
            if (!_blueprints.TryGetValue(id, out var blueprint))
            {
                Debug.LogError($"[ForgeEntityFactory:{FactoryName}] No blueprint registered for '{id}'");
                return null;
            }

            GameObject instance = blueprint.Constructor();
            IStateContext context = contextOverride ?? new DefaultEntityContext(instance);

            FSMHandle handle = FSM_API.Create.CreateInstance(
                blueprint.FsmName,
                context,
                blueprint.ProcessGroup
            );

            if (handle != null && handle.IsValid)
            {
                if (!string.IsNullOrEmpty(blueprint.InitialState))
                {
                    handle.TransitionTo(blueprint.InitialState);
                    Debug.Log($"[ForgeEntityFactory] Manufactured '{id}'. Handle bound to [{blueprint.ProcessGroup} :: {blueprint.FsmName}]. Transitioning to '{blueprint.InitialState}'.");
                }
            }
            else
            {
                Debug.LogError($"[ForgeEntityFactory] Failed to generate valid FSMHandle for {blueprint.FsmName} on {blueprint.ProcessGroup}.");
            }

            return instance;
        }

        public bool CanManufacture(string id) => _blueprints.ContainsKey(id);
    }

    public class DefaultEntityContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; }
        public GameObject RootEntity { get; private set; }

        public DefaultEntityContext(GameObject root)
        {
            RootEntity = root;
            Name = root.name;
        }
    }

    /// <summary>
    /// The MicroPackage definition that loads this pattern into the Arbitrator.
    /// Provides the foundational infrastructure for entity generation.
    /// </summary>
    public class FactoryPatternMicroPackage : IMicroPackage, IArchivalMetadata
    {
        // --- Core Identity ---
        public string PackageId { get; set; } = "Workshop.DesignPatterns.Factory";
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new();

        // --- Architectural Intent ---
        // Universal because factories spawn objects both in the Forge (tools) and the Experience (enemies/loot).
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // Pure logic arrives as a data stream
        public string DeliveryProfileId => "Delivery_DigitalDataStream";

        // Requires the FSM core to bind handles to manufactured instances
        public string[] GetDependencies() => new[] { "Workshop.Core.FSM" };

        public string[] GetExclusions() => new string[0];

        #region --- IArchivalMetadata (The Data Card) ---

        public string Version => "1.0.0";
        public string Author => "The Singularity Workshop";
        public ForgeSpatialZone SpatialZone => ForgeSpatialZone.LogicAndCompute; // The Cortex

        public string ShortDescription => "Foundational Factory Pattern for FSM-bound entities.";
        public string LongDescription => "Provides high-level IForgeFactory abstractions to decouple physical GameObject instantiation from behavioral FSM binding. Essential for complex entity spawning and creator tool manifestation.";
        public string ThumbnailUrl => "Textures/Thumbnails/Archive_DesignPatterns_Factory";

        public int ReleaseYear => 1994; // Original Design Patterns (Gang of Four) year
        public string OriginalAuthor => "Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides";
        public string OriginalPublisher => "Addison-Wesley";
        public string OriginalPlatform => "C++ / Smalltalk";
        public string Era => "The Architectural Renaissance";
        public string HistoricalSignificance => "Established the standard for creational logic; reforged here to bridge the gap between Unity Prefabs and the FSM API.";
        public Color AccentColor => new Color(0.2f, 0.7f, 0.4f); // Assembly Line Green

        #endregion

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[The Cortex]</b> {PackageId} online. IForgeFactory abstractions available to the ecosystem.");
        }

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            // Passive infrastructure; does not require convergence passes.
            arbitrator.None();
        }
    }
}