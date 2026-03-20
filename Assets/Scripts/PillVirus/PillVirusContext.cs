using System.Collections.Generic;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Sandbox.PillVirus
{
    public enum CellType : byte { Empty = 0, Virus = 1, Pill = 2 }

    public struct GridCell
    {
        public CellType Type;
        public int ColorId;     // 0 = None, 1 = Red, 2 = Yellow, etc.
        public int PlayerId;    // Tracks who dropped it
    }

    public class ActivePill
    {
        public int PlayerId;
        public int X, Y;
        public int Color1;
        public int Color2;
        public bool IsHorizontal = true;
    }

    public class PillVirusContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "PillVirus_Multiplayer_Engine";

        public int Width;
        public int Height;
        public int MatchRequirement;
        public int PlayerCount;

        public Dictionary<int, Color> ColorPalette;
        public GridCell[,] Board;
        public List<ActivePill> ActivePlayers;

        // FSM Engine Triggers
        public float FallTimer = 0f;
        public float FallSpeed = 0.5f;
        public bool IsGameOver = false;
        public bool NeedsEvaluation = false;
        public bool NeedsCascade = false;

        public PillVirusContext(int width = 12, int height = 18, int matchReq = 4, int playerCount = 2)
        {
            Width = width;
            Height = height;
            MatchRequirement = matchReq;
            PlayerCount = playerCount;

            Board = new GridCell[Width, Height];
            ActivePlayers = new List<ActivePill>();

            ColorPalette = new Dictionary<int, Color>
            {
                { 1, new Color(0.9f, 0.2f, 0.2f) }, // Red
                { 2, new Color(0.9f, 0.9f, 0.2f) }, // Yellow
                { 3, new Color(0.2f, 0.4f, 0.9f) }, // Blue
                { 4, new Color(0.2f, 0.8f, 0.2f) }, // Green
            };

            GenerateViruses(15);
        }

        public void SpawnPillForPlayer(int playerId)
        {
            int spawnX = (Width / 2) - 1 + (playerId * 2);
            spawnX = Mathf.Clamp(spawnX, 0, Width - 2);

            // Check if spawn point is blocked
            if (Board[spawnX, 0].Type != CellType.Empty || Board[spawnX + 1, 0].Type != CellType.Empty)
            {
                IsGameOver = true;
                return;
            }

            ActivePlayers.Add(new ActivePill
            {
                PlayerId = playerId,
                X = spawnX,
                Y = 0,
                Color1 = Random.Range(1, ColorPalette.Count + 1),
                Color2 = Random.Range(1, ColorPalette.Count + 1),
                IsHorizontal = true
            });
        }

        private void GenerateViruses(int count)
        {
            int placed = 0;
            while (placed < count)
            {
                int rx = Random.Range(0, Width);
                int ry = Random.Range(Height / 2, Height);

                if (Board[rx, ry].Type == CellType.Empty)
                {
                    Board[rx, ry] = new GridCell { Type = CellType.Virus, ColorId = Random.Range(1, ColorPalette.Count + 1) };
                    placed++;
                }
            }
        }
    }
}