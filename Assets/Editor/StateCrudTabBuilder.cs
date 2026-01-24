#if UNITY_EDITOR
using Assets.Scripts;
using Assets.Scripts.Builders.GuiBuilders;
using UnityEngine.UIElements;

public class StateCrudTabBuilder : IHubTabBuilder
{
    public string TabName => "States";
    public string TabIcon => "🧩";

    public VisualElement CreateGui(GuiContext ctx)
    {
        return new GraphicalUserInterfaceBuilder("StateEditor")
            .WithTitle("Reusable State Library")
            .AddChild(new StateGuiBuilder())
            .Build();
    }
}
#endif