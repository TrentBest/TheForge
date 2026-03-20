using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Warlords
{
    public class Warlords_Gui_InGame : IGuiProvider
    {
        public string Title => "WARLORDS: ILLURIA";
        private GuiContext _lastCtx;

        private VisualElement _popupLayer;
        private VisualElement _mapViewport;
        private ModalOverlayBuilder _stackModal;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            // 1. HARDENED ROOT: Ensure we have a strict 100% boundary immediately
            var rootBuilder = new GraphicalUserInterfaceBuilder("Warlords_InGame_Root")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .OnBuild(ve => {
                    ve.style.width = Length.Percent(100);
                    ve.style.height = Length.Percent(100);
                    ve.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
                });

            // 2. MIDDLE SECTION
            var middleSplit = new SplitPanelBuilder(310, Side.Right);

            middleSplit.WithMain(new GraphicalUserInterfaceBuilder("MapViewport")
                .WithBackgroundColor(new Color(0.15f, 0.25f, 0.15f))
                .OnBuild(ve => {
                    _mapViewport = ve;
                    ve.style.flexGrow = 1;
                })
                .AddChild(new Label("[ MAP RENDER TARGET ]") { style = { alignSelf = Align.Center, marginTop = Length.Percent(40), color = Color.white, opacity = 0.5f } })
            );

            middleSplit.WithSidebar(new GraphicalUserInterfaceBuilder("InGame_Sidebar")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                .AddChild(CreateCommandStripBuilder().Build())
                .AddChild(new GraphicalUserInterfaceBuilder("MiniMap_Area")
                    .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                    .OnBuild(ve => { ve.style.flexGrow = 1; ve.style.paddingLeft = 10; ve.style.paddingRight = 10; })
                    .AddChild(new Label("WORLD MAP") { style = { unityTextAlign = TextAnchor.MiddleCenter, color = Color.gray, marginTop = 10 } })
                    .Build())
            );

            rootBuilder.AddChild(middleSplit.CreateGui(ctx));
            rootBuilder.AddChild(CreateClassicBottomBarBuilder().Build());

            var finalRoot = rootBuilder.Build();

            // 3. SAFETY OVERLAY: Use a deferred execution to add the modal
            // This lets the main layout finish its "math" before we slap an absolute overlay on top
            finalRoot.schedule.Execute(() => {
                var stackContent = new GraphicalUserInterfaceBuilder("StackContent")
                    .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                    .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                    .OnBuild(ve => { ve.style.width = 400; ve.style.paddingTop =20; ve.style.paddingBottom = 20; ve.style.paddingRight = 20; ve.style.paddingLeft = 20; })
                    .AddChild(new Label("UNIT STACK") { style = { color = Color.yellow } })
                    .AddButton("CLOSE", () => _stackModal.Hide());

                _stackModal = new ModalOverlayBuilder(stackContent)
                    .WithBackdropColor(new Color(0, 0, 0, 0.8f));

                var modalElement = _stackModal.CreateGui(ctx);
                _stackModal.Hide();
                finalRoot.Add(modalElement);
            }).ExecuteLater(50); // Wait 50ms for layout stability

            return finalRoot;
        }

        private GraphicalUserInterfaceBuilder CreateCommandStripBuilder()
        {
            return new GraphicalUserInterfaceBuilder("CommandStrip")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Center)
                .WithBackgroundColor(new Color(0.45f, 0.45f, 0.45f)) // Classic Grey Metal
                .OnBuild(ve => {
                    ve.style.width = 60;
                    ve.style.borderRightWidth = 2; ve.style.borderRightColor = Color.black;
                    ve.style.paddingTop = 10;
                })
                .AddChild(CreateStripButton("⚔️", null))
                .AddChild(CreateStripButton("❓", null))
                .AddChild(CreateStripButton("Ctr", null))
                .AddChild(CreateStripButton("Nxt", ShowStackSelectionPopup))
                .AddChild(new GraphicalUserInterfaceBuilder("Spacer").OnBuild(ve => ve.style.flexGrow = 1).Build())
                .AddChild(CreateStripButton("Quit", () => Debug.Log("Exit Game Requested")));
        }

        private VisualElement CreateStripButton(string label, Action onClick)
        {
            // Using a simple Button for the strip since it's a primitive, 
            // but styled to match the classic look
            var btn = new Button { text = label };
            btn.style.width = 50; btn.style.height = 45;
            btn.style.marginBottom = 5;
            btn.style.fontSize = 13;
            btn.style.backgroundColor = new Color(0.35f, 0.35f, 0.35f);
            btn.style.borderTopColor = Color.white; btn.style.borderLeftColor = Color.white;
            btn.style.borderBottomColor = Color.black; btn.style.borderRightColor = Color.black;
            btn.style.borderTopWidth = 1; btn.style.borderLeftWidth = 1;
            btn.style.borderBottomWidth = 3; btn.style.borderRightWidth = 3;

            if (onClick != null) btn.clicked += onClick;
            return btn;
        }

        private GraphicalUserInterfaceBuilder CreateClassicBottomBarBuilder()
        {
            return new GraphicalUserInterfaceBuilder("BottomStatsBar")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithBackgroundColor(new Color(0.3f, 0.3f, 0.3f))
                .OnBuild(ve => {
                    ve.style.height = 140;
                    ve.style.borderTopWidth = 4; ve.style.borderTopColor = Color.black;
                    ve.style.paddingLeft = 20; ve.style.paddingRight = 20;
                })
                // Active Unit Section
                .AddChild(new GraphicalUserInterfaceBuilder("UnitDisplay")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                    .AddChild(new Label("SELECTED UNIT: ") { style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold } })
                    .AddChild(new Label("HERO (Lvl 1)") { style = { color = Color.white, marginLeft = 10 } })
                    .Build())

                // Center Action Section
                .AddChild(new GraphicalUserInterfaceBuilder("CenterProduction")
                    .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                    .AddButton("CITY PRODUCTION", () => Debug.Log("Opening City Screen..."))
                    .AddChild(new Label("Current: Light Infantry (2 turns)") { style = { color = Color.gray, fontSize = 11, marginTop = 5 } })
                    .Build())

                // Gold / Turn display
                .AddChild(new Label("💰 540 (+24)") { style = { fontSize = 22, color = new Color(1, 0.8f, 0), unityFontStyleAndWeight = FontStyle.Bold } });
        }

        private void ShowStackSelectionPopup()
        {
            // Clear current modal content
            _popupLayer.Clear();
            _popupLayer.style.display = DisplayStyle.Flex;

            var modalContent = new GraphicalUserInterfaceBuilder("StackModal")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                .OnBuild(ve => {
                    ve.style.width = 450;
                    ve.style.paddingTop = 25; ve.style.paddingBottom = 25;
                    ve.style.paddingLeft = 25; ve.style.paddingRight = 25;
                    ve.style.borderTopWidth = 2; ve.style.borderTopColor = Color.white;
                })
                .AddChild(new Label("SELECT UNITS TO MOVE AS STACK")
                {
                    style = { color = Color.yellow, marginBottom = 20, unityFontStyleAndWeight = FontStyle.Bold, unityTextAlign = TextAnchor.MiddleCenter }
                })
                .AddButton("GROUP 1: Cavalry x2, Infantry x4", () => ClosePopup())
                .AddButton("GROUP 2: Hero, Dragon", () => ClosePopup())
                .AddSeparator()
                .AddButton("CANCEL", ClosePopup)
                .Build();

            _popupLayer.Add(modalContent);
        }

        private void ClosePopup() => _popupLayer.style.display = DisplayStyle.None;

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}