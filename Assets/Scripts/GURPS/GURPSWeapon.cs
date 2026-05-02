using System;
using System.Collections.Generic;

namespace Workshop.GURPS
{
    [Serializable]
    public class GURPSWeapon
    {
        public string Id = Guid.NewGuid().ToString();
        public string Name = "New Weapon";
        public string SourceBookName = "GURPS Basic Set";
        public string Description = "";
        // These define where internal/external components attach
        public List<WeaponSocket> InternalSockets = new List<WeaponSocket>();

        // --- Tier 1: Classification ---
        public WeaponCategory Category = WeaponCategory.RangedKinetic;
        public int TechLevel = 7;
        public int LegalityClass = 4;

        // --- Tier 2: Cost & Weight ---
        public int Cost = 0;
        public float Weight = 1.0f;
        public int MinimumStrength = 10;

        // --- Tier 3: Damage & Type ---
        public string DamageString = "1d pi";
        public DamageType Type = DamageType.Crushing;
        public bool IsArmorPiercing = false;
        public int ArmorDivisor = 1; // e.g., (2) for lasers
        public string HalfDamageRange = "100";
        public string MaxRange = "1000";
        // --- Tier 4: 3e Revised Combat Stats ---
        public string Accuracy = "0";       // Acc
        public string Range = "100/1000";   // 1/2D & Max
        public string RateOfFire = "1";     // RoF
        public string Shots = "1";          // Shots
        public string Recoil = "1";         // Rcl
        public string Malfunction = "Crit"; // Malf
        public string SnapShot = "10";      // SS
        public string Bulk = "0";
        public GURPS_SocketType PrimarySocket = GURPS_SocketType.RightHand;
        public GURPS_SocketType HolsterSocket = GURPS_SocketType.HipLeft;
        // --- Tier 5: Logistical ---
        public string ReloadTime = "3";
        public string UsageBehavior = "One-handed";
        public string ImagePath = "";

        
       
    }
    public enum GURPS_SocketType { Head, Torso, RightHand, LeftHand, HipLeft, HipRight, Back }
    public enum WeaponCategory { Melee, RangedKinetic, RangedEnergy, Explosive, ShipScale }
    public enum DamageType { Crushing, Cutting, Impaling, Piercing, Burning, Special }
    [Serializable]
    public class WeaponSocket
    {
        public string SocketName; // e.g., "Chamber", "MagazineWell", "BatteryPort"
        public SocketPurpose Purpose;
        public string AcceptedItemId; // Optional: Restrict to specific ammo/battery types
    }

    public enum SocketPurpose { Chamber, Magazine, PowerCell, Optics, Underbarrel, InternalMechanism }
}