using UnityEngine;
using TheSingularityWorkshop.FSM_API;
using System.Collections.Generic;

namespace TheSingularityWorkshop.Sandbox.PillVirus
{
    public static class PillVirusLogic
    {
        public static void InitializeFSM()
        {
            global::TheSingularityWorkshop.FSM_API.FSM_API.Create.CreateFiniteStateMachine("PillVirusEngine", 1, "PillVirusGroup")
                .State("Spawning", OnSpawning, null, null)
                .State("Falling", null, OnFallingUpdate, null)
                .State("Evaluating", OnEvaluating, null, null)
                .State("Cascading", null, OnCascadingUpdate, null)
                .State("GameOver", null, null, null)

                .Transition("Spawning", "Falling", ctx => !((PillVirusContext)ctx).IsGameOver)
                .Transition("Spawning", "GameOver", ctx => ((PillVirusContext)ctx).IsGameOver)
                .Transition("Falling", "Evaluating", ctx => ((PillVirusContext)ctx).NeedsEvaluation)
                .Transition("Evaluating", "Cascading", ctx => ((PillVirusContext)ctx).NeedsCascade)
                .Transition("Evaluating", "Spawning", ctx => !((PillVirusContext)ctx).NeedsCascade)
                .Transition("Cascading", "Evaluating", ctx => ((PillVirusContext)ctx).NeedsEvaluation)
                .BuildDefinition();
        }

        // --- INPUT ROUTING ---
        public static void HandleInput(PillVirusContext ctx, KeyCode key)
        {
            if (ctx.NeedsEvaluation || ctx.NeedsCascade || ctx.IsGameOver) return;

            // Player 1 (WASD)
            if (key == KeyCode.A) MovePill(ctx, 0, -1, 0);
            if (key == KeyCode.D) MovePill(ctx, 0, 1, 0);
            if (key == KeyCode.S) MovePill(ctx, 0, 0, 1);
            if (key == KeyCode.W) RotatePill(ctx, 0);

            // Player 2 (Arrows)
            if (key == KeyCode.LeftArrow) MovePill(ctx, 1, -1, 0);
            if (key == KeyCode.RightArrow) MovePill(ctx, 1, 1, 0);
            if (key == KeyCode.DownArrow) MovePill(ctx, 1, 0, 1);
            if (key == KeyCode.UpArrow) RotatePill(ctx, 1);
        }

        private static void MovePill(PillVirusContext ctx, int playerId, int dx, int dy)
        {
            var pill = ctx.ActivePlayers.Find(p => p.PlayerId == playerId);
            if (pill == null) return;

            int newX = pill.X + dx;
            int newY = pill.Y + dy;

            if (IsValidPosition(ctx, pill, newX, newY, pill.IsHorizontal))
            {
                pill.X = newX;
                pill.Y = newY;
            }
        }

        private static void RotatePill(PillVirusContext ctx, int playerId)
        {
            var pill = ctx.ActivePlayers.Find(p => p.PlayerId == playerId);
            if (pill == null) return;

            bool newHorizontal = !pill.IsHorizontal;
            if (IsValidPosition(ctx, pill, pill.X, pill.Y, newHorizontal))
            {
                pill.IsHorizontal = newHorizontal;
                // Swap colors visually on rotate
                (pill.Color1, pill.Color2) = (pill.Color2, pill.Color1);
            }
        }

        private static bool IsValidPosition(PillVirusContext ctx, ActivePill pill, int x, int y, bool isHorizontal)
        {
            if (x < 0 || x >= ctx.Width || y < 0 || y >= ctx.Height) return false;
            if (ctx.Board[x, y].Type != CellType.Empty) return false;

            if (isHorizontal)
            {
                if (x + 1 >= ctx.Width || ctx.Board[x + 1, y].Type != CellType.Empty) return false;
            }
            else
            {
                if (y + 1 >= ctx.Height || ctx.Board[x, y + 1].Type != CellType.Empty) return false;
            }
            return true;
        }

        // --- FSM STATES ---
        private static void OnSpawning(IStateContext context)
        {
            var ctx = (PillVirusContext)context;
            ctx.NeedsEvaluation = false;

            for (int i = 0; i < ctx.PlayerCount; i++)
            {
                if (!ctx.ActivePlayers.Exists(p => p.PlayerId == i))
                    ctx.SpawnPillForPlayer(i);
            }
        }

        private static void OnFallingUpdate(IStateContext context)
        {
            var ctx = (PillVirusContext)context;

            ctx.FallTimer += Time.unscaledDeltaTime;
            if (ctx.FallTimer >= ctx.FallSpeed)
            {
                ctx.FallTimer = 0f;
                List<ActivePill> lockedPills = new List<ActivePill>();

                foreach (var pill in ctx.ActivePlayers)
                {
                    if (!IsValidPosition(ctx, pill, pill.X, pill.Y + 1, pill.IsHorizontal))
                    {
                        // Lock into grid
                        ctx.Board[pill.X, pill.Y] = new GridCell { Type = CellType.Pill, ColorId = pill.Color1, PlayerId = pill.PlayerId };

                        if (pill.IsHorizontal)
                            ctx.Board[pill.X + 1, pill.Y] = new GridCell { Type = CellType.Pill, ColorId = pill.Color2, PlayerId = pill.PlayerId };
                        else
                            ctx.Board[pill.X, pill.Y + 1] = new GridCell { Type = CellType.Pill, ColorId = pill.Color2, PlayerId = pill.PlayerId };

                        lockedPills.Add(pill);
                        ctx.NeedsEvaluation = true;
                    }
                    else
                    {
                        pill.Y++;
                    }
                }

                foreach (var locked in lockedPills) ctx.ActivePlayers.Remove(locked);
            }
        }

        private static void OnEvaluating(IStateContext context)
        {
            var ctx = (PillVirusContext)context;
            ctx.NeedsEvaluation = false;
            ctx.NeedsCascade = false;

            bool[,] toDestroy = new bool[ctx.Width, ctx.Height];

            // Horizontal Scan
            for (int y = 0; y < ctx.Height; y++)
            {
                for (int x = 0; x <= ctx.Width - ctx.MatchRequirement; x++)
                {
                    int color = ctx.Board[x, y].ColorId;
                    if (color == 0) continue;

                    int matchCount = 1;
                    while (x + matchCount < ctx.Width && ctx.Board[x + matchCount, y].ColorId == color)
                        matchCount++;

                    if (matchCount >= ctx.MatchRequirement)
                    {
                        for (int i = 0; i < matchCount; i++) toDestroy[x + i, y] = true;
                        ctx.NeedsCascade = true;
                    }
                }
            }

            // Vertical Scan
            for (int x = 0; x < ctx.Width; x++)
            {
                for (int y = 0; y <= ctx.Height - ctx.MatchRequirement; y++)
                {
                    int color = ctx.Board[x, y].ColorId;
                    if (color == 0) continue;

                    int matchCount = 1;
                    while (y + matchCount < ctx.Height && ctx.Board[x, y + matchCount].ColorId == color)
                        matchCount++;

                    if (matchCount >= ctx.MatchRequirement)
                    {
                        for (int i = 0; i < matchCount; i++) toDestroy[x, y + i] = true;
                        ctx.NeedsCascade = true;
                    }
                }
            }

            // Destroy Matched Cells
            if (ctx.NeedsCascade)
            {
                for (int x = 0; x < ctx.Width; x++)
                {
                    for (int y = 0; y < ctx.Height; y++)
                    {
                        if (toDestroy[x, y])
                            ctx.Board[x, y] = new GridCell { Type = CellType.Empty, ColorId = 0 };
                    }
                }
            }
        }

        private static void OnCascadingUpdate(IStateContext context)
        {
            var ctx = (PillVirusContext)context;
            ctx.NeedsCascade = false;

            // Simple Gravity: Bottom-up scan to pull floating pill pieces down
            // (A true implementation severs links here, but this drops floaters!)
            bool thingsMoved = false;
            for (int x = 0; x < ctx.Width; x++)
            {
                for (int y = ctx.Height - 2; y >= 0; y--)
                {
                    if (ctx.Board[x, y].Type == CellType.Pill && ctx.Board[x, y + 1].Type == CellType.Empty)
                    {
                        ctx.Board[x, y + 1] = ctx.Board[x, y];
                        ctx.Board[x, y] = new GridCell { Type = CellType.Empty, ColorId = 0 };
                        thingsMoved = true;
                    }
                }
            }

            if (thingsMoved) ctx.NeedsCascade = true; // Keep cascading until settled
            else ctx.NeedsEvaluation = true; // Re-evaluate for combo chains!
        }
    }
}