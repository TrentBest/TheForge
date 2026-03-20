using TheSingularityWorkshop.Armada2525;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using UnityEngine;
using UnityEngine.UIElements;

public class Armada2525_Gui_TransportTycoon : IGuiProvider
{
    public string Title => "GALACTIC TRANSPORT TYCOON";

    private Armada2525 _game;
    private GuiContext _lastCtx;

    public VisualElement CreateGui(GuiContext ctx)
    {
        _lastCtx = ctx;
        _game = Object.FindAnyObjectByType<Armada2525>();

        var builder = new GraphicalUserInterfaceBuilder("Tycoon_Root")
            .WithBackgroundColor(new Color(0.05f, 0.05f, 0.1f))
            .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
            .WithPercentSize(100, 100);

        // --- LEFT SIDEBAR: COMPANY STATS ---
        builder.AddChild(c => {
            var sidebar = new VisualElement { style = { width = Length.Percent(25), backgroundColor = new Color(0.1f, 0.1f, 0.15f), borderRightWidth = 2, borderRightColor = Color.magenta, paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15, } };

            sidebar.Add(new Button(() => {
                if (_game != null) _game.SwitchGui("AdvisorsView");
                else Debug.LogWarning("[TransportTycoon] Cannot return, game instance is null.");
            })
            { text = "<< RETURN TO COUNCIL", style = { backgroundColor = Color.clear, color = Color.gray, borderBottomWidth = 1, borderBottomColor = Color.gray, marginBottom = 20 } });

            sidebar.Add(new Label("IMPERIAL LOGISTICS INC.") { style = { color = Color.magenta, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            var fundsBox = new VisualElement { style = { backgroundColor = Color.black, paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10, borderLeftWidth = 2, borderLeftColor = Color.green, marginBottom = 20 } };
            fundsBox.Add(new Label("CORPORATE FUNDS:") { style = { color = Color.gray, fontSize = 10 } });
            fundsBox.Add(new Label("1,450,000 cr") { style = { color = Color.green, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold } });
            sidebar.Add(fundsBox);

            sidebar.Add(new Label("COMPETITION BOARDS") { style = { color = Color.gray, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
            sidebar.Add(new Label("1. Imperial Logistics (You) - 45% Share") { style = { color = Color.white } });
            sidebar.Add(new Label("2. Kraal Void-Runners - 30% Share") { style = { color = Color.red } });
            sidebar.Add(new Label("3. Cygnus Smugglers - 25% Share") { style = { color = Color.yellow } });

            sidebar.Add(new Label("FLEET MANAGEMENT") { style = { color = Color.gray, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 20, marginBottom = 5 } });
            sidebar.Add(new Button(() => Debug.Log("Buying Freighter")) { text = "Buy Light Freighter (50k)", style = { marginBottom = 5 } });
            sidebar.Add(new Button(() => Debug.Log("Buying Tech")) { text = "Research: Slipstream Drives (500k)", style = { backgroundColor = new Color(0.2f, 0.2f, 0.6f), color = Color.white } });

            return sidebar;
        });

        // --- RIGHT AREA: REAL-TIME GALAXY & CONTRACTS ---
        builder.AddChild(c => {
            var rightCol = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };

            // Top: The Custom Real-Time Galaxy Renderer
            var mapArea = new TycoonMapRenderer { style = { flexGrow = 0.7f, backgroundColor = Color.black, borderBottomWidth = 2, borderBottomColor = Color.magenta } };
            rightCol.Add(mapArea);

            // Bottom: Open Contracts
            var contractArea = new VisualElement { style = { flexGrow = 0.3f, paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15, backgroundColor = new Color(0.05f, 0.05f, 0.05f) } };
            contractArea.Add(new Label("OPEN GALACTIC CONTRACTS") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            var contractList = new ScrollView(ScrollViewMode.Horizontal) { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };
            contractList.Add(CreateJobCard("URGENT: 500T Iron", "Sol -> Alpha Centauri", 45000, Color.cyan));
            contractList.Add(CreateJobCard("FOOD RELIEF: 200T", "Sirius -> Vega Prime", 12000, Color.green));
            contractList.Add(CreateJobCard("LUXURY GOODS: 50T", "Arcturus -> Sol", 85000, Color.yellow));
            contractArea.Add(contractList);

            rightCol.Add(contractArea);
            return rightCol;
        });

        return builder.Build();
    }

    private VisualElement CreateJobCard(string title, string route, int payout, Color theme)
    {
        var card = new VisualElement { style = { width = 200, backgroundColor = new Color(0.1f, 0.1f, 0.15f), paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10, marginRight = 10, borderTopWidth = 3, borderTopColor = theme } };
        card.Add(new Label(title) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
        card.Add(new Label(route) { style = { color = Color.gray, fontSize = 10, marginBottom = 10 } });
        card.Add(new Button(() => Debug.Log("Contract Accepted!")) { text = $"ACCEPT ({payout} cr)", style = { backgroundColor = new Color(0.2f, 0.4f, 0.2f), color = Color.white, marginTop = 10 } });
        return card;
    }

    public System.Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
#if UNITY_EDITOR
    public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
}

// --- CUSTOM MAP RENDERER COMPONENT ---
public class TycoonMapRenderer : VisualElement
{
    public Color EmpireColor = Color.red; // Set this to the player's chosen color

    // We will update this dynamically later when reading from the actual GameData
    private Vector2 _mockHomeSystemPos;

    public TycoonMapRenderer()
    {
        // Subscribe to the geometry update to know our exact pixel dimensions
        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

        // Subscribe to the draw loop
        generateVisualContent += OnGenerateVisualContent;
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        // For demonstration, put the home system in the middle-left of the view
        _mockHomeSystemPos = new Vector2(evt.newRect.width * 0.3f, evt.newRect.height * 0.5f);
        MarkDirtyRepaint();
    }

    private void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        float width = layout.width;
        float height = layout.height;
        if (width <= 0 || height <= 0) return;

        var painter = ctx.painter2D;

        // 1. Draw the Base Galactic Cartesian Grid
        painter.lineWidth = 1f;
        painter.strokeColor = new Color(0.5f, 0.5f, 0.5f, 0.15f); // Faint gray
        painter.BeginPath();

        float gridSize = 50f;
        for (float x = 0; x < width; x += gridSize) { painter.MoveTo(new Vector2(x, 0)); painter.LineTo(new Vector2(x, height)); }
        for (float y = 0; y < height; y += gridSize) { painter.MoveTo(new Vector2(0, y)); painter.LineTo(new Vector2(width, y)); }
        painter.Stroke();

        // 2. Draw the Empire Radial Grid
        Color radialColor = EmpireColor;
        radialColor.a = 0.25f; // Keep it semi-transparent so it doesn't overwhelm the map
        painter.strokeColor = radialColor;
        painter.lineWidth = 1.5f;
        painter.BeginPath();

        // Concentric Distance Rings
        for (float r = gridSize; r < width * 1.5f; r += gridSize)
        {
            painter.Arc(_mockHomeSystemPos, r, 0, 360);
        }

        // Radiating Nav-Lanes (every 45 degrees)
        for (int angle = 0; angle < 360; angle += 45)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector2 endPos = _mockHomeSystemPos + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * (width * 1.5f);

            painter.MoveTo(_mockHomeSystemPos);
            painter.LineTo(endPos);
        }
        painter.Stroke();

        // 3. Draw the Home System Marker
        painter.fillColor = EmpireColor;
        painter.BeginPath();
        painter.Arc(_mockHomeSystemPos, 6f, 0, 360);
        painter.Fill();

        // Draw a few mock competitor nodes
        painter.fillColor = Color.yellow;
        painter.BeginPath();
        painter.Arc(_mockHomeSystemPos + new Vector2(200, -100), 4f, 0, 360);
        painter.Fill();

        painter.fillColor = Color.cyan;
        painter.BeginPath();
        painter.Arc(_mockHomeSystemPos + new Vector2(150, 150), 4f, 0, 360);
        painter.Fill();
    }
}