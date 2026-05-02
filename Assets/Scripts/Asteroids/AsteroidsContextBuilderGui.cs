#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.Asteroids
{
    public class AsteroidsContextBuilderGui : IGuiProvider
    {
        public string Title => "ASTEROIDS MASTER SETTINGS";

        // In a real scenario, this might be loaded from a serialized asset or scene object.
        private AsteroidsContext _context = new AsteroidsContext();

        public VisualElement CreateGui(GuiContext ctx)
        {
            var builder = new ForgeContainerBuilder("AsteroidsMasterSettings_Root")
                            .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f))
                            .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                            .WithFlexGrow(1f);

            // --- LEFT SIDEBAR: SYSTEM SETTINGS (30%) ---
            builder.AddChild(new ForgeContainerBuilder("SidebarPane")
                .WithWidth(new StyleLength(Length.Percent(30f)))
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f))
                .WithBorderWidth(0, 2f, 0, 0)
                .WithBorderColor(Color.cyan)
                .WithPadding(15f)

                .AddChild(new ForgeLabelBuilder("CABINET CONFIGURATION")
                    .WithColor(Color.cyan)
                    .WithFontSize(18)
                    .WithBold()
                    .WithMarginBottom(20f))

                // Session Settings
                .AddChild(new ForgeContainerBuilder("SessionBox")
                    .WithBackgroundColor(Color.black)
                    .WithPadding(10f)
                    .WithBorderWidth(0, 0, 0, 2f)
                    .WithBorderColor(Color.yellow)
                    .WithMarginBottom(20f)
                    .AddChild(new ForgeLabelBuilder("ARCADE SESSION RULES")
                        .WithColor(Color.yellow)
                        .WithFontSize(10)
                        .WithBold()
                        .WithMarginBottom(5f))
                    .AddChild(new ForgeTextFieldBuilder("Starting Lives", _context.Score.Lives.ToString())
                        .OnValueChanged(evt => { if (int.TryParse(evt.newValue, out int v)) _context.Score.Lives = v; }))
                    .AddChild(new ForgeTextFieldBuilder("High Score Target", "10000").AsReadOnly(true))
                )

                // Screen Boundaries
                .AddChild(new ForgeContainerBuilder("BoundsBox")
                    .WithBackgroundColor(Color.black)
                    .WithPadding(10f)
                    .WithBorderWidth(0, 0, 0, 2f)
                    .WithBorderColor(Color.green)
                    .WithMarginBottom(20f)
                    .AddChild(new ForgeLabelBuilder("2D SCREEN WRAP BOUNDARIES")
                        .WithColor(Color.green)
                        .WithFontSize(10)
                        .WithBold()
                        .WithMarginBottom(5f))
                    .AddChild(new ForgeTextFieldBuilder("Width Limit (X)", _context.ScreenBounds.x.ToString())
                        .OnValueChanged(evt => { if (float.TryParse(evt.newValue, out float v)) _context.ScreenBounds = new Vector2(v, _context.ScreenBounds.y); }))
                    .AddChild(new ForgeTextFieldBuilder("Height Limit (Y)", _context.ScreenBounds.y.ToString())
                        .OnValueChanged(evt => { if (float.TryParse(evt.newValue, out float v)) _context.ScreenBounds = new Vector2(_context.ScreenBounds.x, v); }))
                )
            );

            // --- RIGHT AREA: REAL-TIME CABINET PREVIEW ---
            builder.AddChild(new ForgeContainerBuilder("PreviewArea")
                .WithFlexGrow(1f)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.02f))
                .AddChild(new ForgeLabelBuilder("CABINET CRT PREVIEW")
                    .WithColor(Color.gray)
                    .WithFontSize(16)
                    .WithBold()
                    .WithMarginBottom(10f))

                // FIXED: Wrapped the custom lambda in a DynamicGuiProvider
                .AddChild(new DynamicGuiProvider(c => {
                    return new AsteroidsScreenPreviewRenderer(_context)
                    {
                        style = {
                            width = Length.Percent(90f),
                            height = Length.Percent(80f),
                            backgroundColor = Color.black,
                            borderLeftWidth = 4f, borderRightWidth = 4f, borderTopWidth = 4f, borderBottomWidth = 4f,
                            borderLeftColor = new Color(0.2f, 0.2f, 0.2f), borderRightColor = new Color(0.2f, 0.2f, 0.2f),
                            borderTopColor = new Color(0.2f, 0.2f, 0.2f), borderBottomColor = new Color(0.2f, 0.2f, 0.2f)
                        }
                    };
                }))
            );

            return builder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var snapshotRoot = CreateGui(new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? "AsteroidsMasterConfig_Snapshot" : System.IO.Path.GetFileNameWithoutExtension(assetPath);
            WorkshopUxmlBaker.Bake(snapshotRoot, fileName);
        }

        public void FromUIDocument(string assetPath)
        {
            ForgeLogger.LogWarning("[AsteroidsContextBuilderGui] FromUIDocument is not supported. This UI is dynamically generated via GuiBuilders.");
        }
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
                float rOffset = radius + UnityEngine.Random.Range(-radius * 0.2f, radius * 0.2f); // Jaggedness
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