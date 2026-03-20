using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class GuiFlowRouterBuilder : IGuiProvider, IGuiRouter
    {
        public string Title { get; private set; }

        // The Routing Map: Maps a String ID to a function that builds a GUI
        private Dictionary<string, Func<IGuiRouter, IGuiProvider>> _routes = new Dictionary<string, Func<IGuiRouter, IGuiProvider>>();

        private string _initialRoute;
        private VisualElement _container;
        private GuiContext _lastCtx;

        public GuiFlowRouterBuilder(string title, string initialRoute)
        {
            Title = title;
            _initialRoute = initialRoute;
        }

        // FLUENT BUILDER: Defines the connections between GUIs
        public GuiFlowRouterBuilder AddRoute(string routeId, Func<IGuiRouter, IGuiProvider> guiFactory)
        {
            _routes[routeId] = guiFactory;
            return this;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            _container = new VisualElement
            {
                style = {
                    flexGrow = 1,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                    backgroundColor = new Color(0.05f, 0.05f, 0.08f) // Deep space background
                }
            };

            // Boot up the first screen
            NavigateTo(_initialRoute);

            return _container;
        }

        // THE MEDIATOR: Destroys the old GUI, builds the new one, and injects it.
        public void NavigateTo(string routeId)
        {
            if (_container == null) return;

            if (!_routes.ContainsKey(routeId))
            {
                Debug.LogError($"[GuiRouter] Route '{routeId}' does not exist!");
                return;
            }

            // Obliterate the old screen completely
            _container.Clear();

            // Build the new screen (passing 'this' so the GUI can route again later)
            IGuiProvider nextGui = _routes[routeId].Invoke(this);

            // Mount it to the visual tree
            _container.Add(nextGui.CreateGui(_lastCtx));
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}