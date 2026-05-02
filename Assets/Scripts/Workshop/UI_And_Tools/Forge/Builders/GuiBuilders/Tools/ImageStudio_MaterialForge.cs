using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;

#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    public class ImageStudio_MaterialForge : IGuiProvider
    {
        public string Title => "Material Forge";

        // --- Conceptual Ontology State ---
        private string _baseSubstance = "Metal";
        private string _surfaceFinish = "Smooth";
        private Color _tintColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        // NEW: Chronology & Exposure State
        private float _artifactAgeYears = 0f;
        private string _exposureEnvironment = "Pristine (None)";

        // --- Memory & Previews ---
        private Material _activeMaterial;
        private GameObject _previewModel;
        private LiveModelPreviewBuilder _livePreviewBuilder;
        private VisualElement _flatPreviewSwatch;

        public ImageStudio_MaterialForge()
        {
            InitializeProceduralMaterial();
        }

        private void InitializeProceduralMaterial()
        {
            _activeMaterial = new Material(Shader.Find("Standard"));
            _activeMaterial.name = "Concept_Material";
            UpdateMaterialFromConcepts();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var theme = GuiSkin.Active?.GlobalDefault;
            Color bgColor = theme != null ? theme.BackgroundColor : new Color(0.1f, 0.1f, 0.1f);
            Color fgColor = theme != null ? theme.TextColor : Color.white;

            var rootBuilder = new GraphicalUserInterfaceBuilder("MaterialForgeRoot")
                .WithAutoGrow()
                .WithFlexLayout(FlexDirection.Row)
                .WithBackgroundColor(bgColor)
                .WithPadding(15);

            // ==========================================
            // LEFT PANEL: The Conceptual Configurator
            // ==========================================
            var leftPanel = new GraphicalUserInterfaceBuilder("OntologyPanel")
                .WithWidth(350)
                .WithMarginRight(20)
                .WithBorderRightWidth(2)
                .WithBorderRightColor(fgColor)
                .WithPaddingRight(15)
                .WithScrollable(true); // Made scrollable for the new data

            leftPanel.AddChild(new ForgeLabelBuilder("MATERIAL ONTOLOGY")
                .WithFontSize(18).WithFontStyle(FontStyle.Bold).WithColor(fgColor)
                .WithMarginBottom(20).Build(ctx));

            // --- 1. SIMILAR TO (Cloning) ---
            leftPanel.AddChild(new ForgeLabelBuilder("1. Inheritance (Similar To...)")
                .WithFontSize(14).WithColor(fgColor).WithMarginBottom(5).Build(ctx));

            leftPanel.AddChild(context => {
                var cloneContainer = new GraphicalUserInterfaceBuilder("CloneContainer")
                    .WithFlexLayout(FlexDirection.Row).WithMarginBottom(15).Build(ctx);

#if UNITY_EDITOR
                var matField = new ObjectField { objectType = typeof(Material), allowSceneObjects = true, style = { flexGrow = 1 } };
                matField.RegisterValueChangedCallback(evt => {
                    if (evt.newValue is Material clonedMat)
                    {
                        _activeMaterial.CopyPropertiesFromMaterial(clonedMat);
                        _tintColor = _activeMaterial.color;
                        RefreshPreviews();
                    }
                });
                cloneContainer.Add(matField);
#else
                cloneContainer.Add(new ForgeLabelBuilder("[Runtime Material Picker Stub]").WithColor(Color.gray).Build(ctx));
#endif
                return cloneContainer;
            });

            // --- 2. BASE SUBSTANCE ---
            leftPanel.AddChild(new ForgeLabelBuilder("2. Base Substance")
                .WithFontSize(14).WithColor(fgColor).WithMarginBottom(5).Build(ctx));

            var substanceChoices = new List<string> { "Plastic", "Metal", "Wood", "Stone", "Fabric", "Glass", "Flesh" };
            leftPanel.AddDropdownData("Substance", substanceChoices, substanceChoices.IndexOf(_baseSubstance), val => {
                _baseSubstance = val; UpdateMaterialFromConcepts();
            });

            // --- 3. SURFACE FINISH ---
            leftPanel.AddChild(new ForgeLabelBuilder("3. Surface Finish & Treatment")
                .WithFontSize(14).WithColor(fgColor).WithMarginTop(10).WithMarginBottom(5).Build(ctx));

            var finishChoices = new List<string> { "Smooth", "Rough", "Polished", "Painted", "Wet" };
            leftPanel.AddDropdownData("Finish", finishChoices, finishChoices.IndexOf(_surfaceFinish), val => {
                _surfaceFinish = val; UpdateMaterialFromConcepts();
            });

            // --- 4. COLORATION ---
            leftPanel.AddChild(new ForgeLabelBuilder("4. Base Coloration")
                .WithFontSize(14).WithColor(fgColor).WithMarginTop(10).WithMarginBottom(5).Build(ctx));

            leftPanel.AddChild(context => {
                var colorContainer = new GraphicalUserInterfaceBuilder("ColorSwatches")
                    .WithFlexLayout(FlexDirection.Row).WithFlexWrap(Wrap.Wrap).WithMarginBottom(15).Build(ctx);

                Color[] swatches = { Color.white, Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.black, new Color(0.4f, 0.2f, 0.1f) };
                foreach (var c in swatches)
                {
                    var btn = new GraphicalUserInterfaceBuilder("Swatch").WithSize(30, 30).WithBackgroundColor(c).WithMargin(2).WithBorderWidth(1).WithBorderColor(Color.gray).Build(ctx);
                    btn.RegisterCallback<PointerDownEvent>(e => { _tintColor = c; UpdateMaterialFromConcepts(); });
                    colorContainer.Add(btn);
                }
                return colorContainer;
            });

            // --- 5. CHRONOLOGY & EXPOSURE (THE NEW MAGIC) ---
            leftPanel.AddChild(new ForgeLabelBuilder("5. Chronology & Exposure")
                .WithFontSize(14).WithColor(GuiSkin.Active?.PrimaryAccent ?? Color.cyan).WithMarginTop(10).WithMarginBottom(5).Build(ctx));

            // Use a logarithmic slider for age so they can choose 1 year or 10,000 years easily
            leftPanel.AddChild(context => {
                var ageSlider = new Slider("Age (Years)", 0f, 10000f) { value = _artifactAgeYears, style = { color = fgColor, marginBottom = 5 } };
                ageSlider.RegisterValueChangedCallback(evt => {
                    _artifactAgeYears = evt.newValue;
                    UpdateMaterialFromConcepts();
                });
                return ageSlider;
            });

            var exposureChoices = new List<string> { "Pristine (None)", "Desert Sand", "Swamp Muck", "Ocean Floor", "Deep Space Radiation" };
            leftPanel.AddDropdownData("Burial / Exposure", exposureChoices, exposureChoices.IndexOf(_exposureEnvironment), val => {
                _exposureEnvironment = val;
                UpdateMaterialFromConcepts();
            });

            rootBuilder.AddChild(leftPanel);

            // ==========================================
            // RIGHT PANEL: The Observation Deck
            // ==========================================
            var rightPanel = new GraphicalUserInterfaceBuilder("PreviewPanel")
                .WithAutoGrow()
                .WithFlexLayout(FlexDirection.Column);

            rightPanel.AddChild(new ForgeLabelBuilder("MATERIAL OBSERVATION DECK")
                .WithFontSize(18).WithFontStyle(FontStyle.Bold).WithColor(fgColor).WithMarginBottom(10).Build(ctx));

            _previewModel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _previewModel.SetActive(false);
            _previewModel.GetComponent<Renderer>().sharedMaterial = _activeMaterial;

            _livePreviewBuilder = new LiveModelPreviewBuilder(_previewModel)
                .WithAutoRotate(true, 20f).WithMouseControl(true).WithZoom(2.5f);

            var previewVisual = _livePreviewBuilder.CreateGui(ctx);
            previewVisual.style.flexGrow = 1;
            previewVisual.style.minHeight = 300;
            previewVisual.style.borderTopWidth = 2; previewVisual.style.borderBottomWidth = 2;
            previewVisual.style.borderLeftWidth = 2; previewVisual.style.borderRightWidth = 2;
            previewVisual.style.borderTopColor = fgColor; previewVisual.style.borderBottomColor = fgColor;
            previewVisual.style.borderLeftColor = fgColor; previewVisual.style.borderRightColor = fgColor;
            previewVisual.style.borderTopLeftRadius = 8; previewVisual.style.borderTopRightRadius = 8;
            rightPanel.AddChild(previewVisual);

            rightPanel.AddChild(context => {
                var flatContainer = new GraphicalUserInterfaceBuilder("FlatPreviewContainer").WithFlexLayout(FlexDirection.Row).WithMarginTop(10).WithHeight(100).Build(ctx);
                var labelBox = new GraphicalUserInterfaceBuilder("FlatLabels").WithAutoGrow().WithFlexLayout(FlexDirection.Column, Justify.Center).Build(ctx);
                labelBox.Add(new ForgeLabelBuilder("Flat Application Profile").WithColor(fgColor).Build(ctx));
                labelBox.Add(new ForgeLabelBuilder("Shows albedo after aging and environmental exposure calculations.").WithColor(Color.gray).WithFontSize(10).Build(ctx));

                _flatPreviewSwatch = new GraphicalUserInterfaceBuilder("FlatSwatch").WithSize(100, 100).WithBackgroundColor(_tintColor).WithBorderWidth(2).WithBorderColor(fgColor).WithBorderBottomLeftRadius(8).WithBorderBottomRightRadius(8).Build(ctx);
                flatContainer.Add(labelBox); flatContainer.Add(_flatPreviewSwatch);
                return flatContainer;
            });

            rootBuilder.AddChild(rightPanel);

            var root = rootBuilder.Build(ctx);
            root.RegisterCallback<DetachFromPanelEvent>(evt => ScrubMemory());
            return root;
        }

        private void UpdateMaterialFromConcepts()
        {
            if (_activeMaterial == null) return;

            // 1. Core Mathematical Properties
            float metallic = 0f;
            float smoothness = 0.5f;
            Color finalColor = _tintColor;

            // 2. Base Substance Layer
            switch (_baseSubstance)
            {
                case "Metal": metallic = 1.0f; smoothness = 0.6f; break;
                case "Wood": metallic = 0.0f; smoothness = 0.2f; break;
                case "Stone": metallic = 0.0f; smoothness = 0.1f; break;
                case "Plastic": metallic = 0.0f; smoothness = 0.7f; break;
                case "Fabric": metallic = 0.0f; smoothness = 0.0f; break;
                case "Glass": metallic = 0.8f; smoothness = 0.9f; break;
                case "Flesh": metallic = 0.0f; smoothness = 0.3f; break;
            }

            // 3. Surface Finish Layer
            switch (_surfaceFinish)
            {
                case "Smooth": break;
                case "Polished": smoothness = Mathf.Clamp01(smoothness + 0.4f); break;
                case "Rough": smoothness = Mathf.Clamp01(smoothness - 0.4f); break;
                case "Painted": metallic *= 0.1f; smoothness = 0.4f; break;
                case "Wet": smoothness = 0.95f; finalColor = Color.Lerp(finalColor, Color.black, 0.2f); break;
            }

            // 4. CHRONOLOGY & EXPOSURE LAYER (The Ravages of Chronos)
            // Calculate an intensity curve. 10 years is light, 1000 years is heavy, 10000 is ruined.
            float ageFactor = Mathf.Clamp01(Mathf.Log10(_artifactAgeYears + 1) / 4f);

            if (ageFactor > 0.01f)
            {
                switch (_exposureEnvironment)
                {
                    case "Desert Sand":
                        // Sand blasts away smoothness, dulls metal, and tints beige
                        smoothness = Mathf.Lerp(smoothness, 0.1f, ageFactor * 0.8f);
                        metallic = Mathf.Lerp(metallic, 0.0f, ageFactor * 0.5f);
                        Color sandColor = new Color(0.76f, 0.7f, 0.5f);
                        finalColor = Color.Lerp(finalColor, sandColor, ageFactor * 0.7f);
                        break;

                    case "Swamp Muck":
                        // Swamp rusts metal aggressively, adds murky green/brown rot, but keeps it somewhat wet/slick
                        if (_baseSubstance == "Metal") metallic = Mathf.Lerp(metallic, 0.1f, ageFactor);
                        else smoothness = Mathf.Lerp(smoothness, 0.2f, ageFactor * 0.5f);

                        Color muckColor = new Color(0.2f, 0.25f, 0.15f);
                        finalColor = Color.Lerp(finalColor, muckColor, ageFactor * 0.8f);
                        break;

                    case "Ocean Floor":
                        // Ocean floor covers things in barnacles/coral (high roughness, erratic colors)
                        smoothness = Mathf.Lerp(smoothness, 0.0f, ageFactor);
                        metallic = Mathf.Lerp(metallic, 0.0f, ageFactor);
                        Color coralColor = new Color(0.4f, 0.5f, 0.4f); // Coraline algae tint
                        finalColor = Color.Lerp(finalColor, coralColor, ageFactor * 0.9f);
                        break;

                    case "Deep Space Radiation":
                        // Space bleaches color out (desaturation) and micro-meteorites pit the surface
                        smoothness = Mathf.Lerp(smoothness, 0.3f, ageFactor * 0.6f);

                        // Desaturate the color (sun bleaching)
                        float gray = finalColor.grayscale;
                        finalColor = Color.Lerp(finalColor, new Color(gray, gray, gray), ageFactor * 0.9f);
                        break;
                }
            }

            // Apply Final Compiled Math to Engine
            _activeMaterial.color = finalColor;
            _activeMaterial.SetFloat("_Metallic", metallic);
            _activeMaterial.SetFloat("_Glossiness", smoothness);

            if (_flatPreviewSwatch != null) _flatPreviewSwatch.style.backgroundColor = finalColor;
        }

        private void RefreshPreviews()
        {
            if (_flatPreviewSwatch != null && _activeMaterial != null)
                _flatPreviewSwatch.style.backgroundColor = _activeMaterial.color;
        }

        private void ScrubMemory()
        {
            _livePreviewBuilder?.Dispose();
            if (_previewModel != null) UnityEngine.Object.DestroyImmediate(_previewModel);
            if (_activeMaterial != null) UnityEngine.Object.DestroyImmediate(_activeMaterial);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}