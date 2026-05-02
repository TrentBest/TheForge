using System;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    [Serializable]
    public class UrbanZoneType
    {
        // Your existing properties
        public byte Id { get; set; }
        public string Name { get; set; }
        public Color DisplayColor { get;  set; }

        // public Color DisplayColor { get; set; } // (Assuming you have this or similar)

        // --- 1. Fix: "does not contain a definition for X" ---
        public static readonly UrbanZoneType Wilderness = new UrbanZoneType { Id = 0, Name = "Wilderness" };
        public static readonly UrbanZoneType Residential = new UrbanZoneType { Id = 1, Name = "Residential" };
        public static readonly UrbanZoneType Commercial = new UrbanZoneType { Id = 2, Name = "Commercial" };
        public static readonly UrbanZoneType Industrial = new UrbanZoneType { Id = 3, Name = "Industrial" };
        public static readonly UrbanZoneType Military = new UrbanZoneType { Id = 4, Name = "Military" };
        public static readonly UrbanZoneType Spaceport = new UrbanZoneType { Id = 5, Name = "Spaceport" };

        // --- 2. Fix: "cannot convert from UrbanZoneType to byte/int" ---
        public static implicit operator byte(UrbanZoneType zone) => zone?.Id ?? 0;
        public static implicit operator int(UrbanZoneType zone) => zone?.Id ?? 0;

        // --- 3. Fix: "Cannot implicitly convert type 'byte' to UrbanZoneType" ---
        public static implicit operator UrbanZoneType(byte id) => GetById(id);
        public static implicit operator UrbanZoneType(int id) => GetById((byte)id);

        // Helper to resolve the correct object instance when casting from a byte
        public static UrbanZoneType GetById(byte id)
        {
            return id switch
            {
                1 => Residential,
                2 => Commercial,
                3 => Industrial,
                4 => Military,
                5 => Spaceport,
                _ => Wilderness // Default fallback
            };
        }
    }
}