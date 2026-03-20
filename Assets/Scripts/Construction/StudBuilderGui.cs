using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Construction
{
    public class StudBuilderGui : IGuiProvider
    {
        public string Title => throw new NotImplementedException();

        public VisualElement CreateGui(GuiContext ctx)
        {
            throw new NotImplementedException();
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
}
