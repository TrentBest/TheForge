using System;

namespace Assets.Scripts.MastersOfOrionII
{
    [Serializable]
    public struct Ship
    {
        public int Id;
        public int DesignId;


        // --- STATE ---
        public float CurrentHP;
        public float Experience;     // 0 to 100 (Crew Veternancy)
        public float Fuel;           // Range limit


    }
}
