using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.GuiTesting
{
    public class Workshop_Gui_LiveModelPreviewTest : IGuiProvider, IDisposable
    {
        public string Title => "LMPB Test Harness";

        public int SeparatorWidth { get; private set; }

        private GameObject _targetObject;
        private LiveModelPreviewBuilder _lmp;
        private VisualElement _previewContainer;
        private GuiContext _activeCtx;
        private bool _isFirstBuild = true;

        // --- Domain Test State ---
        private float _testZoom = 2f;
        private float _testPitch = 10f;
        private float _testYaw = 180f; // 180 usually forces a model to face the camera
        private bool _testAutoOrbit = false;
        private float _testOrbitSpeed = 15f;
        private bool _testUseOriginal = false;
        private bool _testShowGizmos = true;
        private bool _testMouseControl = true;
        private bool _testShowControls = true;

        public Workshop_Gui_LiveModelPreviewTest()
        {
            _targetObject = GameObject.Find("Hermit");
            if (_targetObject == null)
            {
                Debug.LogWarning("[LMPB Test] Could not find a GameObject named 'Hermit' in the scene.");
            }
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _activeCtx = ctx;

            // --- 1. BUILD THE SIDEBAR ---
            var sidebar = new GraphicalUserInterfaceBuilder("Sidebar")
                .WithScrollable(true, ScrollViewMode.Vertical)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f, 1f))
                .WithPadding(10)
                .WithBorderRightWidth(2)
                .WithBorderRightColor(Color.black)
                .AddHeader("LMPB Domain Settings", Color.white)

                // Camera Transforms
                .AddSliderData("Zoom", 0.5f, 15f, _testZoom, v => _testZoom = v)
                .AddSliderData("Pitch", -89f, 89f, _testPitch, v => _testPitch = v)
                .AddSliderData("Yaw", -180f, 180f, _testYaw, v => _testYaw = v)
                .AddSliderData("Orbit Speed", 1f, 50f, _testOrbitSpeed, v => _testOrbitSpeed = v)
                .AddSeparator(Color.gray, SeparatorWidth)

                // Toggles
                .AddToggleData("Use Original Model (Live)", _testUseOriginal, v => _testUseOriginal = v)
                .AddToggleData("Auto Orbit", _testAutoOrbit, v => _testAutoOrbit = v)
                .AddToggleData("Mouse Control", _testMouseControl, v => _testMouseControl = v)
                .AddToggleData("Show View Gizmo", _testShowGizmos, v => _testShowGizmos = v)
                .AddToggleData("Show Bottom Controls", _testShowControls, v => _testShowControls = v)

                // Submit Button
                .AddChild(new ForgeButtonBuilder("Apply & Rebuild Preview", () => RebuildPreview(_activeCtx))
                    .WithBackgroundColor(new Color(0.2f, 0.5f, 0.2f))
                    .WithTextColor(Color.white)
                    .WithFontStyle(FontStyle.Bold)
                    .WithHeight(30)
                    .WithMargin(20, 0, 0, 0));

            // --- 2. BUILD THE MAIN PREVIEW AREA ---
            var mainArea = new GraphicalUserInterfaceBuilder("PreviewContainer")
                .WithBackgroundColor(Color.black)
                .WithAutoGrow(true)
                .OnBuild(ve =>
                {
                    _previewContainer = ve;

                    // THE FIX: Wait until the VisualElement is physically attached to the screen.
                    // This guarantees that ve.schedule.Execute() in your LMPB will actually start ticking!
                    ve.RegisterCallback<AttachToPanelEvent>(evt =>
                    {
                        if (_isFirstBuild)
                        {
                            _isFirstBuild = false;
                            RebuildPreview(_activeCtx);
                        }
                    });
                });

            // --- 3. COMBINE USING SPLIT PANEL BUILDER ---
            return new ForgeSplitPanelBuilder(sidebarWidth: 280, side: Side.Left)
                .WithSidebar(sidebar)
                .WithMain(mainArea)
                .CreateGui(ctx);
        }

        private void RebuildPreview(GuiContext ctx)
        {
            if (_previewContainer == null) return;

            _previewContainer.Clear();

            // Clean up the old builder's cameras and textures
            if (_lmp != null)
            {
                _lmp.Dispose();
            }

            // Dynamic fetch: If Hermit was spawned AFTER the UI was opened, we catch him here!
            if (_targetObject == null)
            {
                _targetObject = GameObject.Find("Hermit");
            }

            if (_targetObject == null)
            {
                var errorLabel = new ForgeLabelBuilder("Target 'Hermit' not found.")
                    .WithColor(Color.red)
                    .WithAlignment(TextAnchor.MiddleCenter)
                    .WithMargin(50, 0);

                _previewContainer.Add(errorLabel.CreateGui(ctx));
                return;
            }

            // Test the fluent API domain!
            _lmp = new LiveModelPreviewBuilder(_targetObject)
                .WithZoom(_testZoom)
                .WithPitch(_testPitch)
                .WithYaw(_testYaw)
                .WithOriginalModel(_testUseOriginal)
                .WithAutoRotate(_testAutoOrbit, _testOrbitSpeed)
                .WithMouseControl(_testMouseControl)
                .WithGizmos(_testShowGizmos)
                .WithControls(_testShowControls);

            _previewContainer.Add(_lmp.CreateGui(ctx));
        }

        public void Dispose()
        {
            if (_lmp != null) _lmp.Dispose();
        }

        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    }
}