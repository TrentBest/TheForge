using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Corsair
{
    public class CorsairsInSpace_Gui_MainMenu : IGuiProvider
    {
        public string Title => "CORSAIRS IN SPACE: MAIN MENU";

        private GuiContext _lastCtx;
        private IGuiRouter _router;

        public CorsairsInSpace_Gui_MainMenu() { }
        public CorsairsInSpace_Gui_MainMenu(IGuiRouter router) { _router = router; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var builder = new GraphicalUserInterfaceBuilder("Corsairs_TrueMainMenu")
                .WithPercentSize(100, 100)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .WithBackgroundColor(Color.black);

            // Title
            builder.AddChild(new ForgeLabelBuilder("CORSAIRS IN SPACE")
                .WithFontSize(48)
                .WithColor(Color.cyan)
                .WithFontStyle(FontStyle.Bold)
                .WithMargin(0, 60)); // 60px bottom margin to space out the buttons

            // Options
            builder.AddChild(new ForgeButtonBuilder("START NEW CAMPAIGN", () => _router?.NavigateTo("InGameHub"))
                .WithWidth(400).WithHeight(60)
                .WithBackgroundColor(new Color(0.2f, 0.05f, 0.05f))
                .WithTextColor(Color.white)
                .WithFontSize(18).WithFontStyle(FontStyle.Bold)
                .WithMargin(0, 0, 20, 0));

            builder.AddChild(new ForgeButtonBuilder("CONTINUE SAVED GAME", () => ForgeLogger.Log("Load Game..."))
                .WithWidth(400).WithHeight(60)
                .WithBackgroundColor(new Color(0.05f, 0.15f, 0.05f))
                .WithTextColor(Color.white)
                .WithFontSize(18).WithFontStyle(FontStyle.Bold)
                .WithMargin(0, 0, 20, 0));

            builder.AddChild(new ForgeButtonBuilder("MULTIPLAYER PROTOCOLS", () => ForgeLogger.Log("Multiplayer Setup..."))
                .WithWidth(400).WithHeight(60)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.15f))
                .WithTextColor(Color.gray)
                .WithFontSize(18).WithFontStyle(FontStyle.Bold)
                .WithMargin(0, 0, 20, 0));

            return builder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}