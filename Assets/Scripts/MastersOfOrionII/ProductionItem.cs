using System;

namespace Assets.Scripts.MastersOfOrionII
{
    [Serializable]
    public class ProductionItem
    {
        public string ItemId;        // "Building_Factory" or "Ship_Cruiser"
        public bool IsUnit;          // True if Ship/Troop, False if Building
        public float ProductionCost; // Total industry needed
        public float Progress;       // Current industry invested
    }
}
