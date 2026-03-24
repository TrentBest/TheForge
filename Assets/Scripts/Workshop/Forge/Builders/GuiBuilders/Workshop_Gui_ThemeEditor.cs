using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif
using TheSingularityWorkshop.Forge.Builders.GuiBuilders.Themes;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class Workshop_Gui_ThemeEditor : IGuiProvider
    {
        public string Title => "Singularity Theme Synthesizer";

        private List<GuiTheme> _loadedThemes = new List<GuiTheme>();
        private GuiTheme _activeTheme;
        private GuiElementType _activeElementType = GuiElementType.Panel;

        // UI Zones
        private VisualElement _editorZone;
        private VisualElement _atomicPreviewZone;
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
            // Dynamically load all SOs in the project for the editor
            string[] guids = AssetDatabase.FindAssets("t:GuiTheme");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                _loadedThemes.Add(AssetDatabase.LoadAssetAtPath<GuiTheme>(path));
            }
#endif
            // Fallback for runtime/testing if none exist
            if (_loadedThemes.Count == 0)
            {
                var fallback = ScriptableObject.CreateInstance<GuiTheme>();
                fallback.name = "Runtime_Holo_Dark";
                _loadedThemes.Add(fallback);
            }
            _activeTheme = _loadedThemes[0];
        }

        private void LoadPreviewableGuis()
        {
            var type = typeof(IGuiProvider);
            _availableGuiProviders = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract && p != typeof(Workshop_Gui_ThemeEditor))
                .Take(6).ToList(); // Limit to 6 for the preview matrix
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1, backgroundColor = new Color(0.05f, 0.05f, 0.06f) } };

            // --- 1. THE CORE MONOLITH (THEME SELECTOR) ---
            var sidebar = new GraphicalUserInterfaceBuilder("ThemeMonolith")
                .WithWidth(220).WithPadding(15)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.09f))
                .WithBorderRightWidth(1).WithBorderRightColor(new Color(0.2f, 0.2f, 0.2f))
                .AddHeader("THEME CORES", Color.gray)
                .AddSeparator(Color.gray, 2);

            var themeScroll = new ScrollView { style = { flexGrow = 1, marginTop = 10 } };
            foreach (var theme in _loadedThemes)
            {
                var btn = new Button(() => SelectTheme(theme)) { text = theme.name };
                // Shock & Awe Style: Stark, geometric buttons
                btn.style.backgroundColor = Color.clear;
                btn.style.color = Color.white;
                btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                btn.style.borderLeftWidth = _activeTheme == theme ? 4 : 0;
                btn.style.borderLeftColor = theme.PrimaryAccent;
                btn.style.paddingLeft = 10;
                themeScroll.Add(btn);
            }
            sidebar.AddChild(themeScroll);
            sidebar.AddButton("+ SYNTHESIZE NEW", () => Debug.Log("TODO: SO Creation"));
            root.Add(sidebar.Build());

            // --- 2. THE ATOMIC FORGE (PROPERTY EDITOR) ---
            var forgeZone = new VisualElement { style = { width = 350, borderRightWidth = 1, borderRightColor = new Color(0.2f, 0.2f, 0.2f), backgroundColor = new Color(0.1f, 0.1f, 0.12f) } };

            // The Atomic Preview (Hovers above the controls)
            _atomicPreviewZone = new VisualElement { style = { height = 200, alignItems = Align.Center, justifyContent = Justify.Center, borderBottomWidth = 1, borderBottomColor = new Color(0.2f, 0.2f, 0.2f), backgroundImage = CreateGridTexture() } };
            forgeZone.Add(_atomicPreviewZone);

            _editorZone = new ScrollView { style = { flexGrow = 1, paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15 } };
            forgeZone.Add(_editorZone);
            root.Add(forgeZone);

            // --- 3. THE HOLO-DECK (DATA CARD MATRIX) ---
            var matrixContainer = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };

            // Global Palette Header
            var paletteHeader = new VisualElement { style = { flexDirection = FlexDirection.Row, paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15, borderBottomWidth = 1, borderBottomColor = new Color(0.2f, 0.2f, 0.2f), backgroundColor = new Color(0.08f, 0.08f, 0.08f) } };
            paletteHeader.Add(new Label("GLOBAL SIGNAL PALETTE") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold, marginRight = 20, alignSelf = Align.Center } });
            paletteHeader.Add(CreateGlobalColorField("Primary", _activeTheme.PrimaryAccent, c => { _activeTheme.PrimaryAccent = c; FullRefresh(); }));
            paletteHeader.Add(CreateGlobalColorField("Secondary", _activeTheme.SecondaryAccent, c => { _activeTheme.SecondaryAccent = c; FullRefresh(); }));
            paletteHeader.Add(CreateGlobalColorField("Alert", _activeTheme.AlertColor, c => { _activeTheme.AlertColor = c; FullRefresh(); }));
            matrixContainer.Add(paletteHeader);

            _matrixZone = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, paddingTop = 20, paddingRight = 20, paddingLeft = 20, paddingBottom = 20 } };

            // FIXED: Explicitly assigning styles without nested object initializer
            var matrixScroll = new ScrollView { style = { flexGrow = 1 } };
            matrixScroll.contentContainer.style.flexGrow = 1;
            matrixScroll.Add(_matrixZone);
            matrixContainer.Add(matrixScroll);

            root.Add(matrixContainer);

            FullRefresh();
            return root;
        }

        private void SelectTheme(GuiTheme theme)
        {
            _activeTheme = theme;
            FullRefresh();
        }

        private void FullRefresh()
        {
            BuildEditorZone();
            RefreshAtomicPreview();
            BuildMatrixZone();
        }

        private void BuildEditorZone()
        {
            _editorZone.Clear();
            var profile = _activeTheme.GetOrCreateProfile(_activeElementType);

            var builder = new GraphicalUserInterfaceBuilder("Properties")
                .AddHeader("ATOMIC ELEMENT", _activeTheme.PrimaryAccent)
                .AddEnumData("Target Class", _activeElementType, v => { _activeElementType = v; FullRefresh(); })
                .AddSeparator(Color.gray, 1)
                .AddFloatData("Corner Radius", profile.CornerRadius, v => { profile.CornerRadius = v; RefreshAtomicPreview(); BuildMatrixZone(); })
                .AddFloatData("Border Width", profile.BorderWidth, v => { profile.BorderWidth = v; RefreshAtomicPreview(); BuildMatrixZone(); });

            _editorZone.Add(builder.Build());

            // Add UI Toolkit native ColorFields for precise RGBA/Hex control
#if UNITY_EDITOR
            _editorZone.Add(new Label("Material Colors:") { style = { color = Color.gray, marginTop = 15, marginBottom = 5 } });
            _editorZone.Add(CreateNativeColorField("Background", profile.BackgroundColor, c => { profile.BackgroundColor = c; RefreshAtomicPreview(); BuildMatrixZone(); }));
            _editorZone.Add(CreateNativeColorField("Border", profile.BorderColor, c => { profile.BorderColor = c; RefreshAtomicPreview(); BuildMatrixZone(); }));
            _editorZone.Add(CreateNativeColorField("Text/Emission", profile.TextColor, c => { profile.TextColor = c; RefreshAtomicPreview(); BuildMatrixZone(); }));
#endif
        }

        private void RefreshAtomicPreview()
        {
            _atomicPreviewZone.Clear();
            var profile = _activeTheme.GetProfile(_activeElementType);

            // We construct a dummy element that visually represents the exact StyleProfile
            var atomicDummy = new VisualElement();

            // Apply the physical traits of the profile
            atomicDummy.style.backgroundColor = profile.BackgroundColor;
            atomicDummy.style.borderTopColor = profile.BorderColor;
            atomicDummy.style.borderBottomColor = profile.BorderColor;
            atomicDummy.style.borderLeftColor = profile.BorderColor;
            atomicDummy.style.borderRightColor = profile.BorderColor;
            atomicDummy.style.borderTopWidth = profile.BorderWidth;
            atomicDummy.style.borderBottomWidth = profile.BorderWidth;
            atomicDummy.style.borderLeftWidth = profile.BorderWidth;
            atomicDummy.style.borderRightWidth = profile.BorderWidth;
            atomicDummy.style.borderTopLeftRadius = profile.CornerRadius;
            atomicDummy.style.borderTopRightRadius = profile.CornerRadius;
            atomicDummy.style.borderBottomLeftRadius = profile.CornerRadius;
            atomicDummy.style.borderBottomRightRadius = profile.CornerRadius;

            // Shape it based on what it is
            if (_activeElementType == GuiElementType.Panel || _activeElementType == GuiElementType.Window)
            {
                atomicDummy.style.width = 250; atomicDummy.style.height = 150;
                atomicDummy.Add(new Label("Sample Content") { style = { color = profile.TextColor, unityTextAlign = TextAnchor.MiddleCenter, flexGrow = 1 } });
            }
            else
            {
                atomicDummy.style.width = 180; atomicDummy.style.height = 40;
                atomicDummy.style.justifyContent = Justify.Center;
                atomicDummy.Add(new Label(_activeElementType.ToString()) { style = { color = profile.TextColor, unityTextAlign = TextAnchor.MiddleCenter, unityFontStyleAndWeight = FontStyle.Bold } });
            }

            _atomicPreviewZone.Add(atomicDummy);
        }

        private void BuildMatrixZone()
        {
            _matrixZone.Clear();
            var panelProfile = _activeTheme.GetProfile(GuiElementType.Panel);
            var headerProfile = _activeTheme.GetProfile(GuiElementType.Header);

            foreach (var providerType in _availableGuiProviders)
            {
                IGuiProvider providerInstance;
                try { providerInstance = (IGuiProvider)Activator.CreateInstance(providerType); }
                catch { continue; }

                // THE DATA CARD
                var dataCard = new VisualElement
                {
                    style = {
                        width = 360, height = 280, marginTop = 15, marginBottom = 15, marginLeft = 15, marginRight = 15, overflow = Overflow.Hidden,
                        // Apply Panel Profile
                        backgroundColor = panelProfile.BackgroundColor,
                        borderTopColor = panelProfile.BorderColor, borderBottomColor = panelProfile.BorderColor, borderLeftColor = panelProfile.BorderColor, borderRightColor = panelProfile.BorderColor,
                        borderTopWidth = panelProfile.BorderWidth, borderBottomWidth = panelProfile.BorderWidth, borderLeftWidth = panelProfile.BorderWidth, borderRightWidth = panelProfile.BorderWidth,
                        borderTopLeftRadius = panelProfile.CornerRadius, borderTopRightRadius = panelProfile.CornerRadius, borderBottomLeftRadius = panelProfile.CornerRadius, borderBottomRightRadius = panelProfile.CornerRadius
                    }
                };

                // The Header (Using Header Profile & Global Accent)
                var cardHeader = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, paddingTop = 8, paddingRight = 8, paddingLeft = 8, paddingBottom = 8, backgroundColor = headerProfile.BackgroundColor, borderBottomWidth = 1, borderBottomColor = _activeTheme.PrimaryAccent } };
                cardHeader.Add(new Label(providerInstance.Title ?? providerType.Name) { style = { color = headerProfile.TextColor, unityFontStyleAndWeight = FontStyle.Bold } });
                dataCard.Add(cardHeader);

                // Nested GUI
                var nestedContainer = new VisualElement { style = { flexGrow = 1, paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10 } };
                try { nestedContainer.Add(providerInstance.CreateGui(new GuiContext())); }
                catch (Exception ex) { nestedContainer.Add(new Label($"Render Error:\n{ex.Message}") { style = { color = _activeTheme.AlertColor } }); }
                dataCard.Add(nestedContainer);

                // Exclusion Dimmer
                var dimOverlay = new VisualElement { style = { position = Position.Absolute, top = 0, left = 0, right = 0, bottom = 0, backgroundColor = new Color(0, 0, 0, 0.85f), display = DisplayStyle.None } };
                var toggleBtn = new Button(() => {
                    dimOverlay.style.display = dimOverlay.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;
                })
                { text = "O", style = { width = 20, height = 20, backgroundColor = Color.clear, color = _activeTheme.SecondaryAccent, borderTopWidth = 0, borderLeftWidth = 0, borderRightWidth = 0, borderBottomWidth = 0 } };

                cardHeader.Add(toggleBtn); // Put toggle in the header
                dataCard.Add(dimOverlay);

                _matrixZone.Add(dataCard);
            }
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

        private VisualElement CreateGlobalColorField(string label, Color value, Action<Color> onChanged)
        {
            // Fallback for runtime since ColorField is Editor-only usually
            var box = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginRight = 15 } };
            box.Add(new Label(label + ":") { style = { color = Color.white, marginRight = 5 } });

            var colorSwatch = new VisualElement { style = { width = 40, height = 20, backgroundColor = value, borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1, borderTopColor = Color.white, borderBottomColor = Color.white, borderLeftColor = Color.white, borderRightColor = Color.white, borderTopLeftRadius = 3, borderTopRightRadius = 3, borderBottomLeftRadius = 3, borderBottomRightRadius = 3 } };
            // TODO: Bind click event to your ColorForgePicker here
            box.Add(colorSwatch);
            return box;
        }

        private Texture2D CreateGridTexture()
        {
            // Creates a subtle blueprint/blueprint grid for the atomic viewer background
            var tex = new Texture2D(64, 64);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    tex.SetPixel(x, y, (x == 0 || y == 0) ? new Color(1, 1, 1, 0.05f) : new Color(0, 0, 0, 0));
                }
            }
            tex.Apply();
            return tex;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}
