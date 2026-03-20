using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Armada2525
{
    public class Armada2525_Gui_RaceSelection : IGuiProvider
    {
        public string Title => "RACE SELECTION";
        private Action _onCustomRaceClicked;
        private Action _onLaunchClicked;
        private Action _onBackClicked;

        private string _selectedRace = "Terran Federation";

        // REQUIRED BY ROUTER
        public Armada2525_Gui_RaceSelection(Action onCustomRaceClicked = null, Action onLaunchClicked = null, Action onBackClicked = null)
        {
            _onCustomRaceClicked = onCustomRaceClicked;
            _onLaunchClicked = onLaunchClicked;
            _onBackClicked = onBackClicked;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var races = new List<string> { "Terran Federation", "Zolarian Empire", "Kaelen Hive", "Synthetica Collective" };

            return new GraphicalUserInterfaceBuilder("RaceSelectionPanel")
                .WithPadding(40).WithWidth(600)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f, 0.9f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Stretch)

                .AddChild(new Label("CHOOSE YOUR FACTION") { style = { color = Color.cyan, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20, alignSelf = Align.Center } })

                // Pre-Made Faction Dropdown
                .AddDropdownData("Standard Factions", races, races.IndexOf(_selectedRace), v => _selectedRace = v)

                .AddChild(new Label("Select a core faction above, or design a custom genetic lineage from scratch.") { style = { color = Color.gray, fontSize = 12, marginTop = 10, marginBottom = 20 } })

                .AddButton("CREATE CUSTOM RACE", () => _onCustomRaceClicked?.Invoke())

                .AddSeparator()

                .AddChild(new GraphicalUserInterfaceBuilder("BottomNav")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithMarginTop(15)
                    .AddButton("<< BACK", () => _onBackClicked?.Invoke())
                    .AddButton("LAUNCH GALAXY >>", () => _onLaunchClicked?.Invoke())
                    .Build())

                .Build();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}