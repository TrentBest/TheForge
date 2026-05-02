using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Empire
{
    public class Empire_Playable_Alpha : IGuiProvider
    {
        public string Title => "SINGULARITY EMPIRE";

        // --- Game Enums & Structs ---
        public enum Terrain { Sea, Land }
        public enum Owner { Neutral = 0, Player = 1, Enemy = 2 }

        public enum UnitType { Army, Fighter, Transport, Destroyer, Submarine, Cruiser, Carrier, Battleship }

        public class UnitDef
        {
            public string Name;
            public string Symbol;
            public int Cost;
            public int MaxMoves;
            public int MaxFuel; // Only used for aircraft
            public bool IsAir;
            public bool IsSea;
        }

        public class City
        {
            public Owner Faction;
            public UnitType ProducingType = UnitType.Army;
            public int ProductionTimer;
            public string Name;
        }

        public class Unit
        {
            public Owner Faction;
            public UnitType Type;
            public int X, Y;
            public int MovesLeft;
            public int Fuel;
        }

        // --- Core Definitions ---
        private readonly Dictionary<UnitType, UnitDef> _unitDictionary = new Dictionary<UnitType, UnitDef>
        {
            { UnitType.Army,       new UnitDef { Name = "Army",       Symbol = "A", Cost = 5,  MaxMoves = 1, MaxFuel = -1, IsAir = false, IsSea = false } },
            { UnitType.Fighter,    new UnitDef { Name = "Fighter",    Symbol = "F", Cost = 10, MaxMoves = 4, MaxFuel = 20, IsAir = true,  IsSea = false } },
            { UnitType.Transport,  new UnitDef { Name = "Transport",  Symbol = "T", Cost = 30, MaxMoves = 2, MaxFuel = -1, IsAir = false, IsSea = true  } },
            { UnitType.Destroyer,  new UnitDef { Name = "Destroyer",  Symbol = "D", Cost = 20, MaxMoves = 3, MaxFuel = -1, IsAir = false, IsSea = true  } },
            { UnitType.Submarine,  new UnitDef { Name = "Submarine",  Symbol = "S", Cost = 25, MaxMoves = 2, MaxFuel = -1, IsAir = false, IsSea = true  } },
            { UnitType.Cruiser,    new UnitDef { Name = "Cruiser",    Symbol = "R", Cost = 50, MaxMoves = 2, MaxFuel = -1, IsAir = false, IsSea = true  } },
            { UnitType.Carrier,    new UnitDef { Name = "Carrier",    Symbol = "C", Cost = 60, MaxMoves = 2, MaxFuel = -1, IsAir = false, IsSea = true  } },
            { UnitType.Battleship, new UnitDef { Name = "Battleship", Symbol = "B", Cost = 75, MaxMoves = 2, MaxFuel = -1, IsAir = false, IsSea = true  } }
        };

        // --- Game State ---
        private int _mapWidth = 36;
        private int _mapHeight = 24;
        private const int TILE_SIZE = 32;

        private Terrain[,] _terrainMap;
        private City[,] _cityMap;
        private bool[,] _fogOfWar; // True = Explored
        private List<Unit> _units;

        private int _turnCounter = 1;
        private Vector2Int _selectedTile = new Vector2Int(-1, -1);

        // --- UI State ---
        private VisualElement _root;
        private VisualElement _mapContainer;
        private VisualElement[,] _tileElements;
        private Label[,] _tileLabels;

        // Data Card UI
        private Label _cardTitle;
        private Label _cardIcon;
        private Label _cardDetails;
        private VisualElement _cardActionContainer;
        private Label _headerLabel;
        private Label _infoLabel;

        public VisualElement CreateGui(GuiContext ctx)
        {
            InitializeGameState();

            // 1. ROOT CONTAINER
            _root = new GraphicalUserInterfaceBuilder("EmpireRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .Build();

            // ==========================================
            // LEFT PANEL: MAP & HEADER
            // ==========================================
            var leftPanel = new GraphicalUserInterfaceBuilder("LeftPanel")
                .WithFlexGrow(3)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .Build();

            var header = new GraphicalUserInterfaceBuilder("Header")
                .OnBuild(ve => ve.style.width = Length.Percent(100))
                .WithHeight(60)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f))
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithPadding(10)
                .WithBorderBottomColor(Color.cyan)
                .WithBorderBottomWidth(2)
                .Build();

            _headerLabel = new Label($"TURN: {_turnCounter} | EXPAND YOUR EMPIRE");
            _headerLabel.style.color = Color.white;
            _headerLabel.style.fontSize = 20;
            _headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(_headerLabel);

            var endTurnBtn = new ForgeButtonBuilder("END TURN")
                .WithBackgroundColor(new Color(0.8f, 0.1f, 0.2f))
                .WithTextColor(Color.white)
                .WithWidth(150)
                .WithHeight(40)
                .WithFontStyle(FontStyle.Bold)
                .OnClick(EndTurn)
                .CreateGui(new GuiContext());
            header.Add(endTurnBtn);
            leftPanel.Add(header);

            // Map Scroll View
            var scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scrollView.style.flexGrow = 1;

            _mapContainer = new GraphicalUserInterfaceBuilder("MapContainer")
                .WithBackgroundColor(Color.black)
                .OnBuild(ve => {
                    ve.style.position = Position.Relative;
                    ve.style.width = _mapWidth * TILE_SIZE;
                    ve.style.height = _mapHeight * TILE_SIZE;
                })
                .Build();

            GenerateMapUI(_mapContainer);
            scrollView.Add(_mapContainer);
            leftPanel.Add(scrollView);

            _infoLabel = new Label("SYSTEM ONLINE.");
            _infoLabel.style.color = Color.cyan;
            _infoLabel.style.paddingLeft = 10;
            _infoLabel.style.paddingBottom = 5;
            leftPanel.Add(_infoLabel);

            _root.Add(leftPanel);

            // ==========================================
            // RIGHT PANEL: DATA CARD & PRODUCTION
            // ==========================================
            var rightPanel = new GraphicalUserInterfaceBuilder("DataCardPanel")
                .OnBuild(ve => ve.style.width = 300)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderLeftColor(Color.gray)
                .WithBorderLeftWidth(2)
                .WithPadding(15)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .Build();

            _cardTitle = new Label("AWAITING INTEL...");
            _cardTitle.style.fontSize = 22;
            _cardTitle.style.color = Color.cyan;
            _cardTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            _cardTitle.style.unityTextAlign = TextAnchor.MiddleCenter;
            rightPanel.Add(_cardTitle);

            var previewBox = new GraphicalUserInterfaceBuilder("PreviewRender")
                .WithHeight(150)
                .WithMarginTop(15)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.02f))
                .WithBorderColor(new Color(0.3f, 0.3f, 0.3f))
                .WithBorderWidth(1)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .Build();

            _cardIcon = new Label("?");
            _cardIcon.style.fontSize = 70;
            _cardIcon.style.color = new Color(0.2f, 0.2f, 0.2f);
            previewBox.Add(_cardIcon);
            rightPanel.Add(previewBox);

            _cardDetails = new Label("\nSelect a unit or city.");
            _cardDetails.style.fontSize = 14;
            _cardDetails.style.color = new Color(0.8f, 0.8f, 0.8f);
            _cardDetails.style.whiteSpace = WhiteSpace.Normal;
            _cardDetails.style.marginTop = 15;
            rightPanel.Add(_cardDetails);

            _cardActionContainer = new GraphicalUserInterfaceBuilder("ActionContainer")
                .WithMarginTop(20)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .Build();
            rightPanel.Add(_cardActionContainer);

            _root.Add(rightPanel);

            // Initial Render
            UpdateFogOfWar();
            UpdateMapVisuals();
            UpdateDataCard(-1, -1);

            return _root;
        }

        // ==============================================================================
        // GAME LOGIC & INITIALIZATION
        // ==============================================================================

        private void InitializeGameState()
        {
            _terrainMap = new Terrain[_mapWidth, _mapHeight];
            _cityMap = new City[_mapWidth, _mapHeight];
            _fogOfWar = new bool[_mapWidth, _mapHeight];
            _units = new List<Unit>();

            // Generate procedural islands
            float offsetX = UnityEngine.Random.Range(0f, 100f);
            float offsetY = UnityEngine.Random.Range(0f, 100f);
            float scale = 0.15f;

            for (int x = 0; x < _mapWidth; x++)
            {
                for (int y = 0; y < _mapHeight; y++)
                {
                    float noise = Mathf.PerlinNoise(x * scale + offsetX, y * scale + offsetY);
                    _terrainMap[x, y] = noise > 0.40f ? Terrain.Land : Terrain.Sea;
                    _fogOfWar[x, y] = false; // Everything starts hidden
                }
            }

            // Scatter Neutral Cities
            int numCities = (_mapWidth * _mapHeight) / 40;
            for (int i = 0; i < numCities; i++)
            {
                int cx = UnityEngine.Random.Range(1, _mapWidth - 1);
                int cy = UnityEngine.Random.Range(1, _mapHeight - 1);
                if (_terrainMap[cx, cy] == Terrain.Land && _cityMap[cx, cy] == null)
                {
                    _cityMap[cx, cy] = new City { Faction = Owner.Neutral, ProductionTimer = 5, Name = $"Sector {cx}-{cy}" };
                }
            }

            // Player Start
            SpawnCapital(2, 2, Owner.Player);

            // Enemy Start
            SpawnCapital(_mapWidth - 3, _mapHeight - 3, Owner.Enemy);
        }

        private void SpawnCapital(int x, int y, Owner owner)
        {
            _terrainMap[x, y] = Terrain.Land;
            _cityMap[x, y] = new City { Faction = owner, ProductionTimer = _unitDictionary[UnitType.Army].Cost, Name = owner == Owner.Player ? "Home Command" : "Enemy Prime" };

            var startingUnit = new Unit { Faction = owner, Type = UnitType.Army, X = x, Y = y, MovesLeft = _unitDictionary[UnitType.Army].MaxMoves, Fuel = _unitDictionary[UnitType.Army].MaxFuel };
            _units.Add(startingUnit);
        }

        private void UpdateFogOfWar()
        {
            // Only player units and cities reveal fog
            int visionRange = 2; // Basic vision

            foreach (var unit in _units.Where(u => u.Faction == Owner.Player))
            {
                RevealArea(unit.X, unit.Y, visionRange);
            }

            for (int x = 0; x < _mapWidth; x++)
            {
                for (int y = 0; y < _mapHeight; y++)
                {
                    if (_cityMap[x, y] != null && _cityMap[x, y].Faction == Owner.Player)
                    {
                        RevealArea(x, y, visionRange);
                    }
                }
            }
        }

        private void RevealArea(int cx, int cy, int range)
        {
            for (int x = cx - range; x <= cx + range; x++)
            {
                for (int y = cy - range; y <= cy + range; y++)
                {
                    if (x >= 0 && x < _mapWidth && y >= 0 && y < _mapHeight)
                    {
                        _fogOfWar[x, y] = true;
                    }
                }
            }
        }

        // ==============================================================================
        // UI RENDERING
        // ==============================================================================

        private void GenerateMapUI(VisualElement container)
        {
            _tileElements = new VisualElement[_mapWidth, _mapHeight];
            _tileLabels = new Label[_mapWidth, _mapHeight];

            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    int captureX = x;
                    int captureY = y;

                    var tile = new VisualElement();
                    tile.style.position = Position.Absolute;
                    tile.style.left = x * TILE_SIZE;
                    tile.style.top = y * TILE_SIZE;
                    tile.style.width = TILE_SIZE;
                    tile.style.height = TILE_SIZE;
                    tile.style.justifyContent = Justify.Center;
                    tile.style.alignItems = Align.Center;

                    tile.RegisterCallback<PointerDownEvent>(e => OnTileClicked(captureX, captureY));

                    var label = new Label("");
                    label.style.fontSize = 20;
                    label.style.unityFontStyleAndWeight = FontStyle.Bold;
                    tile.Add(label);

                    _tileElements[x, y] = tile;
                    _tileLabels[x, y] = label;
                    container.Add(tile);
                }
            }
        }

        private void UpdateMapVisuals()
        {
            for (int x = 0; x < _mapWidth; x++)
            {
                for (int y = 0; y < _mapHeight; y++)
                {
                    var tile = _tileElements[x, y];
                    var label = _tileLabels[x, y];

                    // Fog of War
                    if (!_fogOfWar[x, y])
                    {
                        tile.style.backgroundColor = Color.black;
                        tile.style.borderTopWidth = 0;
                        tile.style.borderLeftWidth = 0;
                        label.text = "";
                        continue;
                    }

                    // Terrain
                    Color bgColor = _terrainMap[x, y] == Terrain.Sea ? new Color(0.1f, 0.15f, 0.4f) : new Color(0.1f, 0.35f, 0.15f);
                    tile.style.backgroundColor = bgColor;

                    tile.style.borderTopColor = new Color(0, 0, 0, 0.3f);
                    tile.style.borderLeftColor = new Color(0, 0, 0, 0.3f);
                    tile.style.borderRightColor = Color.clear;
                    tile.style.borderBottomColor = Color.clear;
                    tile.style.borderTopWidth = 1;
                    tile.style.borderLeftWidth = 1;
                    tile.style.borderRightWidth = 0;
                    tile.style.borderBottomWidth = 0;

                    label.text = "";

                    // Cities
                    if (_cityMap[x, y] != null)
                    {
                        label.text = "♜";
                        label.style.color = GetFactionColor(_cityMap[x, y].Faction);
                    }

                    // Units (Overlaps City)
                    var unitOnTile = _units.FirstOrDefault(u => u.X == x && u.Y == y);
                    if (unitOnTile != null)
                    {
                        var def = _unitDictionary[unitOnTile.Type];
                        label.text = def.Symbol;
                        Color uColor = GetFactionColor(unitOnTile.Faction);
                        if (unitOnTile.MovesLeft == 0) uColor.a = 0.4f; // Dim if exhausted
                        label.style.color = uColor;
                    }

                    // Selection
                    if (_selectedTile.x == x && _selectedTile.y == y)
                    {
                        tile.style.borderTopColor = Color.yellow;
                        tile.style.borderRightColor = Color.yellow;
                        tile.style.borderBottomColor = Color.yellow;
                        tile.style.borderLeftColor = Color.yellow;
                        tile.style.borderTopWidth = 2;
                        tile.style.borderRightWidth = 2;
                        tile.style.borderBottomWidth = 2;
                        tile.style.borderLeftWidth = 2;
                    }
                }
            }
        }

        private void UpdateDataCard(int x, int y)
        {
            _cardActionContainer.Clear(); // Clear old production buttons

            if (x == -1 || y == -1 || !_fogOfWar[x, y])
            {
                _cardTitle.text = "AWAITING INTEL";
                _cardTitle.style.color = Color.cyan;
                _cardIcon.text = "◎";
                _cardIcon.style.color = new Color(0.2f, 0.2f, 0.2f);
                _cardDetails.text = "\nSelect a sector to retrieve telemetry.";
                return;
            }

            var terrain = _terrainMap[x, y];
            var city = _cityMap[x, y];
            var unit = _units.FirstOrDefault(u => u.X == x && u.Y == y);

            if (unit != null)
            {
                var def = _unitDictionary[unit.Type];
                _cardTitle.text = def.Name.ToUpper();
                _cardTitle.style.color = GetFactionColor(unit.Faction);
                _cardIcon.text = def.Symbol;
                _cardIcon.style.color = GetFactionColor(unit.Faction);

                string fuelStr = def.IsAir ? $"\nFUEL: {unit.Fuel} / {def.MaxFuel}" : "";

                _cardDetails.text = $"FACTION: {unit.Faction}\n" +
                                    $"MOVES: {unit.MovesLeft} / {def.MaxMoves}" + fuelStr + "\n" +
                                    $"POSITION: [{x}, {y}]\n";
            }
            else if (city != null)
            {
                _cardTitle.text = city.Name.ToUpper();
                _cardTitle.style.color = GetFactionColor(city.Faction);
                _cardIcon.text = "♜";
                _cardIcon.style.color = GetFactionColor(city.Faction);

                _cardDetails.text = $"FACTION: {city.Faction}\n" +
                                    $"PRODUCING: {_unitDictionary[city.ProducingType].Name}\n" +
                                    $"TIME REMAINING: {city.ProductionTimer} Turns\n" +
                                    $"POSITION: [{x}, {y}]";

                // If player owns city, show production buttons
                if (city.Faction == Owner.Player)
                {
                    _cardActionContainer.Add(new Label("CHANGE PRODUCTION:") { style = { color = Color.white, marginTop = 10, marginBottom = 5 } });

                    foreach (var kvp in _unitDictionary)
                    {
                        var btn = new ForgeButtonBuilder($"Build {kvp.Value.Name} ({kvp.Value.Cost}T)")
                            .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f))
                            .WithMarginBottom(2)
                            .OnClick(() => {
                                city.ProducingType = kvp.Key;
                                city.ProductionTimer = kvp.Value.Cost;
                                UpdateDataCard(x, y);
                                _infoLabel.text = $"City production switched to {kvp.Value.Name}.";
                            })
                            .CreateGui(new GuiContext());
                        _cardActionContainer.Add(btn);
                    }
                }
            }
            else
            {
                _cardTitle.text = "TERRAIN DATA";
                _cardTitle.style.color = Color.gray;
                _cardIcon.text = terrain == Terrain.Sea ? "≈" : "■";
                _cardIcon.style.color = terrain == Terrain.Sea ? new Color(0.2f, 0.3f, 0.6f) : Color.gray;
                _cardDetails.text = $"BIOME: {terrain}\nPOSITION: [{x}, {y}]";
            }
        }

        private Color GetFactionColor(Owner owner)
        {
            if (owner == Owner.Player) return Color.cyan;
            if (owner == Owner.Enemy) return new Color(1f, 0.3f, 0.3f);
            return Color.gray;
        }

        // ==============================================================================
        // INTERACTION & MOVEMENT
        // ==============================================================================

        private void OnTileClicked(int x, int y)
        {
            if (_selectedTile.x != -1)
            {
                var selectedUnit = _units.FirstOrDefault(u => u.X == _selectedTile.x && u.Y == _selectedTile.y);

                if (selectedUnit != null && selectedUnit.Faction == Owner.Player && selectedUnit.MovesLeft > 0)
                {
                    int dx = Mathf.Abs(x - selectedUnit.X);
                    int dy = Mathf.Abs(y - selectedUnit.Y);

                    // Allow diagonal movement (dx<=1 && dy<=1)
                    if (dx <= 1 && dy <= 1 && (dx + dy > 0))
                    {
                        if (ProcessMovement(selectedUnit, x, y))
                        {
                            UpdateFogOfWar();

                            // If unit ran out of moves, deselect. Otherwise keep selected.
                            if (selectedUnit.MovesLeft == 0 || !_units.Contains(selectedUnit))
                                _selectedTile = new Vector2Int(-1, -1);
                            else
                                _selectedTile = new Vector2Int(x, y);

                            UpdateMapVisuals();
                            UpdateDataCard(_selectedTile.x, _selectedTile.y);
                            return;
                        }
                    }
                }
            }

            _selectedTile = new Vector2Int(x, y);
            UpdateMapVisuals();
            UpdateDataCard(x, y);
        }

        private bool ProcessMovement(Unit unit, int targetX, int targetY)
        {
            var unitDef = _unitDictionary[unit.Type];
            var targetTerrain = _terrainMap[targetX, targetY];

            // Terrain validation
            if (!unitDef.IsAir)
            {
                if (unitDef.IsSea && targetTerrain == Terrain.Land)
                {
                    // Ships can visit friendly cities for repair/docking, but not traverse raw land
                    if (_cityMap[targetX, targetY] == null || _cityMap[targetX, targetY].Faction != unit.Faction)
                    {
                        _infoLabel.text = "Naval units cannot move on land.";
                        return false;
                    }
                }
                else if (!unitDef.IsSea && targetTerrain == Terrain.Sea)
                {
                    // Note: True Empire allows boarding Transports here. Simplified for Alpha.
                    _infoLabel.text = "Land units cannot enter the sea.";
                    return false;
                }
            }

            // Combat Check
            var targetUnit = _units.FirstOrDefault(u => u.X == targetX && u.Y == targetY);
            if (targetUnit != null)
            {
                if (targetUnit.Faction != unit.Faction)
                {
                    // Mutual destruction
                    _units.Remove(targetUnit);
                    _units.Remove(unit);
                    _infoLabel.text = "COMBAT RESOLVED: MUTUAL DESTRUCTION.";
                    return true;
                }
                return false; // Can't move onto friendly unit
            }

            // Move
            unit.X = targetX;
            unit.Y = targetY;
            unit.MovesLeft--;

            if (unitDef.IsAir)
            {
                unit.Fuel--;
                _infoLabel.text = $"AIRCRAFT MOVED. FUEL REMAINING: {unit.Fuel}";
            }
            else
            {
                _infoLabel.text = "UNIT MOVED.";
            }

            // City Capture
            if (!unitDef.IsAir && !unitDef.IsSea) // Only Armies capture
            {
                if (_cityMap[targetX, targetY] != null && _cityMap[targetX, targetY].Faction != unit.Faction)
                {
                    _cityMap[targetX, targetY].Faction = unit.Faction;
                    _infoLabel.text = $"CITY CAPTURED BY {unit.Faction.ToString().ToUpper()}!";
                }
            }

            return true;
        }

        // ==============================================================================
        // TURN MANAGEMENT
        // ==============================================================================

        private void EndTurn()
        {
            _selectedTile = new Vector2Int(-1, -1);

            ProcessAircraftCrash(); // Check fuel before enemy turn
            ProcessEnemyTurn();
            ProcessProduction();

            // Reset Player Units
            foreach (var unit in _units.Where(u => u.Faction == Owner.Player))
            {
                unit.MovesLeft = _unitDictionary[unit.Type].MaxMoves;
            }

            _turnCounter++;
            _headerLabel.text = $"TURN: {_turnCounter} | EXPAND YOUR EMPIRE";
            _infoLabel.text = "NEW TURN COMMENCED.";

            UpdateFogOfWar();
            UpdateMapVisuals();
            UpdateDataCard(-1, -1);
        }

        private void ProcessAircraftCrash()
        {
            // If an aircraft ends its turn with 0 fuel and isn't on a friendly city (or carrier), it crashes.
            var crashed = new List<Unit>();
            foreach (var u in _units)
            {
                if (_unitDictionary[u.Type].IsAir && u.Fuel <= 0)
                {
                    bool safe = (_cityMap[u.X, u.Y] != null && _cityMap[u.X, u.Y].Faction == u.Faction);
                    if (!safe) crashed.Add(u);
                    else u.Fuel = _unitDictionary[u.Type].MaxFuel; // Refuel at city
                }
            }

            foreach (var c in crashed)
            {
                _units.Remove(c);
                if (c.Faction == Owner.Player) _infoLabel.text = "WARNING: Aircraft lost due to fuel exhaustion!";
            }
        }

        private void ProcessEnemyTurn()
        {
            var enemyUnits = _units.Where(u => u.Faction == Owner.Enemy).ToList();

            foreach (var unit in enemyUnits)
            {
                int moves = _unitDictionary[unit.Type].MaxMoves;
                while (moves > 0)
                {
                    var dirs = new List<Vector2Int> {
                        new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0)
                    }.OrderBy(m => Guid.NewGuid()).ToList();

                    bool moved = false;
                    foreach (var d in dirs)
                    {
                        int tx = unit.X + d.x;
                        int ty = unit.Y + d.y;
                        if (tx >= 0 && tx < _mapWidth && ty >= 0 && ty < _mapHeight)
                        {
                            // Hack: bypass standard ProcessMovement to avoid triggering player info logs on enemy turn
                            if (_terrainMap[tx, ty] != Terrain.Sea && !_units.Any(u => u.X == tx && u.Y == ty))
                            {
                                unit.X = tx; unit.Y = ty;
                                if (_cityMap[tx, ty] != null) _cityMap[tx, ty].Faction = Owner.Enemy;
                                moved = true;
                                break;
                            }
                        }
                    }
                    if (!moved) break;
                    moves--;
                }
                unit.MovesLeft = _unitDictionary[unit.Type].MaxMoves; // Refill for next turn conceptually
            }
        }

        private void ProcessProduction()
        {
            List<Unit> newSpawns = new List<Unit>();

            for (int x = 0; x < _mapWidth; x++)
            {
                for (int y = 0; y < _mapHeight; y++)
                {
                    var city = _cityMap[x, y];
                    if (city != null && city.Faction != Owner.Neutral)
                    {
                        city.ProductionTimer--;
                        if (city.ProductionTimer <= 0)
                        {
                            // Spawn if tile is empty
                            if (!_units.Any(u => u.X == x && u.Y == y))
                            {
                                newSpawns.Add(new Unit
                                {
                                    Faction = city.Faction,
                                    Type = city.ProducingType,
                                    X = x,
                                    Y = y,
                                    MovesLeft = 0, // Wait til next turn to move
                                    Fuel = _unitDictionary[city.ProducingType].MaxFuel
                                });

                                // Restart production
                                city.ProductionTimer = _unitDictionary[city.ProducingType].Cost;
                            }
                        }
                    }
                }
            }

            _units.AddRange(newSpawns);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}