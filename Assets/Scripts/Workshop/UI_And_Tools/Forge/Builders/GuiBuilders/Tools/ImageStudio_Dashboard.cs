using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    public class ImageStudio_Dashboard : IGuiProvider
    {
        public string Title => "Studio Dashboard";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var theme = GuiSkin.Active?.GlobalDefault;
            Color bgColor = theme != null ? theme.BackgroundColor : new Color(0.1f, 0.1f, 0.1f);
            Color fgColor = theme != null ? theme.TextColor : Color.white;

            var rootBuilder = new GraphicalUserInterfaceBuilder("ImageStudioDashboard")
                .WithAutoGrow()
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Center)
                .WithBackgroundColor(bgColor)
                .WithPadding(30);

            // Hero Headers
            rootBuilder.AddChild(context => new Label("IMAGE STUDIO SUITE")
            {
                style = { fontSize = 36, unityFontStyleAndWeight = FontStyle.Bold, color = fgColor, marginBottom = 10, letterSpacing = 5 }
            });
            rootBuilder.AddChild(context => new Label("Select a forging tool below, or right-click an asset in your world to contextually open it.")
            {
                style = { fontSize = 14, color = fgColor, opacity = 0.6f, marginBottom = 40, unityFontStyleAndWeight = FontStyle.Italic }
            });

            // Tools Grid (Using a nested GUI Builder for layout)
            var gridBuilder = new GraphicalUserInterfaceBuilder("ToolsGrid")
                .WithAutoGrow()
                .WithFlexLayout(FlexDirection.Row, Justify.Center, Align.FlexStart)
                .WithFlexWrap(Wrap.Wrap);

            // Build and attach the cards
            gridBuilder.AddChild(CreateToolCard("Texture Forge", "Live 2D-to-3D UV painting and pixel manipulation.", new Color(0.0f, 1.0f, 1.0f), () => ctx.Log?.Invoke("Routing to Texture Forge...")));
            gridBuilder.AddChild(CreateToolCard("Material Forge", "Physically Based Rendering (PBR) node graphs.", new Color(0.8f, 0.4f, 0.2f), () => ctx.Log?.Invoke("Routing to Material Forge...")));
            gridBuilder.AddChild(CreateToolCard("Mesh Forge", "Procedural 3D generation from 2D maps.", new Color(0.2f, 0.8f, 0.4f), () => ctx.Log?.Invoke("Routing to Mesh Forge...")));

            rootBuilder.AddChild(gridBuilder);

            return rootBuilder.Build();
        }

        // Returns an IGuiProvider so it can be cleanly injected into AddChild()
        private IGuiProvider CreateToolCard(string title, string desc, Color accent, Action onClick)
        {
            var theme = GuiSkin.Active?.GlobalDefault;
            Color cardBg = theme != null ? Color.Lerp(theme.BackgroundColor, Color.white, 0.05f) : new Color(0.15f, 0.15f, 0.15f);
            Color fgColor = theme != null ? theme.TextColor : Color.white;

            var cardBuilder = new GraphicalUserInterfaceBuilder($"Card_{title.Replace(" ", "")}")
                .WithSize(250, 200)
                .WithBackgroundColor(cardBg)
                .WithMargin(15)
                .WithPadding(20)
                .WithBorderTopWidth(4)
                .WithBorderTopColor(accent)
                .WithBorderRadius(8)
                .WithFlexLayout(FlexDirection.Column, Justify.SpaceBetween, Align.Stretch);

            // --- THE NEW RIGHT-CLICK CONTEXT MENU ---
            var cardContextMenu = new ContextMenuGuiBuilder()
                .AddAction($"Launch {title} in New Tab", _ => onClick?.Invoke())
                .AddAction($"Launch {title} in Floating Window", _ => Debug.Log("Launching floating window..."))
                .AddSeparator()
                .AddAction("View Documentation", _ => Debug.Log($"Opening docs for {title}..."));

            cardBuilder.WithContextMenu(cardContextMenu);
            // ----------------------------------------

            // Populate the card contents
            cardBuilder.AddChild(context => new Label(title) { style = { fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, color = fgColor } });
            cardBuilder.AddChild(context => new Label(desc) { style = { fontSize = 12, color = fgColor, opacity = 0.7f, whiteSpace = WhiteSpace.Normal } });

            // Standard Button
            cardBuilder.AddButton("LAUNCH", onClick);

            // Add Hover Effects via the OnBuild hook
            cardBuilder.OnBuild(visualElement => {
                visualElement.RegisterCallback<MouseEnterEvent>(e => visualElement.style.backgroundColor = Color.Lerp(cardBg, accent, 0.1f));
                visualElement.RegisterCallback<MouseLeaveEvent>(e => visualElement.style.backgroundColor = cardBg);
            });

            return cardBuilder;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}