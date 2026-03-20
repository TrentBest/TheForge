using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop
{
   

    public class ViewerGuiProvider<T> : IGuiProvider where T : IViewer
    {
        private T viewedObject;
        private int dimensionality = 2;

        public ViewerGuiProvider(T viewedObject, int dimensionality=2)
        {
            this.viewedObject = viewedObject;
            this.dimensionality = Math.Max(1, dimensionality);
        }

        public string Title { get; set; } = "Viewer";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder($"{viewedObject.DisplayName}_Root")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100);

            // 1D Aspect: The Visual
            root.WithPanel("VisualPanel")
                .WithAutoGrow(true)
                .AddChild(viewedObject.GetVisualAspect())
            .EndPanel();

            // 2D Aspect: The Properties
            if (dimensionality >= 2)
            {
                root.WithPanel("DataPanel")
                    .WithSize(350, 0)
                    .WithBorderLeftWidth(1)
                    .WithBorderLeftColor(new Color(0.3f, 0.3f, 0.3f))
                    .AddChild(viewedObject.GetPropertiesAspect(2))
                .EndPanel();
            }

            return root.CreateGui(ctx);
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

    public interface IViewer
    {
        string DisplayName { get; }
        IGuiProvider GetVisualAspect();
        IGuiProvider GetPropertiesAspect(int dimension);
        int Dimensionality { get; }
    }
}
