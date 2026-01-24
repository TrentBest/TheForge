#if UNITY_EDITOR
using Assets.Scripts;
using Assets.Scripts.Builders.GuiBuilders;
using UnityEngine.UIElements;

public class FsmCrudTabBuilder : IHubTabBuilder
{
    public string TabName => "FSM Definitions";
    public string TabIcon => "🛠️";

    public VisualElement CreateGui(GuiContext ctx)
    {
        // This is where we will hook up your FsmBuilderGui logic
        return new GraphicalUserInterfaceBuilder("FsmEditor")
            .WithTitle("FSM Blueprint Editor")
            .AddChild(new FsmBuilderGui())
            .Build();
    }
}
#endif