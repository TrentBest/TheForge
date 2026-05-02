using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Ants
{
    // ====================================================================
    // THE ALPHA HUB : ENTITY INGESTION & DEPLOYMENT
    // ====================================================================
    public class Ants_Playable_Alpha : IGuiProvider
    {
        public string Title => "Myrmecology Forge Hub";
        private readonly IGuiRouter _router;

        public Ants_Playable_Alpha(IGuiRouter router) { _router = router; }

        public Ants_Playable_Alpha() { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // The Hub uses a Split Panel: Left for Entity Data, Right for Environmental Deployment
            return new ForgeSplitPanelBuilder(sidebarWidth: 380, Side.Left)
                .WithSidebar(BuildEntityProfile(ctx))
                .WithMain(BuildSimulationRoutes(ctx))
                .CreateGui(ctx);
        }

        private IGuiProvider BuildEntityProfile(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("EntityProfile")
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))

                // --- ENTITY HEADER ---
                .AddHeader("ENTITY PROFILE")
                .AddSeparator(new Color(0.8f, 0.4f, 0.1f), 2)
                .AddChild(new Label("TAXONOMY: Formicidae") { style = { color = Color.white, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 } })
                .AddChild(new Label("CLASS: Emergent Micro-Package") { style = { color = new Color(0.6f, 0.6f, 0.7f), fontSize = 12, letterSpacing = 1, marginBottom = 20 } })

                // --- COMPOSITION DATA ---
                .AddChild(new Label("The Ant is not a monolithic object. It is a sovereign data composition of behavioral FSMs, kinematic gestures, and chemical properties.")
                {
                    style = { color = Color.silver, whiteSpace = WhiteSpace.Normal, fontSize = 14, marginBottom = 20 }
                })

                .AddSeparator(new Color(0.3f, 0.3f, 0.3f), 1)
                .AddChild(new Label("CHEMICAL COMPOSITION MAP") { style = { color = new Color(0.8f, 0.4f, 0.1f), unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 10 } })
                .AddChild(new Label("Primary Defense Compound: Formic Acid (CH₂O₂)") { style = { color = Color.white, marginBottom = 10 } })

                // --- DATA LINKAGE BUTTON ---
                // This routes the user out of the Ant package and directly into the Physics/Chemistry package!
                .AddChild(new Button(() => RouteToChemistryForge())
                {
                    text = "INSPECT ATOMIC STRUCTURE",
                    style = { backgroundColor = new Color(0.2f, 0.5f, 0.8f), color = Color.white, height = 40, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 30 }
                })

                // --- NAVIGATION ---
                .AddChild(new Button(() => _router?.NavigateTo("Intro"))
                {
                    text = "<- ABORT DEPLOYMENT",
                    style = { backgroundColor = Color.clear, color = Color.gray, height = 30, marginTop = 40, borderTopWidth = 0, borderBottomWidth = 0, borderLeftWidth = 0, borderRightWidth = 0 }
                });
        }

        private IGuiProvider BuildSimulationRoutes(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("SimRoutes")
                 .WithPadding(40)
                 .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))

                 .AddChild(new Label("SELECT DEPLOYMENT ENVIRONMENT")
                 {
                     style = { color = Color.white, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 40, alignSelf = Align.Center }
                 })

                 .OnBuild(ve =>
                 {
                     var btnContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.Center } };

                     // Option 1: The 2D Unmanaged Array (Cellular Automata)
                     var btnMicro = new ForgeButtonBuilder("2D MICRO-ENVIRONMENT\n\n[ Cellular Automata ]", () => _router?.NavigateTo("AntFarmCA"))
                         .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                         .WithTextColor(new Color(0.8f, 0.4f, 0.1f))
                         .WithHeight(150)
                         .WithWidth(280)
                         .WithMargin(0, 20, 0, 20)
                         .WithFontSize(16)
                         .WithFontStyle(FontStyle.Bold)
                         .Build(); // Assuming Build() returns the VisualElement

                     btnContainer.Add(btnMicro);

                     // Option 2: The 3D Compute Shader (SimAnt)
                     var btnMacro = new ForgeButtonBuilder("3D MACRO-ENVIRONMENT\n\n[ Elastic Compute Scheduling ]", () => _router?.NavigateTo("SimAnt"))
                         .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                         .WithTextColor(new Color(0.8f, 0.4f, 0.1f))
                         .WithHeight(150)
                         .WithWidth(280)
                         .WithMargin(0, 20, 0, 20)
                         .WithFontSize(16)
                         .WithFontStyle(FontStyle.Bold)
                         .Build();

                     btnContainer.Add(btnMacro);

                     ve.Add(btnContainer);
                 });
        }

        private void RouteToChemistryForge()
        {
            // In the Singularity Architecture, we don't just load a scene. We pass a context payload.
            // We tell the router to open the Periodic Table, and we pass "Carbon", "Hydrogen", and "Oxygen" as the focus targets.
            ForgeLogger.Log("[Data Linkage] Exporting CH2O2 Payload to Chemistry Forge...");

            // Example of how you would route this in your ecosystem:
            // _router?.NavigateTo("PeriodicTable", new ContextPayload("CH2O2"));
        }

        private Style[] GetRouteButtonStyle()
        {
            // A helper to keep the massive routing buttons consistent
            return new Style[]
            {
                new Style {
                    backgroundColor = new Color(0.15f, 0.15f, 0.18f),
                    color = new Color(0.8f, 0.4f, 0.1f),
                    height = 150,
                    width = 280,
                    marginRight = 20,
                    marginLeft = 20,
                    fontSize = 16,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    whiteSpace = WhiteSpace.Normal,
                    borderTopWidth = 2,
                    borderTopColor = new Color(0.8f, 0.4f, 0.1f)
                }
            };
        }

        // Implicit cast helper for setting styles on UI Toolkit elements cleanly
        private class Style
        {
            public Color backgroundColor; public Color color; public float height; public float width;
            public float marginRight; public float marginLeft; public float marginTop; public float marginBottom;
            public int fontSize; public FontStyle unityFontStyleAndWeight; public WhiteSpace whiteSpace;
            public float borderTopWidth; public Color borderTopColor; public float borderBottomWidth; public float borderLeftWidth; public float borderRightWidth;

            public static implicit operator UnityEngine.UIElements.StyleColor(Style s) => s.backgroundColor;
            // Note: In a production script, you'd apply these properties directly to the VisualElement.style. 
            // The helper method above simplifies the initialization block.
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}