using UnityEngine;

namespace Assets.Scripts.MastersOfOrionII
{
    // --- ANOMALIES ---

    /// <summary>
    /// Fast travel points.
    /// </summary>
    public struct Wormhole
    {
        public int Id;
        public Vector2Int DestinationCoordinate; // Where does it spit you out?
        public bool IsStable; // Can it collapse?
        public int MaxMassTransit; // Max ship size allowed?
    }
}
