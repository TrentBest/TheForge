#if UNITY_EDITOR
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using UnityEditor.UIElements; // For specific fields like FloatField, IntegerField
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Asteroids.Editor
{
    public class AsteroidsContextBuilderGui : IGuiProvider
    {
        public string Title => "ASTEROIDS MASTER SETTINGS";

        // In a real scenario, this might be loaded from a serialized asset or scene object.
        private AsteroidsContext _context = new AsteroidsContext();

        public VisualElement CreateGui(GuiContext ctx)
        {
            var builder = new GraphicalUserInterfaceBuilder("AsteroidsMasterSettings_Root")
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f)) // Deep space blue/black
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100);

            // --- LEFT SIDEBAR: SYSTEM SETTINGS (30%) ---
            builder.AddChild(c => {
                var sidebar = new VisualElement
                {
                    style = {
                        width = Length.Percent(30),
                        backgroundColor = new Color(0.1f, 0.1f, 0.15f),
                        borderRightWidth = 2,
                        borderRightColor = Color.cyan,
                        paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15
                    }
                };

                sidebar.Add(new Label("CABINET CONFIGURATION") { style = { color = Color.cyan, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });

                // Session Settings Box
                var sessionBox = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 10, paddingRight = 10, paddingTop = 10, paddingBottom = 10, borderLeftWidth = 2, borderLeftColor = Color.yellow, marginBottom = 20 } };
                sessionBox.Add(new Label("ARCADE SESSION RULES") { style = { color = Color.yellow, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });

                var livesField = new IntegerField("Starting Lives") { value = _context.Score.Lives };
                livesField.RegisterValueChangedCallback(evt => _context.Score.Lives = evt.newValue);
                sessionBox.Add(livesField);

                var scoreField = new IntegerField("High Score Target") { value = 10000 };
                sessionBox.Add(scoreField);

                sidebar.Add(sessionBox);

                // Screen Boundaries Box
                var boundsBox = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 10, paddingRight = 10, paddingTop = 10, paddingBottom = 10, borderLeftWidth = 2, borderLeftColor = Color.green, marginBottom = 20 } };
                boundsBox.Add(new Label("2D SCREEN WRAP BOUNDARIES") { style = { color = Color.green, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
                boundsBox.Add(new Label("Defines the coordinate edges where entities warp to the opposite side.") { style = { color = Color.gray, fontSize = 9, whiteSpace = WhiteSpace.Normal, marginBottom = 10 } });

                var boundsX = new FloatField("Width Limit (X)") { value = _context.ScreenBounds.x };
                boundsX.RegisterValueChangedCallback(evt => _context.ScreenBounds = new Vector2(evt.newValue, _context.ScreenBounds.y));

                var boundsY = new FloatField("Height Limit (Y)") { value = _context.ScreenBounds.y };
                boundsY.RegisterValueChangedCallback(evt => _context.ScreenBounds = new Vector2(_context.ScreenBounds.x, evt.newValue));

                boundsBox.Add(boundsX);
                boundsBox.Add(boundsY);
                sidebar.Add(boundsBox);

                return sidebar;
            });

            // --- RIGHT AREA: REAL-TIME CABINET SCREEN PREVIEW (70%) ---
            builder.AddChild(c => {
                var rightCol = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column, alignItems = Align.Center, justifyContent = Justify.Center, backgroundColor = new Color(0.02f, 0.02f, 0.02f) } };

                rightCol.Add(new Label("CABINET CRT PREVIEW") { style = { color = Color.gray, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10, letterSpacing = 2 } });

                // Custom Visual Element Renderer
                var screenPreview = new AsteroidsScreenPreviewRenderer(_context)
                {
                    style = {
                        width = Length.Percent(90),
                        height = Length.Percent(80),
                        backgroundColor = Color.black,
                        borderLeftWidth = 4, borderRightWidth = 4, borderTopWidth = 4, borderBottomWidth = 4,
                        borderLeftColor = new Color(0.2f, 0.2f, 0.2f), borderRightColor = new Color(0.2f, 0.2f, 0.2f),
                        borderTopColor = new Color(0.2f, 0.2f, 0.2f), borderBottomColor = new Color(0.2f, 0.2f, 0.2f)
                    }
                };

                rightCol.Add(screenPreview);

                return rightCol;
            });

            return builder.Build();
        }

        public System.Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => throw new System.NotImplementedException();
        public void ToUIDocument(string assetPath) => throw new System.NotImplementedException();
    }

    // --- CUSTOM MAP RENDERER COMPONENT ---
    public class AsteroidsScreenPreviewRenderer : VisualElement
    {
        private AsteroidsContext _gameContext;

        public AsteroidsScreenPreviewRenderer(AsteroidsContext context)
        {
            _gameContext = context;

            // Subscribe to the draw loop
            generateVisualContent += OnGenerateVisualContent;
        }

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            float width = layout.width;
            float height = layout.height;
            if (width <= 0 || height <= 0) return;

            var painter = ctx.painter2D;
            Vector2 center = new Vector2(width / 2f, height / 2f);

            // 1. Draw Background Grid (Simulating the arcade monitor curve/scanlines)
            painter.lineWidth = 1f;
            painter.strokeColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            painter.BeginPath();
            float gridSize = 20f;
            for (float x = 0; x < width; x += gridSize) { painter.MoveTo(new Vector2(x, 0)); painter.LineTo(new Vector2(x, height)); }
            for (float y = 0; y < height; y += gridSize) { painter.MoveTo(new Vector2(0, y)); painter.LineTo(new Vector2(width, y)); }
            painter.Stroke();

            // 2. Calculate Scale based on Context ScreenBounds vs actual layout size
            float maxBound = Mathf.Max(_gameContext.ScreenBounds.x, _gameContext.ScreenBounds.y);
            if (maxBound == 0) maxBound = 1; // Prevent Div by 0
            float scale = (Mathf.Min(width, height) / 2f) * 0.9f / maxBound; // 0.9f gives a slight margin

            float scaledWidth = _gameContext.ScreenBounds.x * scale;
            float scaledHeight = _gameContext.ScreenBounds.y * scale;

            // 3. Draw the Actual Playable Wrap Boundary
            painter.strokeColor = Color.cyan;
            painter.lineWidth = 2f;
            painter.BeginPath();
            painter.MoveTo(center + new Vector2(-scaledWidth, -scaledHeight));
            painter.LineTo(center + new Vector2(scaledWidth, -scaledHeight));
            painter.LineTo(center + new Vector2(scaledWidth, scaledHeight));
            painter.LineTo(center + new Vector2(-scaledWidth, scaledHeight));
            painter.ClosePath();
            painter.Stroke();

            // 4. Draw a Mock Player Ship in the Center (Vector Graphics Style)
            painter.strokeColor = Color.white;
            painter.fillColor = Color.black;
            painter.lineWidth = 2f;
            painter.BeginPath();
            painter.MoveTo(center + new Vector2(0, -15)); // Nose
            painter.LineTo(center + new Vector2(10, 15)); // Right wing
            painter.LineTo(center + new Vector2(0, 8));   // Engine indent
            painter.LineTo(center + new Vector2(-10, 15));// Left wing
            painter.ClosePath();
            painter.Fill();
            painter.Stroke();

            // 5. Draw a couple of Mock Vector Asteroids
            DrawMockAsteroid(painter, center + new Vector2(-scaledWidth * 0.5f, -scaledHeight * 0.6f), 20f);
            DrawMockAsteroid(painter, center + new Vector2(scaledWidth * 0.7f, scaledHeight * 0.3f), 30f);

            // Force repaint if we want it to animate or update in real-time when adjusting sliders
            MarkDirtyRepaint();
        }

        private void DrawMockAsteroid(Painter2D painter, Vector2 position, float radius)
        {
            painter.strokeColor = Color.gray;
            painter.fillColor = Color.black;
            painter.lineWidth = 1.5f;
            painter.BeginPath();

            // Simple jagged polygon generation
            int points = 8;
            for (int i = 0; i <= points; i++)
            {
                float angle = (i * 360f / points) * Mathf.Deg2Rad;
                float rOffset = radius + Random.Range(-radius * 0.2f, radius * 0.2f); // Jaggedness
                Vector2 pt = position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * rOffset;

                if (i == 0) painter.MoveTo(pt);
                else painter.LineTo(pt);
            }

            painter.Fill();
            painter.Stroke();
        }
    }
}
#endif