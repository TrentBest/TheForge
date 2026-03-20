using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Builders.Staging
{
    public class StageBuilder : MonoBehaviour, IForgeBuilder
    {
        public string ToolName { get; } = "Stage Builder";

        public object Build()
        {
            throw new NotImplementedException();
        }

        public IGuiProvider GetGuiProvider()
        {
            throw new NotImplementedException();
        }

        public Type GetProductType()
        {
            throw new NotImplementedException();
        }
    }
}