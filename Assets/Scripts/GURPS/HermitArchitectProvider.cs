using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    public class HermitArchitectProvider : IGuiProvider
    {
        public string Title => "HERMIT ARCHITECT :: AUTOPHONIC GENERATOR";

        private List<string> _constraints = new List<string>(); // The "Isnts"

        public VisualElement CreateGui(GuiContext ctx)
        {
            var builder = new GraphicalUserInterfaceBuilder("HermitArchitect")
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.1f)) // Deep Void Blue
                .AddChild(new ForgeLabelBuilder("SYSTEMIC CONSTRAINTS (THE 'ISNTS')")
                    .WithFontSize(18).WithBold().WithColor(Color.magenta).Build())
                .AddChild(new ForgeLabelBuilder("Define what your world IS NOT. Hermit will resolve the rest.")
                    .WithFontSize(10).WithColor(Color.gray).Build());

            // Harvester Section
            var harvester = new ForgeContainerBuilder("Harvester")
                .WithPadding(10).WithMarginTop(10).WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f))
                .AddChild(new ForgeTextFieldBuilder("New Constraint", "e.g., No Magic, No Space Travel")
                    .OnValueChanged(evt => { /* Capture for Hermit Input */ })
                    .Build())
                .AddChild(new ForgeButtonBuilder("ADD CONSTRAINT")
                    .OnClick(() => { /* Add to _constraints */ })
                    .Build());

            builder.AddChild(harvester.Build());

            builder.AddSeparator(Color.magenta, 1);

            builder.AddChild(new ForgeButtonBuilder("MANIFEST VIA HERMIT")
                .WithBackgroundColor(new Color(0.4f, 0.1f, 0.6f)) // Hermit Purple
                .OnClick(() => {
                    ForgeLogger.Log("[Hermit] Analyzing constraints to manifest world...");
                    // Logic: Trigger Mini-Hermit services to fill Universe, Societal, and Civic APIs
                }).Build());

            return builder.Build();
        }
        private void ManifestViaHermit()
        {
            ForgeLogger.Log("[Hermit] Manifesting reality from negative constraints...");

            // Access the sovereign engine
            var engine = DigitalGenericUniversalRolePlayingSystem.Engine;
            if (engine == null) return;

            // Harvest the "Isnts" and apply them as systemic changes
            foreach (var constraint in _constraints)
            {
                if (constraint.Contains("No Magic"))
                {
                    engine.Universe.GetActiveUniverse().Mana = ManaLevel.None; // Converting "Isn't" to "Is"
                }

                if (constraint.Contains("Primitive Only"))
                {
                    engine.Universe.GetActiveUniverse().TechLevel = 3;
                    engine.Economy.SetGlobalTechFloor(3);
                }
            }

            // Trigger Mini-Hermits to fill the gaps
            engine.Demographics.GeneratePopulation(); // Birthed via 3d6 on GPU
            engine.Geography.Initialize();             // Map architecture manifested
        }
        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}