using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Warlords
{
    public class Warlords_Gui_MapImageIngestor : IGuiProvider
    {
        public string Title => "CARTOGRAPHER: IMAGE INGESTOR";

        private Texture2D _sourceImage;
        private Texture2D _previewTexture;
        private Image _imageContainer;
        private VisualElement _colorListContainer;
        private VisualElement _mainRoot; // Captured to prevent the NRE!
        private GuiContext _lastCtx;

        private int _startX = 0, _startY = 0;
        private int _endX = 100, _endY = 100;
        private int _gridWidth = 100, _gridHeight = 100;
        private float _colorTolerance = 0.15f;

        // Mapping Data Arrays
        private Dictionary<Color, TerrainType> _colorMapping = new Dictionary<Color, TerrainType>();
        private Dictionary<Color, bool> _blendWithNeighbors = new Dictionary<Color, bool>();
        private Dictionary<Color, string> _poiMapping = new Dictionary<Color, string>();

        private List<Color> _uniqueColors = new List<Color>();
        private Color[] _extractedGridColors;
        private Color? _highlightedColor = null;

        private readonly string[] _poiTypes = new string[] { "None", "City", "Ruin", "Temple", "Armory", "Jail" };

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            // Instantly load and mathematically double the resolution
            Texture2D rawImage = UnityEngine.Resources.Load<Texture2D>(@"Images/Warlords/Illuria");
            if (rawImage != null) _sourceImage = DoubleTextureResolution(rawImage);

            if (_sourceImage != null)
            {
                _endX = _sourceImage.width - 1;
                _endY = _sourceImage.height - 1;
            }

            var splitPanel = new ForgeSplitPanelBuilder(sidebarWidth: 450); // Made slightly wider for new dropdowns

            splitPanel.WithSidebar(new GraphicalUserInterfaceBuilder("Ingestor_Sidebar")
                .WithPadding(15).WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)

                .AddChild(new Label("1. LIVE CROP & CHUNK BOUNDARIES") { style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } })
                .AddIntegerData("Start X (Left)", _startX, v => { _startX = Mathf.Max(0, v); UpdatePreview(); })
                .AddIntegerData("Start Y (Bottom)", _startY, v => { _startY = Mathf.Max(0, v); UpdatePreview(); })
                .AddIntegerData("End X (Right)", _endX, v => { _endX = v; UpdatePreview(); })
                .AddIntegerData("End Y (Top)", _endY, v => { _endY = v; UpdatePreview(); })
                .AddSeparator()
                .AddIntegerData("Target Grid Width", _gridWidth, v => _gridWidth = Mathf.Max(1, v))
                .AddIntegerData("Target Grid Height", _gridHeight, v => _gridHeight = Mathf.Max(1, v))

                .AddChild(new Label("Color Tolerance (Merges similar pixels):") { style = { color = Color.white, marginTop = 10 } })
                .OnBuild(ve => {
                    var slider = new Slider(0.01f, 0.5f) { value = _colorTolerance };
                    slider.RegisterValueChangedCallback(evt => _colorTolerance = evt.newValue);
                    ve.Add(slider);
                })
                .AddButton("2. EXTRACT DOMINANT CHUNKS", AnalyzeImageColors)
                .AddSeparator()

                .AddChild(new Label("3. DOMINANT PALETTE MAPPING") { style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } })
                .OnBuild(ve => {
                    var scrollView = new ScrollView { style = { flexGrow = 1, borderTopWidth = 1, borderTopColor = Color.gray, paddingTop = 10 } };
                    _colorListContainer = new VisualElement();
                    if (_extractedGridColors == null) _colorListContainer.Add(new Label("Click 'Extract' to analyze..") { style = { color = Color.gray } });
                    scrollView.Add(_colorListContainer);
                    ve.Add(scrollView);
                })

                .AddSeparator()
                .AddButton("4. GENERATE JSON MAP", GenerateMapJson)
            );

            splitPanel.WithMain(new GraphicalUserInterfaceBuilder("Ingestor_Main")
                .WithBackgroundColor(Color.black)
                .OnBuild(ve => {
                    ve.style.flexGrow = 1;
                    _imageContainer = new Image { scaleMode = ScaleMode.ScaleToFit, style = { flexGrow = 1 } };
                    ve.Add(_imageContainer);
                })
            );

            // Capture the absolute root to completely bypass the NRE bug!
            _mainRoot = splitPanel.CreateGui(ctx);
            UpdatePreview();
            return _mainRoot;
        }

        private Texture2D DoubleTextureResolution(Texture2D source)
        {
            Texture2D result = new Texture2D(source.width * 2, source.height * 2, source.format, false);
            result.filterMode = FilterMode.Point;
            for (int y = 0; y < result.height; y++)
                for (int x = 0; x < result.width; x++)
                    result.SetPixel(x, y, source.GetPixel(x / 2, y / 2));
            result.Apply();
            return result;
        }

        private void UpdatePreview()
        {
            if (_sourceImage == null) return;
            if (_previewTexture != null) UnityEngine.Object.DestroyImmediate(_previewTexture);

            _previewTexture = new Texture2D(_sourceImage.width, _sourceImage.height);
            _previewTexture.filterMode = FilterMode.Point;

            Color[] pixels = _sourceImage.GetPixels();
            int w = _sourceImage.width, h = _sourceImage.height;

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (x == _startX || x == _endX || y == _startY || y == _endY) pixels[y * w + x] = Color.red;

            if (_highlightedColor.HasValue && _extractedGridColors != null)
            {
                int cropW = _endX - _startX; int cropH = _endY - _startY;
                float chunkW = (float)cropW / _gridWidth; float chunkH = (float)cropH / _gridHeight;

                for (int gy = 0; gy < _gridHeight; gy++)
                {
                    for (int gx = 0; gx < _gridWidth; gx++)
                    {
                        if (_extractedGridColors[gy * _gridWidth + gx] == _highlightedColor.Value)
                        {
                            int pxStart = _startX + Mathf.FloorToInt(gx * chunkW);
                            int pxEnd = _startX + Mathf.FloorToInt((gx + 1) * chunkW) - 1;
                            int pyStart = _startY + Mathf.FloorToInt(gy * chunkH);
                            int pyEnd = _startY + Mathf.FloorToInt((gy + 1) * chunkH) - 1;

                            for (int y = pyStart; y <= pyEnd; y++)
                                for (int x = pxStart; x <= pxEnd; x++)
                                    if (x == pxStart || x == pxEnd || y == pyStart || y == pyEnd)
                                        pixels[y * w + x] = Color.magenta;
                        }
                    }
                }
            }
            _previewTexture.SetPixels(pixels); _previewTexture.Apply(); _imageContainer.image = _previewTexture;
        }

        private void AnalyzeImageColors()
        {
            if (_sourceImage == null) return;
            _highlightedColor = null; _uniqueColors.Clear(); _colorMapping.Clear(); _blendWithNeighbors.Clear(); _poiMapping.Clear();
            _extractedGridColors = new Color[_gridWidth * _gridHeight];

            int cropW = _endX - _startX; int cropH = _endY - _startY;
            float chunkW = (float)cropW / _gridWidth; float chunkH = (float)cropH / _gridHeight;

            for (int gy = 0; gy < _gridHeight; gy++)
            {
                for (int gx = 0; gx < _gridWidth; gx++)
                {
                    int pxStart = _startX + Mathf.FloorToInt(gx * chunkW); int pxEnd = _startX + Mathf.FloorToInt((gx + 1) * chunkW);
                    int pyStart = _startY + Mathf.FloorToInt(gy * chunkH); int pyEnd = _startY + Mathf.FloorToInt((gy + 1) * chunkH);

                    Dictionary<Color, int> chunkColors = new Dictionary<Color, int>();

                    for (int x = pxStart; x < pxEnd; x++)
                    {
                        for (int y = pyStart; y < pyEnd; y++)
                        {
                            Color raw = _sourceImage.GetPixel(x, y);
                            Color core = GetOrRegisterCoreColor(raw);
                            if (!chunkColors.ContainsKey(core)) chunkColors[core] = 0;
                            chunkColors[core]++;
                        }
                    }

                    if (chunkColors.Count > 0)
                    {
                        int maxCount = chunkColors.Values.Max();
                        var dominant = chunkColors.Where(kvp => kvp.Value == maxCount).Select(kvp => kvp.Key).ToList();
                        Color finalDominant = dominant[0];
                        if (dominant.Count > 1 && gx > 0)
                        {
                            Color left = _extractedGridColors[gy * _gridWidth + (gx - 1)];
                            if (dominant.Contains(left)) finalDominant = left;
                        }
                        _extractedGridColors[gy * _gridWidth + gx] = finalDominant;
                    }
                }
            }
            RefreshColorList(); UpdatePreview();
        }

        private Color GetOrRegisterCoreColor(Color rawColor)
        {
            foreach (var existingColor in _uniqueColors)
                if (Vector4.Distance(rawColor, existingColor) <= _colorTolerance) return existingColor;

            _uniqueColors.Add(rawColor);
            _colorMapping.Add(rawColor, TerrainType.Plains);
            _blendWithNeighbors.Add(rawColor, false);
            _poiMapping.Add(rawColor, "None");
            return rawColor;
        }

        private void RefreshColorList()
        {
            if (_colorListContainer == null) return;
            _colorListContainer.Clear();

            var sortedColors = _uniqueColors.OrderByDescending(c => _extractedGridColors.Count(ec => ec == c)).ToList();

            foreach (var color in sortedColors)
            {
                int tileCount = _extractedGridColors.Count(c => c == color);
                if (tileCount == 0) continue;

                var rowBox = new VisualElement { style = { flexDirection = FlexDirection.Column, marginBottom = 8, backgroundColor = new Color(0.15f, 0.15f, 0.18f), paddingTop = 8, paddingRight = 8, paddingLeft = 8, paddingBottom = 8,
                        borderTopLeftRadius = 4, borderBottomRightRadius = 4, borderTopRightRadius = 4, borderBottomLeftRadius =4, borderLeftWidth = 4, borderLeftColor = (_highlightedColor == color) ? Color.magenta : Color.clear } };

                rowBox.RegisterCallback<MouseEnterEvent>(e => rowBox.style.backgroundColor = new Color(0.2f, 0.2f, 0.25f));
                rowBox.RegisterCallback<MouseLeaveEvent>(e => rowBox.style.backgroundColor = new Color(0.15f, 0.15f, 0.18f));
                rowBox.RegisterCallback<ClickEvent>(e => { _highlightedColor = color; RefreshColorList(); UpdatePreview(); });

                // TOP ROW: Swatch, Count, Blend Toggle
                var topRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 5 } };
                topRow.Add(new VisualElement { style = { width = 30, height = 30, backgroundColor = color, marginRight = 10, borderTopColor = Color.white, borderBottomColor = Color.white, borderLeftColor = Color.white, borderRightColor = Color.white, borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1 } });
                topRow.Add(new Label($"({tileCount})") { style = { width = 45, color = Color.gray } });

                var blendToggle = new Toggle("Blend w/ Neighbors (Furls)") { value = _blendWithNeighbors[color] };
                blendToggle.RegisterValueChangedCallback(evt => _blendWithNeighbors[color] = evt.newValue);
                blendToggle.RegisterCallback<ClickEvent>(e => e.StopPropagation()); // Stop row click
                topRow.Add(blendToggle);
                rowBox.Add(topRow);

                // BOTTOM ROW: Terrain Dropdown, POI Dropdown
                var botRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };

                var terrainDrop = new DropdownField(Enum.GetNames(typeof(TerrainType)).ToList(), (int)_colorMapping[color]) { style = { flexGrow = 1, marginRight = 5 } };
                terrainDrop.RegisterValueChangedCallback(evt => _colorMapping[color] = (TerrainType)Enum.Parse(typeof(TerrainType), evt.newValue));
                terrainDrop.RegisterCallback<ClickEvent>(e => e.StopPropagation());
                botRow.Add(terrainDrop);

                var poiDrop = new DropdownField(_poiTypes.ToList(), Array.IndexOf(_poiTypes, _poiMapping[color])) { style = { width = 110 } };
                poiDrop.RegisterValueChangedCallback(evt => _poiMapping[color] = evt.newValue);
                poiDrop.RegisterCallback<ClickEvent>(e => e.StopPropagation());
                botRow.Add(poiDrop);

                rowBox.Add(botRow);
                _colorListContainer.Add(rowBox);
            }
        }

        private void GenerateMapJson()
        {
            if (_extractedGridColors == null) return;
            var mapData = new WarlordsMapData(_gridWidth, _gridHeight) { MapName = "Illuria_100x100" };

            // PASS 1: Assign direct terrains
            TerrainType[] finalTerrain = new TerrainType[_gridWidth * _gridHeight];
            for (int i = 0; i < _extractedGridColors.Length; i++)
            {
                Color c = _extractedGridColors[i];
                if (!_blendWithNeighbors[c]) finalTerrain[i] = _colorMapping[c];
            }

            // PASS 2: Reconcile "Blend With Neighbors" (The Furls)
            for (int y = 0; y < _gridHeight; y++)
            {
                for (int x = 0; x < _gridWidth; x++)
                {
                    int index = y * _gridWidth + x;
                    Color c = _extractedGridColors[index];

                    if (_blendWithNeighbors[c])
                    {
                        // Look at 8 neighbors
                        Dictionary<TerrainType, int> neighborCounts = new Dictionary<TerrainType, int>();
                        for (int ny = y - 1; ny <= y + 1; ny++)
                        {
                            for (int nx = x - 1; nx <= x + 1; nx++)
                            {
                                if (nx == x && ny == y) continue;
                                if (nx >= 0 && nx < _gridWidth && ny >= 0 && ny < _gridHeight)
                                {
                                    Color nc = _extractedGridColors[ny * _gridWidth + nx];
                                    if (!_blendWithNeighbors[nc])
                                    {
                                        TerrainType nt = _colorMapping[nc];
                                        if (!neighborCounts.ContainsKey(nt)) neighborCounts[nt] = 0;
                                        neighborCounts[nt]++;
                                    }
                                }
                            }
                        }

                        // Pick the winner
                        if (neighborCounts.Count > 0)
                            finalTerrain[index] = neighborCounts.OrderByDescending(kvp => kvp.Value).First().Key;
                        else
                            finalTerrain[index] = TerrainType.Plains; // Fallback
                    }
                }
            }

            // PASS 3: Write to MapData and Assign POIs
            for (int y = 0; y < _gridHeight; y++)
            {
                for (int x = 0; x < _gridWidth; x++)
                {
                    int index = y * _gridWidth + x;
                    mapData.TerrainData[index] = finalTerrain[index];

                    Color c = _extractedGridColors[index];
                    string poi = _poiMapping[c];

                    if (poi != "None")
                    {
                        // Safely initialize the POI list if it doesn't exist
                        if (mapData.PointsOfInterest == null) mapData.PointsOfInterest = new List<PointOfInterest>();

                        // NOTE: If you don't have a POIType enum in your actual WarlordsMapData yet, 
                        // you might need to adapt this property assignment!
                        mapData.PointsOfInterest.Add(new PointOfInterest
                        {
                            Name = poi + " at " + x + "," + y,
                            X = x,
                            Y = y
                            // Type = (POIType)Enum.Parse(typeof(POIType), poi) 
                        });
                    }
                }
            }

            // Save JSON to Disk
            string json = JsonUtility.ToJson(mapData, true);
            string filePath = System.IO.Path.Combine(Application.dataPath, "Illuria_Extracted.json");
            System.IO.File.WriteAllText(filePath, json);
            ForgeLogger.Log($"[Cartographer] JSON saved with {mapData.PointsOfInterest?.Count ?? 0} POIs to: {filePath}");

            // The SAFE Seamless Handoff
            var mapEditor = new Warlords_Gui_MapEditor();
            // mapEditor.LoadMapData(mapData); 

            if (_mainRoot != null && _mainRoot.parent != null)
            {
                var parent = _mainRoot.parent;
                parent.Clear();
                parent.Add(mapEditor.CreateGui(_lastCtx));
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}