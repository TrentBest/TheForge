using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop
{
    /// <summary>
    /// A generic GUI provider that acts as a standardized "Lens" for any object implementing IViewer.
    /// Supports multi-modal rendering by splitting the UI into Visual (1D) and Property (2D) panels.
    /// </summary>
    public class ViewerGuiProvider<T> : IGuiProvider where T : IViewer
    {
        private T viewedObject;
        private int dimensionality = 2;

        public ViewerGuiProvider(T viewedObject, int dimensionality = 2)
        {
            this.viewedObject = viewedObject;
            this.dimensionality = Math.Max(1, dimensionality);
        }

        public string Title { get; set; } = "Viewer";

        /// <summary>
        /// Orchestrates the multi-panel layout using the GraphicalUserInterfaceBuilder.
        /// </summary>
        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder($"{viewedObject.DisplayName}_Root")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100);

            // 1D Aspect: The Visual (Primary focus area)
            root.WithPanel("VisualPanel")
                .WithAutoGrow(true)
                .AddChild(viewedObject.GetVisualAspect())
            .EndPanel();

            // 2D Aspect: The Properties (Sidepanel for data/CRUD)
            if (dimensionality >= 2)
            {
                root.WithPanel("DataPanel")
                    .WithWidth(350) // Fixed width for property panels
                    .WithBorderLeftWidth(1)
                    .WithBorderLeftColor(new Color(0.3f, 0.3f, 0.3f, 1f))
                    .AddChild(viewedObject.GetPropertiesAspect(2))
                .EndPanel();
            }

            return root.CreateGui(ctx);
        }

        /// <summary>
        /// Returns a delegate that allows this generic viewer to be injected into other Forge panels.
        /// </summary>
        public Action<VisualElement> GetGuiBuilder()
        {
            return (root) => root.Add(CreateGui(new GuiContext()));
        }

        #region --- Serialization & UIDocuments ---
#if UNITY_EDITOR
        /// <summary>
        /// Bakes the current state of the viewer into a UXML asset.
        /// </summary>
        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(new GuiContext());
            GraphicalUserInterfaceBuilder.ConvertToUIDocument(root, assetPath);
        }

        public void FromUIDocument(string assetPath)
        {
            // Hydration for generic viewers usually happens via the ViewedObject data context.
            ForgeLogger.Log($"[ViewerGuiProvider] Metadata sync from {assetPath} for {viewedObject.DisplayName}");
        }
#else
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
#endif
        #endregion
    }

    public interface IViewer
    {
        string DisplayName { get; }
        IGuiProvider GetVisualAspect();
        IGuiProvider GetPropertiesAspect(int dimension);
        int Dimensionality { get; }
    }
}