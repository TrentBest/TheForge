namespace Assets.Scripts.GURPS.Data
{
    /// <summary>
    /// Represents a statistical "Class" of NPCs on the GPU.
    /// Used during "Experience Chronos" to simulate millions without instancing.
    /// </summary>
    [System.Serializable]
    public struct GURPS_PopulationBucket
    {
        public uint ClassId;       // e.g., "Martial Artist" or "Naval Officer"
        public int Count;          // Total number of units in this bucket

        // Statistical Norms (Mean and Standard Deviation)
        // GURPS 3d6 naturally centers at 10.5
        public float MeanST;
        public float StdDevST;

        public float MeanIQ;
        public float StdDevIQ;

        // Current "Heat" or Activity Level for this group
        public float ActivityLevel;
    }
}