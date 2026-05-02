using UnityEngine;

namespace Assets.Scripts.MastersOfOrionII
{
    /// <summary>
    /// The container for a solar system. 
    /// This is what you click on in the "Galaxy View".
    /// </summary>
    public struct StarSystem
    {
        public int Id;          // Unique Hash
        public int Seed;        // Specific seed for this system
        public Vector3 GridCoordinate; // Where it is on the map

        // System wide aggregates (useful for AI or quick scanning)
        public int StarCount;
        public int PlanetCount;
        public bool HasAnomalies;
    }
}
