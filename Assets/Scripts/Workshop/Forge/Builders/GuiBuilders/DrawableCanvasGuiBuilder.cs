using System;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class DrawableCanvasGuiBuilder : IGuiProvider
    {
        public string Title => "Drawable Canvas";

        public Texture2D Texture { get; private set; }

        private Color _brushColor = Color.white;
        private int _brushSize = 5;
        private float _brushOpacity = 1f;
        private float _brushHardness = 1f;

        private bool _isDrawing = false;
        private Vector2? _lastMousePos = null;
        private Image _imgElement;

        // RAM Cache for massive performance gains over Texture.GetPixel/SetPixel
        private Color[] _pixelData;

        public DrawableCanvasGuiBuilder(int width, int height)
        {
            InitializeTexture(width, height);
            Clear(Color.clear);
        }

        private void InitializeTexture(int width, int height)
        {
            Texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Texture.filterMode = FilterMode.Bilinear; // Smooth rendering for soft brushes
            _pixelData = new Color[width * height];
            if (_imgElement != null) _imgElement.image = Texture;
        }

        // --- PRO-TOOL STATE CONTROL ---
        public void SetBrushColor(Color color) => _brushColor = color;
        public void SetBrushSize(int size) => _brushSize = size;
        public void SetBrushOpacity(float opacity) => _brushOpacity = Mathf.Clamp01(opacity);
        public void SetBrushHardness(float hardness) => _brushHardness = Mathf.Clamp01(hardness);

        // --- TEXTURE INGESTION & IO ---
        public void LoadTexture(Texture2D newTex)
        {
            if (newTex != null)
            {
                Texture = newTex;
                Texture.filterMode = FilterMode.Bilinear;
                _pixelData = Texture.GetPixels();

                if (_imgElement != null) _imgElement.image = Texture;
                Debug.Log($"[ImageForge] Loaded new texture. Size: {Texture.width}x{Texture.height}");
            }
        }

        public void SaveToDisk(string absolutePath)
        {
            try
            {
                byte[] bytes = Texture.EncodeToPNG();
                File.WriteAllBytes(absolutePath, bytes);
                Debug.Log($"[ImageForge] Saved image to {absolutePath}");
            }
            catch (Exception ex) { Debug.LogError($"[ImageForge] Failed to save: {ex.Message}"); }
        }

        // --- ALGORITHMIC FILTERS ---
        public void Clear(Color clearColor)
        {
            for (int i = 0; i < _pixelData.Length; i++) _pixelData[i] = clearColor;
            ApplyPixels();
        }

        public void ApplyGrayscale()
        {
            for (int i = 0; i < _pixelData.Length; i++)
            {
                float gray = _pixelData[i].grayscale;
                _pixelData[i] = new Color(gray, gray, gray, _pixelData[i].a);
            }
            ApplyPixels();
        }

        public void ApplyInvert()
        {
            for (int i = 0; i < _pixelData.Length; i++)
            {
                _pixelData[i] = new Color(1 - _pixelData[i].r, 1 - _pixelData[i].g, 1 - _pixelData[i].b, _pixelData[i].a);
            }
            ApplyPixels();
        }

        public void GeneratePlasmaNoise()
        {
            float offsetX = UnityEngine.Random.Range(0f, 9999f);
            float offsetY = UnityEngine.Random.Range(0f, 9999f);
            float scale = 0.05f;

            for (int y = 0; y < Texture.height; y++)
            {
                for (int x = 0; x < Texture.width; x++)
                {
                    float noise = Mathf.PerlinNoise((x * scale) + offsetX, (y * scale) + offsetY);
                    _pixelData[y * Texture.width + x] = Color.Lerp(Color.black, _brushColor, noise);
                }
            }
            ApplyPixels();
        }

        private void ApplyPixels()
        {
            Texture.SetPixels(_pixelData);
            Texture.Apply();
        }

        // --- UI & INTERACTION ---
        public VisualElement CreateGui(GuiContext ctx)
        {
            var container = new VisualElement { style = { flexGrow = 1, alignItems = Align.Center, justifyContent = Justify.Center, overflow = Overflow.Hidden } };

            _imgElement = new Image { image = Texture, scaleMode = ScaleMode.ScaleToFit };

            _imgElement.style.borderTopWidth = 2; _imgElement.style.borderBottomWidth = 2;
            _imgElement.style.borderLeftWidth = 2; _imgElement.style.borderRightWidth = 2;
            _imgElement.style.borderTopColor = Color.cyan; _imgElement.style.borderBottomColor = Color.cyan;
            _imgElement.style.borderLeftColor = Color.cyan; _imgElement.style.borderRightColor = Color.cyan;

            _imgElement.RegisterCallback<PointerDownEvent>(e =>
            {
                _isDrawing = true;
                _lastMousePos = e.localPosition;
                DrawInterpolatedStroke(_lastMousePos.Value, e.localPosition, _imgElement);
                _imgElement.CapturePointer(e.pointerId);
            });

            _imgElement.RegisterCallback<PointerMoveEvent>(e =>
            {
                if (_isDrawing && _lastMousePos.HasValue)
                {
                    DrawInterpolatedStroke(_lastMousePos.Value, e.localPosition, _imgElement);
                    _lastMousePos = e.localPosition;
                }
            });

            _imgElement.RegisterCallback<PointerUpEvent>(e =>
            {
                _isDrawing = false;
                _lastMousePos = null;
                _imgElement.ReleasePointer(e.pointerId);
            });

            container.Add(_imgElement);
            return container;
        }

        // --- THE PROCREATE SECRET SAUCE ---
        private void DrawInterpolatedStroke(Vector2 startPos, Vector2 endPos, Image img)
        {
            float dist = Vector2.Distance(startPos, endPos);
            int steps = Mathf.Max(1, Mathf.CeilToInt(dist / (_brushSize * 0.15f)));

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector2 lerpedPos = Vector2.Lerp(startPos, endPos, t);
                PaintAtPosition(lerpedPos, img);
            }
            ApplyPixels();
        }

        private void PaintAtPosition(Vector2 localPos, Image img)
        {
            float xPct = localPos.x / img.layout.width;
            float yPct = 1.0f - (localPos.y / img.layout.height);

            int texX = Mathf.Clamp((int)(xPct * Texture.width), 0, Texture.width - 1);
            int texY = Mathf.Clamp((int)(yPct * Texture.height), 0, Texture.height - 1);

            float halfBrush = _brushSize / 2f;
            int bound = Mathf.CeilToInt(halfBrush);

            for (int x = -bound; x <= bound; x++)
            {
                for (int y = -bound; y <= bound; y++)
                {
                    float distFromCenter = Mathf.Sqrt((x * x) + (y * y));
                    float normalizedDist = distFromCenter / halfBrush;

                    if (normalizedDist <= 1f)
                    {
                        int px = Mathf.Clamp(texX + x, 0, Texture.width - 1);
                        int py = Mathf.Clamp(texY + y, 0, Texture.height - 1);

                        // Calculate Soft Edge Falloff
                        float alpha = 1f;
                        if (_brushHardness < 1f && normalizedDist > _brushHardness)
                        {
                            alpha = 1f - ((normalizedDist - _brushHardness) / (1f - _brushHardness));
                        }

                        // Multiply by overall opacity
                        alpha *= _brushOpacity;

                        if (alpha > 0.01f)
                        {
                            int index = py * Texture.width + px;
                            Color currentColor = _pixelData[index];

                            // True Alpha Blending formula
                            Color blendedColor = Color.Lerp(currentColor, new Color(_brushColor.r, _brushColor.g, _brushColor.b, 1f), alpha * _brushColor.a);
                            _pixelData[index] = blendedColor;
                        }
                    }
                }
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}