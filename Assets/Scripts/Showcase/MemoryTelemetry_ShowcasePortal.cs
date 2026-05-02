using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.Staging;

namespace Workshop.UI_And_Tools.Showcase.Portals
{
    public class MemoryTelemetry_ShowcasePortal : IShowcasePortal
    {
        private IGuiRouter _router;

        public string Title => "Zero-GC Memory Engine";
        public string Category => "CORE SYSTEMS";
        public string Overview => "Live visualization of the FSM_Memory contiguous class allocation and heuristic indexing engine. Proving zero-friction, scalable architecture.";
        public List<string> CoreTechnologies => new List<string> { "Contiguous Allocation", "O(1) Indexing", "x2 Heuristic Expansion" };

        public bool IsDiscoverable => true;
        public bool IsUnderConstruction => false;
        public string TelemetryState => "ANALYZING HEAP INTEGRITY";

        public Color AccentColor => new Color(0.2f, 1.0f, 0.2f); // Neon Green for memory/data

        public void InjectRouter(IGuiRouter router)
        {
            _router = router;

            if (_router is ReflectiveRouteGuiBuilder reflectiveRouter)
            {
                reflectiveRouter.RegisterRoute("MemoryTelemetryView", r => new Showcase_MemoryTelemetry());
            }
        }

        public void Launch()
        {
            Debug.Log("[MemoryTelemetry_Portal] Engaging Memory Dashboard...");
            _router?.NavigateTo("MemoryTelemetryView");
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var def = new ShowcaseExperienceDef
            {
                Title = Title,
                Overview = Overview,
                CoreTechnologies = CoreTechnologies,
                IsUnderConstruction = IsUnderConstruction,
                OnLaunchExperience = Launch,
                OnInspectArchitecture = () => Debug.Log("Inspect Architecture triggered for Memory Engine.")
            };

            return new Showcase_ExperiencePortal(def).CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}