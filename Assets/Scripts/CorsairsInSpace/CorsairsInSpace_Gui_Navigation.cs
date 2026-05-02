using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Assets.Scripts.CorsairsInSpace;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Corsair
{
    public class CorsairsInSpace_Gui_Navigation : MonoBehaviour, IGuiProvider
    {
        public string Title => "System Navigation";

        private CorsairLairContext _lairContext;
        private string _selectedTarget = "None";

        public CorsairsInSpace_Gui_Navigation(IGuiRouter r)
        {
        }

        public VisualElement CreateGui(GuiContext context)
        {
            _lairContext = FindAnyObjectByType<CorsairLairContext>();

            var builder = new GraphicalUserInterfaceBuilder("NavigationRoot")
                .WithPadding(10)
                .AddChild(new ForgeLabelBuilder("SYSTEM NAVIGATION & RAID SELECT").WithFontSize(20).WithColor(Color.cyan))
                .AddChild(new GraphicalUserInterfaceBuilder("MapContainer").WithHeight(400).WithBackgroundColor(Color.black))
                .AddChild(new ForgeLabelBuilder("No target selected.").WithFontSize(14).WithColor(Color.white))
                .AddChild(new ForgeButtonBuilder("LAUNCH PIRATE OPERATION", LaunchRaid));

            var root = builder.Build();

            root.schedule.Execute(() => FSM_API.Interaction.Update("CorsairNav_UI")).Every(33);

            return root;
        }

        private void LaunchRaid()
        {
            if (_selectedTarget == "None") return;
            var op = new PirateOperation { OperationName = $"Raid on {_selectedTarget}", Status = "In Transit" };
            _lairContext?.ActiveOperations.Add(op);
            if (_lairContext != null) _lairContext.GlobalHeat += 5.0f;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}