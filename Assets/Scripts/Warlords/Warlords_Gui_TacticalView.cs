using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Warlords
{
    public class Warlords_Gui_TacticalView : IGuiProvider
    {
        public string Title => "WARLORDS: TACTICAL VIEW";

        private string _selectedUnitName = "None";
        private GuiContext _lastCtx;
        private IGuiRouter _router;

        private VisualElement _mapGridInstance;
        private VisualElement _infoPanelInstance;

        public Warlords_Gui_TacticalView(IGuiRouter router)
        {
            _router = router;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var rootBuilder = new GraphicalUserInterfaceBuilder("TacticalView_Root")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(Color.black)
                .OnBuild(ve => ve.style.flexGrow = 1);

            // 1. THE WORLD (Center/Left)
            var mapGridBuilder = new GraphicalUserInterfaceBuilder("MapGrid")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .OnBuild(ve => {
                    ve.style.flexGrow = 1;
                    ve.style.paddingTop = 10; ve.style.paddingLeft = 10;
                    ve.style.paddingBottom = 10; ve.style.paddingRight = 10;
                    _mapGridInstance = ve;
                    GeneratePlayableGrid(12, 12, ctx);
                });
            rootBuilder.AddChild(c => mapGridBuilder.Build());

            // 2. THE COMMAND CONSOLE (Right Side - Fixed 400px)
            var consoleBuilder = new GraphicalUserInterfaceBuilder("CommandConsole")
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .OnBuild(ve => {
                    ve.style.width = 400;
                    ve.style.borderLeftWidth = 3;
                    ve.style.borderLeftColor = Color.gray;
                });

            // Miniature Unit Card
            var infoPanelBuilder = new GraphicalUserInterfaceBuilder("InfoPanel")
                .OnBuild(ve => {
                    ve.style.paddingTop = 15; ve.style.paddingRight = 15;
                    ve.style.paddingBottom = 15; ve.style.paddingLeft = 15;
                    _infoPanelInstance = ve;
                    UpdateSidebar("Select a Unit", 0, ctx);
                });
            consoleBuilder.AddChild(c => infoPanelBuilder.Build());

            // Action Buttons
            var btnAreaBuilder = new GraphicalUserInterfaceBuilder("ButtonArea")
                .OnBuild(ve => {
                    ve.style.marginTop = Length.Auto();
                    ve.style.paddingTop = 20; ve.style.paddingLeft = 20;
                    ve.style.paddingBottom = 20; ve.style.paddingRight = 20;
                })
                .AddChild(c => new ForgeButtonBuilder("END TURN")
                    .WithHeight(50)
                    .WithBackgroundColor(new Color(0.5f, 0.1f, 0.1f))
                    .WithTextColor(Color.white)
                    .CreateGui(c))
                .AddChild(c => new ForgeButtonBuilder("SURRENDER (MAIN MENU)", () => _router?.NavigateTo("MainMenu"))
                    .WithHeight(30)
                    .WithMargin(10, 0, 0, 0)
                    .WithBackgroundColor(Color.black)
                    .WithTextColor(Color.gray)
                    .CreateGui(c));

            consoleBuilder.AddChild(c => btnAreaBuilder.Build());
            rootBuilder.AddChild(c => consoleBuilder.Build());

            return rootBuilder.Build();
        }

        private void GeneratePlayableGrid(int rows, int cols, GuiContext ctx)
        {
            if (_mapGridInstance == null) return;

            for (int r = 0; r < rows; r++)
            {
                var rowBuilder = new GraphicalUserInterfaceBuilder($"Row_{r}")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch);

                for (int c = 0; c < cols; c++)
                {
                    string tileText = "";
                    Action onClick = null;

                    if (r == 5 && c == 5)
                    {
                        tileText = "🛡️";
                        onClick = () => UpdateSidebar("Heavy Infantry", 12, ctx);
                    }
                    else if (r == 2 && c == 8)
                    {
                        tileText = "🏰";
                        onClick = () => UpdateSidebar("White Citadel", 0, ctx);
                    }

                    rowBuilder.AddChild(context => new ForgeButtonBuilder(tileText, onClick)
                        .WithWidth(60).WithHeight(60)
                        .WithMargin(1, 1, 1, 1)
                        .WithBackgroundColor(new Color(0.15f, 0.25f, 0.15f))
                        .CreateGui(context));
                }
                _mapGridInstance.Add(rowBuilder.Build());
            }
        }

        private void UpdateSidebar(string title, int st, GuiContext ctx)
        {
            if (_infoPanelInstance == null) return;
            _infoPanelInstance.Clear();

            _infoPanelInstance.Add(new ForgeLabelBuilder(title.ToUpper())
                .WithFontSize(20)
                .WithColor(Color.yellow)
                .WithFontStyle(FontStyle.Bold)
                .CreateGui(ctx));

            if (st > 0)
            {
                _infoPanelInstance.Add(new ForgeLabelBuilder($"STRENGTH: {st}")
                    .WithColor(Color.white)
                    .CreateGui(ctx));

                _infoPanelInstance.Add(new ForgeLabelBuilder("STATUS: Ready to Move")
                    .WithColor(Color.green)
                    .WithFontSize(10)
                    .CreateGui(ctx));

                _infoPanelInstance.Add(new ForgeButtonBuilder("MOVE STACK")
                    .WithMargin(20, 0, 0, 0)
                    .CreateGui(ctx));
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}