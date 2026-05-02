using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Pong
{
    public class Pong_Autonomous_Background : IGuiProvider
    {
        public string Title => "Background Sim";

        private VisualElement _rootArena;
        private VisualElement _leftPaddle;
        private VisualElement _rightPaddle;
        private VisualElement _ball;

        private Vector2 _ballPos;
        private Vector2 _ballVel;
        private float _leftPaddleY = 50f;
        private float _rightPaddleY = 50f;

        private float _arenaWidth = 100f;
        private float _arenaHeight = 100f;

        private const float PADDLE_WIDTH = 4f;
        private const float PADDLE_HEIGHT = 30f;
        private const float BALL_SIZE = 6f;
        private const float PADDLE_SPEED = 400f;
        private const float INITIAL_BALL_SPEED = 250f;
        private const float PADDLE_MARGIN = 10f;

        private Color _themeColor;

        public Pong_Autonomous_Background(Color themeColor)
        {
            _themeColor = themeColor;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _rootArena = new GraphicalUserInterfaceBuilder("AutoBgArena")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0, 0, 0, 0)) // Transparent background
                .OnBuild(ve =>
                {
                    ve.style.position = Position.Absolute;
                    ve.style.top = 0; ve.style.bottom = 0;
                    ve.style.left = 0; ve.style.right = 0;
                    ve.style.overflow = Overflow.Hidden;
                    ve.pickingMode = PickingMode.Ignore; // CRITICAL: Let clicks pass through to the button!
                }).Build();

            // Setup Entities (Dimmed slightly so text pops)
            Color entityColor = new Color(_themeColor.r, _themeColor.g, _themeColor.b, 0.3f);

            _leftPaddle = CreateEntity("BgLeftPaddle", entityColor, true);
            _rightPaddle = CreateEntity("BgRightPaddle", entityColor, true);
            _ball = CreateEntity("BgBall", entityColor, false);

            _rootArena.Add(_leftPaddle);
            _rootArena.Add(_rightPaddle);
            _rootArena.Add(_ball);

            _rootArena.RegisterCallback<GeometryChangedEvent>(e =>
            {
                _arenaWidth = e.newRect.width;
                _arenaHeight = e.newRect.height;
                if (_ballPos == Vector2.zero && _arenaWidth > 0) ResetBall();
            });

            _rootArena.schedule.Execute(GameLoop).Every(16);

            return _rootArena;
        }

        private VisualElement CreateEntity(string name, Color color, bool isPaddle)
        {
            return new GraphicalUserInterfaceBuilder(name)
                .WithBackgroundColor(color)
                .OnBuild(ve => {
                    ve.style.position = Position.Absolute;
                    ve.style.width = isPaddle ? PADDLE_WIDTH : BALL_SIZE;
                    ve.style.height = isPaddle ? PADDLE_HEIGHT : BALL_SIZE;
                    ve.pickingMode = PickingMode.Ignore;
                }).Build();
        }

        private void GameLoop(TimerState state)
        {
            if (_arenaWidth <= 0) return;

            float dt = state.deltaTime / 1000f;
            if (dt > 0.1f) dt = 0.016f;

            // AI vs AI logic (Hermit Lerp)
            float targetY = _ballPos.y - (PADDLE_HEIGHT / 2) + (BALL_SIZE / 2);

            // Add a tiny bit of random noise so they aren't perfectly synced
            _leftPaddleY = Mathf.Lerp(_leftPaddleY, targetY, dt * UnityEngine.Random.Range(2.5f, 4.5f));
            _rightPaddleY = Mathf.Lerp(_rightPaddleY, targetY, dt * UnityEngine.Random.Range(2.5f, 4.5f));

            _leftPaddleY = Mathf.Clamp(_leftPaddleY, 0, _arenaHeight - PADDLE_HEIGHT);
            _rightPaddleY = Mathf.Clamp(_rightPaddleY, 0, _arenaHeight - PADDLE_HEIGHT);

            UpdateBall(dt);
            RenderPositions();
        }

        private void UpdateBall(float dt)
        {
            _ballPos += _ballVel * dt;

            if (_ballPos.y <= 0) { _ballPos.y = 0; _ballVel.y *= -1; }
            else if (_ballPos.y >= _arenaHeight - BALL_SIZE) { _ballPos.y = _arenaHeight - BALL_SIZE; _ballVel.y *= -1; }

            Rect ballRect = new Rect(_ballPos.x, _ballPos.y, BALL_SIZE, BALL_SIZE);
            Rect leftRect = new Rect(PADDLE_MARGIN, _leftPaddleY, PADDLE_WIDTH, PADDLE_HEIGHT);
            Rect rightRect = new Rect(_arenaWidth - PADDLE_MARGIN - PADDLE_WIDTH, _rightPaddleY, PADDLE_WIDTH, PADDLE_HEIGHT);

            if (_ballVel.x < 0 && ballRect.Overlaps(leftRect))
            {
                _ballPos.x = PADDLE_MARGIN + PADDLE_WIDTH;
                _ballVel.x *= -1.02f; // Slight speed up
            }
            else if (_ballVel.x > 0 && ballRect.Overlaps(rightRect))
            {
                _ballPos.x = _arenaWidth - PADDLE_MARGIN - PADDLE_WIDTH - BALL_SIZE;
                _ballVel.x *= -1.02f;
            }

            // Infinite play: just reset if someone scores
            if (_ballPos.x < -20 || _ballPos.x > _arenaWidth + 20) ResetBall();
        }

        private void RenderPositions()
        {
            _leftPaddle.style.left = PADDLE_MARGIN;
            _leftPaddle.style.top = _leftPaddleY;
            _rightPaddle.style.left = _arenaWidth - PADDLE_MARGIN - PADDLE_WIDTH;
            _rightPaddle.style.top = _rightPaddleY;
            _ball.style.left = _ballPos.x;
            _ball.style.top = _ballPos.y;
        }

        private void ResetBall()
        {
            _ballPos = new Vector2(_arenaWidth / 2 - (BALL_SIZE / 2), _arenaHeight / 2 - (BALL_SIZE / 2));
            float dirX = UnityEngine.Random.value > 0.5f ? 1f : -1f;
            float dirY = UnityEngine.Random.Range(-0.8f, 0.8f);
            _ballVel = new Vector2(dirX, dirY).normalized * INITIAL_BALL_SPEED;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}