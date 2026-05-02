using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.Staging;

namespace Workshop.UI_And_Tools.Showcase.Portals
{
    public class TheForge_ShowcasePortal : IShowcasePortal
    {
        private IGuiRouter _router;

        // --- IShowcasePortal Implementation ---
        public string Title => "The Singularity Forge";
        public string Category => "CORE SYSTEMS";
        public string Overview => "The autopoietic nexus. Define reality parameters, establish sensory pipelines, and manifest diegetic toolchains via Micro-Packages.";
        public List<string> CoreTechnologies => new List<string> { "MicroPackage Arbitrator", "Reflective Routing", "Diegetic Manifestation" };

        public bool IsDiscoverable => true;
        public bool IsUnderConstruction => false;
        public string TelemetryState => "AWAITING NEXUS CONFIGURATION";

        // Replaced NotImplementedException with a thematic "Forge Amber"
        public Color AccentColor => new Color(1.0f, 0.4f, 0.0f);

        public void InjectRouter(IGuiRouter router)
        {
            _router = router;

            // Register the actual Nexus GUI into the router so we can navigate to it
            if (_router is ReflectiveRouteGuiBuilder reflectiveRouter)
            {
                reflectiveRouter.RegisterRoute("ForgeNexus", r => new MicroPackageNexusGui(r));
            }
        }

        public void Launch()
        {
            Debug.Log("[TheForge_Portal] Launching Forge Nexus...");
            // Tell your master router to swap the content area to the Nexus GUI
            _router?.NavigateTo("ForgeNexus");
        }

        // Fulfilling the IGuiProvider interface required by IShowcasePortal 
        // This returns the "Data Card" you built in Showcase_ExperiencePortal
        public VisualElement CreateGui(GuiContext ctx)
        {
            var def = new ShowcaseExperienceDef
            {
                Title = Title,
                Overview = Overview,
                CoreTechnologies = CoreTechnologies,
                IsUnderConstruction = IsUnderConstruction,
                OnLaunchExperience = Launch,
                OnInspectArchitecture = () => Debug.Log("Inspect Architecture triggered for The Forge.")
            };

            return new Showcase_ExperiencePortal(def).CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}