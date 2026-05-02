using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    public static class HexMath
    {
        // The distance from the center of the hex to any corner.
        // If your hex tile is 100 units wide, the radius is roughly 57.7f.
        public static float HexRadius = 50f;

        // 1. HEX TO WORLD (Placing the Stamp)
        // Converts a logical hex coordinate (e.g., Q:1, R:2) into an exact 3D world position
        public static Vector3 HexToWorld(int q, int r)
        {
            // Pointy-topped hex math
            float x = HexRadius * Mathf.Sqrt(3f) * (q + r / 2f);
            float z = HexRadius * (3f / 2f) * r;

            return new Vector3(x, 0, z);
        }

        // 2. WORLD TO HEX (Selecting a Tile on the Terrain)
        // Converts a 3D mouse click (X, Z) into the logical hex coordinate (Q, R)
        public static Vector2Int WorldToHex(Vector3 worldPos)
        {
            // Inverse of the HexToWorld math to get "Fractional" coordinates
            float q = (Mathf.Sqrt(3f) / 3f * worldPos.x - 1f / 3f * worldPos.z) / HexRadius;
            float r = (2f / 3f * worldPos.z) / HexRadius;

            // Round the fractional coordinates to the nearest whole hex
            return HexRound(q, r);
        }

        // 3. HEX ROUNDING ALGORITHM
        // Because of the staggered grid, standard Mathf.Round doesn't work.
        // We must convert to a 3-axis "Cube" coordinate, round the largest difference, and convert back.
        private static Vector2Int HexRound(float fracQ, float fracR)
        {
            float fracS = -fracQ - fracR;

            int q = Mathf.RoundToInt(fracQ);
            int r = Mathf.RoundToInt(fracR);
            int s = Mathf.RoundToInt(fracS);

            float qDiff = Mathf.Abs(q - fracQ);
            float rDiff = Mathf.Abs(r - fracR);
            float sDiff = Mathf.Abs(s - fracS);

            if (qDiff > rDiff && qDiff > sDiff)
            {
                q = -r - s;
            }
            else if (rDiff > sDiff)
            {
                r = -q - s;
            }

            return new Vector2Int(q, r);
        }

        // Bonus: Get all 6 neighbors of a hex!
        public static Vector2Int[] GetNeighbors(Vector2Int hex)
        {
            return new Vector2Int[] {
                new Vector2Int(hex.x + 1, hex.y),     // Right
                new Vector2Int(hex.x + 1, hex.y - 1), // Bottom Right
                new Vector2Int(hex.x, hex.y - 1),     // Bottom Left
                new Vector2Int(hex.x - 1, hex.y),     // Left
                new Vector2Int(hex.x - 1, hex.y + 1), // Top Left
                new Vector2Int(hex.x, hex.y + 1)      // Top Right
            };
        }
    }
}