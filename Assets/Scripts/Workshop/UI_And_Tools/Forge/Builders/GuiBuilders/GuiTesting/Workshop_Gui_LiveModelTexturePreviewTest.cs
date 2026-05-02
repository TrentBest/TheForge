#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.GuiTesting
{
    // 1. FIXED: Implemented IStateContext so the FSM API accepts the context payload
    public class LiveTextureTestContext : IStateContext
    {
        public GameObject ActiveModel;

        // Standard FSM Context requirements (adjust if your interface demands different properties)
        public string Name { get; set; } = "LiveTextureTest";
        public bool IsValid { get; set; } = true;
    }

    public class Workshop_Gui_LiveModelTexturePreviewTest : IGuiProvider
    {
        public string Title => "FORGE: LIVE TEXTURE PIPELINE TEST";

        private RenderTexture _rt;
        private GameObject _cameraObj;
        private GameObject _modelObj;
        private GameObject _lightObj;

        private GameObject _selectedPrefab;
        private Texture2D _selectedTexture;

        private string _processGroup;
        private FSMHandle _fsmHandle;
        private LiveTextureTestContext _fsmContext;

        public VisualElement CreateGui(GuiContext ctx)
        {
            Debug.Log("[TexturePipelineTest] Initializing FSM and Isolated 3D Scene...");

            _processGroup = "LiveTextureTest_" + Guid.NewGuid().ToString().Substring(0, 6);
            _fsmContext = new LiveTextureTestContext();

            // Setup the 3D Scene
            _rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            _rt.Create();

            _cameraObj = new GameObject("TestTexturePreviewCamera");
            _cameraObj.transform.position = new Vector3(0, -5000, -5);
            var cam = _cameraObj.AddComponent<Camera>();
            cam.targetTexture = _rt;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.15f, 0.15f, 0.2f);

            _lightObj = new GameObject("TestLight");
            _lightObj.transform.SetParent(_cameraObj.transform);
            _lightObj.transform.localPosition = new Vector3(2, 2, -2);
            var light = _lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.5f;

            _selectedPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _selectedTexture = Texture2D.whiteTexture;
            RefreshModel();

            RegisterFSM();

            var rootBuilder = new GraphicalUserInterfaceBuilder("LiveTexturePreviewRoot")
                .WithPercentSize(100, 100)
                .WithFlexLayout(FlexDirection.Row)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .OnBuild(ve =>
                {
                    ve.style.flexGrow = 1;

                    // 2. FIXED: The "Asteroids Hangar" Anti-Stall Pattern
                    ve.schedule.Execute(() =>
                    {
                        // A. Tick the FSM locally for this specific UI
                        FSM_API.Interaction.Update(_processGroup);

                        // B. Force the UI to repaint the live texture
                        ve.MarkDirtyRepaint();

                        // C. THE ANTI-STALL MAGIC: Force Unity's Editor loop to stay awake
                        EditorApplication.QueuePlayerLoopUpdate();

                    }).Every(16); // ~60 FPS

                    ve.RegisterCallback<DetachFromPanelEvent>(evt => Cleanup());
                });

            // --- LEFT SIDEBAR: CONFIGURATION ---
            var sidebar = new GraphicalUserInterfaceBuilder("Sidebar")
                .WithWidth(350)
                .WithPadding(15)
                .WithBorderRightWidth(1)
                .WithBorderRightColor(Color.cyan)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .AddHeader("PREVIEW SETTINGS", Color.cyan)
                .OnBuild(ve =>
                {
                    var modelField = new ObjectField("TARGET MODEL") { objectType = typeof(GameObject), allowSceneObjects = false, value = _selectedPrefab };
                    modelField.RegisterValueChangedCallback(evt => {
                        _selectedPrefab = evt.newValue as GameObject;
                        RefreshModel();
                    });
                    ve.Add(modelField);

                    var textureField = new ObjectField("APPLY TEXTURE") { objectType = typeof(Texture2D), allowSceneObjects = false, value = _selectedTexture };
                    textureField.RegisterValueChangedCallback(evt => {
                        _selectedTexture = evt.newValue as Texture2D;
                        ApplyTexture();
                    });
                    ve.Add(textureField);
                });

            rootBuilder.AddChild(sidebar);

            // --- RIGHT SIDE: LIVE PREVIEW PIPELINE ---
            var texturePreview = new TexturePreviewBuilder(_rt)
                .WithBackgroundColor(Color.black)
                .WithZoomSensitivity(0.05f)
                .WithPanSensitivity(0.5f);

            var previewContainer = new GraphicalUserInterfaceBuilder("PreviewContainer")
                .WithPercentSize(100, 100)
                .OnBuild(container => container.style.flexGrow = 1)
                .AddChild(texturePreview);

            rootBuilder.AddChild(previewContainer);

            return rootBuilder.Build();
        }

        private void RegisterFSM()
        {
            // By passing 'processingGroup: _processGroup', we tell your API to build 
            // this definition specifically inside our dynamic UI Editor group.
            FSM_API.Create.CreateFiniteStateMachine("LiveTextureTestFSM", processingGroup: _processGroup)
                .State("Spinning", null, (ctx) => {
                    var c = ctx as LiveTextureTestContext;
                    if (c != null && c.ActiveModel != null)
                    {
                        c.ActiveModel.transform.Rotate(Vector3.up, 1.5f, Space.World);
                        c.ActiveModel.transform.Rotate(Vector3.right, 0.75f, Space.World);
                    }
                }, null)
                .WithInitialState("Spinning")
                .BuildDefinition();

            // Now the instance creator will successfully find the definition inside this exact group!
            _fsmHandle = FSM_API.Create.CreateInstance("LiveTextureTestFSM", _fsmContext, _processGroup);
        }

        private void RefreshModel()
        {
            if (_modelObj != null) UnityEngine.Object.DestroyImmediate(_modelObj);
            if (_selectedPrefab == null) return;

            _modelObj = UnityEngine.Object.Instantiate(_selectedPrefab);
            _modelObj.transform.position = new Vector3(0, -5000, 0);
            _lightObj.transform.LookAt(_modelObj.transform);

            _fsmContext.ActiveModel = _modelObj;
            ApplyTexture();
        }

        private void ApplyTexture()
        {
            if (_modelObj == null || _selectedTexture == null) return;

            var renderers = _modelObj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.material.mainTexture = _selectedTexture;
            }
        }

        private void Cleanup()
        {
            Debug.Log($"[TexturePipelineTest] Cleaning up FSM {_processGroup} and scene.");
            if (_fsmHandle != null) FSM_API.Interaction.DestroyInstance(_fsmHandle);

            if (_cameraObj != null) UnityEngine.Object.DestroyImmediate(_cameraObj);
            if (_modelObj != null) UnityEngine.Object.DestroyImmediate(_modelObj);
            if (_lightObj != null) UnityEngine.Object.DestroyImmediate(_lightObj);
            if (_rt != null) _rt.Release();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}
#endif