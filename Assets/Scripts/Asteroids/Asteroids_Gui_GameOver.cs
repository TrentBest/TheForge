using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Asteroids
{
    public class Asteroids_Gui_GameOver : IGuiProvider
    {
        public string Title => "ASTEROIDS: NEURAL SYNC SEVERED";

        private IGuiRouter _router;
        private GuiContext _guiContext;
        private AsteroidsContext _gameContext;
        private VisualElement _rootContainer;

        // UI Debris Simulation
        private class UIDebris
        {
            public VisualElement Element;
            public Vector2 Position; // Represents percentage (0 to 100)
            public Vector2 Velocity;
            public float Rotation;
            public float RotationSpeed;
        }
        private List<UIDebris> _debrisList = new List<UIDebris>();

        // 1. Parameterless Constructor for Editor/Reflection
        public Asteroids_Gui_GameOver() { }

        // 2. Injected Constructor for Alpha Router
        public Asteroids_Gui_GameOver(IGuiRouter router)
        {
            _router = router;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _guiContext = ctx;
            _gameContext = UnityEngine.Object.FindObjectOfType<AsteroidsContext>();

            // The Root Container - Semi-transparent so the 3D game scene (and any 3D debris) shows through
            _rootContainer = new GraphicalUserInterfaceBuilder("GameOver_Root")
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.05f, 0.0f, 0.02f, 0.85f)) // Deep transparent Crimson/Black
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .Build();

            // --- LAYER 1: UI FLOATING DEBRIS ---
            var debrisLayer = new VisualElement
            {
                name = "DebrisLayer",
                style = { position = Position.Absolute, left = 0, right = 0, top = 0, bottom = 0, overflow = Overflow.Hidden }
            };
            _rootContainer.Add(debrisLayer);
            SpawnDebris(debrisLayer, 40); // Spawn 40 pieces of shrapnel

            // --- LAYER 2: THE FOREGROUND UI ---
            var uiContent = new GraphicalUserInterfaceBuilder("UI_Content")
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .OnBuild(ve => ve.pickingMode = PickingMode.Ignore)
                .Build();

            // Title
            var title = new ForgeLabelBuilder("NEURAL SYNC SEVERED")
                .WithColor(new Color(1f, 0.2f, 0.3f)) // Warning Red
                .WithFontSize(56)
                .WithFontStyle(FontStyle.Bold)
                .CreateGui(_guiContext);
            title.style.letterSpacing = 10;
            uiContent.Add(title);

            // Subtitle / Score
            int finalScore = _gameContext?.Score?.CurrentScore ?? 0;
            var scoreLbl = new ForgeLabelBuilder($"FINAL DATA YIELD: {finalScore.ToString("D6")}")
                .WithColor(Color.gray)
                .WithFontSize(24)
                .CreateGui(_guiContext);
            scoreLbl.style.marginTop = 10;
            scoreLbl.style.marginBottom = 60;
            scoreLbl.style.letterSpacing = 3;
            uiContent.Add(scoreLbl);

            // Buttons Container
            var btnContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            // Play Again (Routes to Hangar to pick a new ship)
            var retryBtn = new ForgeButtonBuilder("RE-ESTABLISH LINK (REDEPLOY)", () => {
                // Tell the context to reset its data before we jump back to the hangar
                if (_gameContext?.Status != null) _gameContext.Status.TransitionTo("Initializing");
                _router?.NavigateTo("HangarBay");
            })
            .WithWidth(350).WithHeight(50)
            .WithBackgroundColor(new Color(0.4f, 0.1f, 0.6f, 0.9f)) // Purple
            .WithTextColor(Color.white)
            .WithFontStyle(FontStyle.Bold)
            .CreateGui(_guiContext);

            retryBtn.style.borderTopWidth = 2; retryBtn.style.borderBottomWidth = 2; retryBtn.style.borderLeftWidth = 2; retryBtn.style.borderRightWidth = 2;
            retryBtn.style.borderTopColor = new Color(0.8f, 0.4f, 1f); retryBtn.style.borderBottomColor = new Color(0.8f, 0.4f, 1f);
            retryBtn.style.borderLeftColor = new Color(0.8f, 0.4f, 1f); retryBtn.style.borderRightColor = new Color(0.8f, 0.4f, 1f);
            retryBtn.style.marginRight = 20;

            // Quit (Routes to Main Menu)
            var quitBtn = new ForgeButtonBuilder("ABORT TO COMMAND", () => {
                _router?.NavigateTo("MainMenu");
            })
            .WithWidth(250).WithHeight(50)
            .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f, 0.9f)) // Dark Gray
            .WithTextColor(Color.gray)
            .WithFontStyle(FontStyle.Bold)
            .CreateGui(_guiContext);

            btnContainer.Add(retryBtn);
            btnContainer.Add(quitBtn);
            uiContent.Add(btnContainer);

            _rootContainer.Add(uiContent);

            // Start the debris animation loop (roughly 60 FPS)
            _rootContainer.schedule.Execute(UpdateDebris).Every(16);

            return _rootContainer;
        }

        // --- DEBRIS SIMULATION LOGIC ---

        private void SpawnDebris(VisualElement container, int count)
        {
            _debrisList.Clear();
            System.Random rand = new System.Random();

            for (int i = 0; i < count; i++)
            {
                // Randomize size and shape (lines vs blocks)
                bool isLine = rand.NextDouble() > 0.5;
                float w = isLine ? (float)(rand.NextDouble() * 40 + 10) : (float)(rand.NextDouble() * 15 + 5);
                float h = isLine ? (float)(rand.NextDouble() * 3 + 1) : w;

                // Color palette (Grays, dark reds, muted purples)
                Color[] colors = {
                    new Color(0.3f, 0.3f, 0.3f, 0.5f),
                    new Color(0.5f, 0.1f, 0.1f, 0.4f),
                    new Color(0.3f, 0.1f, 0.4f, 0.4f),
                    new Color(0.1f, 0.1f, 0.1f, 0.8f)
                };

                var debrisVe = new VisualElement
                {
                    style = {
                        position = Position.Absolute,
                        width = w, height = h,
                        backgroundColor = colors[rand.Next(colors.Length)]
                    }
                };

                container.Add(debrisVe);

                _debrisList.Add(new UIDebris
                {
                    Element = debrisVe,
                    Position = new Vector2((float)rand.NextDouble() * 100f, (float)rand.NextDouble() * 100f),
                    // Velocity in percentage of screen per second
                    Velocity = new Vector2((float)(rand.NextDouble() * 4f - 2f), (float)(rand.NextDouble() * 4f - 2f)),
                    Rotation = (float)rand.NextDouble() * 360f,
                    RotationSpeed = (float)(rand.NextDouble() * 90f - 45f)
                });
            }
        }

        private void UpdateDebris()
        {
            float dt = 0.016f; // approx 16ms delta time

            foreach (var debris in _debrisList)
            {
                // Integrate physics
                debris.Position += debris.Velocity * dt;
                debris.Rotation += debris.RotationSpeed * dt;

                // Screen Wrap (0 to 100%)
                if (debris.Position.x > 105f) debris.Position.x = -5f;
                else if (debris.Position.x < -5f) debris.Position.x = 105f;

                if (debris.Position.y > 105f) debris.Position.y = -5f;
                else if (debris.Position.y < -5f) debris.Position.y = 105f;

                // Apply to UI Element
                debris.Element.style.left = Length.Percent(debris.Position.x);
                debris.Element.style.top = Length.Percent(debris.Position.y);
                debris.Element.style.rotate = new Rotate(new Angle(debris.Rotation));
            }
        }

        // Standard builder accessors
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}