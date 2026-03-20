using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Assets.Scripts.raWWar
{
    [Flags]
    public enum Qualifications : uint
    {
        None = 0,
        BasicInfantry = 1 << 0,
        HeavyWeapons = 1 << 1,
        ArmoredCrew = 1 << 2,
        AtmosphericPilot = 1 << 3,
        OrbitalDrop = 1 << 4,
        // Up to 32 different qualifications
    }

    [Flags]
    public enum Medals : uint
    {
        None = 0,
        PurpleHeart = 1 << 0,
        ValorCross = 1 << 1,
        GalacticCampaign = 1 << 2,
        // Up to 32 distinct medals
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SoldierData
    {
        // Health and Mental Health mapped to 0-255 to save space
        public byte Health;
        public byte MentalHealth;

        // Status flags (e.g., 0 = Alive, 1 = Wounded, 2 = KIA, 3 = AWOL)
        public byte Status;

        // Padding to keep struct alignment optimal (4 bytes so far, let's pad to 4 or 8)
        public byte Rank;

        // Bitmasks for highly compressed state
        public Qualifications Qualifications;
        public Medals Medals;

        // Total Size per Soldier: 12 bytes.
        // 1 Million Soldiers = ~11.4 Megabytes.
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SquadData
    {
        public uint SquadID;

        // The base seed used with Squirrel3 to generate names/faces for this squad just-in-time
        public uint GenerationSeed;

        public uint AssignedVehicleID; // 0 if unassigned

        // Squad capacity based on research (4 to 8)
        public byte ActiveSize;
        public byte Morale; // Squad cohesive morale
        public ushort LocationId; // Planet, Barracks, or Fleet ID

        // Fixed array for the 8 potential soldier slots. 
        // Storing the actual index of the soldier in the global Soldier array.
        public fixed int SoldierIndices[8];

        // Total Size per Squad: ~44 bytes.
    }
}
