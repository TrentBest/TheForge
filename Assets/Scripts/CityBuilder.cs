 
using System.Collections.Generic;
using UnityEngine;
using Workshop.Core.Diagnostics;
using Workshop.GURPS.Construction;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop
{
    public class CityBuilder
    {
        private string cityName;
        private Vector3 siteOrigin; // The "Global" coordinate for this city
        private List<BuildingBuilder> buildings = new List<BuildingBuilder>();
        private string assetPackage;

        public CityBuilder(string cityName)
        {
            this.cityName = cityName;
            this.siteOrigin = Vector3.zero; // Default origin
        }

        public CityBuilder WithSiteOrigin(Vector3 origin)
        {
            this.siteOrigin = origin;
            return this;
        }

        public CityBuilder LinkToAsset(string packageName, string variant)
        {
            this.assetPackage = $"{packageName}_{variant}";
            // Intent: Interoperability with low-poly or high-poly bundles
            return this;
        }

        public CityBuilder WithBuilding(BuildingBuilder building)
        {
            buildings.Add(building);
            return this;
        }

        public void Build()
        {
            ForgeLogger.Log($"Constructing {cityName} at {siteOrigin} with {buildings.Count} buildings.");
            // Logic to instantiate the site context and iterate through building offsets
        }

        public GraphicalUserInterfaceBuilder GetGuiBuilder()
        {
            return new GraphicalUserInterfaceBuilder($"{cityName}_CityEditor")
                .WithTitle($"City Editor: {cityName}")
                .AddStringData("City Name", cityName, name => cityName = name)
                .WithPanel("Infrastructure")
                    //.AddChild(ctx => new Label("Site Coordination Data"))
                .EndPanel();
        }
    }
}