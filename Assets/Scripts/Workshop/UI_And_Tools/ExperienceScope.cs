using System;
using System.Collections.Generic;

namespace Workshop
{
    /// <summary>
    /// Defines the spatial and temporal scale of an Experience.
    /// Drives how the DataWarehouse and GPU Render Pipelines interpret XYZ coordinates.
    /// </summary>
    public class ExperienceScope : IEquatable<ExperienceScope>
    {
        public string Id { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }

        public ExperienceScope(string id, string displayName, string description = "")
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
        }

        // --- Core Scales ---
        public static readonly ExperienceScope NotSet = new ExperienceScope("scope_none", "Undefined");

        public static readonly ExperienceScope Quantum = new ExperienceScope("scope_quantum", "Quantum", "Sub-atomic physics and wave-function simulations.");

        public static readonly ExperienceScope RoomScale = new ExperienceScope("scope_room", "Room Scale", "1:1 Human metric scale. Standard for Narrative and FPS.");

        public static readonly ExperienceScope Planetary = new ExperienceScope("scope_planetary", "Planetary", "City-building and terrain simulation scale.");

        public static readonly ExperienceScope SolarSystem = new ExperienceScope("scope_solar", "Solar System", "Orbital mechanics and interplanetary travel.");

        public static readonly ExperienceScope Galactic = new ExperienceScope("scope_galactic", "Galactic", "4X Strategy scale. 1 Unit = 1 Lightyear.");

        // --- Equality Overrides for Dictionary Mapping ---
        public bool Equals(ExperienceScope other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj) => Equals(obj as ExperienceScope);
        public override int GetHashCode() => Id != null ? Id.GetHashCode() : 0;
        public static bool operator ==(ExperienceScope left, ExperienceScope right) => EqualityComparer<ExperienceScope>.Default.Equals(left, right);
        public static bool operator !=(ExperienceScope left, ExperienceScope right) => !(left == right);
        public override string ToString() => DisplayName;
    }
}