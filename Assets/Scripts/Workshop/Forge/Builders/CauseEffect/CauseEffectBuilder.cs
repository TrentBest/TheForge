using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Builders.CauseEffect
{
    public class CauseEffectBuilder : MonoBehaviour, IForgeBuilder
    {
        public string ToolName { get; } = "Cause & Effect Builder";

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