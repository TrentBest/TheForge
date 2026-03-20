using TheSingularityWorkshop.Armada2525;
using TheSingularityWorkshop.Armada2525.Math;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Armada2525_Gui_GalaxyView : IGuiProvider
{
    public string Title => "Galaxy Command";

    // --- References ---
    private Armada2525 _game;
    private VisualElement _mapViewport; // The clipping container
    private VisualElement _worldContainer; // The moving world (Pan/Zoom target)
    private VisualElement _dataCardContainer;
    private Label _coordsLabel;
    private Toggle _3dModeToggle;
    private GuiContext _lastCtx;

    // --- Navigation State ---
    private Vector2 _panOffset = Vector2.zero; // Pixel offset from center
    private float _zoomLevel = 1.0f; // 0.1 (Far) to 3.0 (Close)
    private bool _isDragging = false;
    private Vector2 _lastPointerPos;
    private Vector2 _lastClickDownPos;

    // --- Galaxy Settings ---
    private const int SECTOR_SIZE = 100; // Base pixels per coordinate unit
    private const int VIEW_BUFFER = 2; // How many extra sectors to render off-screen

    // --- Selection State ---
    private uint _selectedSystemSeed = 0;
    private int _currentSectorX = 0; // The sector the camera is currently looking at
    private int _currentSectorY = 0;

    // --- Object Pooling (Optimization for Epic Scales) ---
    private List<VisualElement> _starPool = new List<VisualElement>();
    private int _activeStarCount = 0;

    public VisualElement CreateGui(GuiContext ctx)
    {
        _lastCtx = ctx;
        _game = UnityEngine.Object.FindAnyObjectByType<Armada2525>();

        // 1. Root Layout (Master-Detail)
        var builder = new GraphicalUserInterfaceBuilder("GalaxyView_Root")
            .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
            .WithPercentSize(100, 100)
            .WithBackgroundColor(Color.black);

        var root = builder.Build();

        // --- LEFT PANEL: THE VIEWPORT (70%) ---
        var mapPanel = new VisualElement
        {
            name = "MapViewport",
            style = {
                flexGrow = 1.0f, // Taking more space for the "Command Center" feel
                overflow = Overflow.Hidden,
                backgroundColor = new Color(0.01f, 0.01f, 0.03f)
            }
        };

        // Add the Tactical Grid visual
        mapPanel.generateVisualContent += DrawTacticalGrid;

        // The World Container
        _worldContainer = new VisualElement
        {
            name = "WorldContainer",
            style = { width = new StyleLength(StyleKeyword.Auto), height = new StyleLength(StyleKeyword.Auto), position = Position.Absolute }
        };
        mapPanel.Add(_worldContainer);

        // HUD Overlay
        var hud = new VisualElement { style = { position = Position.Absolute, top = 20, left = 20, flexDirection = FlexDirection.Row } };
        _coordsLabel = new Label("INITIATING SENSORS..")
        {
            style = { color = Color.cyan, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginRight = 20 }
        };

        _3dModeToggle = new Toggle("Holo-View (3D)") { value = false };
        _3dModeToggle.RegisterValueChangedCallback(evt => Toggle3DMode(evt.newValue));
        _3dModeToggle.style.color = Color.white;

        hud.Add(_coordsLabel);
        hud.Add(_3dModeToggle);
        mapPanel.Add(hud);

        // Input Handling
        mapPanel.RegisterCallback<PointerDownEvent>(OnPointerDown);
        mapPanel.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        mapPanel.RegisterCallback<PointerUpEvent>(OnPointerUp);
        mapPanel.RegisterCallback<WheelEvent>(OnScrollWheel);

        // Initial Center
        mapPanel.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            if (_panOffset == Vector2.zero)
                CenterViewOn(0, 0);
            RefreshView();
        });

        _mapViewport = mapPanel;
        root.Add(mapPanel);

        // --- RIGHT PANEL: DATA CARD (300px fixed for terminal feel) ---
        _dataCardContainer = new VisualElement
        {
            style = {
                width = 300,
                paddingLeft = 15, paddingRight = 15, paddingTop = 25,
                backgroundColor = new Color(0.05f, 0.07f, 0.1f),
                borderLeftWidth = 2, borderLeftColor = Color.cyan
            }
        };
        RenderEmptyDataCard();
        root.Add(_dataCardContainer);

        return root;
    }

    // --------------------------------------------------------------------------------
    // INPUT & NAVIGATION
    // --------------------------------------------------------------------------------

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (evt.button == 0 || evt.button == 2)
        {
            _isDragging = true;
            _lastPointerPos = evt.position;
            _lastClickDownPos = evt.position; // Mark where we clicked
            _mapViewport.CapturePointer(evt.pointerId);
        }
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (_isDragging)
        {
            Vector2 delta = (Vector2)evt.position - _lastPointerPos;
            _panOffset += delta;
            _lastPointerPos = evt.position;
            RefreshView();
        }
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (_isDragging)
        {
            _isDragging = false;
            _mapViewport.ReleasePointer(evt.pointerId);

            // If the mouse didn't move far, treat it as a click!
            if (Vector2.Distance(evt.position, _lastClickDownPos) < 5f)
            {
                OnMapClicked(evt.position);
            }
        }
    }

    private void OnScrollWheel(WheelEvent evt)
    {
        float zoomSpeed = 0.05f;
        float newZoom = _zoomLevel - (evt.delta.y * zoomSpeed);
        _zoomLevel = Mathf.Clamp(newZoom, 0.1f, 4.0f);

        RefreshView();
    }

    private void OnMapClicked(Vector2 screenPos)
    {
        // Translate screen click into world grid coordinates
        Vector2 clickWorld = (screenPos - _panOffset) / _zoomLevel;
        int clickX = Mathf.RoundToInt(clickWorld.x / SECTOR_SIZE);
        int clickY = Mathf.RoundToInt(clickWorld.y / -SECTOR_SIZE);

        // Center on the slice/sector they clicked
        CenterViewOn(clickX, clickY);

        // Auto-zoom if they were zoomed out
        if (_zoomLevel < 1.2f) _zoomLevel = 1.5f;

        RefreshView();
    }

    private void CenterViewOn(int gridX, int gridY)
    {
        if (_mapViewport == null) return;
        float centerX = _mapViewport.resolvedStyle.width / 2f;
        float centerY = _mapViewport.resolvedStyle.height / 2f;
        _panOffset = new Vector2(centerX - (gridX * SECTOR_SIZE), centerY - (-gridY * SECTOR_SIZE));
    }

    // --------------------------------------------------------------------------------
    // RENDERING & POOLING
    // --------------------------------------------------------------------------------

    private void RefreshView()
    {
        if (_worldContainer == null || _mapViewport == null) return;

        // 1. Update Grid Info
        Vector2 centerScreen = new Vector2(_mapViewport.resolvedStyle.width / 2f, _mapViewport.resolvedStyle.height / 2f);
        Vector2 centerWorld = (centerScreen - _panOffset) / _zoomLevel;
        _currentSectorX = Mathf.RoundToInt(centerWorld.x / SECTOR_SIZE);
        _currentSectorY = Mathf.RoundToInt(centerWorld.y / -SECTOR_SIZE);

        _coordsLabel.text = $"SECTOR: {_currentSectorX}, {_currentSectorY} | SCAN: {(_zoomLevel * 100):0}%";

        // 2. Determine Visible Range
        float screenWidth = _mapViewport.resolvedStyle.width;
        float screenHeight = _mapViewport.resolvedStyle.height;
        float scaledSector = SECTOR_SIZE * _zoomLevel;

        int rangeX = Mathf.CeilToInt((screenWidth / scaledSector) / 2f) + VIEW_BUFFER;
        int rangeY = Mathf.CeilToInt((screenHeight / scaledSector) / 2f) + VIEW_BUFFER;

        // 3. Reset Pool
        _activeStarCount = 0;
        HideAllStars();

        // 4. Render Visible Stars
        for (int x = _currentSectorX - rangeX; x <= _currentSectorX + rangeX; x++)
        {
            for (int y = _currentSectorY - rangeY; y <= _currentSectorY + rangeY; y++)
            {
                uint seed = GalaxyMath.GetStarSeed(x, y);
                if (GalaxyMath.HasStar(seed))
                {
                    Vector2 screenPos = GridToScreen(x, y);
                    RenderStarAt(screenPos, x, y, seed);
                }
            }
        }

        // 5. Render Core (Optional fallback, mostly handled by the new grid now)
        if (Mathf.Abs(_currentSectorX) <= rangeX && Mathf.Abs(_currentSectorY) <= rangeY)
        {
            RenderGalacticCore(GridToScreen(0, 0));
        }

        // Force the grid to redraw with the new center
        _mapViewport.MarkDirtyRepaint();
    }

    private void DrawTacticalGrid(MeshGenerationContext mgc)
    {
        var painter = mgc.painter2D;
        float scaledSector = SECTOR_SIZE * _zoomLevel;
        float screenDiag = Mathf.Sqrt(Mathf.Pow(_mapViewport.resolvedStyle.width, 2) + Mathf.Pow(_mapViewport.resolvedStyle.height, 2));

        Vector2 galCenter = _panOffset; // Screen position of World 0,0

        // ------------------------------------------------------
        // 1. GLOBAL GALAXY GRID (Yellow)
        // ------------------------------------------------------
        painter.strokeColor = new Color(1f, 0.8f, 0f, 0.1f); // Faint gold/yellow
        painter.lineWidth = 1;

        int sliceCount = 12; // 30-degree slices

        // Radiating Lines from Galaxy Center to infinity
        for (int i = 0; i < sliceCount; i++)
        {
            float angle = (i / (float)sliceCount) * Mathf.PI * 2f;
            Vector2 endPoint = galCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (screenDiag * 2);
            painter.BeginPath();
            painter.MoveTo(galCenter);
            painter.LineTo(endPoint);
            painter.Stroke();
        }

        // Concentric Circles (Every 5 sectors represents a major galactic ring)
        int numRings = Mathf.CeilToInt((screenDiag * 2f) / (scaledSector * 5f));
        for (int i = 1; i <= numRings; i++)
        {
            DrawCircle(painter, galCenter, i * scaledSector * 5f);
        }

        // Absolute Zero Center Marker
        painter.strokeColor = Color.yellow;
        painter.lineWidth = 2 * _zoomLevel;
        DrawCircle(painter, galCenter, 15 * _zoomLevel);


        // ------------------------------------------------------
        // 2. LOCAL SECTOR GRID (Cyan)
        // ------------------------------------------------------
        Vector2 sectorCenter = GridToScreen(_currentSectorX, _currentSectorY);

        painter.strokeColor = new Color(0f, 1f, 1f, 0.15f); // Bright cyan
        painter.lineWidth = 1;

        // Local Radiating Lines
        for (int i = 0; i < sliceCount; i++)
        {
            float angle = (i / (float)sliceCount) * Mathf.PI * 2f;
            Vector2 endPoint = sectorCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (3 * scaledSector);
            painter.BeginPath();
            painter.MoveTo(sectorCenter);
            painter.LineTo(endPoint);
            painter.Stroke();
        }

        // Local Concentric Circles
        for (int i = 1; i <= 3; i++) // Render 3 rings deep for local scope
        {
            DrawCircle(painter, sectorCenter, i * scaledSector);
        }

        // Sector Center Marker
        painter.strokeColor = Color.cyan;
        painter.lineWidth = 1 * _zoomLevel;
        DrawCircle(painter, sectorCenter, 5 * _zoomLevel);
    }

    // Bulletproof manual circle drawer (Ensures API compatibility across Unity UI Toolkit versions)
    private void DrawCircle(Painter2D painter, Vector2 center, float radius)
    {
        painter.BeginPath();
        int segments = 64; // High-res curve
        for (int i = 0; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            Vector2 p = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            if (i == 0) painter.MoveTo(p);
            else painter.LineTo(p);
        }
        painter.Stroke();
    }

    private Vector2 GridToScreen(int x, int y)
    {
        float wx = x * SECTOR_SIZE * _zoomLevel;
        float wy = y * -SECTOR_SIZE * _zoomLevel;
        return new Vector2(wx + _panOffset.x, wy + _panOffset.y);
    }

    private void RenderStarAt(Vector2 pos, int x, int y, uint seed)
    {
        VisualElement star = GetStarFromPool();
        float baseSize = GalaxyMath.Range(seed, 6, 14, 24);
        float size = baseSize * _zoomLevel;
        Color color = GalaxyMath.GetStarColor(seed);
        bool isSelected = _selectedSystemSeed == seed;

        star.style.width = size;
        star.style.height = size;
        star.style.left = pos.x - (size / 2);
        star.style.top = pos.y - (size / 2);
        star.style.backgroundColor = color;
        star.style.borderTopLeftRadius = size;
        star.style.borderTopRightRadius = size;
        star.style.borderBottomLeftRadius = size;
        star.style.borderBottomRightRadius = size;
        star.style.display = DisplayStyle.Flex;

        star.style.borderBottomWidth = isSelected ? 2 * _zoomLevel : 0;
        star.style.borderTopWidth = isSelected ? 2 * _zoomLevel : 0;
        star.style.borderRightWidth = isSelected ? 2 * _zoomLevel : 0;
        star.style.borderLeftWidth = isSelected ? 2 * _zoomLevel : 0;
        star.style.borderBottomColor = Color.white;
        star.style.borderTopColor = Color.white;
        star.style.borderRightColor = Color.white;
        star.style.borderLeftColor = Color.white;

        star.userData = seed;
        star.name = $"Star_{x}_{y}";

        // LOD Label Management
        Label lbl = star.Q<Label>("StarLabel");
        if (_zoomLevel > 0.8f)
        {
            if (lbl == null) { lbl = new Label(); lbl.name = "StarLabel"; star.Add(lbl); }
            lbl.text = GalaxyMath.GetProceduralName(seed);
            lbl.style.fontSize = 10 * _zoomLevel;
            lbl.style.color = new Color(1, 1, 1, 0.8f);
            lbl.style.top = size + 2;
            lbl.style.display = DisplayStyle.Flex;
            lbl.style.position = Position.Absolute;
        }
        else if (lbl != null)
        {
            lbl.style.display = DisplayStyle.None;
        }
    }

    private void RenderGalacticCore(Vector2 pos)
    {
        var coreSize = 60 * _zoomLevel;
        var core = new VisualElement();
        core.style.position = Position.Absolute;
        core.style.left = pos.x - (coreSize / 2);
        core.style.top = pos.y - (coreSize / 2);
        core.style.width = coreSize;
        core.style.height = coreSize;
        core.style.backgroundColor = Color.black;
        core.style.borderBottomWidth = 2 * _zoomLevel;
        core.style.borderBottomColor = new Color(0.6f, 0.2f, 1f, 0.5f);
        core.style.borderTopWidth = 2 * _zoomLevel;
        core.style.borderTopColor = new Color(0.6f, 0.2f, 1f, 0.5f);
        core.style.borderRightWidth = 2 * _zoomLevel;
        core.style.borderRightColor = new Color(0.6f, 0.2f, 1f, 0.5f);
        core.style.borderLeftWidth = 2 * _zoomLevel;
        core.style.borderLeftColor = new Color(0.6f, 0.2f, 1f, 0.5f);
        core.style.borderTopLeftRadius = coreSize;
        core.style.borderTopRightRadius = coreSize;
        core.style.borderBottomLeftRadius = coreSize;
        core.style.borderBottomRightRadius = coreSize;

        _worldContainer.Add(core);
    }

    private VisualElement GetStarFromPool()
    {
        if (_activeStarCount >= _starPool.Count)
        {
            var newStar = new VisualElement();
            newStar.style.position = Position.Absolute;
            newStar.RegisterCallback<ClickEvent>(evt => OnStarClicked(newStar));
            _worldContainer.Add(newStar);
            _starPool.Add(newStar);
        }

        var star = _starPool[_activeStarCount];
        _activeStarCount++;
        return star;
    }

    private void HideAllStars()
    {
        foreach (var star in _starPool) star.style.display = DisplayStyle.None;
        _worldContainer.Clear(); // Clear non-pooled items like the core
    }

    private void OnStarClicked(VisualElement starEl)
    {
        if (starEl.userData is uint seed)
        {
            _selectedSystemSeed = seed;
            RenderDataCard(0, 0, seed);
            RefreshView();
        }
    }

    // --------------------------------------------------------------------------------
    // DATA & 3D LOGIC
    // --------------------------------------------------------------------------------

    private void Toggle3DMode(bool enabled)
    {
        if (enabled)
        {
            _worldContainer.style.display = DisplayStyle.None;
            _mapViewport.style.backgroundColor = new Color(0, 0, 0, 0);

            var gizmoMsg = new Label("NEURAL LINK: ESTABLISHING HOLO-PROJECTION..")
            {
                name = "HoloLabel",
                style = { color = Color.cyan, alignSelf = Align.Center, marginTop = 200, unityFontStyleAndWeight = FontStyle.Bold }
            };
            _mapViewport.Add(gizmoMsg);
        }
        else
        {
            _mapViewport.style.backgroundColor = new Color(0.01f, 0.01f, 0.03f);
            var label = _mapViewport.Q<Label>("HoloLabel");
            if (label != null) _mapViewport.Remove(label);

            _worldContainer.style.display = DisplayStyle.Flex;
            RefreshView();
        }
    }

    private void RenderDataCard(int x, int y, uint seed)
    {
        _dataCardContainer.Clear();
        int systemId = (int)seed;
        string systemName = GalaxyMath.GetProceduralName(seed).ToUpper();

        bool isColonized = false;
        string statusText = "Uncharted";

        if (_game != null && _game.Data != null)
        {
            var colony = _game.Data.GetColony(systemId);
            //if (colony != null)
            //{
                isColonized = true;
                //systemName = colony.Name.ToUpper();
                statusText = $"Claimed (Pop: {colony.PopulationMillions}M)";
            //}
        }

        var header = new Label(systemName)
        {
            style = { color = Color.cyan, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 }
        };
        _dataCardContainer.Add(header);

        var stats = new Label(
            $"Classification: Class-{seed % 7}\n" +
            $"Luminosity: {GalaxyMath.Range(seed, 0.5f, 5f, 12):F2}\n" +
            $"Status: {statusText}\n" +
            $"Stability: Nominal")
        {
            style = { color = Color.white, fontSize = 12, marginBottom = 20 }
        };
        _dataCardContainer.Add(stats);

        var enterBtn = new Button(() => {
            if (_game != null)
            {
                _game.SwitchGui("StarView");
            }
        })
        {
            text = "ENGAGE ENGINES",
            style = {
                height = 40, backgroundColor = new Color(0, 0.3f, 0.6f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold
            }
        };
        _dataCardContainer.Add(enterBtn);
    }

    private void RenderEmptyDataCard()
    {
        _dataCardContainer.Clear();
        var msg = new Label(">> SELECT SECTOR STAR <<")
        {
            style = { color = Color.gray, alignSelf = Align.Center, marginTop = 50, fontSize = 10 }
        };
        _dataCardContainer.Add(msg);
    }

    public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

#if UNITY_EDITOR
    public void ToUIDocument(string assetPath)
    {
        VisualElement myTree = CreateGui(new GuiContext());
        GraphicalUserInterfaceBuilder.ConvertToUIDocument(myTree, assetPath);
    }
#endif

    public void FromUIDocument(string assetPath)
    {
        VisualElement hydratedUi = GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath);
        _lastCtx?.OnBuilt?.Invoke(hydratedUi);
    }
}