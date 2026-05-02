using UnityEngine;

namespace Workshop.GURPS.Construction
{
    public class WallBuilder
    {
        public float Length;
        public float Height;
        public WallMaterial Material = WallMaterial.Stone;

        public int CalculateBaseDR()
        {
            return Material switch
            {
                WallMaterial.Wood => 2,
                WallMaterial.Stone => 10,
                WallMaterial.ReinforcedConcrete => 50,
                WallMaterial.HyperPlast => 200, // TL 10+
                _ => 1
            };
        }
    }

    public enum WallMaterial { Wood, Stone, Brick, ReinforcedConcrete, HyperPlast }
}