using Assets.Scripts.GURPS;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    public class GURPS_Gui_EntityForge : IGuiProvider
    {
        public string Title => "THE TAILOR: SYSTEMIC ASSEMBLY";

        private readonly GURPS_EntityContext _entityCtx = new GURPS_EntityContext();
        private VisualElement _stageContainer;
        private DataWarehouse _warehouse;
        private GuiContext _lastCtx;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            // 1. ENGINE INITIALIZATION
            // This stands up the ecosystem and ensures the Tailor has memory access
            if (_warehouse == null) _warehouse = new DataWarehouse();
            DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem(_warehouse);

            // 2. SIDEBAR CONSTRUCTION
            var sidebar = new ForgeContainerBuilder()
                .WithWidth(350)
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                .AddChild(new ForgeLabelBuilder("MEASUREMENT CARD")
                    .WithFontSize(20)
                    .WithBold()
                    .WithColor(Color.magenta))
                // The Reflective Editor handles the GURPS-specific stats
                .AddChild(new ReflectiveGuiBuilder<GURPS_EntityContext>(_entityCtx))
                .AddSeparator(Color.gray, 1)
                // Custom logic for swapping character parts
                .AddChild(new ForgeTailorRackBuilder(_entityCtx, RefreshMirrors));

            // 3. MAIN STAGE CONSTRUCTION
            // We use ForgeContainerBuilder to create the row-based mirror area
            var stage = new ForgeContainerBuilder()
                .WithDirection(FlexDirection.Row)
                .WithFlexGrow(1)
                .OnBuild(ve => _stageContainer = ve);

            // 4. LAYOUT ASSEMBLY
            var layout = new ForgeSplitPanelBuilder(350, Side.Left)
                .WithSidebar(sidebar)
                .WithMain(stage);

            var root = layout.CreateGui(ctx);

            // Initial kickoff of the mirrors
            RefreshMirrors();

            return root;
        }

        private void RefreshMirrors()
        {
            if (_stageContainer == null) return;
            _stageContainer.Clear();

            // Mirror 1: Left Profile (-45 degrees)
            _stageContainer.Add(CreateMirrorView(-45f));

            // Mirror 2: Center Stage (0 degrees, Full Opacity)
            _stageContainer.Add(CreateMirrorView(0f, true));

            // Mirror 3: Right Profile (+45 degrees)
            _stageContainer.Add(CreateMirrorView(45f));
        }

        private VisualElement CreateMirrorView(float angleOffset, bool isMain = false)
        {
            // REFACTORED: Replaced manual VE with LiveModelPreviewBuilder logic
            // We assume the EntityContext provides the ActiveModel for the preview
            var lmpb = new LiveModelPreviewBuilder(_entityCtx.ActiveModel)
               // .WithCameraOffset(new Vector3(0, 1.5f, -3))
                .WithAutoRotate(false)
                .WithMouseControl(isMain) // Only the center stage is interactable
                .OnBuild(ve => {
                    ve.style.flexGrow = 1;
                    ve.style.marginTop = 5;
                    ve.style.opacity = isMain ? 1.0f : 0.4f;

                    if (!isMain)
                    {
                        ve.style.borderTopWidth = ve.style.borderBottomWidth = 1;
                        ve.style.borderLeftWidth = ve.style.borderRightWidth = 1;
                        ve.style.borderTopColor = ve.style.borderBottomColor = new Color(0.5f, 0.5f, 0.8f, 0.2f);
                        ve.style.borderLeftColor = ve.style.borderRightColor = new Color(0.5f, 0.5f, 0.8f, 0.2f);
                    }

                    // Apply rotation offset to the model transform here if needed
                });

            return lmpb.CreateGui(_lastCtx);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}