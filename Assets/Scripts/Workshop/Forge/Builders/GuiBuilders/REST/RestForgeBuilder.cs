// File: Assets/Scripts/Workshop/Forge/Builders/GuiBuilders/REST/RestForgeBuilder.cs
using System;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders.REST
{
    public class RestForgeBuilder : MonoBehaviour, IForgeBuilder
    {
        public string ToolName => "REST_API_Configurator";

        // This builder produces API Definitions that other nodes can consume
        public Type GetProductType() => typeof(RestApiDefinition);

        public IGuiProvider GetGuiProvider()
        {
            return new RestConfiguratorGuiProvider();
        }

        public object Build()
        {
            throw new NotImplementedException();
        }
    }
}