using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.PillVirus
{
    public class PillVirus_MainMenu : IGuiProvider
    {
        public string Title => "Main Menu";

        private IGuiRouter _router;

        // 1. DEFAULT CONSTRUCTOR FOR EDITOR PREVIEWS (Prevents NullRef if opened standalone)
        public PillVirus_MainMenu() { }

        // 2. INJECTED CONSTRUCTOR FOR PLAYABLE ALPHA ROUTING
        public PillVirus_MainMenu(IGuiRouter router)
        {
            _router = router;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement
            {
                style = {
                    flexGrow = 1, flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap,
                    backgroundColor = new Color(0.05f, 0.05f, 0.1f)
                }
            };

            // Helper to build quadrant buttons
            VisualElement CreateQuadrant(string text, Color color, Action onClick)
            {
                var container = new VisualElement
                {
                    style = {
                        width = Length.Percent(50),
                        height = Length.Percent(50),
                        paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20
                    }
                };
                var btn = new Button(onClick) { text = text, style = { flexGrow = 1, backgroundColor = color, fontSize = 24, color = Color.white } };
                container.Add(btn);
                return container;
            }

            // Top Left: Single Player -> Routes to Difficulty safely using null check
            root.Add(CreateQuadrant("SINGLE PLAYER", new Color(0.2f, 0.4f, 0.8f), () => {
                if (_router != null) _router.NavigateTo("Difficulty");
                else ForgeLogger.LogWarning("Router is null! Launch via PillVirus_App instead.");
            }));

            // Top Right: Multiplayer
            root.Add(CreateQuadrant("MULTIPLAYER", new Color(0.8f, 0.4f, 0.2f), () => ForgeLogger.Log("Multiplayer TBD")));

            // Bottom Left: Settings
            root.Add(CreateQuadrant("SETTINGS", new Color(0.3f, 0.3f, 0.3f), () => ForgeLogger.Log("Settings TBD")));

            // Bottom Right: Quit
            //root.Add(CreateQuadrant("QUIT", new Color(0.8f, 0.2f, 0.2f), () => Application.Quit()));

            // --- CENTER OVERLAY: Unity Sign In ---
            bool isSignedIn = false; // Mock
            if (!isSignedIn)
            {
                var signInBtn = new Button(() => { ForgeLogger.Log("Authenticating..."); })
                {
                    text = "Sign In With Unity",
                    style = {
                        position = Position.Absolute,
                        left = Length.Percent(50), top = Length.Percent(50),
                        translate = new Translate(Length.Percent(-50), Length.Percent(-50)),
                        width = 250, height = 60, fontSize = 20,
                        backgroundColor = new Color(0.1f, 0.1f, 0.15f), color = Color.white,
                        borderTopWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, borderRightWidth = 2,
                        borderTopColor = new Color(0.6f, 0.8f, 0.9f), borderBottomColor = new Color(0.6f, 0.8f, 0.9f),
                        borderLeftColor = new Color(0.6f, 0.8f, 0.9f), borderRightColor = new Color(0.6f, 0.8f, 0.9f)
                    }
                };
                root.Add(signInBtn);
            }

            return root;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
    }
}