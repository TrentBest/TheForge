using System;


namespace Assets.Scripts.Warlords
{
    [Serializable]
    public class PointOfInterest
    {
        public string Name;
        public int X;
        public int Y;
        public POIType Type;
        public int OwnerId;
        public int ProductionIncome;
    }
}