using System;


namespace Assets.Scripts.Warlords
{
    [Serializable]
    public enum TerrainType : byte
    {
        OpenWater = 0, Plains = 1, Forest = 2, Hills = 3, Mountains = 4, Swamp = 5, Road = 6, Bridge = 7, HorizontalRiverBottom = 8, HorizontalRiverTop = 9, VerticalRiverRight = 10, VerticalRiverLeft = 11
    }
}