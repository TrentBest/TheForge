using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// An Adapter that allows raw VisualElements or Lambdas to participate in the IGuiProvider ecosystem.
    /// Reforged to support both Factory patterns and Instance wrapping.
    /// </summary>
    public class DynamicGuiProvider : IGuiProvider
    {
        private readonly Func<GuiContext, VisualElement> _factory;
        private GuiContext _lastCtx;

        public string Title => "Dynamic UI Container";

        // Constructor for Factory Pattern (Lazy)
        public DynamicGuiProvider(Func<GuiContext, VisualElement> factory)
        {
            _factory = factory;
        }

        // Fix for CS1503: Constructor for Instance Pattern (Immediate)
        public DynamicGuiProvider(VisualElement existingElement)
        {
            _factory = (ctx) => existingElement;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            return _factory?.Invoke(ctx) ?? new VisualElement();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath)
        {
            var elementToSave = CreateGui(new GuiContext());
            if (elementToSave != null)
                GraphicalUserInterfaceBuilder.ConvertToUIDocument(elementToSave, assetPath);
        }
#endif

        public void FromUIDocument(string assetPath)
        {
            _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        }
    }
}