using System;
using System.Collections.Generic;

namespace Assets.Scripts.MastersOfOrionII
{
    [Serializable]
    public class ShipDesign
    {
        public int DesignId;      // Unique ID
        public string DisplayName;
        public HullSize Size;        // Frigate, Cruiser, Dreadnought

        // Components
        public int ArmorLevel;
        public int ShieldLevel;
        public int EngineLevel;
        public int ComputerLevel;

        // Weapons (List of weapon IDs)
        public List<string> WeaponMounts = new List<string>();

        // Stats (Calculated)
        public int ConstructionCost;
        public int MaintenanceCost;
        public int CombatPower;
    }
}
