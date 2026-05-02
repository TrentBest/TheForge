using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Core;
using Workshop.UI_And_Tools.Forge.Hermit.UI;

namespace Workshop.GURPS
{
    public class GURPS_EntityForge : IGuiProvider
    {
        public string Title => "THE TAILOR: SYSTEMIC ASSEMBLY";

        private readonly GURPS_EntityContext _entityCtx = new GURPS_EntityContext();
        private VisualElement _stageContainer;
        private DataWarehouse _warehouse;
        private GuiContext _lastCtx;

        public GURPS_EntityForge()
        {
            _warehouse = new DataWarehouse();
            DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem(_warehouse);
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            // 1. The Tailor's Clipboard (Sidebar)
            var sidebar = new ForgeContainerBuilder("TailorsClipboard")
                .WithWidth(380)
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.05f, 0.02f, 0.05f)) // Deep Tailor Void
                .WithBorderRightWidth(1).WithBorderColor(Color.magenta)
                .AddChild(new ForgeLabelBuilder("THE TAILOR's CLIPBOARD")
                    .WithFontSize(22).WithBold().WithColor(Color.magenta).WithMarginBottom(5)
                    .OnBuild(ve => ve.style.letterSpacing = 2))
                .AddChild(new ForgeLabelBuilder("MEASUREMENTS & TRAITS")
                    .WithFontSize(10).WithColor(Color.cyan).WithMarginBottom(15))

                // The raw numeric/stat data via reflection
                .AddChild(new ReflectiveGuiBuilder<GURPS_EntityContext>(_entityCtx))
                .AddSeparator(Color.gray, 1)

                // Diegetic Entity Configurations
                .AddChild(CreateCastingCard())
                .AddChild(CreateBehaviorCard());

            // 2. The Fitting Room (Main Stage)
            var stage = new ForgeContainerBuilder("FittingRoomStage")
                .WithDirection(FlexDirection.Column)
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.02f))
                .AddChild(new ForgeLabelBuilder("TRIPLE-MIRROR OBSERVATION DECK")
                    .WithColor(new Color(0.4f, 0.4f, 0.4f)).WithMargin(10).WithFontSize(10))
                .OnBuild(ve => _stageContainer = ve);

            var layout = new ForgeSplitPanelBuilder(380, Side.Left)
                .WithSidebar(sidebar)
                .WithMain(stage);

            var root = layout.CreateGui(ctx);

            // Add Hermit CORTEX Link
            new GraphicalUserInterfaceBuilder("EntityHermit")
                .WithHermit(HermitSettings.LoadOrCreate())
                .OnBuild(ve => root.Add(ve))
                .Build();

            RefreshMirrors();
            return root;
        }

        /// <summary>
        /// Generates the diegetic Zelda-style tethering and locomotion logic card.
        /// </summary>
        private IGuiProvider CreateBehaviorCard()
        {
            return new ForgeContainerBuilder("BehaviorCard")
                .WithMarginTop(15).WithPadding(10)
                .WithBackgroundColor(new Color(0.1f, 0.05f, 0.1f))
                .WithBorderWidth(1).WithBorderColor(Color.cyan)
                .AddChild(new ForgeLabelBuilder("LOCOMOTION & TETHERING").WithBold().WithColor(Color.cyan).WithMarginBottom(10))
                .AddChild(new ForgeContainerBuilder("Buttons").WithDirection(FlexDirection.Row).WithFlexWrap(Wrap.Wrap)
                    .AddChild(new ForgeButtonBuilder("Wanderer\n(Untethered)", () => SetBehavior("Wanderer"))
                        .WithFlexGrow(1).WithMargin(2).WithBackgroundColor(Color.black).WithTextColor(Color.white).WithFontSize(10))
                    .AddChild(new ForgeButtonBuilder("Pacer\n(Door Tethered)", () => SetBehavior("Pacer"))
                        .WithFlexGrow(1).WithMargin(2).WithBackgroundColor(Color.black).WithTextColor(Color.white).WithFontSize(10))
                    .AddChild(new ForgeButtonBuilder("Anchored\n(Static)", () => SetBehavior("Anchored"))
                        .WithFlexGrow(1).WithMargin(2).WithBackgroundColor(Color.black).WithTextColor(Color.white).WithFontSize(10))
                );
        }

        /// <summary>
        /// Generates the diegetic role and background casting card.
        /// </summary>
        private IGuiProvider CreateCastingCard()
        {
            return new ForgeContainerBuilder("CastingCard")
                .WithMarginTop(15).WithPadding(10)
                .WithBackgroundColor(new Color(0.1f, 0.05f, 0.1f))
                .WithBorderWidth(1).WithBorderColor(Color.magenta)
                .AddChild(new ForgeLabelBuilder("WORLD INTEGRATION & CASTING").WithBold().WithColor(Color.magenta).WithMarginBottom(10))
                .AddChild(new ForgeContainerBuilder("Buttons").WithDirection(FlexDirection.Row).WithFlexWrap(Wrap.Wrap)
                    .AddChild(new ForgeButtonBuilder("Scenery / Background", () => SetRole("Background"))
                        .WithFlexGrow(1).WithMargin(2).WithBackgroundColor(Color.black).WithTextColor(Color.gray).WithFontSize(10))
                    .AddChild(new ForgeButtonBuilder("Informant / Quest Node", () => SetRole("Informant"))
                        .WithFlexGrow(1).WithMargin(2).WithBackgroundColor(new Color(0.2f, 0.2f, 0)).WithTextColor(Color.yellow).WithFontSize(10))
                    .AddChild(new ForgeButtonBuilder("Hostile Agent", () => SetRole("Hostile"))
                        .WithFlexGrow(1).WithMargin(2).WithBackgroundColor(new Color(0.2f, 0, 0)).WithTextColor(new Color(1f, 0.4f, 0.4f)).WithFontSize(10))
                );
        }

        private void SetBehavior(string behavior) => Debug.Log($"[The Tailor] Subject tethering profile set to: {behavior}");
        private void SetRole(string role) => Debug.Log($"[The Tailor] Subject role cast as: {role}");

        private void RefreshMirrors()
        {
            if (_stageContainer == null) return;

            // Wipe the existing stage (except the title label if you prefer, but we are clearing the layout container)
            _stageContainer.Clear();
            _stageContainer.Add(new ForgeLabelBuilder("TRIPLE-MIRROR OBSERVATION DECK")
                .WithColor(new Color(0.4f, 0.4f, 0.4f)).WithMargin(10).WithFontSize(10).CreateGui(_lastCtx));

            var mirrors = new ForgeContainerBuilder("MirrorRow")
                .WithDirection(FlexDirection.Row)
                .WithFlexGrow(1)
                .WithJustifyContent(Justify.SpaceBetween)
                .WithPadding(20);

            // Left Mirror (Angled, Auto-rotating profile)
            mirrors.AddChild(new ForgeContainerBuilder("LeftMirror")
                .WithFlexGrow(1).WithMarginRight(10).WithBorderColor(new Color(0.3f, 0.1f, 0.3f)).WithBorderWidth(2)
                .AddChild(CreateMirrorView(-45f, false)));

            // Center Mirror (Main, Controllable, Static forward)
            mirrors.AddChild(new ForgeContainerBuilder("CenterMirror")
                .WithFlexGrow(2).WithBorderColor(Color.magenta).WithBorderWidth(2)
                .AddChild(CreateMirrorView(0f, true)));

            // Right Mirror (Angled, Auto-rotating profile)
            mirrors.AddChild(new ForgeContainerBuilder("RightMirror")
                .WithFlexGrow(1).WithMarginLeft(10).WithBorderColor(new Color(0.3f, 0.1f, 0.3f)).WithBorderWidth(2)
                .AddChild(CreateMirrorView(45f, false)));

            _stageContainer.Add(mirrors.CreateGui(_lastCtx));
        }

        private VisualElement CreateMirrorView(float angleOffset, bool isMain = false)
        {
            var lmpb = new LiveModelPreviewBuilder(_entityCtx.ActiveModel)
                // If it's a side mirror, let it auto-rotate to provide profile views. If main, leave it static for mouse control.
                .WithAutoRotate(!isMain)
                .WithMouseControl(isMain);

            // Note: If your LiveModelPreviewBuilder supports an explicit starting angle offset, you would pass 'angleOffset' into it here.

            return lmpb.CreateGui(_lastCtx);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif

        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}