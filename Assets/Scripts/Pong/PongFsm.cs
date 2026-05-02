using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.Pong
{
    public static class PongFsm
    {
        public const string DEF_NAME = "SingularityPongLogic";
        public const string GROUP = "Pong_Physics";

        public static void Build()
        {
            FSM_API.Create.CreateProcessingGroup(GROUP);

            FSM_API.Create.CreateFiniteStateMachine(DEF_NAME, processRate: 1, processingGroup: GROUP)
                .State("Serving",
                    onEnter: (ctx) => {
                        var p = (PongContext)ctx;
                        p.IsBallActive = false;
                        p.BallPos = new Vector2(p.ArenaSize.x / 2, p.ArenaSize.y / 2);
                    },
                    onUpdate: null,
                    onExit: null)
                .State("Playing",
                onEnter: null,
                    onUpdate: (ctx) => UpdatePhysics((PongContext)ctx),
                    onExit: null)
                .State("Scoring",
                    onEnter: (ctx) => { /* Handle Score effects */ },
                    onUpdate: null,
                    onExit: null)
                .WithInitialState("Serving")
                .Transition("Serving", "Playing", (ctx) => Input.anyKeyDown)
                .Transition("Playing", "Scoring", (ctx) => IsOutOfBounds((PongContext)ctx))
                .Transition("Scoring", "Serving", (ctx) => true) // Auto-reset after score processing
                .BuildDefinition();
        }

        private static void UpdatePhysics(PongContext p)
        {
            float dt = Time.deltaTime; // Ideally passed via a Forge Chronos Provider
            p.BallPos += p.BallVel * dt;

            // Bounce logic
            if (p.BallPos.y <= 0 || p.BallPos.y >= p.ArenaSize.y - PongContext.BALL_SIZE)
                p.BallVel.y *= -1;

            // Collision checks using Rect.Overlaps (Standard Forge Pattern)
            Rect ballRect = new Rect(p.BallPos.x, p.BallPos.y, PongContext.BALL_SIZE, PongContext.BALL_SIZE);
            if (ballRect.Overlaps(new Rect(p.PaddleMargin, p.LeftPaddleY, PongContext.PADDLE_WIDTH, PongContext.PADDLE_HEIGHT)))
            {
                p.BallVel.x = Mathf.Abs(p.BallVel.x) * 1.05f;
            }
        }

        private static bool IsOutOfBounds(PongContext p) => p.BallPos.x < -50 || p.BallPos.x > p.ArenaSize.x + 50;
    }
}