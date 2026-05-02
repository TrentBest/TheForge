using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Ants
{
    public class Ants_Gui_Intro : IGuiProvider
    {
        public string Title => "MYRMECOLOGY INTRO";
        private readonly IGuiRouter _router;

        public Ants_Gui_Intro(IGuiRouter router) { _router = router; }

        public Ants_Gui_Intro() { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("AntsIntro")
                .WithPadding(40)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))

                // Header
                .AddChild(new Label("MYRMECOLOGY MICRO-PACKAGE")
                {
                    style = { color = new Color(0.8f, 0.4f, 0.1f), fontSize = 32, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5, alignSelf = Align.Center }
                })
                .AddChild(new Label("ENVIRONMENTAL ARBITRATION & 3D COMPUTE")
                {
                    style = { color = new Color(0.5f, 0.5f, 0.6f), fontSize = 14, letterSpacing = 2, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 40, alignSelf = Align.Center }
                })

                // Explainer Text
                .AddChild(new Label("This showcase demonstrates the Forge's Package Arbitration and unmanaged GPU processing. Ants are not hardcoded entities; they are an emergent Micro-Package.")
                {
                    style = { color = Color.silver, fontSize = 16, whiteSpace = WhiteSpace.Normal, marginBottom = 15 }
                })
                .AddChild(new Label("When the global environment's macro-state (Temperature, Humidity, Biome) reaches the correct thresholds, the Physics Arbiter dynamically allocates unmanaged VRAM and injects the 3D Myrmecology Compute Shader into the Elastic Scheduler.")
                {
                    style = { color = Color.silver, fontSize = 16, whiteSpace = WhiteSpace.Normal, marginBottom = 15 }
                })
                .AddChild(new Label("Millions of agents navigate a 3D packed-voxel pheromone grid, ticking in the background only when the CPU has idle milliseconds.")
                {
                    style = { color = Color.silver, fontSize = 16, whiteSpace = WhiteSpace.Normal, marginBottom = 50 }
                })

                // Action Route
                .AddChild(new Button(() => _router?.NavigateTo("Interactive"))
                {
                    text = "INITIALIZE ARBITRATION",
                    style = {
                        backgroundColor = new Color(0.8f, 0.4f, 0.1f),
                        color = Color.black,
                        height = 50,
                        width = 300,
                        alignSelf = Align.Center,
                        fontSize = 16,
                        unityFontStyleAndWeight = FontStyle.Bold
                    }
                });

            return rootBuilder.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}