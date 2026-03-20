#if UNITY_EDITOR
using Assets.Editor.Singularity;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using UnityEngine.UIElements;

public class LogicCrudTabBuilder : IGuiProvider
{
    public string TabName => "Logic & Transitions";
    public string TabIcon => "⚡";

    public string Title => TabName;

    public VisualElement CreateGui(GuiContext ctx)
    {
        return new GraphicalUserInterfaceBuilder("LogicEditor")
            .WithTitle("Transitions, Actions & Conditions")
            .AddChild(new Label("TODO: Insert LogicRegistryGui logic here."))
            .Build();
    }

    public void FromUIDocument(string assetPath)
    {
        throw new NotImplementedException();
    }

    public Action<VisualElement> GetGuiBuilder()
    {
        throw new NotImplementedException();
    }

    public void ToUIDocument(string assetPath)
    {
        throw new NotImplementedException();
    }
}
#endif