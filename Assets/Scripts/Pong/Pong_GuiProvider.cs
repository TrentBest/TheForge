using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Pong
{
    public class Pong_GuiProvider : IGuiProvider
    {
        public string Title => "SINGULARITY PONG";

        // --- UI Elements ---
        private VisualElement _rootArena;
        private VisualElement _leftPaddle;
        private VisualElement _rightPaddle;
        private VisualElement _ball;
        private Label _scoreLabel;

        // --- Game State ---
        private Vector2 _ballPos;
        private Vector2 _ballVel;
        private float _leftPaddleY = 250f;
        private float _rightPaddleY = 250f;
        private int _leftScore = 0;
        private int _rightScore = 0;

        // --- Input State ---
        private bool _wPressed, _sPressed, _upPressed, _downPressed;

        // --- Configuration ---
        private const float PADDLE_WIDTH = 15f;
        private const float PADDLE_HEIGHT = 100f;
        private const float BALL_SIZE = 15f;
        private const float PADDLE_SPEED = 600f;
        private const float INITIAL_BALL_SPEED = 400f;
        private const float PADDLE_MARGIN = 40f; // Distance from the edge

        // Track dynamic arena size
        private float _arenaWidth = 800f;
        private float _arenaHeight = 600f;

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. THE ARENA (Root Canvas)
            var rootBuilder = new GraphicalUserInterfaceBuilder("PongArena")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.03f)) // Deep void
                .OnBuild(ve =>
                {
                    ve.focusable = true; // Required to capture keyboard input
                    ve.style.overflow = Overflow.Hidden;
                });

            _rootArena = rootBuilder.Build();

            // 2. CENTER DIVIDER (Aesthetics)
            _rootArena.Add(new GraphicalUserInterfaceBuilder("CenterLine")
                .WithBackgroundColor(new Color(1f, 1f, 1f, 0.1f))
                .OnBuild(ve => {
                    ve.style.position = Position.Absolute;
                    ve.style.width = 4;
                    ve.style.height = Length.Percent(100);
                    ve.style.left = Length.Percent(50);
                    ve.style.translate = new Translate(Length.Percent(-50), 0);
                }).Build());

            // 3. SCOREBOARD
            _scoreLabel = new Label("0   -   0");
            _scoreLabel.style.position = Position.Absolute;
            _scoreLabel.style.top = 30;
            _scoreLabel.style.width = Length.Percent(100);
            _scoreLabel.style.unityTextAlign = TextAnchor.UpperCenter;
            _scoreLabel.style.fontSize = 64;
            _scoreLabel.style.color = new Color(0.3f, 0.3f, 0.3f);
            _scoreLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _rootArena.Add(_scoreLabel);

            // 4. LEFT PADDLE
            _leftPaddle = new GraphicalUserInterfaceBuilder("LeftPaddle")
                .WithBackgroundColor(Color.cyan)
                .OnBuild(ve => {
                    ve.style.position = Position.Absolute;
                    ve.style.width = PADDLE_WIDTH;
                    ve.style.height = PADDLE_HEIGHT;
                    ve.style.borderTopRightRadius = 5;
                    ve.style.borderBottomRightRadius = 5;
                }).Build();
            _rootArena.Add(_leftPaddle);

            // 5. RIGHT PADDLE
            _rightPaddle = new GraphicalUserInterfaceBuilder("RightPaddle")
                .WithBackgroundColor(new Color(0.8f, 0.1f, 0.8f)) // Singularity Purple
                .OnBuild(ve => {
                    ve.style.position = Position.Absolute;
                    ve.style.width = PADDLE_WIDTH;
                    ve.style.height = PADDLE_HEIGHT;
                    ve.style.borderTopLeftRadius = 5;
                    ve.style.borderBottomLeftRadius = 5;
                }).Build();
            _rootArena.Add(_rightPaddle);

            // 6. THE BALL
            _ball = new GraphicalUserInterfaceBuilder("Ball")
                .WithBackgroundColor(Color.white)
                .OnBuild(ve => {
                    ve.style.position = Position.Absolute;
                    ve.style.width = BALL_SIZE;
                    ve.style.height = BALL_SIZE;
                    ve.style.borderTopLeftRadius = BALL_SIZE / 2;
                    ve.style.borderTopRightRadius = BALL_SIZE / 2;
                    ve.style.borderBottomLeftRadius = BALL_SIZE / 2;
                    ve.style.borderBottomRightRadius = BALL_SIZE / 2;
                }).Build();
            _rootArena.Add(_ball);

            // --- EVENT BINDINGS ---

            // Track resizing so physics know the boundaries
            _rootArena.RegisterCallback<GeometryChangedEvent>(e =>
            {
                _arenaWidth = e.newRect.width;
                _arenaHeight = e.newRect.height;
                if (_ballPos == Vector2.zero) ResetBall(); // Initial placement
            });

            // Input routing
            _rootArena.RegisterCallback<KeyDownEvent>(OnKeyDown);
            _rootArena.RegisterCallback<KeyUpEvent>(OnKeyUp);

            // Ensure arena keeps focus when clicked
            _rootArena.RegisterCallback<PointerDownEvent>(e => _rootArena.Focus());
            _rootArena.RegisterCallback<AttachToPanelEvent>(e => _rootArena.Focus());

            // --- THE GAME LOOP (Autopoietic Gestalt) ---
            _rootArena.schedule.Execute(GameLoop).Every(16); // ~60fps

            return _rootArena;
        }

        private void GameLoop(TimerState state)
        {
            // TimerState deltaTime is in milliseconds
            float dt = state.deltaTime / 1000f;
            if (dt > 0.1f) dt = 0.016f; // Prevent massive jumps during lag spikes

            UpdatePaddles(dt);
            UpdateBall(dt);
            RenderPositions();
        }

        private void UpdatePaddles(float dt)
        {
            // Left Paddle (W/S)
            if (_wPressed) _leftPaddleY -= PADDLE_SPEED * dt;
            if (_sPressed) _leftPaddleY += PADDLE_SPEED * dt;

            // Right Paddle (Up/Down)
            if (_upPressed) _rightPaddleY -= PADDLE_SPEED * dt;
            if (_downPressed) _rightPaddleY += PADDLE_SPEED * dt;

            // Clamp to screen
            _leftPaddleY = Mathf.Clamp(_leftPaddleY, 0, _arenaHeight - PADDLE_HEIGHT);
            _rightPaddleY = Mathf.Clamp(_rightPaddleY, 0, _arenaHeight - PADDLE_HEIGHT);
        }

        private void UpdateBall(float dt)
        {
            _ballPos += _ballVel * dt;

            // Top/Bottom Wall Bounce
            if (_ballPos.y <= 0)
            {
                _ballPos.y = 0;
                _ballVel.y *= -1;
            }
            else if (_ballPos.y >= _arenaHeight - BALL_SIZE)
            {
                _ballPos.y = _arenaHeight - BALL_SIZE;
                _ballVel.y *= -1;
            }

            // --- Collision Rectangles ---
            Rect ballRect = new Rect(_ballPos.x, _ballPos.y, BALL_SIZE, BALL_SIZE);
            Rect leftRect = new Rect(PADDLE_MARGIN, _leftPaddleY, PADDLE_WIDTH, PADDLE_HEIGHT);
            Rect rightRect = new Rect(_arenaWidth - PADDLE_MARGIN - PADDLE_WIDTH, _rightPaddleY, PADDLE_WIDTH, PADDLE_HEIGHT);

            // Left Paddle Hit
            if (_ballVel.x < 0 && ballRect.Overlaps(leftRect))
            {
                _ballPos.x = PADDLE_MARGIN + PADDLE_WIDTH; // Push out
                _ballVel.x *= -1;
                _ballVel.x *= 1.05f; // Speed up slightly!
                ApplyEnglish(leftRect);
            }
            // Right Paddle Hit
            else if (_ballVel.x > 0 && ballRect.Overlaps(rightRect))
            {
                _ballPos.x = _arenaWidth - PADDLE_MARGIN - PADDLE_WIDTH - BALL_SIZE; // Push out
                _ballVel.x *= -1;
                _ballVel.x *= 1.05f; // Speed up slightly!
                ApplyEnglish(rightRect);
            }

            // --- Scoring ---
            if (_ballPos.x < -50)
            {
                _rightScore++;
                UpdateScore();
                ResetBall();
            }
            else if (_ballPos.x > _arenaWidth + 50)
            {
                _leftScore++;
                UpdateScore();
                ResetBall();
            }
        }

        private void ApplyEnglish(Rect paddleRect)
        {
            // Modifies the Y velocity based on where the ball hit the paddle
            float hitPoint = (_ballPos.y + (BALL_SIZE / 2)) - (paddleRect.y + (PADDLE_HEIGHT / 2));
            float normalizedHit = hitPoint / (PADDLE_HEIGHT / 2); // -1 to 1
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

        private void ResetBall()
        {
            _ballPos = new Vector2(_arenaWidth / 2 - (BALL_SIZE / 2), _arenaHeight / 2 - (BALL_SIZE / 2));

            // Randomize starting direction
            float dirX = UnityEngine.Random.value > 0.5f ? 1f : -1f;
            float dirY = UnityEngine.Random.Range(-0.5f, 0.5f);

            _ballVel = new Vector2(dirX, dirY).normalized * INITIAL_BALL_SPEED;
        }

        private void UpdateScore()
        {
            _scoreLabel.text = $"{_leftScore}   -   {_rightScore}";
        }

        // --- INPUT MAPPING ---
        private void OnKeyDown(KeyDownEvent e)
        {
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