using System;
using System.Collections.Generic;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    // The data schema used by the API and the Builder
    public static class ForgeFilterWindow
    {
        public class Element
        {
            public int level;
            public string name;
            public object userData;

            public Element(int level, string name)
            {
                this.level = level;
                this.name = name;
            }
        }

        public class GroupElement : Element
        {
            public GroupElement(int level, string name) : base(level, name) { }
        }
    }

    public interface IFilterWindowProvider
    {
        void CreateComponentTree(List<ForgeFilterWindow.Element> tree);
        bool GoToChild(ForgeFilterWindow.Element element, bool addIfComponent);
    }
}