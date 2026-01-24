#if UNITY_EDITOR
using Assets.Scripts;
using Assets.Scripts.Builders.GuiBuilders;
using UnityEngine.UIElements;

public class LogicCrudTabBuilder : IHubTabBuilder
{
    public string TabName => "Logic & Transitions";
    public string TabIcon => "⚡";

    public VisualElement CreateGui(GuiContext ctx)
    {
        return new GraphicalUserInterfaceBuilder("LogicEditor")
            .WithTitle("Transitions, Actions & Conditions")
            .AddChild(new Label("TODO: Insert LogicRegistryGui logic here."))
            .Build();
    }
}
#endif