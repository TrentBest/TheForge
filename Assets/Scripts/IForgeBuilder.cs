using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{

    // The contract for anything that can generate a Game Object / Data Construct
    public interface IBuilder
    {
        // Returns the runtime instance (GameObject, or a Data Container)
        object Build();
    }

    // A Builder that knows how to draw itself in your Editor
    public interface IForgeBuilder : IBuilder
    {
        IGuiBuilder GetGuiBuilder();
        string Name { get; }
    }

    // A GuiBuilder knows how to populate a VisualElement context
    public interface IGuiBuilder : IBuilder
    {
        VisualElement Build();
    }
}