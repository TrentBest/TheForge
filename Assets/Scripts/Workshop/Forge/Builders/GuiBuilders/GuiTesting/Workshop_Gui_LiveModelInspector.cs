#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class Workshop_Gui_LiveModelInspector : IGuiProvider
    {
        public string Title => "FORGE: LIVE MODEL INSPECTOR";

        private GameObject _selectedPrefab;
        private VisualElement _previewContainer;
        private GuiContext _lastCtx;
        private GameObject _currentPreviewModel;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var rootBuilder = new GraphicalUserInterfaceBuilder("InspectorRoot")
                .WithPercentSize(100, 100)
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f));

            var sidebar = new GraphicalUserInterfaceBuilder("Sidebar")
                .WithWidth(350)
                .WithPadding(15)
                .WithBorderRightWidth(1)
                .WithBorderRightColor(Color.cyan)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .OnBuild(ve =>
                {
                    ve.Add(new Label("SELECTION PARAMETERS") { style = { color = Color.cyan, marginBottom = 10, unityFontStyleAndWeight = FontStyle.Bold } });

                    var objectField = new ObjectField("SOURCE PREFAB")
                    {
                        objectType = typeof(GameObject),
                        value = _selectedPrefab,
                        allowSceneObjects = false
                    };
                    objectField.RegisterValueChangedCallback(evt =>
                    {
                        _selectedPrefab = evt.newValue as GameObject;
                        RefreshPreview();
                    });
                    ve.Add(objectField);
                })
                .AddSeparator()
                .AddButton("FORCE ENGINE REBOOT", RefreshPreview);

            rootBuilder.AddChild(sidebar.Build());

            rootBuilder.AddChild(new GraphicalUserInterfaceBuilder("PreviewHost")
                .WithBackgroundColor(Color.black)
                .OnBuild(ve =>
                {
                    ve.style.flexGrow = 1;
                    ve.style.justifyContent = Justify.Center;
                    ve.style.alignItems = Align.Center;
                    _previewContainer = ve;

                    if (_selectedPrefab != null) RefreshPreview();
                    else ve.Add(new Label("NO ACTIVE PREFAB DETECTED") { style = { color = Color.gray } });
                })
                .Build());

            return rootBuilder.Build();
        }

        private void RefreshPreview()
        {
            if (_previewContainer == null) return;

            Debug.Log("[Inspector] 1. Cleaning up old preview...");
            if (_currentPreviewModel != null)
            {
                UnityEngine.Object.DestroyImmediate(_currentPreviewModel);
            }
            _previewContainer.Clear();

            if (_selectedPrefab == null)
            {
                Debug.Log("[Inspector] Selection cleared.");
                return;
            }

            Debug.Log($"[Inspector] 2. Instantiating {_selectedPrefab.name}...");
            _currentPreviewModel = UnityEngine.Object.Instantiate(_selectedPrefab);
            _currentPreviewModel.name = "[PREVIEW] " + _selectedPrefab.name;

            // CANARY CHECK: Does this model actually have meshes?
            var renderers = _currentPreviewModel.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                Debug.LogWarning($"[Inspector] WARNING: {_selectedPrefab.name} has no Renderers! It will be invisible.");
            }

            Debug.Log("[Inspector] 3. Building UI Viewport...");

            // STRICT BUILDER PATTERN:
            // We use a wrapper builder to guarantee the LiveModelPreview expands to 100% size
            var previewWrapper = new GraphicalUserInterfaceBuilder("PreviewWrapper")
                .WithPercentSize(100, 100)
                .AddChild(new LiveModelPreviewBuilder(_currentPreviewModel)
                    .WithZoom(2.0f)
                    .WithAutoOrbit(Vector3.up, 15f)
                    .WithMouseControl(true))
                .Build();

            _previewContainer.Add(previewWrapper);
            Debug.Log("[Inspector] 4. Success. Viewport injected into layout.");
        }

        public System.Action<VisualElement> GetGuiBuilder() => ve => ve.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string path) { }
        public void ToUIDocument(string path) { }
    }
}
#endif