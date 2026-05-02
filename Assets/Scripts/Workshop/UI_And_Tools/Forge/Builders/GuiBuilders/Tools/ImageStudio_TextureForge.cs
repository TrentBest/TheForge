using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    public class ImageStudio_TextureForge : IGuiProvider
    {
        public string Title => "Texture Forge";

        // --- Memory & State ---
        private Texture2D _activeTexture;
        private Texture2D _cursorTexture; // The invisible layer for our glowing 3D cursor
        private Color _paintColor = Color.cyan;
        private int _brushSize = 8;
        private PrimitiveType _currentPrimitive = PrimitiveType.Cube;
        private bool _isPainting = false;

        private RectInt _lastCursorRect;

        // --- GUI Elements ---
        private Image _2dCanvas;
        private VisualElement _3dPreviewContainer;
        private GameObject _ghostModel;
        private LiveModelPreviewBuilder _previewBuilder;

        public ImageStudio_TextureForge()
        {
            InitializeSharedTextures();
        }

        private void InitializeSharedTextures()
        {
            _activeTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            _cursorTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };

            ClearTexture(_activeTexture, new Color(0.1f, 0.1f, 0.15f, 1f));
            ClearTexture(_cursorTexture, Color.clear);
        }

        private void ClearTexture(Texture2D tex, Color color)
        {
            Color[] pixels = new Color[tex.width * tex.height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var theme = GuiSkin.Active?.GlobalDefault;
            Color bgColor = theme != null ? theme.BackgroundColor : new Color(0.1f, 0.1f, 0.1f);
            Color fgColor = theme != null ? theme.TextColor : Color.white;

            var root = new GraphicalUserInterfaceBuilder("TextureForgeRoot")
                .WithAutoGrow()
                .WithFlexLayout(FlexDirection.Column)
                .WithBackgroundColor(bgColor)
                .Build();

            // --- 1. THE TOOLBAR ---
            var toolbar = new VisualElement { style = { flexDirection = FlexDirection.Row, paddingBottom = 15, paddingTop = 10, paddingLeft = 10, borderBottomWidth = 2, borderBottomColor = fgColor, marginBottom = 15, alignItems = Align.Center } };

            var meshSelector = new DropdownField("Projection:", new List<string> { "Cube", "Sphere", "Capsule", "Cylinder" }, 0) { style = { width = 200, marginRight = 20, color = fgColor } };
            meshSelector.RegisterValueChangedCallback(evt => {
                if (Enum.TryParse(evt.newValue, out PrimitiveType parsedType))
                {
                    _currentPrimitive = parsedType;
                    Refresh3DPreview(ctx);
                }
            });
            toolbar.Add(meshSelector);

            var brushSlider = new SliderInt("Brush Size", 1, 40) { value = _brushSize, style = { width = 200, marginRight = 20, color = fgColor } };
            brushSlider.RegisterValueChangedCallback(evt => _brushSize = evt.newValue);
            toolbar.Add(brushSlider);

            var colorContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, marginRight = 20, alignItems = Align.Center } };
            colorContainer.Add(new Label("Ink:") { style = { marginRight = 10, color = fgColor } });

            Color[] swatches = { Color.white, Color.black, Color.red, Color.green, new Color(0.2f, 0.6f, 1f), Color.cyan, Color.magenta, Color.yellow };
            foreach (var c in swatches)
            {
                var btn = new VisualElement { style = { width = 24, height = 24, backgroundColor = c, marginRight = 5, borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1, borderTopColor = Color.gray } };
                btn.RegisterCallback<PointerDownEvent>(e => _paintColor = c);
                colorContainer.Add(btn);
            }
            toolbar.Add(colorContainer);

            toolbar.Add(new Button(() => ClearTexture(_activeTexture, new Color(0.1f, 0.1f, 0.15f, 1f))) { text = "Clear Canvas", style = { height = 30 } });

            root.Add(toolbar);

            // --- 2. THE SPLIT STAGE ---
            var splitView = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1, paddingLeft = 10, paddingRight = 10, paddingBottom = 10 } };

            var leftPanel = new VisualElement { style = { flexGrow = 1, borderRightWidth = 2, borderRightColor = fgColor, paddingRight = 15 } };
            leftPanel.Add(new Label("LIVE 3D PROJECTION") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10, color = fgColor, fontSize = 16 } });

            _3dPreviewContainer = new VisualElement { style = { flexGrow = 1, overflow = Overflow.Hidden, borderTopLeftRadius = 8, borderTopRightRadius = 8, borderBottomLeftRadius = 8, borderBottomRightRadius = 8 } };
            leftPanel.Add(_3dPreviewContainer);
            splitView.Add(leftPanel);

            var rightPanel = new VisualElement { style = { flexGrow = 1, paddingLeft = 15, alignItems = Align.Center } };
            rightPanel.Add(new Label("2D TEXTURE UNROLL (PAINT HERE)") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10, color = fgColor, fontSize = 16 } });

            // 2D Canvas setup
            _2dCanvas = new Image { image = _activeTexture, scaleMode = ScaleMode.ScaleToFit };
            _2dCanvas.style.width = 512;
            _2dCanvas.style.height = 512;
            _2dCanvas.style.borderTopWidth = 2;
            _2dCanvas.style.borderBottomWidth = 2;
            _2dCanvas.style.borderLeftWidth = 2;
            _2dCanvas.style.borderRightWidth = 2;
            _2dCanvas.style.borderTopColor = fgColor;
            _2dCanvas.style.borderBottomColor = fgColor;
            _2dCanvas.style.borderLeftColor = fgColor;
            _2dCanvas.style.borderRightColor = fgColor;

            BindPaintingLogic();

            rightPanel.Add(_2dCanvas);
            splitView.Add(rightPanel);
            root.Add(splitView);

            Refresh3DPreview(ctx);
            root.RegisterCallback<DetachFromPanelEvent>(evt => ScrubMemory());

            return root;
        }

        private void Refresh3DPreview(GuiContext ctx)
        {
            if (_3dPreviewContainer == null) return;

            _3dPreviewContainer.Clear();
            _previewBuilder?.Dispose();
            if (_ghostModel != null) UnityEngine.Object.DestroyImmediate(_ghostModel);

            _ghostModel = GameObject.CreatePrimitive(_currentPrimitive);
            _ghostModel.SetActive(false);

            var renderer = _ghostModel.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Standard"));

            // The Color Data
            mat.mainTexture = _activeTexture;
            mat.SetFloat("_Glossiness", 0.1f);

            // The Live Cursor Magic
            mat.EnableKeyword("_EMISSION");
            mat.SetTexture("_EmissionMap", _cursorTexture);
            mat.SetColor("_EmissionColor", Color.white * 2f); // Glow intensely

            renderer.sharedMaterial = mat;

            _previewBuilder = new LiveModelPreviewBuilder(_ghostModel)
                .WithAutoRotate(true, 15f)
                .WithMouseControl(true)
                .WithZoom(4f);

            var previewVisual = _previewBuilder.CreateGui(ctx);
            previewVisual.style.flexGrow = 1;
            _3dPreviewContainer.Add(previewVisual);
        }

        private void BindPaintingLogic()
        {
            _2dCanvas.RegisterCallback<PointerDownEvent>(evt => {
                _isPainting = true;
                _2dCanvas.CapturePointer(evt.pointerId);
                ProcessPointer(evt.localPosition);
            });

            _2dCanvas.RegisterCallback<PointerMoveEvent>(evt => {
                ProcessPointer(evt.localPosition);
            });

            _2dCanvas.RegisterCallback<PointerUpEvent>(evt => {
                _isPainting = false;
                _2dCanvas.ReleasePointer(evt.pointerId);
            });

            _2dCanvas.RegisterCallback<PointerLeaveEvent>(evt => {
                EraseOldCursor();
                _cursorTexture.Apply();
            });
        }

        private void ProcessPointer(Vector2 localPos)
        {
            float normalizedX = localPos.x / _2dCanvas.layout.width;
            float normalizedY = 1f - (localPos.y / _2dCanvas.layout.height);

            if (normalizedX < 0 || normalizedX > 1 || normalizedY < 0 || normalizedY > 1) return;

            int px = Mathf.RoundToInt(normalizedX * _activeTexture.width);
            int py = Mathf.RoundToInt(normalizedY * _activeTexture.height);

            // 1. Draw glowing cursor footprint
            DrawCursor(px, py);

            // 2. If painting, apply ink to the main texture
            if (_isPainting)
            {
                int radius = _brushSize;
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        if (x * x + y * y <= radius * radius)
                        {
                            int targetX = Mathf.Clamp(px + x, 0, _activeTexture.width - 1);
                            int targetY = Mathf.Clamp(py + y, 0, _activeTexture.height - 1);
                            _activeTexture.SetPixel(targetX, targetY, _paintColor);
                        }
                    }
                }
                _activeTexture.Apply();
            }
        }

        private void DrawCursor(int px, int py)
        {
            // Erase the last known position of the cursor
            EraseOldCursor();

            // Calculate new bounds
            int r = _brushSize;
            int xMin = Mathf.Max(0, px - r);
            int yMin = Mathf.Max(0, py - r);
            int width = Mathf.Min(_cursorTexture.width - xMin, r * 2 + 1);
            int height = Mathf.Min(_cursorTexture.height - yMin, r * 2 + 1);

            _lastCursorRect = new RectInt(xMin, yMin, width, height);

            // Draw new cursor ring
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    float distSq = x * x + y * y;
                    // Draw a hollow ring for the cursor outline
                    if (distSq <= r * r && distSq >= (r - 1) * (r - 1))
                    {
                        int targetX = Mathf.Clamp(px + x, 0, _cursorTexture.width - 1);
                        int targetY = Mathf.Clamp(py + y, 0, _cursorTexture.height - 1);
                        _cursorTexture.SetPixel(targetX, targetY, Color.red);
                    }
                }
            }
            _cursorTexture.Apply();
        }

        private void EraseOldCursor()
        {
            if (_lastCursorRect.width > 0 && _lastCursorRect.height > 0)
            {
                Color[] clearColors = new Color[_lastCursorRect.width * _lastCursorRect.height];
                for (int i = 0; i < clearColors.Length; i++) clearColors[i] = Color.clear;

                _cursorTexture.SetPixels(
                    _lastCursorRect.x,
                    _lastCursorRect.y,
                    _lastCursorRect.width,
                    _lastCursorRect.height,
                    clearColors
                );
            }
        }

        private void ScrubMemory()
        {
            _previewBuilder?.Dispose();
            if (_ghostModel != null) UnityEngine.Object.DestroyImmediate(_ghostModel);
            if (_activeTexture != null) UnityEngine.Object.DestroyImmediate(_activeTexture);
            if (_cursorTexture != null) UnityEngine.Object.DestroyImmediate(_cursorTexture);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}