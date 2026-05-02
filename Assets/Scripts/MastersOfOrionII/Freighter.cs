
using Assets.Scripts.Workshop.Core.Physics.Chemistry;
using System;

namespace Assets.Scripts.MastersOfOrionII
{
    [Serializable]
    public class Freighter
    {
        public int Id;
        public string Name;
        public string Status = "Idle";
        public float Health = 100f;
        public float Fuel = 100f;
        public int CargoCapacity = 500;
        public int CurrentCargo = 0;

        public Freighter(int id)
        {
            Id = id;
            Name = $"STC-{Id:000}"; // Standard Transport Class
        }
    }
}
