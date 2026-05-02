using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders.Physics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders.Physics
{
    public class Workshop_Gui_PeriodicTableDemo : IGuiProvider
    {
        public string Title => "Periodic Table Demo";

        private Workshop_Gui_PeriodicTable _tableLogic;
        private VisualElement _2DContainer;
        private VisualElement _3DContainer;
        private DiegeticModelSpinner _spinner; // Track the physics motor
        private bool _is3D = false;

        public Workshop_Gui_PeriodicTableDemo()
        {
            _tableLogic = new Workshop_Gui_PeriodicTable();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement { style = { flexGrow = 1 } };

            var toolbar = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, paddingTop = 5, paddingRight=5, paddingLeft = 5, paddingBottom = 5 }
            };
            var toggle = new Toggle("3D") { value = _is3D, style = { color = Color.white } };
            toolbar.Add(toggle);
            root.Add(toolbar);

            var viewPort = new VisualElement { style = { flexGrow = 1 } };
            root.Add(viewPort);

            // 1. PRE-ALLOCATE 2D REALITY (Active by default)
            _2DContainer = new VisualElement { style = { flexGrow = 1 } };
            _2DContainer.Add(_tableLogic.CreateGui(ctx));
            viewPort.Add(_2DContainer);

            // 2. PRE-ALLOCATE 3D REALITY (Hidden and Suppressed)
            _3DContainer = new VisualElement { style = { flexGrow = 1, display = DisplayStyle.None } };
            _3DContainer.Add(Build3DProjection(ctx));
            viewPort.Add(_3DContainer);

            // 3. THE SEAMLESS SWAP LOGIC
            toggle.RegisterValueChangedCallback(evt => {
                _is3D = evt.newValue;

                if (_is3D)
                {
                    // Hide 2D, Show 3D, Ignite Physics
                    _2DContainer.style.display = DisplayStyle.None;
                    _3DContainer.style.display = DisplayStyle.Flex;
                    if (_spinner != null) _spinner.enabled = true;
                }
                else
                {
                    // Hide 3D, Show 2D, Kill Physics and reset orientation
                    _2DContainer.style.display = DisplayStyle.Flex;
                    _3DContainer.style.display = DisplayStyle.None;
                    if (_spinner != null)
                    {
                        _spinner.enabled = false;
                        _spinner.ResetToFlat();
                    }
                }
            });

            return root;
        }

        private VisualElement Build3DProjection(GuiContext ctx)
        {
            GameObject screenQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            screenQuad.name = "TableProjection_Mesh";
            screenQuad.transform.localScale = new Vector3(4f, 2.5f, 1f);
            screenQuad.SetActive(false);

            var lmp = new LiveModelPreviewBuilder(screenQuad)
                .WithPitch(0f)  // Perfectly flat to mimic 2D
                .WithYaw(0f)
                .WithZoom(2.8f) // Tuned aspect fit
                .WithMouseControl(true) // Allows breaking the illusion once 3D is active
                .WithPreviewLifecycle(
                    onInit: (ghost, group) => {
                        var uiDoc = ghost.AddComponent<UIDocument>();
                        var inWorld = ghost.AddComponent<InWorldGuiBuilder>();

                        inWorld.SetResolutionSettings(new Vector2(4f, 2.5f), 2048);
                        inWorld.Initialize(_tableLogic);

                        ghost.SetActive(true);

                        // Attach but SUPPRESS the motor
                        _spinner = ghost.AddComponent<DiegeticModelSpinner>();
                        _spinner.enabled = false;
                    }
                );

            var preview = lmp.CreateGui(ctx);
            UnityEngine.Object.DestroyImmediate(screenQuad);
            return preview;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }

    // Updated Motor with memory of its original flat state
    public class DiegeticModelSpinner : MonoBehaviour
    {
        public float RotationSpeed = 15f;
        private Quaternion _flatOrientation;

        private void Awake()
        {
            _flatOrientation = transform.rotation;
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, RotationSpeed * Time.deltaTime, Space.Self);
        }

        public void ResetToFlat()
        {
            transform.rotation = _flatOrientation;
        }
    }
}