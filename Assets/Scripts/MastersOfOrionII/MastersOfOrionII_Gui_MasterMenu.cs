using Assets.Scripts.MastersOfOrionII;
using System;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_MasterMenu : IGuiProvider
    {
        public string Title => "ARMADA 2525: BOOT SEQUENCE";

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Here is your pure, enum-free Mediator configuration!
            // We wire up the "Stupid GUIs" exclusively through these Action delegates.
            var router = new GuiFlowRouterBuilder(Title, initialRoute: "MainMenu")

                .AddRoute("MainMenu", r => new MastersOfOrionII_Gui_MainMenu(
                    onGenesisClicked: () => r.NavigateTo("ScenarioSetup")
                ))

                .AddRoute("ScenarioSetup", r => new MastersOfOrionII_Gui_ScenarioSetup(
                    onContinueClicked: () => r.NavigateTo("RaceSelection"),
                    onBackClicked: () => r.NavigateTo("MainMenu")
                ))

                .AddRoute("RaceSelection", r => new MastersOfOrionII_Gui_RaceSelection(
                    onCustomRaceClicked: () => r.NavigateTo("CustomRaceDesigner"),
                    onLaunchClicked: () => r.NavigateTo("InGameOrienation"),
                    onBackClicked: () => r.NavigateTo("ScenarioSetup")
                ))

                .AddRoute("CustomRaceDesigner", r => new MastersOfOrionII_Gui_RaceDesigner(
                    onSaveAndLaunchClicked: () => r.NavigateTo("InGameOrienation"),
                    onBackClicked: () => r.NavigateTo("RaceSelection")
                ))
                .AddRoute("InGameHUD", r => new MastersOfOrionII_Gui_InGameHUD(
                    onQuitToMainMenu: () => r.NavigateTo("MainMenu")
                ))

                // Future implementations waiting to be hooked up!
                .AddRoute("InGameOrienation", r => new DummyScreen("ORIENTATION: WELCOME TO THE GALAXY",
                    onNext: () => r.NavigateTo("InGameHUD")))

                .AddRoute("InGameHUD", r => new DummyScreen("IN GAME HUD (SIMULATION RUNNING)",
                    onNext: () => r.NavigateTo("MainMenu")));

            // The router handles everything from here.
            return router.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }

    // A quick dummy screen so you can test the full loop without writing the real GUIs yet
    public class DummyScreen : IGuiProvider
    {
        public string Title { get; }
        private Action _onNext;

        public DummyScreen(string title, Action onNext) { Title = title; _onNext = onNext; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("Dummy")
                .WithPadding(40).WithBackgroundColor(new UnityEngine.Color(0.1f, 0.1f, 0.15f))
                .AddChild(new Label(Title) { style = { color = UnityEngine.Color.yellow, fontSize = 24, marginBottom = 20 } })
                .AddButton("CONTINUE >>", () => _onNext?.Invoke())
                .Build();
        }
        public Action<VisualElement> GetGuiBuilder() => (r) => r.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string p) { }
        public void FromUIDocument(string p) { }
    }
}