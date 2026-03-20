using TheSingularityWorkshop.Builders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PartViewerProvider : IGuiProvider
{
    private ConstructClassification _currentPart;
    private Color _appliedColor;
    private GameObject _contextObject;
    public PartViewerProvider(ConstructClassification part, Color themeColor)
    {
        _currentPart = part;
        _appliedColor = themeColor;
    }

    // Standard title implementation seen in your other builders
    public string Title => $"{_currentPart.Name} Viewer";

    public VisualElement CreateGui(GuiContext ctx)
    {
        // Executes the builder to return the actual UIElements panel
        return new VisualElement();
    }

    //public GraphicalUserInterfaceBuilder GetGuiBuilder()
    //{
        // Utilizes your LiveModelPreviewBuilder to create the rotating "Toy Room" display
        // The .WithModel call expects a mesh name matching the classification
       // return new LiveModelPreviewBuilder(_contextObject)
            //.WithModel(_currentPart.Name)
           // .WithColor(_appliedColor)
           // .WithAutoRotation(true)
           // .WithSize(300, 300);
   // }

    // These methods are typically left as placeholders in your Forge provider implementations
    public void FromUIDocument(string assetPath) { throw new NotImplementedException(); }
    public void ToUIDocument(string assetPath) { throw new NotImplementedException(); }

    // Explicit interface implementation to return the Action-based builder if required by your UI system
    Action<VisualElement> IGuiProvider.GetGuiBuilder()
    {
        return (root) => root.Add(CreateGui(null));
    }
}