#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_ShipDesigner : IGuiProvider
    {
        public string Title => "NAVAL ARCHITECTURE BUREAU";

        private MastersOfOrionII_Game _game;
        private GuiContext _lastCtx;

        // --- UI Layout Areas ---
        private VisualElement _sidebarPanel;
        private VisualElement _configPanel;
        private VisualElement _blueprintPanel;
        private Label _statsLabel;

        // --- ENUMS & STATE ---
        private enum DesignTab { Hull, Systems, Propulsion, Weapons, Defense, Crew, Cargo, Security }
        private DesignTab _currentTab = DesignTab.Hull;

        private Dictionary<DesignTab, bool> _hiredFirms = new Dictionary<DesignTab, bool>
        {
            { DesignTab.Hull, true }, { DesignTab.Systems, true },
            { DesignTab.Propulsion, false }, { DesignTab.Weapons, false },
            { DesignTab.Defense, false }, { DesignTab.Crew, false },
            { DesignTab.Cargo, false }, { DesignTab.Security, false }
        };

        private Dictionary<DesignTab, long> _firmCosts = new Dictionary<DesignTab, long>
        {
            { DesignTab.Propulsion, 50000 }, { DesignTab.Weapons, 120000 },
            { DesignTab.Defense, 80000 }, { DesignTab.Crew, 25000 },
            { DesignTab.Cargo, 10000 }, { DesignTab.Security, 35000 }
        };

        // Design State
        private string _baseName = "Wraith";
        private int _version = 1;

        // Data-driven Hull state
        private HullRole _selectedRole;
        private float _magnitude = 3.0f; // Default to a Frigate scale
        private int _currentDeck = 1;
        private int _maxDecks = 3;
        private float _metersPerHex = 5f;

        // Stats
        private float _maxCapacity = 1000f;
        private float _usedCapacity = 0f;
        private float _powerGenerated = 0f;
        private float _powerRequired = 0f;
        private long _cost = 0;

        // Blueprint Drawing State
        private bool _mirrorSymmetry = true;
        private List<Vector2> _snapPoints = new List<Vector2>();
        private List<Vector2> _drawnVertices = new List<Vector2>();
        private bool _isHullClosed = false;

        private readonly float CANVAS_WIDTH = 400f;
        private readonly float CANVAS_HEIGHT = 400f;

        public MastersOfOrionII_Gui_ShipDesigner()
        {
            if (HullRole.Database.Count > 0) _selectedRole = HullRole.Database[0];
            UpdateScaleCalculations();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            _game = UnityEngine.Object.FindAnyObjectByType<MastersOfOrionII_Game>();

            var builder = new GraphicalUserInterfaceBuilder("ShipDesigner_Root")
                .WithBackgroundColor(new Color(0.01f, 0.02f, 0.04f, 1f))
                .WithPercentSize(100, 100)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

            // --- TOP BAR ---
            builder.AddChild(c =>
            {
                var topBar = new VisualElement { style = { height = 60, flexDirection = FlexDirection.Row, backgroundColor = new Color(0.05f, 0.07f, 0.1f), borderBottomWidth = 2, borderBottomColor = Color.cyan, paddingLeft = 20, paddingRight = 20, alignItems = Align.Center } };
                topBar.Add(new Label("NAVAL ARCHITECTURE BUREAU // HULL SCHEMATICS") { style = { color = Color.cyan, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1 } });
                topBar.Add(new Button(() => _game?.SwitchGui("ShipYardView")) { text = "EXIT DESIGNER", style = { height = 35, width = 120, backgroundColor = new Color(0.4f, 0.1f, 0.1f), color = Color.white } });
                return topBar;
            });

            // --- MAIN WORKSPACE ---
            builder.AddChild(c =>
            {
                var workspace = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row } };

                // 1. LEFT SIDEBAR
                _sidebarPanel = new VisualElement { style = { width = 220, backgroundColor = new Color(0.03f, 0.05f, 0.08f), borderRightWidth = 2, borderRightColor = Color.cyan } };
                RenderSidebar();
                workspace.Add(_sidebarPanel);

                // 2. RIGHT WORKSPACE
                var rightArea = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };

                // Viewports
                var viewportsRow = new VisualElement { style = { height = Length.Percent(60), flexDirection = FlexDirection.Row } };

                // 3D Viewport
                var modelView = new VisualElement { style = { flexGrow = 1, backgroundColor = new Color(0, 0, 0, 0.8f), marginTop = 10, marginBottom = 10, marginLeft = 10, marginRight = 10, borderTopWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, borderRightWidth = 2, borderTopColor = Color.cyan, borderBottomColor = Color.cyan, borderLeftColor = Color.cyan, borderRightColor = Color.cyan, alignItems = Align.Center, justifyContent = Justify.Center } };
                modelView.Add(new Label(">>> 3D HULL RENDER PORT <<<\nOrbit Camera Active") { style = { color = new Color(0, 1, 1, 0.3f), unityTextAlign = TextAnchor.MiddleCenter } });
                viewportsRow.Add(modelView);

                // Deck Blueprint View (The Canvas)
                var blueprintView = new VisualElement { style = { flexGrow = 1, backgroundColor = new Color(0.02f, 0.04f, 0.08f), marginTop = 10, marginBottom = 10, marginLeft = 10, marginRight = 10, borderTopWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, borderRightWidth = 2, borderTopColor = Color.green, borderBottomColor = Color.green, borderLeftColor = Color.green, borderRightColor = Color.green } };
                _blueprintPanel = new VisualElement { style = { flexGrow = 1 } };

                GenerateHexSnapPoints(CANVAS_WIDTH, CANVAS_HEIGHT, 20f);
                RenderBlueprintCanvas();

                blueprintView.Add(_blueprintPanel);
                viewportsRow.Add(blueprintView);
                rightArea.Add(viewportsRow);

                // Configuration Panel
                var configArea = new VisualElement { style = { height = Length.Percent(40), flexDirection = FlexDirection.Row, backgroundColor = new Color(0.05f, 0.05f, 0.08f), borderTopWidth = 2, borderTopColor = Color.cyan } };
                _configPanel = new VisualElement { style = { flexGrow = 1, paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20 } };
                RenderConfigPanel();
                configArea.Add(_configPanel);

                // Master Stats Readout
                var statsPanel = new VisualElement { style = { width = 300, backgroundColor = new Color(0.02f, 0.02f, 0.03f), borderLeftWidth = 2, borderLeftColor = Color.cyan, paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15 } };
                _statsLabel = new Label() { style = { color = Color.white, fontSize = 12, whiteSpace = WhiteSpace.Normal } };
                UpdateStats();
                statsPanel.Add(new Label("DESIGN SPECS & TOLERANCES") { style = { color = Color.cyan, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
                statsPanel.Add(_statsLabel);

                var saveBtn = new Button(() => Debug.Log("Saving Design")) { text = "CERTIFY & SAVE DESIGN", style = { height = 40, backgroundColor = new Color(0, 0.4f, 0.2f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } };
                statsPanel.Add(saveBtn);

                configArea.Add(statsPanel);
                rightArea.Add(configArea);

                workspace.Add(rightArea);
                return workspace;
            });

            return builder.Build();
        }

        // --------------------------------------------------------------------------------
        // SIDEBAR & CONFIG
        // --------------------------------------------------------------------------------

        private void RenderSidebar()
        {
            _sidebarPanel.Clear();

            var vBox = new VisualElement { style = { backgroundColor = new Color(0.1f, 0.1f, 0.15f), marginTop = 10, marginBottom = 10, marginLeft = 10, marginRight = 10, paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10, borderLeftWidth = 4, borderLeftColor = Color.yellow } };
            vBox.Add(new Label("CLASS DESIGNATION") { style = { color = Color.gray, fontSize = 9, marginBottom = 5 } });

            var nameInput = new TextField { value = _baseName, style = { backgroundColor = Color.black, color = Color.white, marginBottom = 5 } };
            nameInput.RegisterValueChangedCallback(e => { _baseName = e.newValue; UpdateStats(); });
            vBox.Add(nameInput);

            vBox.Add(new Label($"Mark (Version): {_version.ToRoman()}") { style = { color = Color.yellow, fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold } });
            _sidebarPanel.Add(vBox);

            _sidebarPanel.Add(new Label("ACTIVE DEPARTMENTS") { style = { color = Color.gray, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 10, marginLeft = 10, marginRight = 10 } });

            foreach (DesignTab tab in Enum.GetValues(typeof(DesignTab)))
            {
                if (_hiredFirms[tab])
                {
                    var btn = new Button(() => { _currentTab = tab; RenderConfigPanel(); RenderSidebar(); })
                    {
                        text = tab.ToString().ToUpper(),
                        style = {
                            height = 45, marginBottom = 2, unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 20,
                            backgroundColor = _currentTab == tab ? new Color(0, 0.4f, 0.6f) : Color.clear,
                            color = _currentTab == tab ? Color.white : Color.gray,
                            borderLeftWidth = _currentTab == tab ? 4 : 0, borderLeftColor = Color.cyan
                        }
                    };
                    _sidebarPanel.Add(btn);
                }
            }
        }

        private void RenderConfigPanel()
        {
            _configPanel.Clear();
            _configPanel.Add(new Label($"{_currentTab.ToString().ToUpper()} CONFIGURATION") { style = { color = Color.white, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } });

            if (_currentTab == DesignTab.Hull)
            {
                // 1. ROLE DROPDOWN
                var roleNames = HullRole.Database.Select(r => r.Name).ToList();
                int defaultIndex = _selectedRole != null ? roleNames.IndexOf(_selectedRole.Name) : 0;

                var roleDropdown = new DropdownField("Strategic Role:", roleNames, defaultIndex) { style = { marginBottom = 15 } };
                roleDropdown.RegisterValueChangedCallback(e =>
                {
                    _selectedRole = HullRole.Database.Find(r => r.Name == e.newValue);
                    UpdateStats();
                });
                _configPanel.Add(roleDropdown);

                // 2. MAGNITUDE SLIDER
                _configPanel.Add(new Label("Hull Magnitude defines the exponential physical scale of the construct.") { style = { color = Color.gray, marginBottom = 5 } });

                var magSlider = new Slider("Magnitude (1.0 - 15.0)", 1f, 15f) { value = _magnitude, showInputField = true, style = { marginBottom = 5 } };

                // 3. SCALE COMPARISON READOUT
                var scaleLabel = new Label(GetScaleComparison(_magnitude)) { style = { color = new Color(0.85f, 0.4f, 0.1f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } };

                magSlider.RegisterValueChangedCallback(e =>
                {
                    _magnitude = e.newValue;
                    scaleLabel.text = GetScaleComparison(_magnitude);
                    UpdateScaleCalculations();
                    RenderBlueprintCanvas(); // Redraw canvas to update the scale legend
                });

                _configPanel.Add(magSlider);
                _configPanel.Add(scaleLabel);

                _configPanel.Add(new Label("ALLOY COMPOSITION:") { style = { color = Color.cyan, marginTop = 15, marginBottom = 5 } });
                _configPanel.Add(new DropdownField(new List<string> { "Standard Titanium", "Tritanium Armor", "Neutronium Plate" }, "Standard Titanium") { style = { width = 300 } });
            }
            else if (_currentTab == DesignTab.Systems)
            {
                _configPanel.Add(new Label("Hire specialized design firms to unlock advanced subsystems for this chassis.") { style = { color = Color.gray, marginBottom = 15 } });

                var grid = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };

                foreach (var firm in _firmCosts)
                {
                    bool isHired = _hiredFirms[firm.Key];
                    var card = new VisualElement { style = { width = 200, backgroundColor = new Color(0.1f, 0.1f, 0.15f), marginTop = 5, marginBottom = 5, marginLeft = 5, marginRight = 5, paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10, borderLeftWidth = 4, borderLeftColor = isHired ? Color.green : Color.gray } };

                    card.Add(new Label($"{firm.Key} Systems") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });
                    card.Add(new Label(isHired ? "FIRM CONTRACTED" : $"Contract: {firm.Value:N0} cr") { style = { color = isHired ? Color.green : Color.yellow, fontSize = 10, marginBottom = 10 } });

                    var btn = new Button(() => { _hiredFirms[firm.Key] = true; RecalculateStats(); RenderSidebar(); RenderConfigPanel(); }) { text = isHired ? "HIRED" : "HIRE FIRM", style = { height = 30, backgroundColor = isHired ? new Color(0, 0.3f, 0) : new Color(0.2f, 0.2f, 0.3f) } };
                    btn.SetEnabled(!isHired);
                    card.Add(btn);

                    grid.Add(card);
                }
                _configPanel.Add(grid);
            }
        }

        // Contextualizes the math so the designer understands what they are building
        private string GetScaleComparison(float mag)
        {
            if (mag < 2f) return "Scale Equivalent: Strike Craft (F-35 / X-Wing)";
            if (mag < 4f) return "Scale Equivalent: Escort Vessel (Submarine / Corvette)";
            if (mag < 6f) return "Scale Equivalent: Capital Ship (Aircraft Carrier / Star Destroyer)";
            if (mag < 8f) return "Scale Equivalent: Orbital Megastructure (O'Neill Cylinder / Citadel)";
            if (mag < 10f) return "Scale Equivalent: Planetoid (Death Star / Small Moon)";
            if (mag < 13f) return "Scale Equivalent: Planetary Body (Earth / Jupiter)";
            return "Scale Equivalent: Stellar Megastructure (Unicron / Dyson Sphere)";
        }

        private void UpdateScaleCalculations()
        {
            // Mathematical derivations of scale
            _maxDecks = Mathf.Max(1, Mathf.FloorToInt(Mathf.Pow(1.8f, _magnitude - 1)));
            _maxCapacity = Mathf.Pow(10, _magnitude) * 10f; // Tonnage

            // This is the magical part: As magnitude goes up, the hexes on the drawing canvas represent exponentially more space!
            _metersPerHex = Mathf.Pow(10, _magnitude / 2.5f);

            _drawnVertices.Clear();
            _isHullClosed = false;
            _currentDeck = 1;

            RecalculateStats();
        }

        // --------------------------------------------------------------------------------
        // HEX BLUEPRINT CANVAS (INTERACTIVE DRAWING)
        // --------------------------------------------------------------------------------

        private void GenerateHexSnapPoints(float width, float height, float radius)
        {
            _snapPoints.Clear();
            float hexWidth = Mathf.Sqrt(3) * radius;
            float hexHeight = 2 * radius;
            float horizSpacing = hexWidth;
            float vertSpacing = 0.75f * hexHeight;

            int cols = Mathf.CeilToInt(width / horizSpacing) + 2;
            int rows = Mathf.CeilToInt(height / vertSpacing) + 2;

            float xOffset = (width / 2f) - ((cols * horizSpacing) / 2f);
            float yOffset = (height / 2f) - ((rows * vertSpacing) / 2f);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    float x = c * horizSpacing + ((r % 2 == 1) ? hexWidth / 2f : 0) + xOffset;
                    float y = r * vertSpacing + yOffset;

                    Vector2 center = new Vector2(x, y);
                    _snapPoints.Add(center);

                    for (int i = 0; i < 6; i++)
                    {
                        float angleDeg = 30 + 60 * i;
                        float angleRad = angleDeg * Mathf.Deg2Rad;
                        Vector2 vertex = new Vector2(center.x + radius * Mathf.Cos(angleRad), center.y + radius * Mathf.Sin(angleRad));

                        if (!_snapPoints.Any(p => Vector2.Distance(p, vertex) < 1f))
                            _snapPoints.Add(vertex);
                    }
                }
            }
        }

        private void RenderBlueprintCanvas()
        {
            _blueprintPanel.Clear();

            // 1. Controls Overlay
            var overlay = new VisualElement { style = { flexDirection = FlexDirection.Row, backgroundColor = new Color(0, 0.2f, 0, 0.5f), paddingTop = 5, paddingBottom = 5, paddingLeft = 10, paddingRight = 10, alignItems = Align.Center } };

            var mirrorToggle = new Toggle("MIRROR SYMMETRY") { value = _mirrorSymmetry, style = { color = Color.cyan } };
            mirrorToggle.RegisterValueChangedCallback(e => _mirrorSymmetry = e.newValue);
            overlay.Add(mirrorToggle);

            // Dynamic Scale Legend!
            string scaleText = _metersPerHex > 1000 ? $"{_metersPerHex / 1000f:F1} km" : $"{_metersPerHex:F1} m";
            overlay.Add(new Label($"|  SCALE: 1 HEX = {scaleText}") { style = { color = Color.yellow, fontSize = 10, marginLeft = 15, flexGrow = 1 } });

            overlay.Add(new Button(() => { if (_currentDeck > 1) { _currentDeck--; RenderBlueprintCanvas(); } }) { text = "-", style = { width = 30 } });
            overlay.Add(new Label($"DECK {_currentDeck} / {_maxDecks}") { style = { color = Color.white, width = 80, unityTextAlign = TextAnchor.MiddleCenter } });
            overlay.Add(new Button(() => { if (_currentDeck < _maxDecks) { _currentDeck++; RenderBlueprintCanvas(); } }) { text = "+", style = { width = 30 } });

            var clearBtn = new Button(() => { _drawnVertices.Clear(); _isHullClosed = false; RecalculateStats(); RenderBlueprintCanvas(); }) { text = "CLEAR DECK", style = { backgroundColor = new Color(0.4f, 0.1f, 0.1f), marginLeft = 10 } };
            overlay.Add(clearBtn);

            _blueprintPanel.Add(overlay);

            // 2. The Drawing Area
            var drawingArea = new VisualElement { style = { flexGrow = 1, overflow = Overflow.Hidden, backgroundColor = new Color(0, 0.05f, 0.1f) } };

            float originX = CANVAS_WIDTH / 2f;
            float originY = CANVAS_HEIGHT / 2f;

            drawingArea.Add(new VisualElement { style = { position = Position.Absolute, left = 0, top = originY, width = CANVAS_WIDTH, height = 1, backgroundColor = new Color(1, 1, 1, 0.2f) } });
            drawingArea.Add(new VisualElement { style = { position = Position.Absolute, left = originX, top = 0, width = 1, height = CANVAS_HEIGHT, backgroundColor = new Color(1, 1, 1, 0.2f) } });

            foreach (var point in _snapPoints)
            {
                drawingArea.Add(new VisualElement { style = { position = Position.Absolute, left = point.x, top = point.y, width = 2, height = 2, backgroundColor = new Color(0, 1, 1, 0.15f), translate = new StyleTranslate(new Translate(Length.Percent(-50), Length.Percent(-50))) } });
            }

            Color lineColor = _isHullClosed ? Color.green : Color.cyan;
            for (int i = 0; i < _drawnVertices.Count; i++)
            {
                if (i > 0) drawingArea.Add(CreateLineElement(_drawnVertices[i - 1], _drawnVertices[i], lineColor, 2f));

                if (_mirrorSymmetry && i > 0)
                {
                    Vector2 startMirror = new Vector2(CANVAS_WIDTH - _drawnVertices[i - 1].x, _drawnVertices[i - 1].y);
                    Vector2 endMirror = new Vector2(CANVAS_WIDTH - _drawnVertices[i].x, _drawnVertices[i].y);
                    drawingArea.Add(CreateLineElement(startMirror, endMirror, lineColor, 2f));
                }
            }

            if (_isHullClosed && _drawnVertices.Count > 0)
            {
                Vector2 first = _drawnVertices[0];
                Vector2 last = _drawnVertices.Last();
                drawingArea.Add(CreateLineElement(last, first, lineColor, 2f));

                if (_mirrorSymmetry)
                {
                    Vector2 startMirror = new Vector2(CANVAS_WIDTH - last.x, last.y);
                    Vector2 endMirror = new Vector2(CANVAS_WIDTH - first.x, first.y);
                    drawingArea.Add(CreateLineElement(startMirror, endMirror, lineColor, 2f));
                }
            }

            drawingArea.RegisterCallback<PointerDownEvent>(e =>
            {
                if (_isHullClosed) { _drawnVertices.Clear(); _isHullClosed = false; }
                Vector2 localPos = e.localPosition;
                Vector2 nearest = _snapPoints.OrderBy(p => Vector2.Distance(p, localPos)).First();

                if (_drawnVertices.Count == 0) { _drawnVertices.Add(nearest); }
                else
                {
                    bool clickedStart = Vector2.Distance(nearest, _drawnVertices[0]) < 15f;
                    bool startIsCenter = Mathf.Abs(_drawnVertices[0].x - originX) < 5f;
                    bool currentIsCenter = Mathf.Abs(nearest.x - originX) < 5f;
                    bool symmetryClosure = _mirrorSymmetry && startIsCenter && currentIsCenter && !clickedStart;

                    if (clickedStart || symmetryClosure)
                    {
                        if (symmetryClosure) _drawnVertices.Add(nearest);
                        _isHullClosed = true;
                    }
                    else { _drawnVertices.Add(nearest); }
                }
                RecalculateStats();
                RenderBlueprintCanvas();
            });

            _blueprintPanel.Add(drawingArea);
        }

        private VisualElement CreateLineElement(Vector2 start, Vector2 end, Color color, float thickness)
        {
            float length = Vector2.Distance(start, end);
            float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;

            var line = new VisualElement { style = { position = Position.Absolute, left = start.x, top = start.y - (thickness / 2f), width = length, height = thickness, backgroundColor = color, transformOrigin = new TransformOrigin(0, Length.Percent(50)) } };
            line.transform.rotation = Quaternion.Euler(0, 0, angle);
            return line;
        }

        // --------------------------------------------------------------------------------
        // LOGIC & STATS
        // --------------------------------------------------------------------------------

        private void RecalculateStats()
        {
            _usedCapacity = _drawnVertices.Count * (_magnitude * 15f);
            if (_mirrorSymmetry) _usedCapacity *= 1.8f;

            _cost = (long)(Mathf.Pow(10, _magnitude) * 500);
            foreach (var firm in _firmCosts) { if (_hiredFirms[firm.Key]) _cost += firm.Value; }

            UpdateStats();
        }

        private void UpdateStats()
        {
            if (_statsLabel == null) return;

            bool isOverTolerances = _usedCapacity > _maxCapacity;
            string capColor = isOverTolerances ? "<color=#ff0000>" : "<color=#ffffff>";

            float minX = 400f, maxX = 0f, minY = 400f, maxY = 0f;
            foreach (var v in _drawnVertices)
            {
                if (v.x < minX) minX = v.x; if (v.x > maxX) maxX = v.x;
                if (v.y < minY) minY = v.y; if (v.y > maxY) maxY = v.y;
            }
            if (_mirrorSymmetry && _drawnVertices.Count > 0)
            {
                float mMinX = 400f - maxX; float mMaxX = 400f - minX;
                if (mMinX < minX) minX = mMinX; if (mMaxX > maxX) maxX = mMaxX;
            }

            float physicalLength = ((maxY - minY) / 40f) * _metersPerHex;
            float physicalWidth = ((maxX - minX) / 40f) * _metersPerHex;

            string lengthStr = physicalLength > 1000 ? $"{physicalLength / 1000f:F2}km" : $"{physicalLength:F1}m";
            string widthStr = physicalWidth > 1000 ? $"{physicalWidth / 1000f:F2}km" : $"{physicalWidth:F1}m";
            string roleStr = _selectedRole != null ? _selectedRole.Name : "Unassigned";

            _statsLabel.text =
                $"<b>Class:</b> {_baseName} Mk {_version.ToRoman()}\n" +
                $"<b>Role:</b> {roleStr}\n" +
                $"<b>Magnitude:</b> {_magnitude:F1}\n" +
                $"<b>Physical Bounds:</b> {widthStr} x {lengthStr}\n" +
                $"<b>Est. Cost:</b> {_cost:N0} cr\n\n" +
                $"<b>Structural Vol:</b> {capColor}{_usedCapacity:N0} / {_maxCapacity:N0} t</color>\n";

            if (isOverTolerances)
            {
                _statsLabel.text += "\n<color=#ff0000><b>CRITICAL WARNING:</b></color>\nDesign exceeds safe structural tolerances. Vessel will suffer a severe risk of catastrophic hull failure.";
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}

#endif