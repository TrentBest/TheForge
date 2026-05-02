using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Pong
{
    public enum PongGameMode
    {
        Hotseat,
        AiHermit,
        AiNeuralNet,
        Multiplayer
    }

    public class Pong_Playable_Alpha : IGuiProvider
    {
        public string Title => "SINGULARITY PONG - ALPHA";

        private PongGameMode _mode;

        // --- UI Elements ---
        private VisualElement _rootArena;
        private VisualElement _leftPaddle;
        private VisualElement _rightPaddle;
        private VisualElement _ball;
        private Label _scoreLabel;
        private Label _statusLabel;

        // --- Game State ---
        private Vector2 _ballPos;
        private Vector2 _ballVel;
        private float _leftPaddleY = 250f;
        private float _rightPaddleY = 250f;
        private int _leftScore = 0;
        private int _rightScore = 0;
        private bool _isPlaying = false;

        // --- Input State ---
        private bool _wPressed, _sPressed, _upPressed, _downPressed;

        // --- Configuration ---
        private const float PADDLE_WIDTH = 15f;
        private const float PADDLE_HEIGHT = 100f;
        private const float BALL_SIZE = 15f;
        private const float PADDLE_SPEED = 600f;
        private const float INITIAL_BALL_SPEED = 400f;
        private const float PADDLE_MARGIN = 40f;

        private float _arenaWidth = 800f;
        private float _arenaHeight = 600f;

        public Pong_Playable_Alpha(PongGameMode mode)
        {
            _mode = mode;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("PongArena")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.03f))
                .OnBuild(ve =>
                {
                    ve.focusable = true;
                    ve.style.overflow = Overflow.Hidden;
                });

            _rootArena = rootBuilder.Build();

            // Center Line
            _rootArena.Add(new GraphicalUserInterfaceBuilder("CenterLine")
                .WithBackgroundColor(new Color(1f, 1f, 1f, 0.1f))
                .OnBuild(ve => {
                    ve.style.position = Position.Absolute;
                    ve.style.width = 4;
                    ve.style.height = Length.Percent(100);
                    ve.style.left = Length.Percent(50);
                    ve.style.translate = new Translate(Length.Percent(-50), 0);
                }).Build());

            // Scoreboard
            _scoreLabel = new Label("0   -   0");
            _scoreLabel.style.position = Position.Absolute;
            _scoreLabel.style.top = 30;
            _scoreLabel.style.width = Length.Percent(100);
            _scoreLabel.style.unityTextAlign = TextAnchor.UpperCenter;
            _scoreLabel.style.fontSize = 64;
            _scoreLabel.style.color = new Color(0.3f, 0.3f, 0.3f);
            _scoreLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _rootArena.Add(_scoreLabel);

            // Status Label (For Multiplayer/Initialization)
            _statusLabel = new Label(_mode == PongGameMode.Multiplayer ? "CONNECTING TO MATCHMAKING..." : "PRESS ANY KEY TO SERVE");
            _statusLabel.style.position = Position.Absolute;
            _statusLabel.style.bottom = 30;
            _statusLabel.style.width = Length.Percent(100);
            _statusLabel.style.unityTextAlign = TextAnchor.UpperCenter;
            _statusLabel.style.fontSize = 24;
            _statusLabel.style.color = Color.cyan;
            _rootArena.Add(_statusLabel);

            // Paddles & Ball
            _leftPaddle = CreateEntity("LeftPaddle", Color.cyan, true);
            _rightPaddle = CreateEntity("RightPaddle", new Color(0.8f, 0.1f, 0.8f), true);
            _ball = CreateEntity("Ball", Color.white, false);

            _rootArena.Add(_leftPaddle);
            _rootArena.Add(_rightPaddle);
            _rootArena.Add(_ball);

            // Event Bindings
            _rootArena.RegisterCallback<GeometryChangedEvent>(e =>
            {
                _arenaWidth = e.newRect.width;
                _arenaHeight = e.newRect.height;
                if (_ballPos == Vector2.zero && _mode != PongGameMode.Multiplayer) ResetBall();
            });

            _rootArena.RegisterCallback<KeyDownEvent>(OnKeyDown);
            _rootArena.RegisterCallback<KeyUpEvent>(OnKeyUp);
            _rootArena.RegisterCallback<PointerDownEvent>(e => _rootArena.Focus());
            _rootArena.RegisterCallback<AttachToPanelEvent>(e => _rootArena.Focus());

            // Multi-player Mock Lock
            if (_mode == PongGameMode.Multiplayer)
            {
                _isPlaying = false;
                _ball.style.display = DisplayStyle.None;
            }

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
                    if (!isPaddle)
                    {
                        ve.style.borderTopLeftRadius = BALL_SIZE / 2;
                        ve.style.borderTopRightRadius = BALL_SIZE / 2;
                        ve.style.borderBottomLeftRadius = BALL_SIZE / 2;
                        ve.style.borderBottomRightRadius = BALL_SIZE / 2;
                    }
                }).Build();
        }

        private void GameLoop(TimerState state)
        {
            if (_mode == PongGameMode.Multiplayer) return; // Locked in mock networking state
            if (!_isPlaying) return;

            float dt = state.deltaTime / 1000f;
            if (dt > 0.1f) dt = 0.016f;

            UpdatePaddles(dt);
            UpdateBall(dt);
            RenderPositions();
        }

        private void UpdatePaddles(float dt)
        {
            // Left Paddle (Human P1)
            if (_wPressed) _leftPaddleY -= PADDLE_SPEED * dt;
            if (_sPressed) _leftPaddleY += PADDLE_SPEED * dt;

            // Right Paddle Logic
            if (_mode == PongGameMode.Hotseat)
            {
                if (_upPressed) _rightPaddleY -= PADDLE_SPEED * dt;
                if (_downPressed) _rightPaddleY += PADDLE_SPEED * dt;
            }
            else if (_mode == PongGameMode.AiHermit)
            {
                // Sluggish Lerp - Prone to overshoot and slow starts
                float targetY = _ballPos.y - (PADDLE_HEIGHT / 2) + (BALL_SIZE / 2);
                _rightPaddleY = Mathf.Lerp(_rightPaddleY, targetY, dt * 3.5f);
            }
            else if (_mode == PongGameMode.AiNeuralNet)
            {
                // Strict tracking, but capped by max paddle speed to give player a chance
                float targetY = _ballPos.y - (PADDLE_HEIGHT / 2) + (BALL_SIZE / 2);
                _rightPaddleY = Mathf.MoveTowards(_rightPaddleY, targetY, (PADDLE_SPEED * 0.85f) * dt);
            }

            // Clamp both
            _leftPaddleY = Mathf.Clamp(_leftPaddleY, 0, _arenaHeight - PADDLE_HEIGHT);
            _rightPaddleY = Mathf.Clamp(_rightPaddleY, 0, _arenaHeight - PADDLE_HEIGHT);
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
                _ballVel.x *= -1.05f;
                ApplyEnglish(leftRect);
            }
            else if (_ballVel.x > 0 && ballRect.Overlaps(rightRect))
            {
                _ballPos.x = _arenaWidth - PADDLE_MARGIN - PADDLE_WIDTH - BALL_SIZE;
                _ballVel.x *= -1.05f;
                ApplyEnglish(rightRect);
            }

            if (_ballPos.x < -50) { _rightScore++; ScoreReset(); }
            else if (_ballPos.x > _arenaWidth + 50) { _leftScore++; ScoreReset(); }
        }

        private void ApplyEnglish(Rect paddleRect)
        {
            float hitPoint = (_ballPos.y + (BALL_SIZE / 2)) - (paddleRect.y + (PADDLE_HEIGHT / 2));
            float normalizedHit = hitPoint / (PADDLE_HEIGHT / 2);
            _ballVel.y = normalizedHit * INITIAL_BALL_SPEED;
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

        private void ScoreReset()
        {
            _scoreLabel.text = $"{_leftScore}   -   {_rightScore}";
            _isPlaying = false;
            _statusLabel.text = "PRESS ANY KEY TO SERVE";
            ResetBall();
        }

        private void ResetBall()
        {
            _ballPos = new Vector2(_arenaWidth / 2 - (BALL_SIZE / 2), _arenaHeight / 2 - (BALL_SIZE / 2));
            float dirX = UnityEngine.Random.value > 0.5f ? 1f : -1f;
            float dirY = UnityEngine.Random.Range(-0.5f, 0.5f);
            _ballVel = new Vector2(dirX, dirY).normalized * INITIAL_BALL_SPEED;
            RenderPositions();
        }

        private void OnKeyDown(KeyDownEvent e)
        {
            if (!_isPlaying && _mode != PongGameMode.Multiplayer)
            {
                _isPlaying = true;
                _statusLabel.text = "";
            }

            if (e.keyCode == KeyCode.W) _wPressed = true;
            if (e.keyCode == KeyCode.S) _sPressed = true;
            if (e.keyCode == KeyCode.UpArrow) _upPressed = true;
            if (e.keyCode == KeyCode.DownArrow) _downPressed = true;
        }

        private void OnKeyUp(KeyUpEvent e)
        {
            if (e.keyCode == KeyCode.W) _wPressed = false;
            if (e.keyCode == KeyCode.S) _sPressed = false;
            if (e.keyCode == KeyCode.UpArrow) _upPressed = false;
            if (e.keyCode == KeyCode.DownArrow) _downPressed = false;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}