// File: Assets/Scripts/Workshop/Cast/Role.cs
using System;
using UnityEngine;

namespace TheSingularityWorkshop.Cast
{
    [Serializable]
    public class Role
    {
        [Header("Identity")]
        public byte Id { get; set; }
        public string Name { get; set; }

        [Header("Presentation")]
        public Color NameplateColor { get; set; }
        public string IconResourcePath { get; set; }

        [Header("Behavioral Defaults")]
        public bool IsUnique { get; set; } // E.g., Can there be only ONE Protagonist?
        public float BaseAggression { get; set; } // 0.0 (Pacifist) to 1.0 (Hostile)
        public float DefaultArmorBonus { get; set; }

        // --- 1. Built-in Defaults (The original enum values) ---
        public static readonly Role Protagonist = new Role { Id = 0, Name = "Protagonist", NameplateColor = Color.green, IsUnique = true, BaseAggression = 0f };
        public static readonly Role Antagonist = new Role { Id = 1, Name = "Antagonist", NameplateColor = Color.red, IsUnique = false, BaseAggression = 1f };
        public static readonly Role Supporting = new Role { Id = 2, Name = "Supporting", NameplateColor = Color.cyan, IsUnique = false, BaseAggression = 0.2f };
        public static readonly Role Extra = new Role { Id = 3, Name = "Extra", NameplateColor = Color.gray, IsUnique = false, BaseAggression = 0f };

        // --- 2. Backwards Compatibility & Conversions ---
        public static implicit operator byte(Role role) => role?.Id ?? 3;
        public static implicit operator int(Role role) => role?.Id ?? 3;
        public static implicit operator Role(byte id) => GetById(id);
        public static implicit operator Role(int id) => GetById((byte)id);

        // --- 3. ID Resolution ---
        public static Role GetById(byte id)
        {
            // Note: In your full ecosystem, this should ideally query your DataWarehouse 
            // or a dynamic Registry so user-created Roles (Id 4, 5, 6...) are returned.
            return id switch
            {
                0 => Protagonist,
                1 => Antagonist,
                2 => Supporting,
                3 => Extra,
                _ => Extra // Default fallback
            };
        }

        // --- 4. NEW: String Resolution (Replaces Enum Parsing!) ---

        /// <summary>
        /// Converts a string name back into the rich Role object.
        /// </summary>
        public static Role GetByName(string name)
        {
            // Just like GetById, eventually this can query your DataWarehouse for custom user-roles!
            return name switch
            {
                "Protagonist" => Protagonist,
                "Antagonist" => Antagonist,
                "Supporting" => Supporting,
                "Extra" => Extra,
                _ => Extra // Default fallback
            };
        }

        /// <summary>
        /// Mimics Enum.TryParse so your UI Dropdowns can safely convert strings back to Roles.
        /// </summary>
        public static bool TryParse(string name, out Role result)
        {
            result = GetByName(name);

            // If the name isn't recognized, GetByName defaults to Extra.
            // We only consider it a "Failed" parse if it returned Extra BUT the user didn't actually type "Extra".
            return result != Extra || name == "Extra";
        }
    }
}