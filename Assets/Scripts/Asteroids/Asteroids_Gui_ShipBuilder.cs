#if UNITY_EDITOR
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEditor.UIElements; // Required for FloatField
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Asteroids.Editor
{
    public class Asteroids_Gui_ShipBuilder : IGuiProvider
    {
        public string Title => "CUSTOM SHIPWRIGHT";

        private AsteroidsContext _context;
        private GuiContext _guiContext;
        private VisualElement _rootContainer;
        private HangarTheme _theme;

        private Action _onBack;
        private Action<GameObject> _onSave;

        private FighterOrbitView _builderOrbitView;

        // Builder State
        private List<GameObject> _attachedParts = new List<GameObject>();
        private GameObject _selectedPart = null;
        private string _shipName = "Custom Interceptor";

        public Asteroids_Gui_ShipBuilder(AsteroidsContext context, Action onBack, Action<GameObject> onSave, HangarTheme theme = null)
        {
            _context = context;
            _onBack = onBack;
            _onSave = onSave;
            _theme = theme ?? new HangarTheme();
        }

        public VisualElement CreateGui(GuiContext context)
        {
            _guiContext = context;
            _rootContainer = new VisualElement { style = { flexGrow = 1 } };
            Refresh();
            return _rootContainer;
        }

        private void Refresh()
        {
            _rootContainer.Clear();

            // 3-Pane Custom Layout
            var mainLayout = new GraphicalUserInterfaceBuilder("Builder_Layout")
                .WithBackgroundColor(_theme.BorderDark)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch)
                .WithPercentSize(100, 100);

            mainLayout.AddChild(new GenericGuiProvider(CreateLibraryPanel));
            mainLayout.AddChild(new GenericGuiProvider(CreateViewportPanel));
            mainLayout.AddChild(new GenericGuiProvider(CreateInspectorPanel));

            _rootContainer.Add(mainLayout.Build());
        }

        // --- LEFT PANE: PARTS LIBRARY ---
        private VisualElement CreateLibraryPanel()
        {
            var panel = new VisualElement { style = { width = 280, backgroundColor = _theme.BaseBackground, paddingLeft = 15, paddingRight = 15, paddingTop = 15, borderRightWidth = 2, borderRightColor = _theme.PrimaryAccent } };

            panel.Add(new Label("MODULE LIBRARY") { style = { color = _theme.TitleText, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } });

            var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };

            if (_context.fighterPartPrefabs != null)
            {
                foreach (var partPrefab in _context.fighterPartPrefabs)
                {
                    if (partPrefab == null) continue;

                    var card = new VisualElement { style = { flexDirection = FlexDirection.Row, backgroundColor = _theme.PanelBackground, marginBottom = 10, paddingLeft = 5, paddingRight = 5, paddingTop = 5, paddingBottom = 5, alignItems = Align.Center, borderLeftWidth = 2, borderLeftColor = _theme.BorderDark } };

                    var miniView = new FighterOrbitView(partPrefab, _theme, false, new Vector3(-90, 0, 0), 10f)
                    {
                        style = { width = 50, height = 50, marginRight = 10, borderBottomLeftRadius = 25, borderBottomRightRadius = 25, borderTopLeftRadius = 25, borderTopRightRadius = 25, borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1, borderTopColor = _theme.BorderDark, borderBottomColor = _theme.BorderDark, borderLeftColor = _theme.BorderDark, borderRightColor = _theme.BorderDark }
                    };
                    miniView.pickingMode = PickingMode.Ignore;
                    miniView.ForceRender();

                    card.Add(miniView);

                    var rightCol = new VisualElement { style = { flexGrow = 1 } };
                    rightCol.Add(new Label(partPrefab.name) { style = { color = _theme.TextNormal, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 10, marginBottom = 5 } });

                    var addBtn = new Button(() => AttachPartToShip(partPrefab)) { text = "+ ATTACH", style = { height = 20, backgroundColor = _theme.BaseBackground, color = _theme.PrimaryAccent } };
                    rightCol.Add(addBtn);

                    card.Add(rightCol);
                    scroll.Add(card);
                }
            }

            panel.Add(scroll);

            var backBtn = new Button(() => _onBack?.Invoke()) { text = "<< ABORT ASSEMBLY", style = { height = 40, marginTop = 10, backgroundColor = _theme.PanelBackground, color = _theme.TextMuted } };
            panel.Add(backBtn);

            return panel;
        }

        // --- CENTER PANE: VIEWPORT ---
        private VisualElement CreateViewportPanel()
        {
            var panel = new VisualElement { style = { flexGrow = 1, backgroundColor = _theme.BorderDark, paddingLeft = 20, paddingRight = 20, paddingTop = 20, paddingBottom = 20, alignItems = Align.Center, justifyContent = Justify.Center } };

            panel.Add(new Label("ASSEMBLY BAY") { style = { color = _theme.TitleText, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, letterSpacing = 2, marginBottom = 10 } });

            // Initialize Orbit View with an empty GameObject as the root!
            if (_builderOrbitView == null)
            {
                GameObject emptyRoot = new GameObject("Workbench_Root");
                _builderOrbitView = new FighterOrbitView(emptyRoot, _theme, true, new Vector3(-90, 0, 0), 15f)
                {
                    style = { width = Length.Percent(100), minHeight = 400, flexGrow = 1, borderTopWidth = 2, borderBottomWidth = 2, borderTopColor = _theme.PrimaryAccent, borderBottomColor = _theme.PrimaryAccent }
                };
                UnityEngine.Object.DestroyImmediate(emptyRoot); // Clean up the temp source
            }

            panel.Add(_builderOrbitView);

            // Viewer Controls
            var controls = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 10 } };
            controls.Add(new Button(() => _builderOrbitView.Zoom(-2f)) { text = "ZOOM IN (+)", style = { width = 120 } });
            controls.Add(new Button(() => _builderOrbitView.Zoom(2f)) { text = "ZOOM OUT (-)", style = { width = 120 } });
            panel.Add(controls);

            return panel;
        }

        // --- RIGHT PANE: HIERARCHY & TRANSFORMS ---
        private VisualElement CreateInspectorPanel()
        {
            var panel = new VisualElement { style = { width = 300, backgroundColor = _theme.PanelBackground, paddingLeft = 15, paddingRight = 15, paddingTop = 15, borderLeftWidth = 2, borderLeftColor = _theme.PrimaryAccent } };

            // Callsign
            var nameField = new TextField("Callsign:") { value = _shipName, style = { marginBottom = 20 } };
            nameField.Q<Label>().style.color = _theme.TextMuted;
            nameField.RegisterValueChangedCallback(e => _shipName = e.newValue);
            panel.Add(nameField);

            panel.Add(new Label("ATTACHED MODULES") { style = { color = _theme.PrimaryAccent, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });

            // List of Attached Parts
            var hierarchyScroll = new ScrollView(ScrollViewMode.Vertical) { style = { height = 150, backgroundColor = _theme.BaseBackground, borderTopWidth = 1, borderBottomWidth = 1, borderTopColor = _theme.BorderDark, borderBottomColor = _theme.BorderDark, marginBottom = 20 } };

            foreach (var part in _attachedParts)
            {
                if (part == null) continue;
                bool isSelected = _selectedPart == part;

                var partBtn = new Button(() => { _selectedPart = part; Refresh(); })
                {
                    text = part.name.Replace("(Clone)", ""),
                    style = { unityTextAlign = TextAnchor.MiddleLeft, backgroundColor = isSelected ? _theme.PrimaryAccent : Color.clear, color = isSelected ? Color.black : _theme.TextNormal, borderLeftWidth = isSelected ? 4 : 0, borderLeftColor = Color.white }
                };
                hierarchyScroll.Add(partBtn);
            }
            panel.Add(hierarchyScroll);

            // Transform Inspector
            if (_selectedPart != null)
            {
                panel.Add(new Label($"EDITING: {_selectedPart.name.Replace("(Clone)", "")}") { style = { color = _theme.TitleText, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                panel.Add(CreateVector3Editor("Position", _selectedPart.transform.localPosition, v => { _selectedPart.transform.localPosition = v; _builderOrbitView.ForceRender(); }));
                panel.Add(CreateVector3Editor("Rotation", _selectedPart.transform.localEulerAngles, v => { _selectedPart.transform.localEulerAngles = v; _builderOrbitView.ForceRender(); }));
                panel.Add(CreateVector3Editor("Scale", _selectedPart.transform.localScale, v => { _selectedPart.transform.localScale = v; _builderOrbitView.ForceRender(); }));

                var removeBtn = new Button(() => {
                    _attachedParts.Remove(_selectedPart);
                    UnityEngine.Object.DestroyImmediate(_selectedPart);
                    _selectedPart = null;
                    _builderOrbitView.ForceRender();
                    Refresh();
                })
                { text = "REMOVE MODULE", style = { marginTop = 20, backgroundColor = new Color(0.4f, 0.1f, 0.1f), color = Color.white } };
                panel.Add(removeBtn);
            }
            else
            {
                panel.Add(new Label("Select a module from the list above to edit its position.") { style = { color = _theme.TextMuted, whiteSpace = WhiteSpace.Normal } });
            }

            var spacer = new VisualElement { style = { flexGrow = 1 } };
            panel.Add(spacer);

            var saveBtn = new Button(() => SaveAndReturn()) { text = "FABRICATE SHIP", style = { height = 50, marginBottom = 10, backgroundColor = _theme.PrimaryAccent, color = _theme.TextActive, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 16 } };
            panel.Add(saveBtn);

            return panel;
        }

        // --- UTILITIES ---

        private void AttachPartToShip(GameObject prefab)
        {
            if (_builderOrbitView == null || _builderOrbitView.ModelInstance == null) return;

            // Instantiate into the isolated preview scene, parented to the ModelInstance root
            var newPart = UnityEngine.Object.Instantiate(prefab, _builderOrbitView.ModelInstance.transform);
            newPart.transform.localPosition = Vector3.zero;
            newPart.transform.localRotation = Quaternion.identity;
            newPart.transform.localScale = Vector3.one;

            _attachedParts.Add(newPart);
            _selectedPart = newPart;

            _builderOrbitView.ForceRender();
            Refresh();
        }

        private VisualElement CreateVector3Editor(string label, Vector3 currentValue, Action<Vector3> onValueChanged)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 5, alignItems = Align.Center } };
            row.Add(new Label(label) { style = { width = 60, color = _theme.TextMuted, fontSize = 10 } });

            var xField = new FloatField() { value = currentValue.x, style = { flexGrow = 1, marginRight = 2 } };
            var yField = new FloatField() { value = currentValue.y, style = { flexGrow = 1, marginRight = 2 } };
            var zField = new FloatField() { value = currentValue.z, style = { flexGrow = 1 } };

            // When any field changes, construct a new Vector3 and fire the callback
            Action updateValue = () => onValueChanged(new Vector3(xField.value, yField.value, zField.value));

            xField.RegisterValueChangedCallback(e => updateValue());
            yField.RegisterValueChangedCallback(e => updateValue());
            zField.RegisterValueChangedCallback(e => updateValue());

            row.Add(xField); row.Add(yField); row.Add(zField);
            return row;
        }

        private void SaveAndReturn()
        {
            // 1. Create a real GameObject in the active Unity scene
            GameObject finalShip = new GameObject(_shipName);
            finalShip.SetActive(false); // Hide it immediately

            // 2. Clone all our manipulated parts from the preview scene into the real scene object
            foreach (var part in _attachedParts)
            {
                if (part == null) continue;
                var clone = UnityEngine.Object.Instantiate(part, finalShip.transform);
                // Maintain the custom transforms!
                clone.transform.localPosition = part.transform.localPosition;
                clone.transform.localRotation = part.transform.localRotation;
                clone.transform.localScale = part.transform.localScale;
            }

            // 3. Save to Context and Exit
            _onSave?.Invoke(finalShip);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}
#endif