// File: Assets/Scripts/Workshop/Forge/Builders/GuiBuilders/Workshop_Gui_ThemeEditor.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class Workshop_Gui_ThemeEditor : IGuiProvider
    {
        public string Title => "Singularity Theme Synthesizer";

        private List<GuiTheme> _loadedThemes = new List<GuiTheme>();
        private GuiTheme _activeTheme; // Injected by CRUD Builder during edit
        private GuiElementType _activeElementType = GuiElementType.Panel;

        // Dynamic Zones managed by the Editor Form
        private VisualElement _microTuningZone;
        private VisualElement _atomicPreviewDummy;
        private VisualElement _matrixZone;

        private List<Type> _availableGuiProviders = new List<Type>();

        public Workshop_Gui_ThemeEditor()
        {
            LoadThemes();
            LoadPreviewableGuis();
        }

        private void LoadThemes()
        {
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("t:GuiTheme");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                _loadedThemes.Add(AssetDatabase.LoadAssetAtPath<GuiTheme>(path));
            }
#endif
            if (_loadedThemes.Count == 0)
            {
                _loadedThemes.Add(GetVisionaryFallbackTheme());
            }
        }

        private void LoadPreviewableGuis()
        {
            var type = typeof(IGuiProvider);
            _availableGuiProviders = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract && p != typeof(Workshop_Gui_ThemeEditor))
                .Take(6).ToList();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. Instantiating the Ecosystem CRUD_Builder
            var crudProvider = new CRUD_Builder<GuiTheme>(
                title: "THEME CORES",
                dataSource: () => _loadedThemes,
                getDisplayName: theme => string.IsNullOrEmpty(theme.name) ? "New Synthesized Theme" : theme.name,
                buildEditorForm: BuildEditorForm, // Passes the active theme to our custom builder
                onSave: SaveTheme,
                onDelete: DeleteTheme,
                getSubtitle: theme => "Aesthetic Profile"
            );

            // 2. Override CRUD styling to match the High-Contrast Synthesizer vibe
            crudProvider.Style.RootBackground = new Color(0.05f, 0.05f, 0.06f);
            crudProvider.Style.ListBackground = new Color(0.08f, 0.08f, 0.09f);
            crudProvider.Style.SelectedItemBackground = new Color(0.12f, 0.12f, 0.15f);
            crudProvider.Style.AccentColor = new Color(1f, 0f, 0.5f, 1f); // Cyber Magenta
            crudProvider.Style.ListWidth = 250f;

            return crudProvider.CreateGui(ctx);
        }

        // --- THE EDITOR FORM GENERATOR ---
        private VisualElement BuildEditorForm(GuiTheme theme)
        {
            _activeTheme = theme;

            // Pre-seed properties if this is a completely blank "New" object from CRUD
            if (string.IsNullOrEmpty(theme.name))
            {
                theme.name = $"Synthesized_Theme_{_loadedThemes.Count + 1}";
                var fallback = GetVisionaryFallbackTheme();
                theme.PrimaryAccent = fallback.PrimaryAccent;
                theme.SecondaryAccent = fallback.SecondaryAccent;
                theme.AlertColor = fallback.AlertColor;
                theme.GlobalDefault = fallback.GlobalDefault.Clone();
            }

            var container = new GraphicalUserInterfaceBuilder("ThemeFormRoot")
                .WithFlexLayout(FlexDirection.Column)
                .WithFlexGrow(1f)
                .Build();

            // 1. Asset Naming & Global Palette
            var headerBuilder = new GraphicalUserInterfaceBuilder("HeaderSection")
                .AddStringData("Asset ID (Name)", _activeTheme.name, v => _activeTheme.name = v)
                .AddHeader("GLOBAL SIGNAL PALETTE", Color.gray)
                .AddChild(new GraphicalUserInterfaceBuilder("PaletteRow")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                    .WithPaddingBottom(15).WithBorderBottomWidth(1).WithBorderBottomColor(new Color(0.2f, 0.2f, 0.2f))
                    .AddChild(CreateColorSwatch("Primary", _activeTheme.PrimaryAccent, c => { _activeTheme.PrimaryAccent = c; FullPreviewRefresh(); }))
                    .AddChild(CreateColorSwatch("Secondary", _activeTheme.SecondaryAccent, c => { _activeTheme.SecondaryAccent = c; FullPreviewRefresh(); }))
                    .AddChild(CreateColorSwatch("Alert", _activeTheme.AlertColor, c => { _activeTheme.AlertColor = c; FullPreviewRefresh(); }))
                );
            container.Add(headerBuilder.Build());

            // 2. Micro-Tuning Zone (Will be dynamically cleared and rebuilt when Dropdown changes)
            _microTuningZone = new VisualElement { style = { flexGrow = 1, marginTop = 15 } };
            container.Add(_microTuningZone);
            RebuildMicroTuningZone(); // Populate it

            // 3. Matrix Zone (Data Cards)
            _matrixZone = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, marginTop = 20, paddingTop = 20, borderTopWidth = 1, borderTopColor = new Color(0.2f, 0.2f, 0.2f) } };
            container.Add(new Label("HOLO-DECK MATRIX PREVIEW") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 20 } });
            container.Add(_matrixZone);
            RebuildMatrixZone(); // Populate it

            return container;
        }

        private void RebuildMicroTuningZone()
        {
            _microTuningZone.Clear();
            var profile = _activeTheme.GetOrCreateProfile(_activeElementType);

            var splitBuilder = new GraphicalUserInterfaceBuilder("ForgeSplit")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch);

            // LEFT SIDE: The Sliders
            var controls = new GraphicalUserInterfaceBuilder("Controls")
                .WithWidth(350).WithPaddingRight(20)
                .AddHeader("ATOMIC ELEMENT MICRO-TUNING", _activeTheme.PrimaryAccent)
                // When we change elements, rebuild this exact zone to show the new data values!
                .AddEnumData("Target Class", _activeElementType, v => { _activeElementType = v; RebuildMicroTuningZone(); })

                .AddHeader("CORNER RADII", Color.gray)
                .AddFloatData("Top Left", profile.BorderTopLeftRadius, v => { profile.BorderTopLeftRadius = v; FullPreviewRefresh(); })
                .AddFloatData("Top Right", profile.BorderTopRightRadius, v => { profile.BorderTopRightRadius = v; FullPreviewRefresh(); })
                .AddFloatData("Bottom Right", profile.BorderBottomRightRadius, v => { profile.BorderBottomRightRadius = v; FullPreviewRefresh(); })
                .AddFloatData("Bottom Left", profile.BorderBottomLeftRadius, v => { profile.BorderBottomLeftRadius = v; FullPreviewRefresh(); })

                .AddHeader("BORDER WEIGHTS", Color.gray)
                .AddFloatData("Top", profile.BorderTopWidth, v => { profile.BorderTopWidth = v; FullPreviewRefresh(); })
                .AddFloatData("Right", profile.BorderRightWidth, v => { profile.BorderRightWidth = v; FullPreviewRefresh(); })
                .AddFloatData("Bottom", profile.BorderBottomWidth, v => { profile.BorderBottomWidth = v; FullPreviewRefresh(); })
                .AddFloatData("Left", profile.BorderLeftWidth, v => { profile.BorderLeftWidth = v; FullPreviewRefresh(); });

            // RIGHT SIDE: The Live Hovering Dummy
            var previewCol = new GraphicalUserInterfaceBuilder("PreviewCol")
                .WithFlexGrow(1f).WithHeight(300)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                .WithBorderAllColor(new Color(0.2f, 0.2f, 0.2f)).WithBorderWidth(1)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            _atomicPreviewDummy = new VisualElement();

            var previewRoot = previewCol.Build();
            previewRoot.style.backgroundImage = CreateGridTexture();
            previewRoot.Add(_atomicPreviewDummy);

            splitBuilder.AddChild(controls);
            splitBuilder.AddChild(previewRoot);

            _microTuningZone.Add(splitBuilder.Build());

            // Add Colors dynamically below the sliders if in Editor
#if UNITY_EDITOR
            var colorBuilder = new GraphicalUserInterfaceBuilder("MaterialColors").WithMarginTop(10)
                .AddChild(new Label("Material Colors:") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } })
                .AddChild(CreateNativeColorField("Background", profile.BackgroundColor, c => { profile.BackgroundColor = c; FullPreviewRefresh(); }))
                .AddChild(CreateNativeColorField("Border", profile.BorderColor, c => { profile.BorderColor = c; FullPreviewRefresh(); }))
                .AddChild(CreateNativeColorField("Text/Emission", profile.TextColor, c => { profile.TextColor = c; FullPreviewRefresh(); }));
            _microTuningZone.ElementAt(0).ElementAt(0).Add(colorBuilder.Build());
#endif

            RefreshAtomicDummy();
        }

        private void FullPreviewRefresh()
        {
            RefreshAtomicDummy();
            RebuildMatrixZone();
        }

        private void RefreshAtomicDummy()
        {
            if (_atomicPreviewDummy == null) return;
            _atomicPreviewDummy.Clear();

            var profile = _activeTheme.GetProfile(_activeElementType);

            _atomicPreviewDummy.style.backgroundColor = profile.BackgroundColor;
            _atomicPreviewDummy.style.borderTopColor = profile.BorderColor; _atomicPreviewDummy.style.borderBottomColor = profile.BorderColor;
            _atomicPreviewDummy.style.borderLeftColor = profile.BorderColor; _atomicPreviewDummy.style.borderRightColor = profile.BorderColor;

            _atomicPreviewDummy.style.borderTopWidth = profile.BorderTopWidth; _atomicPreviewDummy.style.borderBottomWidth = profile.BorderBottomWidth;
            _atomicPreviewDummy.style.borderLeftWidth = profile.BorderLeftWidth; _atomicPreviewDummy.style.borderRightWidth = profile.BorderRightWidth;

            _atomicPreviewDummy.style.borderTopLeftRadius = profile.BorderTopLeftRadius; _atomicPreviewDummy.style.borderTopRightRadius = profile.BorderTopRightRadius;
            _atomicPreviewDummy.style.borderBottomLeftRadius = profile.BorderBottomLeftRadius; _atomicPreviewDummy.style.borderBottomRightRadius = profile.BorderBottomRightRadius;

            if (_activeElementType == GuiElementType.Panel || _activeElementType == GuiElementType.Window)
            {
                _atomicPreviewDummy.style.width = 250; _atomicPreviewDummy.style.height = 150;
                _atomicPreviewDummy.Add(new Label("Atomic Content Simulation") { style = { color = profile.TextColor, unityTextAlign = TextAnchor.MiddleCenter, flexGrow = 1 } });
            }
            else
            {
                _atomicPreviewDummy.style.width = 180; _atomicPreviewDummy.style.height = 40;
                _atomicPreviewDummy.style.justifyContent = Justify.Center;
                _atomicPreviewDummy.Add(new Label(_activeElementType.ToString().ToUpper()) { style = { color = profile.TextColor, unityTextAlign = TextAnchor.MiddleCenter, unityFontStyleAndWeight = FontStyle.Bold } });
            }
        }

        private void RebuildMatrixZone()
        {
            if (_matrixZone == null) return;
            _matrixZone.Clear();

            var panelProfile = _activeTheme.GetProfile(GuiElementType.Panel);
            var headerProfile = _activeTheme.GetProfile(GuiElementType.Header);

            foreach (var providerType in _availableGuiProviders)
            {
                IGuiProvider providerInstance;
                try { providerInstance = (IGuiProvider)Activator.CreateInstance(providerType); } catch { continue; }

                var dataCard = new VisualElement
                {
                    style = {
                        width = 360, height = 280, marginTop = 15, marginBottom = 15, marginRight = 15, overflow = Overflow.Hidden,

                        backgroundColor = panelProfile.BackgroundColor,
                        borderTopColor = panelProfile.BorderColor, borderBottomColor = panelProfile.BorderColor, borderLeftColor = panelProfile.BorderColor, borderRightColor = panelProfile.BorderColor,
                        
                        // Extruded Block borders!
                        borderTopWidth = panelProfile.BorderTopWidth, borderBottomWidth = panelProfile.BorderBottomWidth,
                        borderLeftWidth = panelProfile.BorderLeftWidth, borderRightWidth = panelProfile.BorderRightWidth,
                        borderTopLeftRadius = panelProfile.BorderTopLeftRadius, borderTopRightRadius = panelProfile.BorderTopRightRadius,
                        borderBottomLeftRadius = panelProfile.BorderBottomLeftRadius, borderBottomRightRadius = panelProfile.BorderBottomRightRadius
                    }
                };

                var cardHeader = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, paddingTop = 10, paddingRight = 10, paddingLeft = 10, paddingBottom = 10,
                        backgroundColor = headerProfile.BackgroundColor, borderBottomWidth = headerProfile.BorderBottomWidth, borderBottomColor = _activeTheme.PrimaryAccent } };
                cardHeader.Add(new Label(providerInstance.Title ?? providerType.Name) { style = { color = headerProfile.TextColor, unityFontStyleAndWeight = FontStyle.Bold } });
                dataCard.Add(cardHeader);

                var nestedContainer = new VisualElement { style = { flexGrow = 1, paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10, } };
                try { nestedContainer.Add(providerInstance.CreateGui(new GuiContext())); }
                catch (Exception ex) { nestedContainer.Add(new Label($"Render Error:\n{ex.Message}") { style = { color = _activeTheme.AlertColor } }); }
                dataCard.Add(nestedContainer);

                _matrixZone.Add(dataCard);
            }
        }

        // --- CRUD PERSISTENCE DELEGATES ---
        private void SaveTheme(GuiTheme theme)
        {
#if UNITY_EDITOR
            if (!AssetDatabase.Contains(theme))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
                if (!AssetDatabase.IsValidFolder("Assets/Resources/Themes")) AssetDatabase.CreateFolder("Assets/Resources", "Themes");

                // Save new SO into the project
                AssetDatabase.CreateAsset(theme, $"Assets/Resources/Themes/{theme.name}.asset");
                if (!_loadedThemes.Contains(theme)) _loadedThemes.Add(theme);
            }
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
#endif
            Debug.Log($"[Theme Synthesizer] Persisted {theme.name} to Database.");
        }

        private void DeleteTheme(GuiTheme theme)
        {
#if UNITY_EDITOR
            if (AssetDatabase.Contains(theme))
            {
                string path = AssetDatabase.GetAssetPath(theme);
                AssetDatabase.DeleteAsset(path);
            }
#endif
            _loadedThemes.Remove(theme);
            Debug.Log($"[Theme Synthesizer] Extracted {theme.name} from Database.");
        }

        // --- UTILITIES ---
        private static GuiTheme GetVisionaryFallbackTheme()
        {
            var visionary = ScriptableObject.CreateInstance<GuiTheme>();
            visionary.name = "Singularity_Asymmetrical_Core";

            visionary.PrimaryAccent = new Color(1f, 0f, 0.5f, 1f);
            visionary.SecondaryAccent = new Color(0f, 1f, 1f, 1f);
            visionary.AlertColor = new Color(1f, 0.9f, 0.1f, 1f);

            // The Panel - Extruded Block Vibe
            visionary.Profiles.Add(new StyleProfile
            {
                TargetType = GuiElementType.Panel,
                BackgroundColor = new Color(0.04f, 0.03f, 0.06f, 0.98f),
                BorderColor = visionary.SecondaryAccent,
                TextColor = new Color(0.95f, 0.95f, 0.95f, 1f),
                BorderTopWidth = 1f,
                BorderRightWidth = 1f,
                BorderBottomWidth = 4f,
                BorderLeftWidth = 4f,
                CornerRadius = 0f
            });

            // The Button - Asymmetrical Opposites
            visionary.Profiles.Add(new StyleProfile
            {
                TargetType = GuiElementType.ButtonPrimary,
                BackgroundColor = visionary.PrimaryAccent,
                BorderColor = visionary.PrimaryAccent,
                TextColor = Color.white,
                BorderTopWidth = 1f,
                BorderRightWidth = 1f,
                BorderBottomWidth = 3f,
                BorderLeftWidth = 3f,
                BorderTopLeftRadius = 15f,
                BorderBottomRightRadius = 15f,
                BorderTopRightRadius = 0f,
                BorderBottomLeftRadius = 0f
            });

            return visionary;
        }

#if UNITY_EDITOR
        private VisualElement CreateNativeColorField(string label, Color value, Action<Color> onChanged)
        {
            var field = new ColorField(label) { value = value };
            field.RegisterValueChangedCallback(e => onChanged(e.newValue));
            field.style.marginBottom = 5;
            return field;
        }
#endif

        private VisualElement CreateColorSwatch(string label, Color value, Action<Color> onChanged)
        {
            var box = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginRight = 15 } };
            box.Add(new Label(label + ":") { style = { color = Color.white, marginRight = 5 } });
            var colorSwatch = new VisualElement { style = { width = 40, height = 20, backgroundColor = value, borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1,
                    borderTopColor = Color.white, borderBottomColor = Color.white, borderLeftColor = Color.white, borderRightColor = Color.white, borderTopLeftRadius = 3 } };
            box.Add(colorSwatch);
            return box;
        }

        private Texture2D CreateGridTexture()
        {
            var tex = new Texture2D(64, 64);
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                    tex.SetPixel(x, y, (x == 0 || y == 0) ? new Color(1, 1, 1, 0.05f) : new Color(0, 0, 0, 0));
            tex.Apply();
            return tex;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}