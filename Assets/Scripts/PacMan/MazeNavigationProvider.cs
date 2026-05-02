using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.PacMan
{
    public static class MazeNavigationProvider
    {
        public static void ProcessPacManMovement(PacManContext ctx)
        {
            // Capture Intent (Pong Pattern)
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) ctx.IntentDirection = Vector2Int.up;
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) ctx.IntentDirection = Vector2Int.down;
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) ctx.IntentDirection = Vector2Int.left;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) ctx.IntentDirection = Vector2Int.right;

            // Try to turn if intent is clear
            if (IsValidMove(ctx, ctx.PacManPosition + ctx.IntentDirection))
                ctx.CurrentDirection = ctx.IntentDirection;

            // Resolve Move
            Vector2Int nextPos = ctx.PacManPosition + ctx.CurrentDirection;
            if (IsValidMove(ctx, nextPos)) ctx.PacManPosition = nextPos;
        }

        public static void ProcessGhostMovement(PacManContext ctx, GhostContext ghost)
        {
            // Set Target based on state
            if (ctx.IsEnergized) ghost.TargetTile = new Vector2Int(0, 0); // Flee to corner
            else ghost.TargetTile = ctx.PacManPosition; // Simple Chase

            Vector2Int bestMove = ghost.Position;
            float minDist = float.MaxValue;

            // Check all 4 cardinal directions
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in directions)
            {
                Vector2Int potential = ghost.Position + dir;
                if (IsValidMove(ctx, potential))
                {
                    float dist = Vector2.Distance(potential, ghost.TargetTile);
                    if (dist < minDist) { minDist = dist; bestMove = potential; }
                }
            }
            ghost.Position = bestMove;
        }

        public static bool IsValidMove(PacManContext ctx, Vector2Int pos)
        {
            if (pos.x < 0 || pos.x >= ctx.Width || pos.y < 0 || pos.y >= ctx.Height) return false;
            return ctx.MazeGrid[pos.x, pos.y] != GridTile.Wall;
        }

        public static void EvaluateGameState(PacManContext ctx)
        {
            GridTile currentTile = ctx.MazeGrid[ctx.PacManPosition.x, ctx.PacManPosition.y];

            // Consumption Logic
            if (currentTile == GridTile.Pellet)
            {
                ctx.Score += 10;
                ctx.MazeGrid[ctx.PacManPosition.x, ctx.PacManPosition.y] = GridTile.Empty;
                PacManAcousticEngine.Play(PacManAcousticEngine.WakaChime);
            }
            else if (currentTile == GridTile.PowerPellet)
            {
                ctx.Score += 50;
                ctx.MazeGrid[ctx.PacManPosition.x, ctx.PacManPosition.y] = GridTile.Empty;
                ctx.Handle.TransitionTo("Energized"); // Manual transition
            }

            // Collision with Ghosts
            foreach (var ghost in ctx.Ghosts)
            {
                if (ghost.Position == ctx.PacManPosition)
                {
                    if (ctx.IsEnergized)
                    {
                        ghost.Position = ghost.SpawnPoint; // Eat ghost
                        ctx.Score += 200;
                    }
                    else
                    {
                        HandlePacManDeath(ctx);
                    }
                }
            }
        }

        private static void HandlePacManDeath(PacManContext ctx)
        {
            ctx.Lives--;
            if (ctx.Lives <= 0) ctx.Handle.TransitionTo("GameOver");
            else ResetEntities(ctx);
        }

        public static void ResetEntities(PacManContext ctx)
        {
            ctx.PacManPosition = ctx.PacManSpawn;
            foreach (var ghost in ctx.Ghosts) ghost.Position = ghost.SpawnPoint;
            ctx.Handle.TransitionTo("Ready");
        }
    }
}