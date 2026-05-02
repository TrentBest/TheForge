using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.Pong
{
    public class PongContext : IStateContext
    {
        public string Name { get; set; } = "PongSession";
        public bool IsValid { get; set; } = false;

       

        // --- Arena State ---
        public Vector2 ArenaSize = new Vector2(800, 600);
        public float PaddleMargin = 40f;

        // --- Entities ---
        public Vector2 BallPos;
        public Vector2 BallVel;
        public float LeftPaddleY = 250f;
        public float RightPaddleY = 250f;

        // --- Gameplay Stats ---
        public int LeftScore = 0;
        public int RightScore = 0;
        public PongGameMode Mode;
        public bool IsBallActive = false;

        // --- Constants ---
        public const float PADDLE_WIDTH = 15f;
        public const float PADDLE_HEIGHT = 100f;
        public const float BALL_SIZE = 15f;
        public const float INITIAL_SPEED = 400f;

        public PongContext(PongGameMode mode) => Mode = mode;
    }
}