using UnityEngine;
using Workshop.BardsTale.World;
using Workshop.Core.Diagnostics;

namespace Workshop.BardsTale.Logic
{
    public class BardsTaleExplorationProvider
    {
        private const int GRID_SIZE = 22; // Classic Bard's Tale map size

        public void Tick(BardsTaleExperienceContext ctx)
        {
            if (ctx.IsMoving) return;

            // Simple Input Check (Wire to ForgeInputContext later)
            if (Input.GetKeyDown(KeyCode.W)) TryMove(ctx, ctx.CurrentFacingDirection);
            if (Input.GetKeyDown(KeyCode.S)) TryMove(ctx, -ctx.CurrentFacingDirection);
            if (Input.GetKeyDown(KeyCode.A)) Rotate(ctx, -1); // Left 90
            if (Input.GetKeyDown(KeyCode.D)) Rotate(ctx, 1);  // Right 90
        }

        private void TryMove(BardsTaleExperienceContext ctx, Vector2Int delta)
        {
            Vector2Int nextPos = ctx.CurrentGridPosition + delta;

            // Handle Wrap-around (Pac-Man logic)
            nextPos.x = (nextPos.x + GRID_SIZE) % GRID_SIZE;
            nextPos.y = (nextPos.y + GRID_SIZE) % GRID_SIZE;

            // Collision Check via MapContext
            if (ctx.CurrentMap.GetWall(ctx.CurrentGridPosition, delta) == TileFeature.Wall)
            {
                ForgeLogger.Log("BONK! Wall ahead.");
                return;
            }

            ctx.CurrentGridPosition = nextPos;
            ForgeLogger.Log($"Moved to: {ctx.CurrentGridPosition}");
        }

        private void Rotate(BardsTaleExperienceContext ctx, int direction)
        {
            // Simple 90-degree turn logic
            int x = ctx.CurrentFacingDirection.x;
            int y = ctx.CurrentFacingDirection.y;

            // Direction 1 = Right, -1 = Left
            ctx.CurrentFacingDirection = (direction == 1)
                ? new Vector2Int(y, -x)
                : new Vector2Int(-y, x);

            ForgeLogger.Log($"Facing: {ctx.CurrentFacingDirection}");
        }
    }
}