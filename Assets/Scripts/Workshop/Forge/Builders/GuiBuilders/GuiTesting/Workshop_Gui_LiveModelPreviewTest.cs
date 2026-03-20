using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class Workshop_Gui_LiveModelPreviewTest : IGuiProvider
    {
        private GameObject targetObject;
        private LiveModelPreviewBuilder lmp;

        public string Title => "Live Model Preview Test";

        public VisualElement CreateGui(GuiContext ctx)
        {
           return lmp.CreateGui(ctx);
        }

        public void FromUIDocument(string assetPath)
        {
            lmp.FromUIDocument(assetPath);
        }

        public Action<VisualElement> GetGuiBuilder()
        {
            return lmp.GetGuiBuilder();
        }

        public void ToUIDocument(string assetPath)
        {
             lmp.ToUIDocument(assetPath);
        }

        public Workshop_Gui_LiveModelPreviewTest()
        {
            targetObject = GameObject.Instantiate(GameObject.Find("Hermit"));
            lmp = new LiveModelPreviewBuilder(targetObject);

        }
    }
}