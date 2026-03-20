using UnityEngine.UIElements;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace Assets.Editor.raWWar
{
    public class StatusModule : IEditorModule
    {
        public string ModuleName => "Status";
        public VisualElement CreateGui()
        {
            var builder = new GraphicalUserInterfaceBuilder("Status")
                .WithPadding(20);
            builder.AddChild(new Label("GALAXY OVERVIEW") { style = { fontSize = 24 } });
            builder.AddChild(new Label("Active Fronts: 1,240\nTotal Soldiers: 45,000,000"));
            return builder.Build();
        }
    }
}