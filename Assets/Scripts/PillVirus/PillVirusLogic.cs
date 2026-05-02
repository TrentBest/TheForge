using UnityEngine;
using TheSingularityWorkshop.FSM_API;
using System.Collections.Generic;

namespace Assets.Scripts.PillVirus
{
    public static class PillVirusLogic
    {
        public static void InitializeFSM()
        {
            FSM_API.Create.CreateFiniteStateMachine("PillVirusEngine", 1, "PillVirusGroup")
                 .State("Spawning", OnSpawning, null, null)
                 .State("Falling", null, OnFallingUpdate, null)
                 .State("Evaluating", OnEvaluating, null, null)
                 .State("Cascading", null, OnCascadingUpdate, null)
                 .State("GameOver", null, null, null)
                 .State("Victory", null, null, null)

                 .Transition("Spawning", "Falling", ctx => !((PillVirusContext)ctx).IsGameOver && !((PillVirusContext)ctx).NeedsEvaluation)
                 .Transition("Spawning", "Cascading", ctx => !((PillVirusContext)ctx).IsGameOver && ((PillVirusContext)ctx).NeedsEvaluation)
                 .Transition("Spawning", "GameOver", ctx => ((PillVirusContext)ctx).IsGameOver)
                 .Transition("Falling", "Evaluating", ctx => ((PillVirusContext)ctx).NeedsEvaluation)
                 .Transition("Evaluating", "Victory", ctx => ((PillVirusContext)ctx).IsVictory)
                 .Transition("Evaluating", "Cascading", ctx => ((PillVirusContext)ctx).NeedsCascade && !((PillVirusContext)ctx).IsVictory)
                 .Transition("Evaluating", "Spawning", ctx => !((PillVirusContext)ctx).NeedsCascade && !((PillVirusContext)ctx).IsVictory)
                 .Transition("Cascading", "Evaluating", ctx => ((PillVirusContext)ctx).NeedsEvaluation)
                 .BuildDefinition();
        }

        public static void HandleInput(PillVirusContext ctx, KeyCode key, bool isDown)
        {
            if (isDown) ctx.HeldKeys.Add(key); else ctx.HeldKeys.Remove(key);
            if (!isDown || ctx.NeedsEvaluation || ctx.NeedsCascade || ctx.IsGameOver || ctx.ActivePlayers.Count == 0) return;

            var p = ctx.ActivePlayers[0];

            // D-Pad: Left/Right
            if (key == KeyCode.LeftArrow || key == KeyCode.A) TryMove(ctx, p, -1, 0);
            if (key == KeyCode.RightArrow || key == KeyCode.D) TryMove(ctx, p, 1, 0);

            // Buttons: A (CW) and B (CCW)
            if (key == KeyCode.K || key == KeyCode.Z || key == KeyCode.W) TryRotate(ctx, p, true);
            if (key == KeyCode.L || key == KeyCode.X || key == KeyCode.E) TryRotate(ctx, p, false);
        }

        private static void TryMove(PillVirusContext ctx, ActivePill p, int dx, int dy)
        {
            if (IsValid(ctx, p.X + dx, p.Y + dy, p.IsHorizontal))
            {
                p.X += dx; p.Y += dy;
                if (p.IsResting) p.LockTimer = 0f; // Reset slip-time on move
                PillVirusAcousticEngine.Play(PillVirusAcousticEngine.Move);
            }
        }

        private static void TryRotate(PillVirusContext ctx, ActivePill p, bool cw)
        {
            bool nextH = !p.IsHorizontal;
            if (IsValid(ctx, p.X, p.Y, nextH)) ApplyRot(p, nextH, cw);
            else if (IsValid(ctx, p.X - 1, p.Y, nextH)) { p.X--; ApplyRot(p, nextH, cw); } // Kick Left
            else if (IsValid(ctx, p.X + 1, p.Y, nextH)) { p.X++; ApplyRot(p, nextH, cw); } // Kick Right
        }

        private static void ApplyRot(ActivePill p, bool h, bool cw)
        {
            p.IsHorizontal = h;
            if (cw) (p.Color1, p.Color2) = (p.Color2, p.Color1);
            p.LockTimer = 0f;
            PillVirusAcousticEngine.Play(PillVirusAcousticEngine.Rotate);
        }

        private static bool IsValid(PillVirusContext ctx, int x, int y, bool h)
        {
            if (x < 0 || y < 0 || y >= ctx.Height || (h && x + 1 >= ctx.Width) || (!h && y + 1 >= ctx.Height)) return false;
            if (ctx.Board[x, y].Type != CellType.Empty) return false;
            return h ? ctx.Board[x + 1, y].Type == CellType.Empty : ctx.Board[x, y + 1].Type == CellType.Empty;
        }

        private static void OnSpawning(IStateContext context)
        {
            var ctx = (PillVirusContext)context;
            ctx.ComboMultiplier = 1;

            if (ctx.PendingGarbage > 0)
            {
                for (int i = 0; i < ctx.PendingGarbage; i++)
                {
                    int rx = Random.Range(0, ctx.Width);
                    if (ctx.Board[rx, 0].Type == CellType.Empty)
                        ctx.Board[rx, 0] = new GridCell { Type = CellType.Pill, ColorId = Random.Range(1, 4), LinkId = 0 };
                }
                ctx.PendingGarbage = 0; ctx.NeedsEvaluation = true; return;
            }

            if (ctx.Board[3, 0].Type != CellType.Empty) { ctx.IsGameOver = true; PillVirusAcousticEngine.Play(PillVirusAcousticEngine.Death); return; }
            ctx.ActivePlayers.Add(new ActivePill { X = 3, Y = 0, Color1 = ctx.NextColor1, Color2 = ctx.NextColor2 });
            ctx.NextColor1 = Random.Range(1, 4); ctx.NextColor2 = Random.Range(1, 4);
            ctx.NeedsEvaluation = false;
        }

        public static void OnFallingUpdate(IStateContext context)
        {
            var ctx = (PillVirusContext)context;
            if (ctx.ActivePlayers.Count == 0) return;

            bool soft = ctx.HeldKeys.Contains(KeyCode.DownArrow) || ctx.HeldKeys.Contains(KeyCode.S);
            float speed = soft ? 0.05f : ctx.FallSpeed;

            ctx.FallTimer += Time.unscaledDeltaTime;
            if (ctx.FallTimer >= speed)
            {
                ctx.FallTimer = 0;
                var p = ctx.ActivePlayers[0]; // FIXED: Removed 'var' conflict from previous version
                if (IsValid(ctx, p.X, p.Y + 1, p.IsHorizontal)) { p.Y++; p.IsResting = false; }
                else
                {
                    p.IsResting = true; p.LockTimer += speed;
                    if (p.LockTimer >= p.LockDelay || soft) { LockPill(ctx, p); ctx.NeedsEvaluation = true; PillVirusAcousticEngine.Play(PillVirusAcousticEngine.Land); }
                }
            }
        }

        private static void LockPill(PillVirusContext ctx, ActivePill p)
        {
            int lid = Random.Range(100, 9999);
            ctx.Board[p.X, p.Y] = new GridCell { Type = CellType.Pill, ColorId = p.Color1, LinkId = lid };
            if (p.IsHorizontal) ctx.Board[p.X + 1, p.Y] = new GridCell { Type = CellType.Pill, ColorId = p.Color2, LinkId = lid };
            else ctx.Board[p.X, p.Y + 1] = new GridCell { Type = CellType.Pill, ColorId = p.Color2, LinkId = lid };
            ctx.ActivePlayers.Clear();
        }

        private static void OnEvaluating(IStateContext context)
        {
            var ctx = (PillVirusContext)context;
            ctx.NeedsEvaluation = false; ctx.NeedsCascade = false;
            bool[,] toDestroy = new bool[ctx.Width, ctx.Height];
            int matchCount = 0;

            for (int y = 0; y < ctx.Height; y++)
                for (int x = 0; x < ctx.Width; x++)
                {
                    int c = ctx.Board[x, y].ColorId; if (c == 0) continue;
                    int h = 1; while (x + h < ctx.Width && ctx.Board[x + h, y].ColorId == c) h++;
                    if (h >= 4) { matchCount++; for (int i = 0; i < h; i++) toDestroy[x + i, y] = true; }
                    int v = 1; while (y + v < ctx.Height && ctx.Board[x, y + v].ColorId == c) v++;
                    if (v >= 4) { matchCount++; for (int i = 0; i < v; i++) toDestroy[x, y + i] = true; }
                }

            if (matchCount > 0)
            {
                if (matchCount >= 2) ctx.PendingGarbage += (matchCount - 1);
                int virusesCleared = 0;
                for (int y = 0; y < ctx.Height; y++)
                    for (int x = 0; x < ctx.Width; x++)
                        if (toDestroy[x, y])
                        {
                            if (ctx.Board[x, y].Type == CellType.Virus) virusesCleared++;
                            int lid = ctx.Board[x, y].LinkId;
                            if (lid != 0)
                            { // Sever Link of partner
                                if (x > 0 && ctx.Board[x - 1, y].LinkId == lid) ctx.Board[x - 1, y].LinkId = 0;
                                if (x < ctx.Width - 1 && ctx.Board[x + 1, y].LinkId == lid) ctx.Board[x + 1, y].LinkId = 0;
                                if (y > 0 && ctx.Board[x, y - 1].LinkId == lid) ctx.Board[x, y - 1].LinkId = 0;
                                if (y < ctx.Height - 1 && ctx.Board[x, y + 1].LinkId == lid) ctx.Board[x, y + 1].LinkId = 0;
                            }
                            ctx.Board[x, y] = new GridCell { Type = CellType.Empty };
                        }
                ctx.RemainingViruses -= virusesCleared;
                ctx.CurrentScore += (virusesCleared * 100 * ctx.ComboMultiplier);
                ctx.NeedsCascade = true; ctx.ComboMultiplier++;
                PillVirusAcousticEngine.Play(PillVirusAcousticEngine.Clear);
                if (ctx.RemainingViruses <= 0) { ctx.IsVictory = true; PillVirusAcousticEngine.Play(PillVirusAcousticEngine.Victory); }
            }
        }

        private static void OnCascadingUpdate(IStateContext context)
        {
            var ctx = (PillVirusContext)context;
            ctx.NeedsCascade = false;
            bool moved = false;
            for (int y = ctx.Height - 2; y >= 0; y--)
                for (int x = 0; x < ctx.Width; x++)
                {
                    if (ctx.Board[x, y].Type != CellType.Empty && ctx.Board[x, y].Type != CellType.Virus && ctx.Board[x, y + 1].Type == CellType.Empty)
                    {
                        int lid = ctx.Board[x, y].LinkId;
                        bool canFall = true;
                        if (lid != 0)
                        { // Horizontal Pair Logic
                            if (x < ctx.Width - 1 && ctx.Board[x + 1, y].LinkId == lid) { if (ctx.Board[x + 1, y + 1].Type != CellType.Empty) canFall = false; }
                            else if (x > 0 && ctx.Board[x - 1, y].LinkId == lid) { if (ctx.Board[x - 1, y + 1].Type != CellType.Empty) canFall = false; }
                        }
                        if (canFall) { ctx.Board[x, y + 1] = ctx.Board[x, y]; ctx.Board[x, y] = new GridCell { Type = CellType.Empty }; moved = true; }
                    }
                }
            if (moved) ctx.NeedsCascade = true; else ctx.NeedsEvaluation = true;
        }
    }
}