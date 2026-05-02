using System.Collections.Generic;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.PillVirus
{
    public enum CellType : byte { Empty = 0, Virus = 1, Pill = 2, Garbage = 3 }

    public struct GridCell
    {
        public CellType Type;
        public int ColorId;
        public int LinkId; // 0 = solo, non-zero = linked pair
    }

    public class ActivePill
    {
        public int X, Y;
        public int Color1, Color2;
        public bool IsHorizontal = true;
        public float LockTimer = 0f;
        public float LockDelay = 0.6f;
        public bool IsResting = false;

        public int PlayerId { get; internal set; }
    }

    public class PillVirusContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "PillVirus_Engine_Context";
        public int PlayerCount { get; internal set; }

        public GridCell[,] Board;
        public List<ActivePill> ActivePlayers = new List<ActivePill>();
        public HashSet<KeyCode> HeldKeys = new HashSet<KeyCode>();

        public int Width = 8;
        public int Height = 16;
        public int RemainingViruses;
        public float FallTimer = 0f;
        public float FallSpeed = 0.8f;
        public bool IsGameOver = false, IsVictory = false;
        public bool NeedsEvaluation = false, NeedsCascade = false;

        // Garbage & Preview
        public int PendingGarbage = 0;

        public int NextColor1, NextColor2;
        public int CurrentScore = 0;
        public int Level = 0;
        public int ComboMultiplier = 1;

        public Dictionary<int, Color> ColorPalette = new Dictionary<int, Color>
        {
            { 1, new Color(0.9f, 0.2f, 0.2f) }, // Red
            { 2, new Color(0.9f, 0.9f, 0.2f) }, // Yellow
            { 3, new Color(0.2f, 0.4f, 0.9f) }, // Blue
        };

        public PillVirusContext()
        {
            Board = new GridCell[Width, Height];
            Level = PillVirusConfig.Level;
            FallSpeed = PillVirusConfig.FallSpeed;
            RemainingViruses = PillVirusConfig.VirusCount;
            NextColor1 = Random.Range(1, 4);
            NextColor2 = Random.Range(1, 4);
            GenerateViruses();
        }

        private void GenerateViruses()
        {
            int placed = 0;
            while (placed < RemainingViruses)
            {
                int rx = Random.Range(0, Width), ry = Random.Range(Height / 2, Height);
                if (Board[rx, ry].Type == CellType.Empty)
                {
                    Board[rx, ry] = new GridCell { Type = CellType.Virus, ColorId = Random.Range(1, 4) };
                    placed++;
                }
            }
        }
    }
}