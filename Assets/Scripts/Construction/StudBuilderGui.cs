using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

namespace Assets.Scripts.Construction
{
    class StudBuilderGui : IGuiProvider
    {
        public VisualElement CreateGui(GuiContext ctx)
        {
            VisualElement root = new VisualElement() { name = "StudBuilderGui" };

            return root;
        }
    }
}
