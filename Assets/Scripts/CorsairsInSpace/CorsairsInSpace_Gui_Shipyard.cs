using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Assets.Scripts.CorsairsInSpace;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Corsair
{
    public class CorsairsInSpace_Gui_Shipyard : MonoBehaviour, IGuiProvider
    {
        public string Title => "Shipyard Console";

        private CorsairLairContext _lairContext;
        private string _selectedClass = "Shuttle";

        public CorsairsInSpace_Gui_Shipyard(IGuiRouter r)
        {
        }

        public VisualElement CreateGui(GuiContext context)
        {
            _lairContext = FindAnyObjectByType<CorsairLairContext>();

            var listBuilder = new GraphicalUserInterfaceBuilder("ShipyardList")
                .WithPadding(5)
                .AddChild(new ForgeLabelBuilder("AVAILABLE CHASSIS").WithFontSize(16).WithColor(Color.white));

            foreach (var shipClass in _lairContext?.UnlockedShipClasses ?? new List<string> { "Shuttle" })
            {
                string currentClass = shipClass;
                listBuilder.AddChild(new ForgeButtonBuilder(currentClass, () => _selectedClass = currentClass));
            }

            var detailBuilder = new GraphicalUserInterfaceBuilder("ShipyardDetails")
                .WithPadding(15)
                .AddChild(new ForgeLabelBuilder($"DESIGN: {_selectedClass}").WithFontSize(18).WithColor(Color.yellow))
                // Box Representation via Builder
                .AddChild(new GraphicalUserInterfaceBuilder("ShipPreview").WithHeight(300).WithBackgroundColor(Color.black))
                .AddChild(new ForgeLabelBuilder("STATUS: READY FOR PRODUCTION").WithFontSize(12).WithColor(Color.green))
                .AddChild(new ForgeButtonBuilder("COMMENCE PRODUCTION", () => Debug.Log($"Building {_selectedClass}...")));

            // Pure flex row composition instead of raw SplitPanel for guaranteed compilation
            var root = new GraphicalUserInterfaceBuilder("ShipyardRoot")
                .WithFlexLayout(FlexDirection.Row)
                .WithAutoGrow(true)
                .AddChild(listBuilder)
                .AddChild(detailBuilder)
                .Build();

            root.schedule.Execute(() => FSM_API.Interaction.Update("Shipyard_UI")).Every(33);

            return root;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}