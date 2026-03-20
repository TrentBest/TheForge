using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Builders.Timing
{
    public class TimeBuilder : MonoBehaviour, IForgeBuilder
    {
        public string ToolName { get; } = "Time Builder";

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

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}