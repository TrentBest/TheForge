using UnityEngine;
using UnityEngine.UIElements;
using System;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.Core.Diagnostics;


namespace Assets.Scripts.Warlords
{
    public class Warlords_Gui_MapEditor : IGuiProvider
    {
        public string Title => "WARLORDS: MAP FORGE";

        private VisualElement _mainContainer;
        private string _mapName = "Illuria";
        private int _mapSizeX = 20;
        private int _mapSizeY = 20;
        private object _selectedBrush = TerrainType.Plains;
        private VisualElement _gridRoot;
        private GuiContext _lastCtx;

        private TileManager _tileManager;

        // Discrete Binary Zoom: 8 levels (1/8 size to 1/1)
        private int _zoomStep = 7;
        private readonly float[] _zoomLevels = new float[] {
            0.125f, 0.25f, 0.375f, 0.5f, 0.625f, 0.75f, 0.875f, 1.0f
        };
        private const int BaseSize = 256;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            _tileManager = GameObject.FindAnyObjectByType<TileManager>();

            _mainContainer = new VisualElement();
            _mainContainer.style.flexGrow = 1;

            _mainContainer.Add(CreateEditorInterface());
            return _mainContainer;
        }

        private VisualElement CreateEditorInterface()
        {
            var builder = new GraphicalUserInterfaceBuilder("MapEditor_Main")
                .WithPercentSize(100, 100)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

            // --- TOP HEADER ---
            builder.AddChild(ctx =>
            {
                var header = new VisualElement();
                header.style.height = 60;
                header.style.backgroundColor = new Color(0.08f, 0.08f, 0.08f);
                header.style.flexDirection = FlexDirection.Row;
                header.style.alignItems = Align.Center;
                header.style.paddingLeft = 20;
                header.style.borderBottomWidth = 2;
                header.style.borderBottomColor = Color.black;

                header.Add(new Label("MAP:") { style = { color = Color.gray, marginRight = 5 } });
                var nameField = new TextField { value = _mapName, style = { width = 150 } };
                nameField.RegisterValueChangedCallback(evt => _mapName = evt.newValue);
                header.Add(nameField);

                var wField = new IntegerField("W:") { value = _mapSizeX, style = { width = 70, marginLeft = 15 } };
                var hField = new IntegerField("H:") { value = _mapSizeY, style = { width = 70, marginLeft = 10 } };
                header.Add(wField);
                header.Add(hField);

                var applyBtn = new Button(() =>
                {
                    _mapSizeX = wField.value;
                    _mapSizeY = hField.value;
                    _mainContainer.Clear();
                    _mainContainer.Add(CreateEditorInterface());
                })
                { text = "GENERATE", style = { backgroundColor = new Color(0.3f, 0.1f, 0.1f), marginLeft = 10 } };
                header.Add(applyBtn);

                header.Add(new VisualElement { style = { flexGrow = 1 } });
                header.Add(new Label($"ZOOM: {(_zoomLevels[_zoomStep] * 100):0}%") { style = { marginRight = 20, color = Color.cyan } });

                return header;
            });

            builder.AddChild(ctx =>
            {
                var body = new VisualElement();
                body.style.flexGrow = 1;
                body.style.flexDirection = FlexDirection.Row;

                // --- TOOLBAR (SIDEBAR) ---
                var toolbar = new VisualElement();
                toolbar.style.width = 220;
                toolbar.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
                toolbar.style.paddingTop = 10;
                toolbar.style.paddingBottom = 10;
                toolbar.style.paddingLeft = 10;
                toolbar.style.paddingRight = 10;

                AddBrushSection(toolbar, "TERRAIN", Enum.GetValues(typeof(TerrainType)));
                AddBrushSection(toolbar, "STRUCTURES", Enum.GetValues(typeof(POIType)));

                toolbar.Add(new VisualElement { style = { flexGrow = 1 } });
                var saveBtn = new Button(SaveMapData) { text = "SAVE MAP" };
                saveBtn.style.height = 45;
                saveBtn.style.backgroundColor = new Color(0.4f, 0.15f, 0.15f);
                toolbar.Add(saveBtn);

                body.Add(toolbar);

                // --- GRID ---
                var scroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
                scroll.style.flexGrow = 1;
                scroll.style.backgroundColor = Color.black;

                _gridRoot = new VisualElement();
                _gridRoot.style.paddingTop = 512;
                _gridRoot.style.paddingBottom = 512;
                _gridRoot.style.paddingLeft = 512;
                _gridRoot.style.paddingRight = 512;

                _gridRoot.RegisterCallback<WheelEvent>(OnScrollZoom);

                BuildGrid();

                scroll.Add(_gridRoot);
                body.Add(scroll);
                return body;
            });

            return builder.Build();
        }

        private void AddBrushSection(VisualElement parent, string title, Array values)
        {
            var label = new Label(title);
            label.style.marginTop = 10;
            label.style.color = Color.gray;
            label.style.fontSize = 11;
            parent.Add(label);

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.flexWrap = Wrap.Wrap;

            foreach (var val in values)
            {
                var btn = new Button(() => _selectedBrush = val);
                btn.style.width = 48;
                btn.style.height = 48;
                btn.style.marginBottom = 4;
                btn.style.marginRight = 4;

                if (_tileManager != null)
                {
                    Sprite s = null;
                    if (val is TerrainType t) s = _tileManager.GetSprite(t);
                    else if (val is POIType p) s = _tileManager.GetSprite(p);

                    if (s != null)
                    {
                        btn.style.backgroundImage = new StyleBackground(s);
                        btn.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
                    }
                }
                container.Add(btn);
            }
            parent.Add(container);
        }

        private void BuildGrid()
        {
            _gridRoot.Clear();
            float scale = _zoomLevels[_zoomStep];
            float scaledSize = BaseSize * scale;

            for (int y = 0; y < _mapSizeY; y++)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.height = scaledSize;

                for (int x = 0; x < _mapSizeX; x++)
                {
                    var tile = new VisualElement();
                    tile.style.width = scaledSize;
                    tile.style.height = scaledSize;
                    tile.style.overflow = Overflow.Visible;

                    tile.userData = new TileData { Terrain = TerrainType.Plains };

                    tile.RegisterCallback<MouseDownEvent>(e => PaintTile(tile));
                    tile.RegisterCallback<MouseEnterEvent>(e => { if (e.pressedButtons == 1) PaintTile(tile); });

                    UpdateTileVisual(tile);
                    row.Add(tile);
                }
                _gridRoot.Add(row);
            }
        }

        private void OnScrollZoom(WheelEvent evt)
        {
            if (evt.ctrlKey)
            {
                int direction = evt.delta.y > 0 ? -1 : 1;
                _zoomStep = Mathf.Clamp(_zoomStep + direction, 0, _zoomLevels.Length - 1);
                RefreshGridDimensions();
                evt.StopPropagation();
            }
        }

        private void RefreshGridDimensions()
        {
            float scale = _zoomLevels[_zoomStep];
            float scaledSize = BaseSize * scale;
            foreach (var row in _gridRoot.Children())
            {
                row.style.height = scaledSize;
                foreach (var tile in row.Children())
                {
                    tile.style.width = scaledSize;
                    tile.style.height = scaledSize;
                    UpdateTileVisual(tile);
                }
            }
        }

        private void UpdateTileVisual(VisualElement tile)
        {
            if (!(tile.userData is TileData data)) return;
            tile.Clear();

            float scale = _zoomLevels[_zoomStep];
            float sW = BaseSize * scale;
            float sH = (BaseSize + 128) * scale;

            var visual = new VisualElement();
            visual.style.position = Position.Absolute;
            visual.style.width = sW;
            visual.style.height = sH;
            visual.style.bottom = 0;
            visual.pickingMode = PickingMode.Ignore;

            if (_tileManager != null)
            {
                var s = _tileManager.GetSprite(data.Terrain);
                if (s != null) visual.style.backgroundImage = new StyleBackground(s);
            }
            tile.Add(visual);

            if (data.POI.HasValue)
            {
                var poi = new VisualElement();
                poi.style.position = Position.Absolute;
                poi.style.width = sW;
                poi.style.height = sW;
                poi.style.bottom = 0;
                poi.pickingMode = PickingMode.Ignore;
                var ps = _tileManager?.GetSprite(data.POI.Value);
                if (ps != null) poi.style.backgroundImage = new StyleBackground(ps);
                tile.Add(poi);
            }
        }

        private void PaintTile(VisualElement tile)
        {
            if (tile.userData is TileData data)
            {
                if (_selectedBrush is TerrainType t) data.Terrain = t;
                else if (_selectedBrush is POIType p) data.POI = p;
                UpdateTileVisual(tile);
            }
        }

        private class TileData { public TerrainType Terrain; public POIType? POI; }
        private void SaveMapData() => ForgeLogger.Log($"MAP FORGE: Saved {_mapName}");

        public System.Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}