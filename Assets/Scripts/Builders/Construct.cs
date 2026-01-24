using Assets.Scripts.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

namespace Assets.Scripts.Builders
{
    public class Construct
    {
        /// <summary>
        /// the data the construct holds as a string, the key is the ConstructDataType
        /// </summary>
        private Dictionary<int, string> data;

        public Construct(Dictionary<int, string> data)
        {
            this.data = data;
        }


        //API
        static public ConstructBuilder Create(string constructClassification)
        {
            return new ConstructBuilder(constructClassification);
        }
    }

    public class ConstructBuilder : IForgeBuilder
    {
        public ConstructBuilder(string constructClassification)
        {
        }

        public string Name => throw new NotImplementedException();

        public object Build()
        {
            throw new NotImplementedException();
        }

        public IGuiBuilder GetGuiBuilder()
        {
            //This is the $$$$$
            throw new NotImplementedException();
        }
    }

    public class ConstructClassification
    {
        public string Name { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> Fields { get; set;  } = new Dictionary<string, object>();

        /// <summary>
        /// key is process group, and the list of strings are the FSM definitions it consumes.
        /// </summary>
        public Dictionary<string, List<string>> Behaviors { get; set; } = new Dictionary<string, List<string>>();
    }

    public class ConstructClassificationBuilder : IForgeBuilder
    {
        public string Name => throw new NotImplementedException();

        public object Build()
        {
            throw new NotImplementedException();
        }

        public IGuiBuilder GetGuiBuilder()
        {
            return new ConstructClassificationGuiBuilder();
        }
    }

    public class ConstructClassificationGuiBuilder : IGuiBuilder
    {
        public VisualElement Build()
        {
            return new GraphicalUserInterfaceBuilder("ConstructBuilder").WithPanel("Properties", true)
                .WithPanel("PropertiesList").WithScrollable(true, ScrollViewMode.VerticalAndHorizontal)
                
                .ContinueWithParentPanel().WithPanel("Fields", true).WithScrollable(true, ScrollViewMode.VerticalAndHorizontal)
                .ContinueWithParentPanel().WithPanel("Behaviors", true).AddChild(new FsmBuilderGui()).Build();
        }

        object IBuilder.Build()
        {
            return Build();
        }
    }
}
