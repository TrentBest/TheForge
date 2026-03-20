#if UNITY_EDITOR
using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders.Themes;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class Workshop_Gui_ThemeEditor : IGuiProvider
    {
        public string Title => "FORGE: LIVE THEME EDITOR";

        private GuiTheme _activeTheme;
        private GuiElementType _selectedElementType = GuiElementType.ButtonPrimary;
        private VisualElement _previewContainer;
        private VisualElement _controlsContainer;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _activeTheme = GuiSkin.Active;

            var rootBuilder = new GraphicalUserInterfaceBuilder("ThemeEditorRoot")
                .WithPercentSize(100, 100)
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f));

            // --- LEFT SIDEBAR: CONTROLS ---
            var sidebarBuilder = new GraphicalUserInterfaceBuilder("ControlsSidebar")
                .WithWidth(400)
                .WithPadding(15)
                .WithBorderRightWidth(1)
                .WithBorderRightColor(new Color(0.3f, 0.3f, 0.3f))
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .OnBuild(ve =>
                {
                    ve.Add(new Label("GLOBAL PALETTE") { style = { color = Color.cyan, marginBottom = 10, unityFontStyleAndWeight = FontStyle.Bold } });

                    ve.Add(CreateColorField("Primary Accent", _activeTheme.PrimaryAccent, c => _activeTheme.PrimaryAccent = c));
                    ve.Add(CreateColorField("Secondary Accent", _activeTheme.SecondaryAccent, c => _activeTheme.SecondaryAccent = c));
                    ve.Add(CreateColorField("Alert Color", _activeTheme.AlertColor, c => _activeTheme.AlertColor = c));

                    var spacer = new VisualElement { style = { height = 20 } };
                    ve.Add(spacer);

                    ve.Add(new Label("COMPONENT PROFILES") { style = { color = Color.cyan, marginBottom = 10, unityFontStyleAndWeight = FontStyle.Bold } });

                    // Dropdown to select which component type we are editing
                    var typeSelector = new EnumField("Target Element", _selectedElementType);
                    typeSelector.RegisterValueChangedCallback(evt =>
                    {
                        _selectedElementType = (GuiElementType)evt.newValue;
                        BuildProfileControls();
                        RefreshPreview();
                    });
                    ve.Add(typeSelector);

                    _controlsContainer = new VisualElement { style = { marginTop = 15 } };
                    ve.Add(_controlsContainer);

                    BuildProfileControls(); // Populate initial controls
                });

            rootBuilder.AddChild(sidebarBuilder.Build());

            // --- RIGHT SIDE: LIVE PREVIEW ---
            var previewAreaBuilder = new GraphicalUserInterfaceBuilder("PreviewArea")
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                .OnBuild(ve =>
                {
                    ve.style.flexGrow = 1;
                    ve.style.justifyContent = Justify.Center;
                    ve.style.alignItems = Align.Center;

                    _previewContainer = new VisualElement();
                    ve.Add(_previewContainer);
                    RefreshPreview();
                });

            rootBuilder.AddChild(previewAreaBuilder.Build());

            return rootBuilder.Build();
        }

        private void BuildProfileControls()
        {
            if (_controlsContainer == null) return;
            _controlsContainer.Clear();

            var profile = _activeTheme.GetOrCreateProfile(_selectedElementType);

            _controlsContainer.Add(CreateColorField("Background Color", profile.BackgroundColor, c => { profile.BackgroundColor = c; RefreshPreview(); }));
            _controlsContainer.Add(CreateColorField("Border Color", profile.BorderColor, c => { profile.BorderColor = c; RefreshPreview(); }));
            _controlsContainer.Add(CreateColorField("Text Color", profile.TextColor, c => { profile.TextColor = c; RefreshPreview(); }));

            _controlsContainer.Add(CreateFloatField("Border Width", profile.BorderWidth, v => { profile.BorderWidth = v; RefreshPreview(); }));
            _controlsContainer.Add(CreateFloatField("Corner Radius", profile.CornerRadius, v => { profile.CornerRadius = v; RefreshPreview(); }));
        }

        private void RefreshPreview()
        {
            if (_previewContainer == null) return;
            _previewContainer.Clear();

            var profile = _activeTheme.GetOrCreateProfile(_selectedElementType);

            // We build a mock element to show off the current settings live
            var mockElement = new Label($"Preview: {_selectedElementType}")
            {
                style =
                {
                    backgroundColor = profile.BackgroundColor,
                    borderTopColor = profile.BorderColor,
                    borderBottomColor = profile.BorderColor,
                    borderLeftColor = profile.BorderColor,
                    borderRightColor = profile.BorderColor,
                    borderTopWidth = profile.BorderWidth,
                    borderBottomWidth = profile.BorderWidth,
                    borderLeftWidth = profile.BorderWidth,
                    borderRightWidth = profile.BorderWidth,
                    borderTopLeftRadius = profile.CornerRadius,
                    borderTopRightRadius = profile.CornerRadius,
                    borderBottomLeftRadius = profile.CornerRadius,
                    borderBottomRightRadius = profile.CornerRadius,
                    color = profile.TextColor,
                    paddingTop = 15,
                    paddingBottom = 15,
                    paddingLeft = 30,
                    paddingRight = 30,
                    fontSize = 16,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };

            _previewContainer.Add(mockElement);

            // Mark the ScriptableObject as dirty so Unity saves the changes
            UnityEditor.EditorUtility.SetDirty(_activeTheme);
        }

        // Helper wrappers for UI Toolkit fields
        private VisualElement CreateColorField(string label, Color initialValue, Action<Color> onValueChanged)
        {
            var field = new ColorField(label) { value = initialValue };
            field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            return field;
        }

        private VisualElement CreateFloatField(string label, float initialValue, Action<float> onValueChanged)
        {
            var field = new FloatField(label) { value = initialValue };
            field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            return field;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}
#endif