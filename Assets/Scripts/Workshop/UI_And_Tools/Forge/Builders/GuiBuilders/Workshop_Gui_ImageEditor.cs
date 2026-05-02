using System;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// Generalized In-World or Editor Image Manipulation Tool.
    /// Features interactive eyedropper pixel selection and threshold-based color swapping.
    /// </summary>
    public class Workshop_Gui_ImageEditor : IGuiProvider
    {
        public string Title => "FORGE IMAGE EDITOR";

        private GuiContext _guiContext;
        private VisualElement _rootContainer;
        private VisualElement _controlsContainer;
        private VisualElement _previewContainer;

        // --- IMAGE STATE ---
        private Texture2D _sourceTexture;
        private Texture2D _processedTexture;

        // --- SWAP COMMAND STATE ---
        private Color _sourceColor = Color.green; // Default to green
        private Color _targetColor = Color.magenta;
        private float _threshold = 0.1f; // +/- 10% on all channels
        private bool _preserveLuminosity = true;
        private bool _isEyedropperActive = false;

        public Workshop_Gui_ImageEditor() { }

        public Workshop_Gui_ImageEditor(Texture2D initialTexture)
        {
            _sourceTexture = initialTexture;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _guiContext = ctx;

            _rootContainer = new GraphicalUserInterfaceBuilder("ImageEditorRoot")
                .WithPercentSize(100, 100)
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f)) // Deep space blue
                .Build();

            var splitPanel = new ForgeSplitPanelBuilder(400, Side.Left)
                .WithSidebar(new DynamicGuiProvider(c => {
                    _controlsContainer = new VisualElement { style = { flexGrow = 1 } };
                    RenderMainControls();
                    return _controlsContainer;
                }))
                .WithMain(new DynamicGuiProvider(c => BuildPreviewPanel()));

            _rootContainer.Add(splitPanel.CreateGui(ctx));

            if (_sourceTexture != null) ProcessPixelSwap();

            return _rootContainer;
        }

        // --- CONTROL PANEL ROUTING ---

        private void RenderMainControls()
        {
            _controlsContainer.Clear();

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.paddingLeft = 20; scroll.style.paddingRight = 20; scroll.style.paddingTop = 20; scroll.style.paddingBottom = 40;
            scroll.style.backgroundColor = new Color(0.12f, 0.12f, 0.14f);

            scroll.Add(new Label("PIXEL SWAP COMMAND") { style = { fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.4f, 0.8f, 1f), marginBottom = 20 } });

            // 1. FILE IO
            scroll.Add(new Label("1. SOURCE MEDIA") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
            var loadBtn = new ForgeButtonBuilder("LOAD TEST TEXTURE", () => {
                _sourceTexture = Resources.Load<Texture2D>("Images/Warlords/TargetTile"); // Fallback
                if (_sourceTexture == null) _sourceTexture = CreateSyntheticTestTexture();
                ProcessPixelSwap();
                RenderPreviewImages();
            }).WithHeight(35).WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f)).WithTextColor(Color.white).CreateGui(_guiContext);
            scroll.Add(loadBtn);

            scroll.Add(new VisualElement { style = { height = 1, backgroundColor = Color.black, marginTop = 15, marginBottom = 15 } });

            // 2. SOURCE COLOR (Eyedropper)
            scroll.Add(new Label("2. SOURCE PIXEL (What to replace)") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });

            var sourcePreviewRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 10 } };
            var sourceColorBox = new VisualElement { style = { width = 60, height = 40, backgroundColor = _sourceColor, borderTopWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, borderRightWidth = 2, borderTopColor = Color.white, borderBottomColor = Color.white, borderLeftColor = Color.white, borderRightColor = Color.white } };
            sourcePreviewRow.Add(sourceColorBox);

            var eyedropperBtn = new Button(() => {
                _isEyedropperActive = !_isEyedropperActive;
                RenderMainControls(); // Refresh UI to show active state
                RenderPreviewImages(); // Refresh Preview to show glowing border
            })
            { text = _isEyedropperActive ? "[ CANCEL EYEDROPPER ]" : "ACTIVATE EYEDROPPER" };
            eyedropperBtn.style.flexGrow = 1; eyedropperBtn.style.height = 40;
            eyedropperBtn.style.backgroundColor = _isEyedropperActive ? new Color(0.8f, 0.4f, 0.1f) : new Color(0.2f, 0.2f, 0.3f);
            eyedropperBtn.style.color = Color.white; eyedropperBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            sourcePreviewRow.Add(eyedropperBtn);
            scroll.Add(sourcePreviewRow);

            if (_isEyedropperActive)
            {
                scroll.Add(new Label(">> Click on the BEFORE image to pick a color.") { style = { color = new Color(0.8f, 0.4f, 0.1f), unityFontStyleAndWeight = FontStyle.Italic, marginBottom = 10 } });
            }

            var thresholdSlider = new Slider("Channel Threshold (+/- Range)", 0.0f, 1.0f) { value = _threshold };
            thresholdSlider.RegisterValueChangedCallback(e => { _threshold = e.newValue; ProcessPixelSwap(); });
            scroll.Add(thresholdSlider);

            scroll.Add(new VisualElement { style = { height = 1, backgroundColor = Color.black, marginTop = 15, marginBottom = 15 } });

            // 3. TARGET COLOR (ColorForgePicker)
            scroll.Add(new Label("3. TARGET COLOR (Become this color)") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });

            var targetPreviewRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 10 } };
            var targetColorBox = new VisualElement { style = { width = 60, height = 40, backgroundColor = _targetColor, borderTopWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, borderRightWidth = 2, borderTopColor = Color.white, borderBottomColor = Color.white, borderLeftColor = Color.white, borderRightColor = Color.white } };
            targetPreviewRow.Add(targetColorBox);

            var pickTargetBtn = new Button(() => { RenderColorPickerOverlay(); }) { text = "DEFINE TARGET COLOR" };
            pickTargetBtn.style.flexGrow = 1; pickTargetBtn.style.height = 40;
            pickTargetBtn.style.backgroundColor = new Color(0.2f, 0.2f, 0.3f);
            pickTargetBtn.style.color = Color.white; pickTargetBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            targetPreviewRow.Add(pickTargetBtn);
            scroll.Add(targetPreviewRow);

            var lumToggle = new Toggle("Preserve Original Luminosity") { value = _preserveLuminosity };
            lumToggle.RegisterValueChangedCallback(e => { _preserveLuminosity = e.newValue; ProcessPixelSwap(); });
            scroll.Add(lumToggle);

            scroll.Add(new VisualElement { style = { height = 1, backgroundColor = Color.black, marginTop = 15, marginBottom = 15 } });

            // 4. ACTION
            var applyBtn = new ForgeButtonBuilder("FORCE RE-RENDER", ProcessPixelSwap)
                .WithHeight(45).WithBackgroundColor(new Color(0.2f, 0.5f, 0.2f)).WithTextColor(Color.white).WithFontStyle(FontStyle.Bold).CreateGui(_guiContext);
            scroll.Add(applyBtn);

            var saveBtn = new ForgeButtonBuilder("SAVE MODIFIED TEXTURE", SaveTexture)
                .WithHeight(40).WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f)).WithTextColor(Color.gray).WithFontStyle(FontStyle.Bold).CreateGui(_guiContext);
            saveBtn.style.marginTop = 10;
            scroll.Add(saveBtn);

            _controlsContainer.Add(scroll);
        }

        private void RenderColorPickerOverlay()
        {
            _controlsContainer.Clear();

            var wrapper = new VisualElement { style = { flexGrow = 1, backgroundColor = new Color(0.1f, 0.1f, 0.12f), paddingLeft = 10, paddingRight = 10, paddingTop = 20 } };
            wrapper.Add(new Label("SELECT REPLACEMENT COLOR") { style = { fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, color = Color.white, marginBottom = 20 } });

            // Inject the ColorForgePicker
            var pickerProvider = new ColorForgePicker(_targetColor, (newColor) => {
                _targetColor = newColor;
                ProcessPixelSwap(); // Live update as they drag the slider!
            });

            var pickerUI = pickerProvider.CreateGui(_guiContext);
            pickerUI.style.flexGrow = 1;
            wrapper.Add(pickerUI);

            var closeBtn = new Button(() => { RenderMainControls(); }) { text = "CONFIRM & RETURN" };
            closeBtn.style.height = 50; closeBtn.style.backgroundColor = new Color(0.2f, 0.6f, 0.2f); closeBtn.style.color = Color.white; closeBtn.style.unityFontStyleAndWeight = FontStyle.Bold; closeBtn.style.marginTop = 20; closeBtn.style.marginBottom = 20;
            wrapper.Add(closeBtn);

            _controlsContainer.Add(wrapper);
        }

        // --- PREVIEW PANEL & EYEDROPPER LOGIC ---

        private VisualElement BuildPreviewPanel()
        {
            _previewContainer = new VisualElement { style = { flexGrow = 1, backgroundColor = new Color(0.02f, 0.02f, 0.03f), justifyContent = Justify.Center, alignItems = Align.Center } };
            RenderPreviewImages();
            return _previewContainer;
        }

        private void RenderPreviewImages()
        {
            if (_previewContainer == null) return;
            _previewContainer.Clear();

            var wrapper = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.Center, width = Length.Percent(100) } };

            if (_sourceTexture != null)
            {
                var beforeContainer = new VisualElement { style = { alignItems = Align.Center, marginRight = 20, flexGrow = 1 } };
                var beforeLbl = new Label(_isEyedropperActive ? "[ EYEDROPPER ACTIVE - CLICK IMAGE ]" : "BEFORE") { style = { color = _isEyedropperActive ? new Color(0.8f, 0.4f, 0.1f) : Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } };
                beforeContainer.Add(beforeLbl);

                var imgBefore = new Image { image = _sourceTexture, scaleMode = ScaleMode.ScaleToFit };
                imgBefore.style.width = 400; imgBefore.style.height = 400;
                imgBefore.style.backgroundColor = new Color(0, 0, 0, 0.5f); // Helps see bounds

                // EYEDROPPER EVENT BINDING
                imgBefore.RegisterCallback<PointerDownEvent>(evt => OnBeforeImageClicked(evt, imgBefore));

                // Clean UI Toolkit visual feedback for Eyedropper mode
                if (_isEyedropperActive)
                {
                    imgBefore.style.borderTopWidth = 3; imgBefore.style.borderBottomWidth = 3; imgBefore.style.borderLeftWidth = 3; imgBefore.style.borderRightWidth = 3;
                    imgBefore.style.borderTopColor = new Color(0.8f, 0.4f, 0.1f); imgBefore.style.borderBottomColor = new Color(0.8f, 0.4f, 0.1f);
                    imgBefore.style.borderLeftColor = new Color(0.8f, 0.4f, 0.1f); imgBefore.style.borderRightColor = new Color(0.8f, 0.4f, 0.1f);
                }

                beforeContainer.Add(imgBefore);
                wrapper.Add(beforeContainer);
            }

            if (_processedTexture != null)
            {
                var afterContainer = new VisualElement { style = { alignItems = Align.Center, marginLeft = 20, flexGrow = 1 } };
                afterContainer.Add(new Label("AFTER (SWAP APPLIED)") { style = { color = new Color(0.4f, 0.8f, 1f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                var imgAfter = new Image { image = _processedTexture, scaleMode = ScaleMode.ScaleToFit };
                imgAfter.style.width = 400; imgAfter.style.height = 400;
                imgAfter.style.backgroundColor = new Color(0, 0, 0, 0.5f);

                afterContainer.Add(imgAfter);
                wrapper.Add(afterContainer);
            }

            if (_sourceTexture == null && _processedTexture == null)
            {
                wrapper.Add(new Label("NO MEDIA LOADED") { style = { color = Color.gray, fontSize = 24 } });
            }

            _previewContainer.Add(wrapper);
        }

        private void OnBeforeImageClicked(PointerDownEvent evt, Image imgControl)
        {
            if (!_isEyedropperActive || _sourceTexture == null) return;

            // Map the VisualElement local mouse click to the Texture's UV coordinates
            Vector2 localPos = evt.localPosition;
            float eleW = imgControl.layout.width;
            float eleH = imgControl.layout.height;
            float texW = _sourceTexture.width;
            float texH = _sourceTexture.height;

            if (eleW <= 0 || eleH <= 0) return;

            float imgAspect = texW / texH;
            float eleAspect = eleW / eleH;

            float drawW = eleW;
            float drawH = eleH;
            float offsetX = 0;
            float offsetY = 0;

            // Calculate ScaleToFit bounds (Letterbox or Pillarbox)
            if (imgAspect > eleAspect)
            {
                drawH = eleW / imgAspect;
                offsetY = (eleH - drawH) / 2f;
            }
            else
            {
                drawW = eleH * imgAspect;
                offsetX = (eleW - drawW) / 2f;
            }

            // Ensure click is actually inside the drawn image, not the black bars
            if (localPos.x >= offsetX && localPos.x <= offsetX + drawW &&
                localPos.y >= offsetY && localPos.y <= offsetY + drawH)
            {
                float u = (localPos.x - offsetX) / drawW;
                float v = 1f - ((localPos.y - offsetY) / drawH); // UI is Y-down, Texture is Y-up

                // Attempt to read pixel. Fails gracefully if texture isn't Read/Write enabled.
                try
                {
                    Color picked = _sourceTexture.GetPixelBilinear(u, v);
                    _sourceColor = picked;
                    _isEyedropperActive = false; // Auto-turn off

                    RenderMainControls(); // Update left panel to show new color
                    ProcessPixelSwap();   // Instantly apply the swap
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ImageEditor] Cannot read pixel. Ensure Source Texture has 'Read/Write' enabled in Import Settings. Error: {ex.Message}");
                }
            }
        }

        // --- ALGORITHMIC PROCESSING ---

        private void ProcessPixelSwap()
        {
            if (_sourceTexture == null) return;

            if (!_sourceTexture.isReadable)
            {
                Debug.LogWarning("[ImageEditor] Source texture must be marked as 'Read/Write' in Unity Import Settings to manipulate pixels.");
                return;
            }

            int width = _sourceTexture.width;
            int height = _sourceTexture.height;
            _processedTexture = new Texture2D(width, height, _sourceTexture.format, false);
            _processedTexture.filterMode = _sourceTexture.filterMode;

            Color[] pixels = _sourceTexture.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
            {
                Color p = pixels[i];
                if (p.a < 0.1f) continue; // Skip transparency

                // Absolute Channel Difference
                float rDiff = Mathf.Abs(p.r - _sourceColor.r);
                float gDiff = Mathf.Abs(p.g - _sourceColor.g);
                float bDiff = Mathf.Abs(p.b - _sourceColor.b);

                if (rDiff <= _threshold && gDiff <= _threshold && bDiff <= _threshold)
                {
                    if (_preserveLuminosity)
                    {
                        // Convert original and target to HSV, combine Target Hue/Sat with Original Value
                        float oH, oS, oV, tH, tS, tV;
                        Color.RGBToHSV(p, out oH, out oS, out oV);
                        Color.RGBToHSV(_targetColor, out tH, out tS, out tV);

                        pixels[i] = Color.HSVToRGB(tH, tS, oV); // Keep original shading (oV)
                        pixels[i].a = p.a;
                    }
                    else
                    {
                        // Hard flat replacement
                        pixels[i] = _targetColor;
                        pixels[i].a = p.a;
                    }
                }
            }

            _processedTexture.SetPixels(pixels);
            _processedTexture.Apply();

            RenderPreviewImages(); // Update the right side panel
        }

        // --- UTILITIES ---

        private void SaveTexture()
        {
            if (_processedTexture == null) return;
            byte[] bytes = _processedTexture.EncodeToPNG();
            string path = Path.Combine(Application.dataPath, $"Resources/Images/ModifiedTexture_{DateTime.Now.Ticks}.png");
            File.WriteAllBytes(path, bytes);
            Debug.Log($"[ImageEditor] Saved modified texture to: {path}");
        }

        private Texture2D CreateSyntheticTestTexture()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Draw a green circle on a gray background to test swapping
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2, size / 2));
                    if (dist < 40)
                    {
                        // Green with some shading variation to test luminosity preservation
                        float shade = 1f - (dist / 40f);
                        tex.SetPixel(x, y, new Color(0, 0.5f + (shade * 0.5f), 0, 1f));
                    }
                    else tex.SetPixel(x, y, new Color(0.2f, 0.2f, 0.2f));
                }
            }
            tex.Apply();
            return tex;
        }

        // Standard Bridge Methods
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string path) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), path);
#endif
        public void FromUIDocument(string path) => _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(path));
    }
}