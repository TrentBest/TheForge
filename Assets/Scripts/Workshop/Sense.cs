using System;
using System.Collections.Generic;

namespace Workshop
{
    /// <summary>
    /// Represents a sensory capability within the Experience. 
    /// Designed as an extensible class to allow for dynamic, data-driven expansion 
    /// (e.g., Psionics, Magical Detection, Cybernetic Scanners) without recompiling.
    /// </summary>
    public class Sense : IEquatable<Sense>
    {
        public string Id { get; private set; }
        public string DisplayName { get; private set; }
        public string Category { get; private set; } // e.g., "Physical", "Mental", "Magical", "Cybernetic"
        public string Description { get; private set; }

        public Sense(string id, string displayName, string category = "Physical", string description = "")
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            Description = description;
        }

        // ==========================================
        // BUILT-IN CORE SENSES (Replaces the Enum)
        // ==========================================
        public static readonly Sense Vision = new Sense("sense_vision", "Vision");
        public static readonly Sense Audio = new Sense("sense_audio", "Hearing");
        public static readonly Sense Touch = new Sense("sense_touch", "Touch");
        public static readonly Sense Smell = new Sense("sense_smell", "Smell");
        public static readonly Sense Taste = new Sense("sense_taste", "Taste");

        // ==========================================
        // EXAMPLES OF DYNAMIC EXPANSION
        // ==========================================
        public static readonly Sense Psionic = new Sense(
            "sense_psionic",
            "Psionic Resonance",
            "Mental",
            "The ability to perceive thoughts, emotional echoes, and psychic disturbances."
        );

        public static readonly Sense ArcaneSight = new Sense(
            "sense_arcane",
            "Arcane Sight",
            "Magical",
            "Perception of mana lines, spell residues, and magical auras."
        );

        // ==========================================
        // EQUALITY OVERRIDES 
        // (Crucial so these work perfectly as Dictionary Keys in the DataWarehouse)
        // ==========================================
        public bool Equals(Sense other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj) => Equals(obj as Sense);

        public override int GetHashCode() => Id != null ? Id.GetHashCode() : 0;

        public static bool operator ==(Sense left, Sense right) => EqualityComparer<Sense>.Default.Equals(left, right);

        public static bool operator !=(Sense left, Sense right) => !(left == right);

        public override string ToString() => DisplayName;
    }
}