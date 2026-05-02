using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    public class Workshop_Gui_RouterManager : IGuiProvider
    {
        public string Title => "Live Router Switchboard & Editor";

        private readonly IGuiRouter _targetRouter;
        public Texture2D RepresentationalImage => null;

        public Workshop_Gui_RouterManager(IGuiRouter targetRouter)
        {
            _targetRouter = targetRouter;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("RouterManagerRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f))
                .WithPadding(20)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

            rootBuilder.AddChild(new ForgeLabelBuilder($"SWITCHBOARD & ROUTE EDITOR")
                .WithFontSize(24)
                .WithColor(Color.cyan)
                .WithFontStyle(FontStyle.Bold).Build());

            rootBuilder.AddChild(new ForgeLabelBuilder($"ACTIVE ROUTE: {_targetRouter.CurrentRoute}")
                .WithFontSize(14)
                .WithColor(Color.yellow)
                .WithMargin(0, 20).Build());

            var cardListContainer = new ScrollView { style = { flexGrow = 1 } };
            var wrapContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, justifyContent = Justify.Center } };
            cardListContainer.Add(wrapContainer);
            rootBuilder.AddChild(cardListContainer);

            var availableRoutes = _targetRouter.GetAvailableRoutes().ToList();

            foreach (var route in availableRoutes)
            {
                IGuiProvider provider = _targetRouter.GetProvider(route);
                if (provider == null) continue;

                var routeCard = new VisualElement
                {
                    style = {
                        width = 300, height = 350, backgroundColor = new Color(0.15f, 0.15f, 0.2f),
                        borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1,
                        borderTopColor = new Color(0.3f, 0.3f, 0.35f), borderBottomColor = new Color(0.3f, 0.3f, 0.35f), borderLeftColor = new Color(0.3f, 0.3f, 0.35f), borderRightColor = new Color(0.3f, 0.3f, 0.35f),
                        borderTopLeftRadius = 8, borderTopRightRadius = 8, borderBottomLeftRadius = 8, borderBottomRightRadius = 8,
                        paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15,
                        marginTop = 10, marginBottom = 10, marginLeft = 10, marginRight = 10,
                        justifyContent = Justify.SpaceBetween
                    }
                };

                var imageElement = new Image
                {
                    image = provider.RepresentationalImage,
                    style = {
                        width = Length.Percent(100), height = 180, alignSelf = Align.Center,
                        borderTopLeftRadius = 5, borderTopRightRadius = 5, borderBottomLeftRadius = 5, borderBottomRightRadius = 5
                    }
                };
                if (provider.RepresentationalImage == null) imageElement.style.backgroundColor = new Color(0.25f, 0.25f, 0.3f);
                routeCard.Add(imageElement);

                routeCard.Add(new ForgeLabelBuilder($"{provider.Title} ({route})")
                    .WithFontSize(16)
                    .WithColor(Color.white)
                    .WithWhiteSpace(WhiteSpace.Normal)
                    .WithMargin(0, 10)
                    .WithFontStyle(FontStyle.Bold).Build());

                var buttonRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceAround } };

                var navigateBtn = new ForgeButtonBuilder(route == _targetRouter.CurrentRoute ? "Active" : $"Navigate To", () => _targetRouter.NavigateTo(route))
                    .WithBackgroundColor(route == _targetRouter.CurrentRoute ? new Color(0.2f, 0.6f, 0.2f) : new Color(0.3f, 0.3f, 0.3f))
                    .WithTextColor(Color.white)
                    .WithHeight(40)
                    .WithMargin(0, 0, 5, 5)
                    .Build();
                navigateBtn.style.flexGrow = 1;
                buttonRow.Add(navigateBtn);

                var copyBtn = new ForgeButtonBuilder($"Copy Route", () => Debug.Log($"Copying logic for: {route}"))
                    .WithBackgroundColor(new Color(0.3f, 0.3f, 0.6f))
                    .WithTextColor(Color.white)
                    .WithHeight(40)
                    .WithMargin(0, 0, 0, 0)
                    .Build();
                copyBtn.style.flexGrow = 1;
                buttonRow.Add(copyBtn);

                routeCard.Add(buttonRow);
                wrapContainer.Add(routeCard);
            }

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}